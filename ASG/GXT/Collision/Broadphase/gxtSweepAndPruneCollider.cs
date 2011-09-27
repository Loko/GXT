using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using GXT.Rendering;

namespace GXT
{
    /// <summary>
    /// A broadphase data structure based on a linked list implementation
    /// of the "Sweep and Sort" algorithm.  Objects which change their bounds 
    /// must explicitly call the update method of the collider.  This is much more 
    /// efficient than a brute force recalculation of the list and exploits persistance.
    /// 
    /// Current implementation sorts on both axis, does not keep a tail pointer, and most importantly
    /// does not yet have a working pair manager.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    /// <typeparam name="T">Type of the associated game object</typeparam>
    public class gxtSweepAndPruneCollider<T>
    {
        private enum gxtSAPAxis
        {
            X_AXIS = 0,
            Y_AXIS = 1
        };

        /// <summary>
        /// Represents an endpoint on one axis of the object's AABB
        /// </summary>
        /// <typeparam name="T">Game Object Type</typeparam>
        private class gxtSAPEndpoint
        {
            public gxtSAPBox box;
            public gxtSAPEndpoint prev;
            public gxtSAPEndpoint next;
            public float value;
            public bool isMin;
            public float otherValue;

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
        }

        /// <summary>
        /// Front of our linked list of endpoints
        /// </summary>
        private gxtSAPEndpoint head;
        // list for collision pairs
        // int count
        // iterator?

        /// <summary>
        /// Map of pairs
        /// </summary>
        private Dictionary<gxtBroadphaseCollisionPair<T>, bool> pairManager;

