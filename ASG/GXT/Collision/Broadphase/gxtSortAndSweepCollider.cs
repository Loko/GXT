using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using GXT.Rendering;

namespace GXT
{
    /// <summary>
    /// An efficient broadphase collider using a linked list implmentation of the Sort and Sweep algorithm (aka Sweep and Prune).
    /// Keeps a persistent collection of object-object collision pairs to avoid expensive brute force tests.  Sorting is performed 
    /// on both axes.  Also supports ray, point, and other shape casting queries.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // TODO: A SEARCH AXIS FOR ADDOBJ, ETC WHEN TRYING TO FIND A BOX VIA AN OBJECT
    // Tail node, possibly a midpoint node to speed up raycasting
    public class gxtSortAndSweepCollider<T> : gxtIBroadPhaseCollider<T>
    {
        #region Endpoints/Boxes
        /// <summary>
        /// Represents an endpoint on the object's AABB
        /// </summary>
        /// <typeparam name="T">Game Object Type</typeparam>
        private class gxtSAPEndpoint
        {
            public gxtSAPBox box;
            public gxtSAPEndpoint[] prev;
            public gxtSAPEndpoint[] next;
            public float[] value;
            public bool isMin;

            public gxtSAPEndpoint()
            {
                value = new float[2];
                prev = new gxtSAPEndpoint[2];
                next = new gxtSAPEndpoint[2];
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                if (prev == null)
                    builder.Append("NULL");
                builder.Append("<-[ " + value + " " + GetHashCode() + " " + isMin + " ]->");
                //builder.Append("<-[" + value + "]->");
                if (next == null)
                    builder.Append("NULL");
                return builder.ToString();
            }

            public string ToString(int id)
            {
                StringBuilder builder = new StringBuilder();
                if (prev == null)
                    builder.Append("NULL");
                builder.Append("<-[ " + value + " " + id + " " + isMin + " ]->");
                //builder.Append("<-[" + value + "]->");
                if (next == null)
                    builder.Append("NULL");
                return builder.ToString();
            }
        }

        /// <summary>
        /// Represents an AABB and the associated game object
        /// </summary>
        /// <typeparam name="T">Game Object Type</typeparam>
        private class gxtSAPBox
        {
            public gxtSAPEndpoint min;
            public gxtSAPEndpoint max;
            public T obj;

            public gxtSAPBox()
            {
                min = new gxtSAPEndpoint();
                max = new gxtSAPEndpoint();
            }
        }
        #endregion

        private gxtSAPEndpoint[] listHead;
        private HashSet<gxtBroadphaseCollisionPair<T>> pairManager;

        /// <summary>
        /// Determines if the sort and sweep collider has already been initialized
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return listHead != null && pairManager != null;
        }

        /// <summary>
        /// Sets up lists and sentinel nodes
        /// </summary>
        public void Initialize()
        {
            gxtDebug.Assert(!IsInitialized(), "Sort and Sweep Collider Has Already Been Initialized!");

            listHead = new gxtSAPEndpoint[2];
            pairManager = new HashSet<gxtBroadphaseCollisionPair<T>>();

            // using sentinels makes for faster/easier insertion code
            // it removes the need for special head/tail checks
            // based on code seen in RTCD
            gxtSAPBox sentinel = new gxtSAPBox();
            for (int i = 0; i < 2; i++)
            {
                sentinel.min.prev[i] = null;
                sentinel.min.next[i] = sentinel.max;
                sentinel.max.prev[i] = sentinel.min;
                sentinel.max.next[i] = null;
                sentinel.min.value[i] = float.NegativeInfinity;
                sentinel.max.value[i] = float.PositiveInfinity;
                listHead[i] = sentinel.min;
            }
            sentinel.min.box = sentinel;
            sentinel.max.box = sentinel;
            sentinel.min.isMin = false;
            sentinel.max.isMin = true;
        }

