using System;
using Microsoft.Xna.Framework;

namespace GXT.Physics
{
    /// <summary>
    /// A delegate intended to carry all necessary information needed in the result of a collison 
    /// between two geoms.  This delegate is used for gxtGeom's OnCollision event.  Boolean return 
    /// allows you to cancel the response of the collision.
    /// </summary>
    /// <param name="geomA">Geom A</param>
    /// <param name="geomB">Geom B</param>
    /// <param name="collisionResult">Result</param>
    /// <returns>True if you want the physics solver to perform collision response, false if you handled things yourself or don't want it performed</returns>
    public delegate bool gxtGeomCollisionEventHandler(gxtGeom geomA, gxtGeom geomB, gxtCollisionResult collisionResult);

    /// <summary>
    /// A delegate intended to be invoked when two geoms first seperate from collision.  Resting contacts do 
    /// not count as seperation, the two geoms must be fully out of collision. 
    /// </summary>
    /// <param name="geomA">Geom A</param>
    /// <param name="geomB">Geom B</param>
    public delegate void gxtGeomSeperationEventHandler(gxtGeom geomA, gxtGeom geomB);

    /// <summary>
    /// A class that handles a physical/geometrical representation of an object in the
    /// physics simulation.  Can be tied to a gxtRigidBody for real time dynamics.  Register a handler 
    /// to the OnCollision delegate if you want the gxtGeom to act like a trigger.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: CLEAN UP CONSTRUCTORS, TEST CLONE FUNCTION
    // Once narrowphase collider is an object, just have a reference to the world
    // could then provide IsInWorld() functions and the like
    public class gxtGeom : IEquatable<gxtGeom>
    {
        #region Fields
        // collision vars
        private bool collisionEnabled;
        private bool collisionResponseEnabled;
        private gxtCollisionGroup collidesWith;
        private gxtCollisionGroup collisionGroups;

        // shapes
        private gxtPolygon localPolygon;
        private Vector2 localPositionOffset;
        private gxtPolygon worldPolygon;
        private gxtAABB localAABB;
        private gxtAABB worldAABB;
        private Vector2 position;
        private float rotation;

        // references/id
        private gxtRigidBody rigidBody;
        private gxtIBroadPhaseCollider<gxtGeom> broadphaseCollider;
        private object tag;
        private uint id;
        private gxtPhysicsMaterial material;

        /// <summary>
        /// Event which is fired when this geom collides with another.
        /// Is continually fired while they stay in contact.
        /// Be sure to check if it is null before 
        /// invoking it as not all geoms with have an event tied to it.
        /// </summary>
        public event gxtGeomCollisionEventHandler OnCollision;

        /// <summary>
        /// Event which is fired when this geom seperates with another.
        /// Is not continually fired when they stay seperated.
        /// </summary>
        public event gxtGeomSeperationEventHandler OnSeperation;
        #endregion

        #region Properties
        /// <summary>
        /// A flag enabling/disabling collision detection for this geom
        /// </summary>
        public bool CollisionEnabled { get { return collisionEnabled; } set { collisionEnabled = value; } }

        /// <summary>
        /// A flag determining if this geom should be modified by the default gxtPhysicsWorld solver
        /// OnCollision events will still be fired regardless of this value
        /// </summary>
        public bool CollisionResponseEnabled { get { return collisionResponseEnabled; } set { collisionResponseEnabled = value; } }

        /// <summary>
        /// Collision groups this geom will collide with
        /// </summary>
        public gxtCollisionGroup CollidesWithGroups { get { return collidesWith; } set { collidesWith = value; } }

        /// <summary>
        /// Collision groups this geom belongs to
        /// </summary>
        public gxtCollisionGroup CollisionGroups { get { return collisionGroups; } set { collisionGroups = value; } }
        
        /// <summary>
        /// Local space polygon for the geom.  This polygon's centroid must be the origin for transformations to
        /// work correctly.
        /// </summary>
        public gxtPolygon LocalPolygon { get { return localPolygon; } }

        /// <summary>
        /// Polygonal representation of the geom in world space. 
        /// </summary>
        public gxtPolygon Polygon { get { return worldPolygon; } set { SetupPolygon(value); } }