        /// <summary>
        /// Empty?
        /// </summary>
        public bool IsEmpty { get { return head == null; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtSweepAndPruneCollider()
        {

        }

        /// <summary>
        /// Initializes internals
        /// </summary>
        public void Initialize()
        {
            pairManager = new Dictionary<gxtBroadphaseCollisionPair<T>, bool>();
        }

        private bool Overlap(float minA1, float minA2, float maxA1, float maxA2, float minB1, float minB2, float maxB1, float maxB2)
        {
            if (maxA1 < minB1 || minA1 > maxB1) return false;
            if (maxA2 < minB2 || minA2 > maxB2) return false;
            return true;
        }

        private bool Overlap(gxtSAPBox boxA, gxtSAPBox boxB)
        {
            if (boxA.max.value < boxB.min.value || boxA.min.value > boxB.max.value) return false;
            if (boxA.max.otherValue < boxB.min.otherValue || boxA.min.otherValue > boxB.max.otherValue) return false;
            return true;
        }

        /// <summary>
        /// Adds an object to the collider
        /// </summary>
        /// <param name="obj">Game Object</param>
        /// <param name="aabb">Object Bounding Box</param>
        public void AddObject(T obj, ref gxtAABB aabb)
        {
            // zero extent allowed, but negative extents are not
            gxtDebug.Assert(!aabb.IsEmpty(), "Cannot add negative extent AABB to collider");

            // construct box, associate actual object
            gxtSAPBox box = new gxtSAPBox();
            box.obj = obj;
            
            // calc intervals on sort axis
            float min, max, otherMin, otherMax;
            GetMinAndMaxValues(0, ref aabb, out min, out max, out otherMin, out otherMax);

            // create endpoints with correct values and isMin flag
            gxtSAPEndpoint minEndpoint = new gxtSAPEndpoint();
            minEndpoint.value = min;
            minEndpoint.isMin = true;
            minEndpoint.box = box;
            minEndpoint.otherValue = otherMin;

            gxtSAPEndpoint maxEndpoint = new gxtSAPEndpoint();
            maxEndpoint.value = max;
            maxEndpoint.isMin = false;
            maxEndpoint.box = box;
            maxEndpoint.otherValue = otherMax;

            box.min = minEndpoint;
            box.max = maxEndpoint;

            if (head == null)
            {
                // if empty, add to the beginning of the list
                head = minEndpoint;
                head.next = maxEndpoint;
                maxEndpoint.prev = minEndpoint;
                return;
            }
            else if (min < head.value)
            {
                // if less than the head, insert in front
                minEndpoint.next = head; ;
                head.prev = minEndpoint;
                head = minEndpoint;
            }
            else
            {
                // otherwise walk the list
                gxtSAPEndpoint tmpMinNode = head;
                while (tmpMinNode.next != null && minEndpoint.value > tmpMinNode.value)
                {
                    // check to see if obj is already in the collection?
                    // check for collision pairs?
                    tmpMinNode = tmpMinNode.next;
                }

                // insert min endpoint node after tmpMinNode
                minEndpoint.prev = tmpMinNode;
                minEndpoint.next = tmpMinNode.next;
                if (tmpMinNode.next != null)
                    tmpMinNode.next.prev = minEndpoint;
                tmpMinNode.next = minEndpoint;

                // check for overlaps?
            }

            // insert max endpoint node after tmpMaxNode
            // start search at min
            gxtSAPEndpoint tmpMaxNode = minEndpoint;
            while (tmpMaxNode.next != null && maxEndpoint.value > tmpMaxNode.value)
            {
                // check to see if obj already exists in the collection?
                tmpMaxNode = tmpMaxNode.next;
            }

            // insert max endpoint after tmpMaxNode
            maxEndpoint.prev = tmpMaxNode;
            maxEndpoint.next = tmpMaxNode.next;
            if (tmpMaxNode.next != null)
                tmpMaxNode.next.prev = maxEndpoint;
            tmpMaxNode.next = maxEndpoint;

            // CHECK FOR OVERLAPS
            
            gxtSAPEndpoint node = head;
            while (node != null)
            {
                if (node.isMin)
                {
                    if (node.value > box.max.value)
                        break;
                    if (Overlap(box.min.value, box.min.otherValue, box.max.value, box.max.otherValue, node.box.min.value,
                        node.box.min.otherValue, node.box.max.value, node.box.max.otherValue))
                        AddPair(obj, node.box.obj);
                }
                else if (node.value < box.min.value)
                    break;
                node = node.next;
            }
            
            // walk rest of the list to find a repeat occurence??
        }

        /// <summary>
        /// Updates an object in the collider.  This only needs to be called when the 
        /// bounding box for the object changes.
        /// </summary>
        /// <param name="obj">Game Object</param>
        /// <param name="aabb">Object Bounding Box</param>
        public void UpdateObject(T obj, ref gxtAABB aabb)
        {
            // zero extent allowed, but negative extents are not
            gxtDebug.Assert(!aabb.IsEmpty(), "Cannot add negative extent AABB to collider");

            // find target node
            gxtSAPEndpoint node = head;
            while (node != null && !node.box.obj.Equals(obj))
            {
                node = node.next;
            }

            // if not found, log a message and return
            if (node == null)
            {
                gxtLog.WriteLineV(VerbosityLevel.WARNING, "The object: {0} was not found in the collider and cannot be updated", obj.ToString());
                return;
            }

            // find/set new values
            float min, max, otherMin, otherMax;
            GetMinAndMaxValues(0, ref aabb, out min, out max, out otherMin, out otherMax);

            gxtSAPEndpoint minEndpoint = node;
            gxtSAPEndpoint maxEndpoint = node.box.max;
            minEndpoint.value = min;
            minEndpoint.otherValue = otherMin;
            maxEndpoint.value = max;
            maxEndpoint.otherValue = otherMax;
            gxtSAPBox box = minEndpoint.box;

            gxtSAPEndpoint t;

            for (t = minEndpoint.prev; t != null && minEndpoint.value < t.value; t = t.prev)
            {
                if (!t.isMin)
                {
                    if (Overlap(box, t.box))
                        AddPair(obj, t.box.obj);
                }
            }

            if (t != minEndpoint.prev)
                MoveEndpoint(minEndpoint, t);

            bool insertBefore = true;
            for (t = maxEndpoint.next; t != null && maxEndpoint.value > t.value; t = t.next)
            {
                if (t.isMin)
                {
                    if (Overlap(box, t.box))
                        AddPair(obj, t.box.obj);
                }
                if (t.next == null)
                {
                    insertBefore = false;
                    break;
                }
            }

            if (t != maxEndpoint.next)
            {
                if (insertBefore)
                    MoveEndpoint(maxEndpoint, t.prev);
                else
                    MoveEndpoint(maxEndpoint, t);
            }


            for (t = minEndpoint.next; t != null && minEndpoint.value > t.value; t = t.next)
            {
                if (!t.isMin)
                {
                    if (!Overlap(box, t.box))
                        RemovePair(obj, t.box.obj);
                }
            }

            if (t != minEndpoint.next)
                MoveEndpoint(minEndpoint, t.prev);

            for (t = maxEndpoint.prev; t != null && maxEndpoint.value < t.value; t = t.prev)
            {
                if (t.isMin)
                {
                    if (!Overlap(box, t.box))
                        RemovePair(obj, t.box.obj);
                }
            }

            if (t != maxEndpoint.prev)
                MoveEndpoint(maxEndpoint, t);
        }

        /// <summary>
        /// Removes the object from the collider
        /// </summary>
        /// <param name="obj">Game Object</param>
        /// <returns>If removal was successful</returns>
        public bool RemoveObject(T obj)
        {
            // first walk the list to find the associated object 
            gxtSAPEndpoint minTarget = head;
            while (minTarget != null && !minTarget.box.obj.Equals(obj))
                minTarget = minTarget.next;
            
            // early return if not found
            if (minTarget == null)
            {
                gxtLog.WriteLineV(VerbosityLevel.WARNING, "The object: {0} was not found in the collider and cannot be removed", obj.ToString());
                return false;
            }

            //remove the endpoints
            RemoveEndpoint(minTarget);
            RemoveEndpoint(minTarget.box.max);
            // remove associated pairs
            RemoveAssociatedPairs(obj);
            return true;
        }

        /// <summary>
        /// Removes an endpoint from the list, properly handling each special case
        /// </summary>
        /// <param name="endpoint"></param>
        private void RemoveEndpoint(gxtSAPEndpoint endpoint)
        {
            gxtDebug.Assert(endpoint != null);

            if (endpoint.prev == null)
                head = head.next;
            else
                endpoint.prev.next = endpoint.next;
            if (endpoint.next != null)
                endpoint.next.prev = endpoint.prev;
        }

        // AddAfterEndpoint()
        private void MoveEndpoint(gxtSAPEndpoint endpoint, gxtSAPEndpoint destination)
        {
            gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, destination == null);
            if (endpoint.prev == null)
                head = head.next;
            else
                endpoint.prev.next = endpoint.next;
            if (endpoint.next != null)
                endpoint.next.prev = endpoint.prev;

            endpoint.prev = destination;
            if (destination != null)
            {
                endpoint.next = destination.next;
                if (destination.next != null)
                {

                    destination.next.prev = endpoint;
                }
                destination.next = endpoint;
            }
            else
            {
                head.prev = endpoint;
                endpoint.next = head;
                head = endpoint;
                /*if (head.next != null)
                {
                    head.next.prev = head;
                }*/
            }
            
        }

        /// <summary>
        /// Gets the min and max values of the given AABB on the 
        /// designated sorting axis
        /// </summary>
        /// <param name="axis">Sorting Axis</param>
        /// <param name="aabb">AABB</param>
        /// <param name="min">Min Value</param>
        /// <param name="max">Max Value</param>
        private void GetMinAndMaxValues(gxtSAPAxis axis, ref gxtAABB aabb, out float min, out float max)
        {
            if (axis == gxtSAPAxis.X_AXIS)
            {
                min = aabb.Position.X - aabb.Extents.X;
                max = aabb.Position.X + aabb.Extents.X;
            }
            else
            {
                min = aabb.Position.Y - aabb.Extents.Y;
                max = aabb.Position.Y + aabb.Extents.Y;
            }
        }

        private void GetMinAndMaxValues(gxtSAPAxis axis, ref gxtAABB aabb, out float min, out float max, out float otherMin, out float otherMax)
        {
            if (axis == gxtSAPAxis.X_AXIS)
            {
                min = aabb.Position.X - aabb.Extents.X;
                max = aabb.Position.X + aabb.Extents.X;

                otherMin = aabb.Position.Y - aabb.Extents.Y;
                otherMax = aabb.Position.Y + aabb.Extents.Y;
            }
            else
            {
                min = aabb.Position.Y - aabb.Extents.Y;
                max = aabb.Position.Y + aabb.Extents.Y;

                otherMin = aabb.Position.Y - aabb.Extents.Y;
                otherMax = aabb.Position.Y + aabb.Extents.Y;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objB"></param>
        private void AddPair(T objA, T objB)
        {
            if (objA.Equals(objB))
                return;

            gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "adding pair");

            gxtBroadphaseCollisionPair<T> pair = new gxtBroadphaseCollisionPair<T>(objA, objB);

            if (pairManager.ContainsKey(pair))
                return;

            pairManager.Add(pair, true);
            // add if a new pair, otherwise return an existing pair
            // initially the pointers are null
            // if (pair.objA == null)
            //{
                    // report nothing, persistent pair
            //}
            // else
            // {
                    // update pointers
                    // set in array
                    // store pair indices in mKeys
                    // mark as new
            // }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objB"></param>
        private void RemovePair(T objA, T objB)
        {
            gxtBroadphaseCollisionPair<T> pair = new gxtBroadphaseCollisionPair<T>(objA, objB);
            
            pairManager.Remove(pair);
        }

        private void RemoveAssociatedPairs(T obj)
        {
            foreach (gxtBroadphaseCollisionPair<T> pair in pairManager.Keys)
            {
                if (pair.objA.Equals(obj) || pair.objB.Equals(obj))
                    pairManager.Remove(pair);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<gxtBroadphaseCollisionPair<T>> GetCollisionPairs()
        {
            return pairManager.Keys.GetEnumerator();
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
                gxtSAPEndpoint node = head;
                int count = 0;
                while (node != null)
                {
                    builder.Append(node.ToString(count));
                    node = node.next;
                    count++;
                }
            }
            return builder.ToString();
        }

        public string PairDebugTrace()
        {
            StringBuilder builder = new StringBuilder();
            foreach (gxtBroadphaseCollisionPair<T> pair in pairManager.Keys)
            {
                builder.Append("\nA: " + pair.objA.GetHashCode() + "\nB: " + pair.objB.GetHashCode());
            }
            return builder.ToString();
        }

        public void DebugDraw(Color minIntervalColor, Color maxIntervalColor, float yBaseLine, float xBaseLine)
        {
            gxtSAPEndpoint node = head;
            int count = 0;
            while (node != null)
            {
                Vector2 pos = new Vector2(node.value, node.otherValue);
                gxtDebugDrawer.Singleton.AddString(count.ToString(), pos, Color.White, 0.0f);
                //gxtDebugDrawer.Singleton.AddLine(new Vector2(node.value, -400), new Vector2(node.value, yBaseLine), node.isMin ? minIntervalColor : maxIntervalColor, 0.0f);
                //gxtDebugDrawer.Singleton.AddLine(new Vector2(-400, node.otherValue), new Vector2(xBaseLine, node.otherValue), node.isMin ? minIntervalColor : maxIntervalColor, 0.0f);
                if (node.next != null)
                {
                    //Vector2 min = new Vector2(node.value, node.otherValue);
                    Vector2 max = new Vector2(node.next.value, node.next.otherValue);
                    gxtDebugDrawer.Singleton.AddLine(pos, max, Color.Gray, 0.0f);
                }
                node = node.next;
                count++;
            }
        }
        /*
        public List<T> GetCollisionPairs()
        {
            List<T> pairs = new List<T>();
            return pairs;
        }
        */
    }
}