        /// <summary>
        /// Min-Max AABB overlap test using boxes
        /// </summary>
        /// <param name="boxA">BoxA</param>
        /// <param name="boxB">BoxB</param>
        /// <returns>If Intersecting</returns>
        private bool Overlap(gxtSAPBox boxA, gxtSAPBox boxB)
        {
            if (boxA.max.value[0] < boxB.min.value[0] || boxA.min.value[0] > boxB.max.value[0]) return false;
            if (boxA.max.value[1] < boxB.min.value[1] || boxA.min.value[1] > boxB.max.value[1]) return false;
            return true;
        }

        /// <summary>
        /// Overlap test which takes an AABB and a box as arguments
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="aabb">AABB</param>
        /// <returns>If intersecting</returns>
        private bool Overlap(gxtSAPBox box, gxtAABB aabb)
        {
            if (box.max.value[0] < aabb.Min.X || box.min.value[0] > aabb.Max.X) return false;
            if (box.max.value[1] < aabb.Min.Y || box.min.value[1] > aabb.Max.Y) return false;
            return true;
        }

        /// <summary>
        /// Min-Max AABB contains test using boxes
        /// </summary>
        /// <param name="box">Box</param>
        /// <param name="pt">Pt</param>
        /// <returns>If contained</returns>
        private bool Contains(gxtSAPBox box, Vector2 pt)
        {
            if (pt.X < box.min.value[0] || pt.X > box.max.value[0]) return false;
            if (pt.Y < box.min.value[1] || pt.Y > box.max.value[1]) return false;
            return true;
        }