        /// <summary>
        /// Local space AABB for the geom.  Computed with the vertices of the local space polygon.
        /// </summary>
        public gxtAABB LocalAABB { get { return localAABB; } }

        /// <summary>
        /// World space AABB for the geom.  Computed with the vertices of the world space polygon.
        /// </summary>
        public gxtAABB AABB { get { return worldAABB; } }

        /// <summary>
        /// The position of the geom in world space.  A geom's position is equivalent to the centroid
        /// of the world space polygon, unless you used SetupFromLocalPolygon
        /// </summary>
        public Vector2 Position { get { return position; } }

        /// <summary>
        /// The rotation (in radians) of the geom in world space.  Rotation is relative to the given orientation 
        /// of the local space polygon.
        /// </summary>
        public float Rotation { get { return rotation; } }
        
        /// <summary>
        /// Persistent broadphase collider for the physics world.  Need this reference for internal updates
        /// </summary>
        public gxtIBroadPhaseCollider<gxtGeom> BrodphaseCollider { get { return broadphaseCollider; } set { broadphaseCollider = value; } }

        /// <summary>
        /// Rigid body linked to the geom.  The geom's Update() method is dependant on this rigid body's values to
        /// make changes to it's position and orientation.  A geom does not need to have a rigid body, and this may
        /// be optimal for static geometry.  This means that changes must be called manually and that you cannot assume the 
        /// rigid body has a value.
        /// </summary>
        public gxtRigidBody RigidBody { get { return rigidBody; } set { rigidBody = value; } }

        /// <summary>
        /// Application defined object related to the body (e.g. player actor).  May be null, use HasAttachedTag() to verify it has 
        /// a value.
        /// </summary>
        public object Tag { get { return tag; } set { tag = value; } }

        /// <summary>
        /// The unique identifier for the geom.  Used for fast equality comparisons.
        /// </summary>
        public uint Id { get { return id; } }

        /// <summary>
        /// A shared reference material attached to the geom (can be null)
        /// If null, DEFAULT_FRICTION and DEFAULT_RESTITUTION will be used
        /// </summary>
        public gxtPhysicsMaterial Material { get { return material; } set { material = value; } }
        #endregion Fields

        #region GeomId
        // counter for all geoms
        private static uint nextGeomId = 0;

        /// <summary>
        /// Returns a new valid identifier for the geom
        /// </summary>
        /// <returns>Uint id</returns>
        private static uint GetNextGeomId()
        {
            return nextGeomId++;
        }

        /// <summary>
        /// Resets geom id counter.  Use only after clearing all old geoms, or you
        /// will get invalid, non unique identifiers.
        /// </summary>
        public static void ResetGeomIds()
        {
            nextGeomId = 0;
        }
        #endregion GeomId