        private bool DistanceToCenter(gxtSAPBox box, Vector2 pt, ref float distSq)
        {
            if (Contains(box, pt))
            {
                Vector2 center = new Vector2((box.min.value[0] + box.max.value[0]) * 0.5f, (box.min.value[1] + box.max.value[1]) * 0.5f);
                distSq = (pt - center).LengthSquared();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the AABB equivalent of an SAPBox
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private gxtAABB GetAABB(gxtSAPBox box)
        {
            Vector2 min = new Vector2(box.min.value[0], box.min.value[1]);
            Vector2 max = new Vector2(box.max.value[0], box.max.value[1]);
            Vector2 center = (min + max) * 0.5f;
            Vector2 extents = max - center;
            return new gxtAABB(center, extents);
        }

        /// <summary>
        /// Adds a new object to the collider
        /// </summary>
        /// <param name="obj">Game Object</param>
        /// <param name="aabb">Associated AABB</param>
        public void AddObject(T obj, ref gxtAABB aabb)
        {
            // zero extent allowed, but negative extents are not
            gxtDebug.Assert(!aabb.IsEmpty(), "Cannot add negative extent AABB to collider");

            // get values
            Vector2 aabbMin = aabb.Min;
            Vector2 aabbMax = aabb.Max;

            // set all values and references
            gxtSAPBox box = new gxtSAPBox();
            box.obj = obj;
            box.min.value[0] = aabbMin.X;
            box.min.value[1] = aabbMin.Y;
            box.max.value[0] = aabbMax.X;
            box.max.value[1] = aabbMax.Y;
            box.min.isMin = true;
            box.min.box = box;
            box.max.box = box;

            // Inserts the new endpoints
            gxtSAPEndpoint endpoint;
            for (int i = 0; i < 2; i++)
            {
                endpoint = listHead[i];
                while (endpoint.value[i] < box.min.value[i])
                    endpoint = endpoint.next[i];

                box.min.prev[i] = endpoint.prev[i];
                box.min.next[i] = endpoint;
                endpoint.prev[i].next[i] = box.min;
                endpoint.prev[i] = box.min;

                while (endpoint.value[i] < box.max.value[i])
                    endpoint = endpoint.next[i];

                box.max.prev[i] = endpoint.prev[i];
                box.max.next[i] = endpoint;
                endpoint.prev[i].next[i] = box.max;
                endpoint.prev[i] = box.max;
            }
            
            // this method below could be replaced with a zero extents add at float.minvalue
            // and then an update with the real aabb.  This is still an O(n) operation
            // According to RTCD it could be done in the code above...
            endpoint = listHead[0];
            while (endpoint != null)
            {
                if (endpoint.isMin)
                {
                    if (endpoint.value[0] > box.max.value[0])
                        break;
                    if (Overlap(endpoint.box, box))
                        AddPair(endpoint.box.obj, box.obj);
                }
                else if (endpoint.value[0] < box.min.value[0])
                    break;
                endpoint = endpoint.next[0];
            }
        }

        /// <summary>
        /// Updates an existing object in the collider with a newly computed AABB
        /// </summary>
        /// <param name="obj">Game Object</param>
        /// <param name="aabb">Associated AABB</param>
        public void UpdateObject(T obj, ref gxtAABB aabb)
        {
            // zero extent allowed, but negative extents are not
            gxtDebug.Assert(!aabb.IsEmpty(), "Cannot update negative extent AABB in the collider");

            // try to find the associated box
            gxtSAPEndpoint endpoint = listHead[0];
            while (endpoint != null)
            {
                if (obj.Equals(endpoint.box.obj))
                    break;
                endpoint = endpoint.next[0];
            }

            // if not found log a message and return
            if (endpoint == null)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The object: {0} was not found in the collider and cannot be updated", obj.ToString());
                return;
            }

            gxtSAPBox box = endpoint.box;

            // get/update values
            Vector2 aabbMin = aabb.Min;
            Vector2 aabbMax = aabb.Max;
            box.min.value[0] = aabbMin.X;
            box.min.value[1] = aabbMin.Y;
            box.max.value[0] = aabbMax.X;
            box.max.value[1] = aabbMax.Y;

            // sort on both axis
            for (int i = 0; i < 2; i++)
            {
                gxtSAPEndpoint min = box.min, max = box.max, t;

                // try to move the min endpoint left
                for (t = min.prev[i]; min.value[i] < t.value[i]; t = t.prev[i])
                {
                    if (!t.isMin)
                        if (Overlap(box, t.box))
                            AddPair(obj, t.box.obj);
                }

                if (t != min.prev[i])
                    ShiftEndpoint(i, min, t);

                // try to move the max endpoint right
                for (t = max.next[i]; max.value[i] > t.value[i]; t = t.next[i])
                {
                    if (t.isMin)
                        if (Overlap(box, t.box))
                            AddPair(obj, t.box.obj);
                }

                if (t != max.next[i])
                    ShiftEndpoint(i, max, t.prev[i]);

                // try to move the min endpoint right
                for (t = min.next[i]; min.value[i] > t.value[i]; t = t.next[i])
                {
                    if (!t.isMin)
                        RemovePair(obj, t.box.obj);
                }

                if (t != min.next[i])
                    ShiftEndpoint(i, min, t.prev[i]);

                // try to move the max endpoint left
                for (t = max.prev[i]; max.value[i] < t.value[i]; t = t.prev[i])
                {
                    if (t.isMin)
                        RemovePair(obj, t.box.obj);
                }

                if (t != max.prev[i])
                    ShiftEndpoint(i, max, t);
            }
        }

        /// <summary>
        /// Removes an object from the collider
        /// </summary>
        /// <param name="obj">Game Object</param>
        /// <returns>If removed</returns>
        public bool RemoveObject(T obj)
        {
            // find the endpoint/box with the game object
            gxtSAPEndpoint endpoint = listHead[0];
            while (endpoint != null)
            {
                if (obj.Equals(endpoint.box.obj))
                    break;
                endpoint = endpoint.next[0];
            }

            // early return if not found
            if (endpoint == null)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The object: {0} was not found in the collider and cannot be removed", obj.ToString());
                return false;
            }

            // remove the min/max endpoint, and all pairs which contain the object
            RemoveEndpoint(endpoint);
            RemoveEndpoint(endpoint.box.max);
            RemoveAssociatedPairs(obj);
            return true;
        }

        
        public void RemoveAllObjects()
        {
            gxtDebug.Assert(IsInitialized(), "Cannot remove all objects if collider hasn't been initialized!");
            gxtSAPEndpoint endpoint = listHead[0].next[0];
            while (endpoint.next[0] != null)
            {
                RemoveEndpoint(endpoint);
            }
            pairManager.Clear();
        }
        