        #region Constructors/Clone
        /// <summary>
        /// Default constructor.  Assigns new and valid identifier.
        /// Make sure you setup a polygon after calling this default constructor
        /// </summary>
        public gxtGeom(bool initCollisionEnabled = true)
        {
            id = GetNextGeomId();

            collisionEnabled = initCollisionEnabled;
            collisionResponseEnabled = true;
            collidesWith = gxtCollisionGroup.ALL;
            collisionGroups = gxtCollisionGroup.ALL;

            rigidBody = null;
            tag = null;
            OnCollision = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="initCollisionEnabled"></param>
        public gxtGeom(gxtPolygon polygon, bool initCollisionEnabled = true)
        {
            id = GetNextGeomId();
            
            this.collisionEnabled = initCollisionEnabled;
            collidesWith = gxtCollisionGroup.ALL;
            collisionGroups = gxtCollisionGroup.ALL;

            rigidBody = null;
            tag = null;

            position = Vector2.Zero;
            rotation = 0.0f;

            OnCollision = null;
            collisionResponseEnabled = true;

            SetupPolygon(polygon);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collisionEnabled"></param>
        /// <param name="polygon"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public gxtGeom(gxtPolygon polygon, Vector2 position, float rotation, bool initCollisionEnabled = true)
        {
            id = GetNextGeomId();

            this.collisionEnabled = initCollisionEnabled;
            collidesWith = gxtCollisionGroup.ALL;
            collisionGroups = gxtCollisionGroup.ALL;

            rigidBody = null;
            tag = null;

            this.position = position;
            this.rotation = rotation;

            OnCollision = null;
            collisionResponseEnabled = true;

            SetupPolygon(polygon);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collisionEnabled"></param>
        /// <param name="polygon"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="body"></param>
        /// <param name="tag"></param>
        public gxtGeom(gxtPolygon polygon, Vector2 position, float rotation, gxtRigidBody body, object tag, bool initCollisionEnabled = true)
        {
            id = GetNextGeomId();

            this.collisionEnabled = initCollisionEnabled;
            collidesWith = gxtCollisionGroup.ALL;
            collisionGroups = gxtCollisionGroup.ALL;

            this.rigidBody = body;
            this.tag = tag;

            this.position = position;
            this.rotation = rotation;

            SetupPolygon(polygon);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="onCollisionHandler"></param>
        /// <param name="tag"></param>
        /// <param name="collisionGroups"></param>
        /// <returns></returns>
        public static gxtGeom CreateTrigger(gxtPolygon polygon, Vector2 position, float rotation, gxtGeomCollisionEventHandler onCollisionHandler, object tag = null, gxtCollisionGroup collisionGroups = gxtCollisionGroup.ALL)
        {
            gxtGeom triggerGeom = new gxtGeom(polygon, position, rotation, true);
            triggerGeom.CollidesWithGroups = collisionGroups;
            triggerGeom.CollisionGroups = collisionGroups;
            triggerGeom.CollisionResponseEnabled = false;
            triggerGeom.Tag = tag;
            return triggerGeom;
        }

        /// <summary>
        /// Performs a deep copy of passed in geom
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        // todo: copy rigid body too?  copy tag?
        public static gxtGeom Copy(gxtGeom geom)
        {
            // sets id
            gxtGeom clone = new gxtGeom();
            // copy non-reference types
            clone.position = geom.position;
            clone.rotation = geom.rotation;
            clone.collisionEnabled = geom.collisionEnabled;
            clone.collidesWith = geom.collidesWith;
            clone.collisionGroups = geom.collisionGroups;
            clone.localAABB = geom.localAABB;
            clone.worldAABB = geom.worldAABB;
            // copy reference types
            gxtGeomCollisionEventHandler collisionHandlerCopy = geom.OnCollision;
            gxtGeomSeperationEventHandler seperationHandlerCopy = geom.OnSeperation;
            clone.localPolygon = gxtPolygon.Copy(geom.localPolygon);
            clone.worldPolygon = gxtPolygon.Copy(geom.worldPolygon);
            object tagCopy = geom.tag;
            gxtRigidBody bodyCopy = geom.rigidBody;
            // set copies
            clone.OnCollision = collisionHandlerCopy;
            clone.OnSeperation = seperationHandlerCopy;
            clone.rigidBody = bodyCopy;
            clone.tag = tagCopy;
            return clone;
        }
        #endregion Constructors/Clone

        #region Checks
        /// <summary>
        /// Determines if the geom has an associated Rigid Body
        /// </summary>
        /// <returns>Attached Body?</returns>
        public bool HasAttachedBody()
        {
            return rigidBody != null;
        }

        /// <summary>
        /// Determines if the geom has an associated Tag
        /// </summary>
        /// <returns>Attached Tag?</returns>
        public bool HasAttachedTag()
        {
            return tag != null;
        }

        /// <summary>
        /// Determines if the geom is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            // is valid checks not working yet
            //if (worldPolygon.IsValid() || localPolygon.IsValid())
                    //return false;

            if (localAABB == null || worldAABB == null)
                return false;

            if (localPolygon == null || worldPolygon == null)
                return false;

            if (!localAABB.IsValid() || !worldAABB.IsValid())
                return false;

            return true;
        }
        #endregion Checks

        #region Updates
        /// <summary>
        /// Will setup a new world space polygon
        /// Recomputes new local polygon, local AABB, and world AABB for you
        /// </summary>
        /// <param name="polygon">New World Polygon</param>
        private void SetupPolygon(gxtPolygon polygon)
        {
            if (broadphaseCollider != null)
                broadphaseCollider.RemoveObject(this);

            worldPolygon = polygon;

            // calc local poly and AABB using centroid translation
            Vector2 centroid = polygon.GetCentroid();
            Vector2[] localVertices = new Vector2[polygon.NumVertices];
            for (int i = 0; i < localVertices.Length; i++)
            {
                localVertices[i] = polygon.v[i] - centroid;
            }
            localPositionOffset = Vector2.Zero;
            localPolygon = new gxtPolygon(localVertices);
            localAABB = gxtGeometry.ComputeAABB(localVertices);

            // applies existing position and rotation characteristics to the 
            // world polygon
            Matrix rotationMatrix;
            Matrix.CreateRotationZ(rotation, out rotationMatrix);
            for (int i = 0; i < polygon.NumVertices; i++)
            {
                worldPolygon.v[i] = Vector2.Transform(localPolygon.v[i], rotationMatrix) + position;
            }

            worldAABB = gxtAABB.Update(position, rotation, localAABB);
            
            if (broadphaseCollider != null)
                broadphaseCollider.AddObject(this, ref worldAABB);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        public void SetupFromLocalPolygon(gxtPolygon polygon)
        {
            SetupFromLocalPolygon(polygon, Vector2.Zero, 0.0f);
        }

        /// <summary>
        /// Will this have issues with algorithms that assume position == centroid?
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="localPositionOffset"></param>
        /// <param name="localRotationOffset"></param>
        public void SetupFromLocalPolygon(gxtPolygon polygon, Vector2 localPositionOffset, float localRotationOffset)
        {
            if (broadphaseCollider != null)
                broadphaseCollider.RemoveObject(this);

            Vector2[] verts = new Vector2[polygon.NumVertices];
            Matrix localRotMat = Matrix.CreateRotationZ(localRotationOffset);
            for (int i = 0; i < polygon.NumVertices; ++i)
            {
                verts[i] = Vector2.Transform(polygon.v[i], localRotMat) + localPositionOffset;
            }

            localPolygon = new gxtPolygon(verts);
            localAABB = gxtGeometry.ComputeAABB(verts);
            this.localPositionOffset = localPositionOffset;

            worldPolygon = new gxtPolygon(new Vector2[verts.Length]);

            //Matrix worldRotMat = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0.0f);
            Matrix worldRotMat = Matrix.CreateRotationZ(rotation);
            for (int i = 0; i < polygon.NumVertices; i++)
            {
                worldPolygon.v[i] = Vector2.Transform(localPolygon.v[i], worldRotMat) + position;
            }

            worldAABB = gxtAABB.Update(position, rotation, localAABB);

            if (broadphaseCollider != null)
                broadphaseCollider.AddObject(this, ref worldAABB);
        }

        /// <summary>
        /// Updates the geom's rotation and position to 
        /// match that of the rigid body (if one is attched)
        /// </summary>
        public void Update()
        {
            gxtDebug.SlowAssert(IsValid(), "Geom was not setup properly!");
            if (HasAttachedBody())
            {
                SetPosition(rigidBody.Position);
                SetRotation(rigidBody.Rotation);
            }
        }

        /// <summary>
        /// Translates the geom from it's current position
        /// in world coordinates by the given translation vector
        /// </summary>
        /// <param name="t">Translation vector</param>
        public void Translate(Vector2 t)
        {
            if (t == Vector2.Zero) return;

            position += t;
            worldPolygon.Translate(t);
            worldAABB = new gxtAABB(worldAABB.Position + t, worldAABB.Extents);
            
            if (broadphaseCollider != null)
                broadphaseCollider.UpdateObject(this, ref worldAABB);
        }

        /// <summary>
        /// Sets the position of the geom
        /// The centroid of the world polygon 
        /// will equal the passed in value
        /// </summary>
        /// <param name="pos">Position</param>
        public void SetPosition(Vector2 pos)
        {
            if (position == pos) return;

            Vector2 prevPos = position;
            Vector2 trans = pos - prevPos;
            Translate(trans);
        }

        /// <summary>
        /// Sets the rotation of the geom
        /// The rotation of the world polygon will 
        /// equal the passed in value offset from the local 
        /// polygon's orientation
        /// </summary>
        /// <param name="rot">Rotation</param>
        public void SetRotation(float rot)
        {
            if (rotation == rot) return;

            rotation = rot;
            Matrix rotMat;
            Matrix.CreateRotationZ(rotation, out rotMat);
            for (int i = 0; i < worldPolygon.NumVertices; i++)
            {
                worldPolygon.v[i] = Vector2.Transform(localPolygon.v[i], rotMat) + position;
            }

            worldAABB = gxtAABB.Update(position, rotation, localAABB);
            
            if (broadphaseCollider != null)
                broadphaseCollider.UpdateObject(this, ref worldAABB);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector2 GetWorldCentroid()
        {
            if (localPositionOffset == Vector2.Zero)
                return position;
            float cos = gxtMath.Cos(rotation);
            float sin = gxtMath.Sin(rotation);
            float tx = cos * localPositionOffset.X - sin * localPositionOffset.Y;
            float ty = sin * localPositionOffset.X + cos * localPositionOffset.Y;
            return position + new Vector2(tx, ty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector2 GetLocalCentroid()
        {
            return localPositionOffset;
        }

        /// <summary>
        /// Gets the geom friction
        /// </summary>
        /// <returns></returns>
        public float GetFriction()
        {
            if (material == null)
                return gxtPhysicsMaterial.DEFAULT_FRICTION;
            else
                return material.Friction;
        }

        /// <summary>
        /// Gets the geom restitution
        /// </summary>
        /// <returns></returns>
        public float GetRestitution()
        {
            if (material == null)
                return gxtPhysicsMaterial.DEFAULT_RESTITUTION;
            else
                return material.Restitution;
        }
        #endregion Updates

        #region Collision
        // should these really be static?

        /// <summary>
        /// Boolean intersection test
        /// Considers collision enabled flag and collision groups before 
        /// running AABB and convexity based algorithms
        /// </summary>
        /// <param name="geomA">Geom A</param>
        /// <param name="geomB">Geom B</param>
        /// <returns></returns>
        public static bool Intersects(gxtGeom geomA, gxtGeom geomB)
        {
            if (!geomA.CollisionEnabled || !geomB.CollisionEnabled)
                return false;
            if ((geomA.collidesWith & geomB.collisionGroups) == gxtCollisionGroup.NONE || (geomB.collidesWith & geomA.collisionGroups) == gxtCollisionGroup.NONE)
                return false;
            if (!geomA.AABB.Intersects(geomB.AABB))
                return false;
            return (gxtGJKCollider.Intersects(ref geomA.worldPolygon, geomA.position, ref geomB.worldPolygon, geomB.position));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomA"></param>
        /// <param name="geomB"></param>
        /// <returns></returns>
        public static gxtCollisionResult Collide(gxtGeom geomA, gxtGeom geomB)
        {
            gxtCollisionResult collisionResult = new gxtCollisionResult();
            if (!geomA.CollisionEnabled || !geomB.CollisionEnabled)
                return collisionResult;
            if ((geomA.collidesWith & geomB.collisionGroups) == gxtCollisionGroup.NONE || (geomB.collidesWith & geomA.collisionGroups) == gxtCollisionGroup.NONE)
                return collisionResult;
            if (!geomA.AABB.Intersects(geomB.AABB))
                return collisionResult;

            return gxtGJKCollider.Collide(geomA.worldPolygon, geomA.position, geomB.worldPolygon, geomB.position);
        }

        public static bool PointCast(gxtGeom geom, Vector2 pt, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL)
        {
            if (!geom.collisionEnabled)
                return false;
            if ((geom.collidesWith & collisionGroup) == gxtCollisionGroup.NONE)
                return false;
            return gxtGJKCollider.Contains(geom.worldPolygon, pt);
        }

        public static bool RayCast(gxtGeom geom, gxtRay ray, out gxtRayHit rayHit, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL, float tmax = float.MaxValue)
        {
            rayHit = new gxtRayHit();
            if (!geom.collisionEnabled)
                return false;
            if ((geom.collidesWith & collisionGroup) == gxtCollisionGroup.NONE)
                return false;
            
            float t;
            Vector2 pt, normal;
            if (gxtGJKCollider.RayCast(ray, geom.worldPolygon, tmax, out t, out pt, out normal))
            {
                rayHit.Intersection = true;
                rayHit.Distance = t;
                rayHit.Point = pt;
                rayHit.Normal = normal;
                rayHit.Geom = geom;
            }
            return rayHit.Intersection;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomA"></param>
        /// <param name="geomB"></param>
        /// <param name="collisionResult"></param>
        /// <returns></returns>
        public static bool FireOnCollisionEvents(gxtGeom geomA, gxtGeom geomB, gxtCollisionResult collisionResult)
        {
            bool performResponse = true;

            if (geomA.OnCollision != null)
                if (!geomA.OnCollision(geomA, geomB, collisionResult))
                    performResponse = false;
            if (geomB.OnCollision != null)
            {
                // we need to reverse normal and swap the contact points
                collisionResult.Normal = -collisionResult.Normal;
                Vector2 tmp = collisionResult.ContactPointA;
                collisionResult.ContactPointA = collisionResult.ContactPointB;
                collisionResult.ContactPointB = tmp;

                if (!geomB.OnCollision(geomB, geomA, collisionResult))
                    performResponse = false;
            }
            
            return performResponse;
        }

        public static void FireOnSeperationEvents(gxtGeom geomA, gxtGeom geomB)
        {
            // should the event always pass the attached geom as the first parameter??
            if (geomA.OnSeperation != null)
                geomA.OnSeperation(geomA, geomB);
            if (geomB.OnSeperation != null)
                geomB.OnSeperation(geomB, geomA);
        }

        #region CollisionGroupMethods
        /// <summary>
        /// Adds a collision group or a collection of collision groups for the geom to belong to
        /// </summary>
        /// <param name="cgroup">CollisionGroup</param>
        public void AddCollisionGroup(params gxtCollisionGroup[] cgroup)
        {
            for (int i = 0; i < cgroup.Length; i++)
            {
                CollisionGroups = (CollisionGroups | cgroup[i]);
            }
        }
        /// <summary>
        /// Removes a collision group or a collection of collision groups
        /// </summary>
        /// <param name="cgroup"></param>
        public void RemoveCollisionGroup(params gxtCollisionGroup[] cgroup)
        {
            for (int i = 0; i < cgroup.Length; i++)
            {
                CollisionGroups = (CollisionGroups & ~cgroup[i]);
            }
        }

        /// <summary>
        /// Adds a group that this geom will collide with
        /// </summary>
        /// <param name="cgroup"></param>
        public void AddCollidesWithGroup(params gxtCollisionGroup[] cgroup)
        {
            for (int i = 0; i < cgroup.Length; i++)
            {
                CollidesWithGroups = (CollidesWithGroups | cgroup[i]);
            }
        }

        /// <summary>
        /// Removes a group that this geom will collide with
        /// </summary>
        /// <param name="cgroup"></param>
        public void RemoveCollidesWithGroup(params gxtCollisionGroup[] cgroup)
        {
            for (int i = 0; i < cgroup.Length; i++)
            {
                CollidesWithGroups = (CollidesWithGroups & ~cgroup[i]);
            }
        }
        #endregion CollisionGroupManagement

        #region EqualityOverrides
        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is gxtGeom)
                return Equals((gxtGeom)obj);
            else
                return false;
        }

        /// <summary>
        /// Fast id based equality comparison
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(gxtGeom other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Equality operator override
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(gxtGeom a, gxtGeom b)
        {
            return a.Id == b.Id;
        }

        /// <summary>
        /// Inequality operator override
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(gxtGeom a, gxtGeom b)
        {
            return a.Id != b.Id;
        }

        /// <summary>
        /// Required for equality/inequality overrides
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        #endregion EqualityOverrides
    }
}