        /// <summary>
        /// Internal function that unlinks the endpoint and inserts it after the node
        /// Sentinels prevent need for special head/tail/null checks
        /// </summary>
        /// <param name="axis">Axis to perfrom shift</param>
        /// <param name="endpoint">Endpoint to move</param>
        /// <param name="node">Node to insert after</param>
        private void ShiftEndpoint(int axis, gxtSAPEndpoint endpoint, gxtSAPEndpoint node)
        {
            endpoint.prev[axis].next[axis] = endpoint.next[axis];
            endpoint.next[axis].prev[axis] = endpoint.prev[axis];

            endpoint.prev[axis] = node;
            endpoint.next[axis] = node.next[axis];
            node.next[axis].prev[axis] = endpoint;
            node.next[axis] = endpoint;
        }

        /// <summary>
        /// Internal function to remove the endpoint from the function
        /// </summary>
        /// <param name="endpoint">Endpoint to remove</param>
        private void RemoveEndpoint(gxtSAPEndpoint endpoint)
        {
            endpoint.prev[0].next[0] = endpoint.next[0];
            endpoint.next[0].prev[0] = endpoint.prev[0];
            endpoint.prev[1].next[1] = endpoint.next[1];
            endpoint.next[1].prev[1] = endpoint.prev[1];
            endpoint.box = null;
        }

        /// <summary>
        /// Adds a collision pair if and only if the two objects are not the same and the 
        /// pair does not already exist in the pair manager
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objB"></param>
        private void AddPair(T objA, T objB)
        {
            // don't add if they are the same object
            if (objA.Equals(objB))
                return;

            gxtBroadphaseCollisionPair<T> pair = new gxtBroadphaseCollisionPair<T>(objA, objB);

            // hash set ensures existing pairs are not added
            pairManager.Add(pair);

            // if we were using a dictionary it would look like this
            //if (pairManager.ContainsKey(pair))
            //    return;
            //pairManager.Add(pair, true);
        }

        /// <summary>
        /// Removes a collision pair
        /// </summary>
        /// <param name="objA">Object A</param>
        /// <param name="objB">Object B</param>
        private void RemovePair(T objA, T objB)
        {
            gxtBroadphaseCollisionPair<T> pair = new gxtBroadphaseCollisionPair<T>(objA, objB);
            pairManager.Remove(pair);
        }

        /// <summary>
        /// Call when the object is removed from the collider
        /// All pairs including the removed object will be cleared with this function
        /// </summary>
        /// <param name="obj">Object</param>
        private void RemoveAssociatedPairs(T obj)
        {
            pairManager.RemoveWhere(item => { return item.objA.Equals(obj) || item.objB.Equals(obj); });
            //item => item.category.Name == name
            // iterators break when underlying collections are modified, so we need to copy 
            // all the keys to an array and iterate over that instead
            /*
            gxtBroadphaseCollisionPair<T>[] pairKeys = new gxtBroadphaseCollisionPair<T>[pairManager.Count];
            pairManager.Keys.CopyTo(pairKeys, 0);
            for (int i = 0; i < pairKeys.Length; i++)
            {
                if (pairKeys[i].objA.Equals(obj) || pairKeys[i].objB.Equals(obj))
                    pairManager.Remove(pairKeys[i]);
            }
            */
        }

        /// <summary>
        /// Gets array of broadphase collision pairs
        /// </summary>
        /// <returns></returns>
        public gxtBroadphaseCollisionPair<T>[] GetCollisionPairs()
        {
            // copies pairs into an array
            gxtBroadphaseCollisionPair<T>[] pairs = new gxtBroadphaseCollisionPair<T>[pairManager.Count];
            pairManager.CopyTo(pairs, 0, pairManager.Count);
            return pairs;
        }

        /// <summary>
        /// Gets all the objects whose AABB's contain the given point
        /// </summary>
        /// <param name="pt">Pt</param>
        /// <returns>List of hit objects</returns>
        public List<T> PointCastAll(Vector2 pt)
        {
            List<T> hitObjects = new List<T>();

            gxtSAPEndpoint endpoint = listHead[0].next[0];
            while (endpoint != null)
            {
                if (endpoint.isMin)
                {
                    if (endpoint.value[0] > pt.X)
                        break;
                    if (Contains(endpoint.box, pt))
                        hitObjects.Add(endpoint.box.obj);
                }
                else if (endpoint.value[0] > pt.X)
                    break;
                endpoint = endpoint.next[0];
            }

            return hitObjects;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public T PointCast(Vector2 pt)
        {
            float distSq = float.MaxValue;
            T nearestObject = default(T);
            float t = float.MaxValue;

            gxtSAPEndpoint endpoint = listHead[0].next[0];
            while (endpoint != null)
            {
                if (endpoint.isMin)
                {
                    if (endpoint.value[0] > pt.X)
                        break;
                    if (DistanceToCenter(endpoint.box, pt, ref t))
                    {
                        if (t < distSq)
                        {
                            nearestObject = endpoint.box.obj;
                            distSq = t;
                        }
                    }
                }
                else if (endpoint.value[0] > pt.X)
                    break;
                endpoint = endpoint.next[0];
            }

            return nearestObject;
        }

        /// <summary>
        /// Gets all the objects whose AABB intersects the ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="tmax">Max Distance</param>
        /// <param name="insideIsCollision">If the ray origin inside an AABB still counts as a collision</param>
        /// <returns>List of all intersected objects</returns>
        public List<T> RayCastAll(gxtRay ray, float tmax = float.MaxValue, bool insideIsCollision = true)
        {
            List<T> hitObjects = new List<T>();

            gxtSAPEndpoint endpoint = listHead[0];

            while (endpoint != null)
            {
                if (endpoint.isMin)
                {
                    if (endpoint.value[0] > ray.Origin.X)
                        break;
                    else if (endpoint.box.max.value[0] > ray.Origin.X)
                        break;  // necessary for boxes whose min is less than the origin, but whose max isn't
                }
                endpoint = endpoint.next[0];
            }
            
            // this should still work for unit length rays on the y axis
            if (ray.Direction.X < 0.0f)
            {
                float rdx = gxtMath.Abs(ray.Direction.X * tmax);
                // if is min and dx between the ray origin and the endpoint is 
                // greater than tmax we can stop searching
                while (endpoint != null)
                {
                    if (!endpoint.isMin)
                        if (ray.Origin.X - endpoint.value[0] > rdx)
                            break;
                    // special sentinel check??
                    // might be able to swap contains check for min/max tricks
                    if (endpoint.box.obj != null && ray.IntersectsAABB(GetAABB(endpoint.box), tmax, insideIsCollision))
                        if (!hitObjects.Contains(endpoint.box.obj))
                            hitObjects.Add(endpoint.box.obj);
                    endpoint = endpoint.prev[0];
                }
            }
            else
            {
                float rdx = gxtMath.Abs(ray.Direction.X * tmax);
                while (endpoint != null)
                {
                    if (endpoint.isMin)
                        if (endpoint.value[0] - ray.Origin.X > rdx)
                            break;
                    // special sentinel check??
                    // might be able to swap contains check for min/max tricks
                    if (endpoint.box.obj != null && ray.IntersectsAABB(GetAABB(endpoint.box), tmax, insideIsCollision))
                        if (!hitObjects.Contains(endpoint.box.obj))
                            hitObjects.Add(endpoint.box.obj);
                    endpoint = endpoint.next[0];
                }
            }

            return hitObjects;
        }

        /// <summary>
        /// Returns a list of all objects with AABBs overlapping the 
        /// passed in bounding box.  Does not add the AABB to the collider.
        /// </summary>
        /// <param name="aabb">AABB</param>
        /// <returns>List of overlapping AABBs</returns>
        public List<T> AABBCastAll(gxtAABB aabb)
        {
            List<T> hitObjects = new List<T>();

            Vector2 aabbMin = aabb.Min;
            Vector2 aabbMax = aabb.Max;

            gxtSAPEndpoint endpoint = listHead[0].next[0];


            while (endpoint != null)
            {
                if (endpoint.isMin)
                {
                    if (endpoint.value[0] > aabbMax.X)
                        break;
                    if (Overlap(endpoint.box, aabb))
                        if (!hitObjects.Contains(endpoint.box.obj))
                            hitObjects.Add(endpoint.box.obj);
                }
                else if (endpoint.value[0] < aabbMin.X)
                    break;
                endpoint = endpoint.next[0];
            }

            return hitObjects;
        }
        

        /// <summary>
        /// Returns readout of the structure
        /// Useful for debugging
        /// </summary>
        /// <returns></returns>
        public string DebugTrace()
        {
            StringBuilder builder = new StringBuilder();
            if (IsEmpty)
            {
                builder.Append("EMPTY LIST");
            }
            else
            {
                gxtSAPEndpoint node = listHead[0];
                int count = 0;
                while (node != null)
                {
                    builder.Append(node.ToString(count));
                    node = node.next[0];
                    count++;
                }
            }
            return builder.ToString();
        }

        public string DebugTracePairs()
        {
            StringBuilder builder = new StringBuilder();
            foreach (gxtBroadphaseCollisionPair<T> pair in pairManager)
            {
                builder.Append("\nA: " + pair.objA.GetHashCode() + "\nB: " + pair.objB.GetHashCode());
            }
            return builder.ToString();
        }

        /// <summary>
        /// For testing purposes only
        /// </summary>
        /// <param name="minIntervalColor"></param>
        /// <param name="maxIntervalColor"></param>
        /// <param name="yBaseLine"></param>
        /// <param name="xBaseLine"></param>
        public void DebugDraw(Color minIntervalColor, Color maxIntervalColor, float yBaseLine, float xBaseLine)
        {
            gxtSAPEndpoint node = listHead[0];
            int count = 0;
            while (node != null)
            {
                if (gxtMath.IsNaN(node.value[0]))
                    continue;
                Vector2 pos = new Vector2(node.value[0], node.value[1]);
                gxtDebugDrawer.Singleton.AddString(count.ToString(), pos, Color.White, 0.0f);
                gxtDebugDrawer.Singleton.AddLine(new Vector2(node.value[0], -400), new Vector2(node.value[0], yBaseLine), node.isMin ? minIntervalColor : maxIntervalColor, 0.0f);
                //gxtDebugDrawer.Singleton.AddLine(new Vector2(-400, node.value[1]), new Vector2(xBaseLine, node.value[1]), node.isMin ? minIntervalColor : maxIntervalColor, 0.0f);
                if (node.next[0] != null)
                {
                    Vector2 max = new Vector2(node.next[0].value[0], node.next[0].value[1]);
                    gxtDebugDrawer.Singleton.AddLine(pos, max, Color.Gray, 0.0f);
                }
                node = node.next[0];
                count++;
            }
        }

        /// <summary>
        /// Determines if the collider is empty
        /// </summary>
        public bool IsEmpty { get { return Count == 0; } }

        /// <summary>
        /// Gets count of total objects in the collider
        /// </summary>
        public int Count
        {
            get
            {
                // start at -2 because we always have two sentinel endpoints
                int count = -2;
                gxtSAPEndpoint node = listHead[0];
                while (node != null)
                {
                    count++;
                    node = node.next[0];
                }
                gxtDebug.Assert(count < 0, "Sentinels are not setup properly!");
                return count;
            }
        }
    }
}
