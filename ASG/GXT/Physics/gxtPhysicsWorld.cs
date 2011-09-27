using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Physics
{
    /// <summary>
    /// Collision Filters that use bits for fast comparisons
    /// </summary>
    public enum gxtCollisionGroup
    {
        NONE = 0,
        GROUP1 = 1,
        GROUP2 = 2,
        GROUP3 = 4,
        GROUP4 = 8,
        GROUP5 = 16,
        GROUP6 = 32,
        GROUP7 = 64,
        GROUP8 = 128,
        GROUP9 = 256,
        GROUP10 = 512,
        GROUP11 = 1024,
        GROUP12 = 2048,
        GROUP13 = 4096,
        GROUP14 = 8192,
        GROUP15 = 16384,
        GROUP16 = 32768,
        ALL = int.MaxValue
    };

    /// <summary>
    /// A physical world which manages the interactions of dynamic rigid bodies and convex shapes.
    /// Provides gravity, data structures for geometry, rigid bodies, and aliased collision groups.  
    /// Runs efficient broadphase and narrow phase object tests.  Supports ray, point, and other shape 
    /// casting. An efficient and robust solver is also provided to resolve collisions between rigid bodies.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: FIX THE COLLISION SOLVER, IMPLEMENT JOINTS
    public class gxtPhysicsWorld : gxtIUpdate
    {
        #region Fields
        public const float PHYSICS_SCALE = 30.0f;
        public const float ONE_OVER_PHYSICS_SCALE = 1.0f / 30.0f;
        public static readonly Vector2 DEFAULT_GRAVITY = new Vector2(0.0f, 9.8f);

        private bool enabled;
        private Vector2 gravity;
        private bool gravityEnabled;

        // geom collections
        private List<gxtGeom> geomList;
        private List<gxtGeom> geom_add_list;
        private List<gxtGeom> geom_remove_list;
        // body collections
        private List<gxtRigidBody> bodyList;
        private List<gxtRigidBody> body_add_list;
        private List<gxtRigidBody> body_remove_list;

        // String map for aliased collision groups
        private Dictionary<string, gxtCollisionGroup> collisionGroupsMap;
        // The broadphase collider used to determine collision pairs and speed up casts/tests
        private gxtSortAndSweepCollider<gxtGeom> broadphaseCollider;

        /// <summary>
        /// Determines if dynamics and collisions will be processed
        /// Bodies and Geoms can still be added even if the PhysicsWorld is 
        /// disabled, but they will not be updated
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Force of gravity in the world.  Will be applied to every dynamic rigid body which has its
        /// IgnoreGravity flag set to false
        /// </summary>
        public Vector2 Gravity { get { return gravity; } set { gravity = value; } }

        /// <summary>
        /// Flag which enables/disables the application of gravity on all bodies in the world
        /// </summary>
        public bool GravityEnabled { get { return gravityEnabled; } set { gravityEnabled = value; } }

        /// <summary>
        /// Geom broadphase collision pairs
        /// </summary>
        private gxtBroadphaseCollisionPair<gxtGeom>[] testPairs;
        private Dictionary<int, gxtBroadphaseCollisionPair<gxtGeom>> broadphasePairs;
        private Dictionary<gxtBroadphaseCollisionPair<gxtGeom>, gxtContactPair> narrowphaseContacts;
        #endregion Fields

        #region Constructor/Init
        /// <summary>
        /// Constructs the physics world
        /// </summary>
        public gxtPhysicsWorld() { }

        /// <summary>
        /// Determines if the world has been initialized
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return (geomList != null && geom_remove_list != null && geom_add_list != null && bodyList != null && body_remove_list != null &&
                body_add_list != null && collisionGroupsMap != null && broadphaseCollider != null);
        }

        /// <summary>
        /// Initializes physics world internals
        /// </summary>
        public void Initialize()
        {
            Initialize(true, DEFAULT_GRAVITY);
        }

        /// <summary>
        /// Initializes physics world internals
        /// </summary>
        /// <param name="initEnabled">Enabled Flag</param>
        /// <param name="worldGravity">World Gravity</param>
        public void Initialize(bool initEnabled, Vector2 worldGravity)
        {
            gxtDebug.Assert(!IsInitialized(), "Physics World Has Already Been Initialized");

            enabled = initEnabled;
            gravity = worldGravity;
            gravityEnabled = true;

            geomList = new List<gxtGeom>();
            geom_remove_list = new List<gxtGeom>();
            geom_add_list = new List<gxtGeom>();

            bodyList = new List<gxtRigidBody>();
            body_remove_list = new List<gxtRigidBody>();
            body_add_list = new List<gxtRigidBody>();

            collisionGroupsMap = new Dictionary<string, gxtCollisionGroup>();

            broadphaseCollider = new gxtSortAndSweepCollider<gxtGeom>();
            broadphaseCollider.Initialize();

            broadphasePairs = new Dictionary<int, gxtBroadphaseCollisionPair<gxtGeom>>();
            narrowphaseContacts = new Dictionary<gxtBroadphaseCollisionPair<gxtGeom>, gxtContactPair>();
            // narrow phase init?

            //testPairs = new List<gxtGeomTestPair>();
            //collider = new gxtSweepAndPruneArrayCollider();
            //collider.Initialize();
        }
        #endregion Constructor/Init

        #region Add/Remove
        public void AddGeom(gxtGeom geom)
        {
            gxtDebug.SlowAssert(!geom_add_list.Contains(geom));
            
            if (!geom_add_list.Contains(geom))
                geom_add_list.Add(geom);
        }

        public void RemoveGeom(gxtGeom geom)
        {
            gxtDebug.SlowAssert(!geom_remove_list.Contains(geom));
            
            if (!geom_remove_list.Contains(geom))
                geom_remove_list.Add(geom);
        }

        public void AddBody(gxtRigidBody body)
        {
            gxtDebug.SlowAssert(!body_add_list.Contains(body));

            if (!body_add_list.Contains(body))
                body_add_list.Add(body);
        }

        public void RemoveBody(gxtRigidBody body)
        {
            gxtDebug.SlowAssert(!body_remove_list.Contains(body));

            if (!body_remove_list.Contains(body))
                body_remove_list.Add(body);
        }

        public void AddCollisionGroupName(string name, gxtCollisionGroup collisionGroup)
        {
            /*
            if (collisionGroupsMap.ContainsValue(collisionGroup))
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Collision group exists under another name");
            }
            */
            collisionGroupsMap.Add(name, collisionGroup);
        }

        public void RemoveCollisionGroupName(string name)
        {
            collisionGroupsMap.Remove(name);
        }

        public gxtCollisionGroup GetCollisionGroup(string name)
        {
            gxtCollisionGroup cgroup;
            if (collisionGroupsMap.TryGetValue(name, out cgroup))
            {
                return cgroup;
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "No Aliased Collision Group Exists in the World with the Name: {0}", cgroup.ToString());
                return gxtCollisionGroup.NONE;
            }
        }
        #endregion Add/Remove

        /// <summary>
        /// Updates the simulation by the elapsed time step (in seconds)
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Update(GameTime gameTime)
        {
            Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        /// <summary>
        /// Updates the simulation by the given time step
        /// You can forcefully call this method instead of Update(gameTime) 
        /// if you want to use a different dt than the total elapsed seconds
        /// </summary>
        /// <param name="dt">Delta Time</param>
        public void Update(float dt)
        {
            if (!Enabled) return;

            // process added and removed items
            ProcessAddedItems();
            ProcessRemovedItems();
            ClearLists();

            // perform collision detection
            BroadPhase();
            NarrowPhase();

            int i;
            // update all bodies, including gravity if enabled
            if (gravityEnabled)
            {
                for (i = 0; i < bodyList.Count; ++i)
                {
                    if (bodyList[i].IsFixed)
                        continue;
                    if (!bodyList[i].IgnoreGravity)
                    {
                        // apply force of gravity
                        if (bodyList[i].IsDynamic)
                            bodyList[i].ApplyForce(Gravity * bodyList[i].Mass);
                        else if (bodyList[i].IsKinematic)
                            bodyList[i].Acceleration += Gravity;
                    }
                    // update the body's linear and angular velocity
                    bodyList[i].Update(dt);
                }
            }
            else
            {
                for (i = 0; i < bodyList.Count; ++i)
                {
                    // update the body's linear and angular velocity
                    bodyList[i].Update(dt);
                }
            }

            // resolve all contacts
            Solve(dt);

            // update the body's position/rotation
            for (i = 0; i < bodyList.Count; ++i)
            {
                bodyList[i].Integrate(dt);
            }

            // update geoms to reflect changes in rigid bodies
            for (i = 0; i < geomList.Count; ++i)
            {
                geomList[i].Update();
            }      
        }

        /// <summary>
        /// Handles removal requests for objects in the PhysicsWorld
        /// </summary>
        private void ProcessRemovedItems()
        {
            for (int i = 0; i < geom_remove_list.Count; i++)
            {
                geomList.Remove(geom_remove_list[i]);
                broadphaseCollider.RemoveObject(geom_remove_list[i]);
            }
            for (int i = 0; i < body_remove_list.Count; i++)
            {
                bodyList.Remove(body_remove_list[i]);
            }
        }

        /// <summary>
        /// Handles add requests for objects in the PhysicsWorld
        /// </summary>
        private void ProcessAddedItems()
        {
            for (int i = 0; i < geom_add_list.Count; i++)
            {
                if (!geomList.Contains(geom_add_list[i]))
                {
                    // when adding for the first time we need to register the object
                    // with the broadphase collider
                    geomList.Add(geom_add_list[i]);
                    gxtAABB geomAABB = geom_add_list[i].AABB;
                    broadphaseCollider.AddObject(geom_add_list[i], ref geomAABB);
                    geom_add_list[i].BrodphaseCollider = broadphaseCollider;
                }
                else
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Attempt Was Made To Add An Existing Geom (id: {0}) To The Physics World", geom_add_list[i].Id);
                }
            }
            for (int i = 0; i < body_add_list.Count; i++)
            {
                if (!bodyList.Contains(body_add_list[i]))
                    bodyList.Add(body_add_list[i]);
                else
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Attempt Was Made To Add An Existing Rigid Body To The Physics World");
            }
        }

        /// <summary>
        /// Clears data structures used for add/remove requests
        /// </summary>
        private void ClearLists()
        {
            geom_remove_list.Clear();
            geom_add_list.Clear();

            body_add_list.Clear();
            body_remove_list.Clear();

            //testPairs.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resetGeomIds"></param>
        public void Dispose(bool resetGeomIds = true)
        {
            if (resetGeomIds)
                gxtGeom.ResetGeomIds();
            ClearLists();
            // should collider have a clear all method?
            for (int i = 0; i < geomList.Count; ++i)
            {
                if (!broadphaseCollider.RemoveObject(geomList[i]))
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "A geom in the world was not in the broadphase collider!");
            }
            narrowphaseContacts.Clear();
            geomList.Clear();
            bodyList.Clear();
            collisionGroupsMap.Clear();
        }

        /// <summary>
        /// Runs quick broadphase tests to prune more expensive calculations
        /// This brute force check will eventually be replaced by a persistent collider
        /// </summary>
        public void BroadPhase()
        {
            broadphasePairs.Clear(); // not sure this will work well for OnSeperationEvents
            testPairs = broadphaseCollider.GetCollisionPairs();
            for (int i = 0; i < testPairs.Length; ++i)
            {
                int hash = testPairs[i].GetHashCode();
                if (!broadphasePairs.ContainsKey(hash))
                {
                    broadphasePairs.Add(hash, testPairs[i]);
                }
            }
        }

        /// <summary>
        /// Checks if it is possible for two geoms to collide given their settings
        /// Optional AABB test is available
        /// </summary>
        /// <param name="geomA">Geom A</param>
        /// <param name="geomB">Geom B</param>
        /// <param name="testAABB">Optional AABB test</param>
        /// <returns>If the two geoms can collide</returns>
        public bool CanCollide(gxtGeom geomA, gxtGeom geomB, bool testAABB = false)
        {
            if (!geomA.CollisionEnabled || !geomB.CollisionEnabled)
                return false;
            if ((geomA.CollisionGroups & geomB.CollidesWithGroups) == gxtCollisionGroup.NONE || (geomA.CollidesWithGroups & geomB.CollisionGroups) == gxtCollisionGroup.NONE)
                return false;
            if (testAABB)
                if (!gxtAABB.Intersects(geomA.AABB, geomB.AABB))
                    return false;
            return true;
        }

        /// <summary>
        /// Determines if an object with the given collision group can collide with the geom
        /// </summary>
        /// <param name="collisionGroup">Collision Group</param>
        /// <param name="geom">Geom</param>
        /// <returns>If object and geom can collide</returns>
        public bool CanCollide(gxtCollisionGroup collisionGroup, gxtGeom geom)
        {
            if (!geom.CollisionEnabled)
                return false;
            if ((collisionGroup & geom.CollisionGroups) == gxtCollisionGroup.NONE)
                return false;
            return true;
        }

        /// <summary>
        /// Runs more expensive narrowphase tests to determine object-object intersections
        /// No need for dt here...
        /// </summary>
        public void NarrowPhase()
        {
            
            //if (testPairs == null)
            //    return;

            HashSet<gxtBroadphaseCollisionPair<gxtGeom>> broadphaseSet = new HashSet<gxtBroadphaseCollisionPair<gxtGeom>>(testPairs);
            HashSet<gxtBroadphaseCollisionPair<gxtGeom>> seperated = new HashSet<gxtBroadphaseCollisionPair<gxtGeom>>(narrowphaseContacts.Keys);
            seperated.ExceptWith(broadphaseSet);
            
            foreach (gxtBroadphaseCollisionPair<gxtGeom> seperatedPair in seperated)
            {
                gxtGeom.FireOnSeperationEvents(seperatedPair.objA, seperatedPair.objB);
                narrowphaseContacts.Remove(seperatedPair);
            }
            

            foreach (gxtBroadphaseCollisionPair<gxtGeom> pair in broadphaseSet)
            {
                gxtCollisionResult collisionResult;
                if (CanCollide(pair.objA, pair.objB))
                {
                    collisionResult = gxtGJKCollider.Collide(pair.objA.Polygon, pair.objA.Position, pair.objB.Polygon, pair.objB.Position);
                    if (collisionResult.Intersection)
                    {
                        // response can be canceled by user
                        if (gxtGeom.FireOnCollisionEvents(pair.objA, pair.objB, collisionResult))
                        {
                            gxtContactPair c;
                            // If the response isn't enabled, the contact won't be either
                            if (pair.objA.CollisionResponseEnabled && pair.objB.CollisionResponseEnabled)
                            {
                                // either adjust the exisiting contact or create a new one
                                if (narrowphaseContacts.ContainsKey(pair))
                                {
                                    c = narrowphaseContacts[pair];
                                    c.SetupContact(pair.objA, pair.objB, ref collisionResult, gxtFrictionType.AVERAGE_SQRT, true);
                                }
                                else
                                {
                                    c = new gxtContactPair();
                                    c.SetupContact(pair.objA, pair.objB, ref collisionResult, gxtFrictionType.AVERAGE_SQRT, true);
                                    narrowphaseContacts.Add(pair, c);
                                }
                            }
                            else
                            {
                                // same thing as above, except the contacts are disabled
                                if (narrowphaseContacts.ContainsKey(pair))
                                {
                                    c = narrowphaseContacts[pair];
                                    c.SetupContact(pair.objA, pair.objB, ref collisionResult, gxtFrictionType.AVERAGE_SQRT, false);
                                }
                                else
                                {
                                    c = new gxtContactPair();
                                    c.SetupContact(pair.objA, pair.objB, ref collisionResult, gxtFrictionType.AVERAGE_SQRT, false);
                                    narrowphaseContacts.Add(pair, c);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (narrowphaseContacts.ContainsKey(pair))
                        {
                            gxtGeom.FireOnSeperationEvents(pair.objA, pair.objB);
                            narrowphaseContacts.Remove(pair);
                        }
                    }
                }
            }
            

            // broadphase pairs should be a hashset
            // construct a new temporary hashset
            // make new hashset = broapdhasepairs.exceptwith(narrowpahseContacts.keys);
            // fire on seperation events for each pair in the except hashset
            // remove every contact from the difference set
            // now, all previous contacts that aren't in broadphase collision anymore are handled
            // iterate thru the broadphase pairs
            // if intersection
                // fire on collision, make a new contact or update an existing one
            // else
                // if in the contact list
                    // fire onseperation event
                // remove the contact
            /*
            gxtDebug.Assert(testPairs != null);

            //narrowphaseContacts.Clear();

            // process all the broadphase pairs, determine if can collide (as a result of geom properties first)
            for (int i = 0; i < testPairs.Length; i++)
            {
                gxtBroadphaseCollisionPair<gxtGeom> pair = testPairs[i];
                gxtCollisionResult collisionResult;
                if (CanCollide(pair.objA, pair.objB))
                {
                    collisionResult = gxtGJKCollider.Collide(pair.objA.Polygon, pair.objA.Position, pair.objB.Polygon, pair.objB.Position);
                    if (collisionResult.Intersection)
                    {
                        if (gxtGeom.FireOnCollisionEvents(pair.objA, pair.objB, collisionResult))
                        {
                            if (pair.objA.CollisionResponseEnabled && pair.objB.CollisionResponseEnabled)
                            {
                                gxtContact c;
                                if (narrowphaseContacts.ContainsKey(pair.GetHashCode()))
                                {
                                    c = narrowphaseContacts[pair.GetHashCode()];
                                    c.SetupContact(pair.objA, pair.objB, ref collisionResult, gxtFrictionType.AVERAGE);
                                }
                                else
                                {
                                    c = new gxtContact();
                                    c.SetupContact(pair.objA, pair.objB, ref collisionResult, gxtFrictionType.AVERAGE);
                                    narrowphaseContacts.Add(pair.GetHashCode(), c);
                                }
                            }

                            // existing contacts preserve values needed for impulses, but collision data is changed
                        }
                    }
                    else
                    {
                        // if no intersection remove the contact
                        int hashCode = pair.GetHashCode();
                        narrowphaseContacts.Remove(hashCode);
                    }
                }
            }

            // remove contacts that didn't have a broadphase overlap
            List<int> removalHashCodes = new List<int>();
            foreach (int hashCode in narrowphaseContacts.Keys)
            {
                bool found = false;
                for (int i = 0; i < testPairs.Length; ++i)
                {
                    if (testPairs[i].GetHashCode() == hashCode)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    removalHashCodes.Add(hashCode);
            }

            for (int i = 0; i < removalHashCodes.Count; i++)
            {
                narrowphaseContacts.Remove(removalHashCodes[i]);
            }
            */
            
        }

        /*
        private int ContactDepthComparison(gxtContact a, gxtContact b)
        {
            return b.depth.CompareTo(a.depth);
            //return 
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Solve(float dt)
        {
            float invDt = 1.0f / dt;

            gxtContactPair[] contactPairs = new gxtContactPair[narrowphaseContacts.Count];
            narrowphaseContacts.Values.CopyTo(contactPairs, 0);
            //Comparison<gxtContact> depthComparison = new Comparison<gxtContact>(
            Array.Sort<gxtContactPair>(contactPairs);

            // prune objects that are static, that have response disabled
            // sort by contact depth
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "New...");

            int numEnabledContacts = 0;
            for (int i = 0; i < contactPairs.Length; ++i)
            {
                if (contactPairs[i].Enabled)
                {
                    numEnabledContacts++;
                    contactPairs[i].PreStepImpulse(invDt);
                }
            }

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Num Enabled Contacts: {0}", numEnabledContacts);

            // optimal iterations seems to be between 7 and 15 
            int iterations = 10;
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < contactPairs.Length; ++j)
                {
                    if (contactPairs[j].Enabled)
                        contactPairs[j].ApplyImpulses();
                }
            }
        }

        /// <summary>
        /// Returns all the geoms intersected by the point which collide with the given collision group
        /// </summary>
        /// <param name="pt">Pt</param>
        /// <param name="hitGeoms">Intersected Geoms</param>
        /// <param name="collisionGroup">Collision Group</param>
        /// <returns>Returns true if at least 1 geom is intersected by the point</returns>
        public bool PointCastAll(Vector2 pt, out List<gxtGeom> hitGeoms, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL)
        {
            if (collisionGroup == gxtCollisionGroup.NONE)
            {
                hitGeoms = new List<gxtGeom>();
                return false;
            }

            hitGeoms = broadphaseCollider.PointCastAll(pt);

            for (int i = hitGeoms.Count - 1; i >= 0; i--)
            {
                if (!CanCollide(collisionGroup, hitGeoms[i]))
                    hitGeoms.RemoveAt(i);
                else if (!gxtGJKCollider.Contains(hitGeoms[i].Polygon, pt))
                    hitGeoms.RemoveAt(i);
            }

            return hitGeoms.Count != 0;
        }

        /// <summary>
        /// Returns all the geoms intersected by the point which collide with the given collision group
        /// </summary>
        /// <param name="pt">Pt</param>
        /// <param name="hitGeoms">Intersected Geoms</param>
        /// <param name="collisionGroupName">Collision Group String Alias</param>
        /// <returns>Returns true if at least 1 geom is intersected by the point</returns>
        public bool PointCastAll(Vector2 pt, out List<gxtGeom> hitGeoms, string collisionGroupName)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return PointCastAll(pt, out hitGeoms, cgroup);
        }

        /// <summary>
        /// Returns one geom intersected by the pt which will collide with the given collision group
        /// In cases where multiple geoms are intersected by the point, the geom is chosen whose centroid 
        /// is closest to the passed in point
        /// </summary>
        /// <param name="pt">Pt</param>
        /// <param name="hitGeom">Intersected Geom</param>
        /// <param name="collisionGroup">Collision Group</param>
        /// <returns>Returns true if a geom is intersected by the point</returns>
        public bool PointCast(Vector2 pt, out gxtGeom hitGeom, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL)
        {
            hitGeom = null;
            List<gxtGeom> geoms;
            if (PointCastAll(pt, out geoms, collisionGroup))
            {
                float minDist = float.MaxValue;
                float tmpDist;
                for (int i = 0; i < geoms.Count; i++)
                {
                    // should really be by the world centroid instead
                    tmpDist = (pt - geoms[i].GetWorldCentroid()).LengthSquared();
                    if (tmpDist < minDist)
                    {
                        hitGeom = geoms[i];
                        minDist = tmpDist;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns one geom intersected by the pt which will collide with the given collision group
        /// In cases where multiple geoms are intersected by the point, the geom is chosen whose centroid 
        /// is closest to the point
        /// </summary>
        /// <param name="pt">Pt</param>
        /// <param name="hitGeom">Intersected Geom</param>
        /// <param name="collisionGroupName">Collision Group</param>
        /// <returns>Returns true if a geom is intersected by the point</returns>
        public bool PointCast(Vector2 pt, out gxtGeom hitGeom, string collisionGroupName)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return PointCast(pt, out hitGeom, cgroup);
        }

        /// <summary>
        /// Similar to RayCast() but all intersections, not just the nearest one, is processed and 
        /// added to the passed in list of ray hits.  Said list is not sorted by distance.  If you 
        /// require a sorted list you must do this yourself in the calling function.
        /// Returns true if at least one geom is intersected.
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="rayHits">List of Ray Hits</param>
        /// <param name="collisionGroup">Collision Group</param>
        /// <param name="tmax">Max Ray Distance</param>
        /// <returns>If any geoms are intersected</returns>
        public bool RayCastAll(gxtRay ray, out List<gxtRayHit> rayHits,  gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL, float tmax = float.MaxValue)
        {
            rayHits = new List<gxtRayHit>();

            if (collisionGroup == gxtCollisionGroup.NONE)
                return false;

            List<gxtGeom> geoms = broadphaseCollider.RayCastAll(ray, tmax);
            
            if (geoms.Count == 0)
                return false;

            // variables in ray hit
            float t;
            Vector2 pt, normal;

            for (int i = 0; i < geoms.Count; i++)
            {
                if (CanCollide(collisionGroup, geoms[i]))
                {
                    if (gxtGJKCollider.RayCast(ray, geoms[i].Polygon, tmax, out t, out pt, out normal))
                    {
                        gxtRayHit rayHit = new gxtRayHit();
                        rayHit.Intersection = true;
                        rayHit.Distance = t;
                        rayHit.Normal = normal;
                        rayHit.Point = pt;
                        rayHit.Geom = geoms[i];
                        rayHits.Add(rayHit);
                    }
                }
            }

            return rayHits.Count > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="rayHits">List of Ray Hits</param>
        /// <param name="collisionGroupName">Collision Group</param>
        /// <param name="tmax">Max Ray Distance</param>
        /// <returns></returns>
        public bool RayCastAll(gxtRay ray, out List<gxtRayHit> rayHits, string collisionGroupName, float tmax = float.MaxValue)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return RayCastAll(ray, out rayHits, cgroup, tmax);
        }

        /// <summary>
        /// Casts a ray into the world with the given max distance and collision filter
        /// Packages the nearest hit object and information on the ray cast into the rayHit structure
        /// Returns true if an object is intersected by the ray, false otherwise
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="rayHit">Ray Hit</param>
        /// <param name="tmax">Max Ray Distance</param>
        /// <param name="collisionGroup">Collision Groups</param>
        /// <returns>If an object was hit</returns>
        public bool RayCast(gxtRay ray, out gxtRayHit rayHit, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL, float tmax = float.MaxValue)
        {
            rayHit = new gxtRayHit();
            
            if (collisionGroup == gxtCollisionGroup.NONE)
                return false;

            List<gxtGeom> geoms = broadphaseCollider.RayCastAll(ray, tmax, true);

            if (geoms.Count == 0)
                return false;

            float t;
            float minDist = tmax;
            Vector2 pt, normal;
            for (int i = 0; i < geoms.Count; i++)
            {
                if (CanCollide(collisionGroup, geoms[i]))
                {
                    if (gxtGJKCollider.RayCast(ray, geoms[i].Polygon, tmax, out t, out pt, out normal))
                    {
                        if (t <= minDist)
                        {
                            rayHit.Intersection = true;
                            rayHit.Distance = t;
                            rayHit.Normal = normal;
                            rayHit.Point = pt;
                            rayHit.Geom = geoms[i];
                            minDist = t;
                        }
                    }
                }
            }

            return rayHit.Intersection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="rayHit"></param>
        /// <param name="collisionGroupName"></param>
        /// <param name="tmax"></param>
        /// <returns></returns>
        public bool RayCast(gxtRay ray, out gxtRayHit rayHit, string collisionGroupName, float tmax = float.MaxValue)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return RayCast(ray, out rayHit, cgroup, tmax);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="hitGeoms"></param>
        /// <param name="collisionGroup"></param>
        /// <returns></returns>
        public bool AABBCastAll(gxtAABB aabb, out List<gxtGeom> hitGeoms, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL)
        {
            gxtDebug.Assert(aabb.Extents.X >= 0.0f && aabb.Extents.Y >= 0.0f, "An AABB with negative extents will not intersect anything");

            if (collisionGroup == gxtCollisionGroup.NONE)
            {
                hitGeoms = new List<gxtGeom>();
                return false;
            }

            hitGeoms = broadphaseCollider.AABBCastAll(aabb);

            if (hitGeoms.Count == 0)
                return false;

            gxtPolygon boxPolygon = gxtGeometry.ComputePolygonFromAABB(aabb);
            gxtPolygon geomPolygon;
            for (int i = hitGeoms.Count - 1; i >= 0; i--)
            {
                if (!CanCollide(collisionGroup, hitGeoms[i]))
                    hitGeoms.RemoveAt(i);
                else
                {
                    geomPolygon = hitGeoms[i].Polygon;
                    if (!gxtGJKCollider.Intersects(ref boxPolygon, aabb.Position, ref geomPolygon, hitGeoms[i].Position))
                        hitGeoms.RemoveAt(i);
                }
            }

            return hitGeoms.Count != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="hitGeoms"></param>
        /// <param name="collisionGroupName"></param>
        /// <returns></returns>
        public bool AABBCastAll(gxtAABB aabb, out List<gxtGeom> hitGeoms, string collisionGroupName)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return AABBCastAll(aabb, out hitGeoms, cgroup);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="hitGeom"></param>
        /// <param name="collisionGroup"></param>
        /// <returns></returns>
        public bool AABBCast(gxtAABB aabb, out gxtGeom hitGeom, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL)
        {
            hitGeom = null;
            List<gxtGeom> geoms;
            if (AABBCastAll(aabb, out geoms, collisionGroup))
            {
                float minDist = float.MaxValue;
                float tmpDist;
                for (int i = 0; i < geoms.Count; i++)
                {
                    tmpDist = (aabb.Position - geoms[i].GetWorldCentroid()).LengthSquared();
                    if (tmpDist < minDist)
                    {
                        hitGeom = geoms[i];
                        minDist = tmpDist;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="hitGeom"></param>
        /// <param name="collisionGroupName"></param>
        /// <returns></returns>
        public bool AABBCast(gxtAABB aabb, out gxtGeom hitGeom, string collisionGroupName)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return AABBCast(aabb, out hitGeom, cgroup);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="hitGeoms"></param>
        /// <param name="collisionGroup"></param>
        /// <returns></returns>
        public bool SphereCastAll(gxtSphere sphere, out List<gxtGeom> hitGeoms, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL)
        {
            if (collisionGroup == gxtCollisionGroup.NONE)
            {
                hitGeoms = new List<gxtGeom>();
                return false;
            }

            // we pass an aabb of the sphere thru the broadphase collider...for now
            // narrowphase tests use the actual sphere
            gxtAABB aabb = new gxtAABB(sphere.Position, new Vector2(sphere.Radius));
            hitGeoms = broadphaseCollider.AABBCastAll(aabb);

            if (hitGeoms.Count == 0)
                return false;

            gxtPolygon geomPolygon;
            for (int i = hitGeoms.Count - 1; i >= 0; i--)
            {
                if (!CanCollide(collisionGroup, hitGeoms[i]))
                    hitGeoms.RemoveAt(i);
                else
                {
                    geomPolygon = hitGeoms[i].Polygon;
                    // must ue the guranteed world centroid to ensure an accurate test
                    // http://www.gamedev.net/topic/308060-circle-polygon-intersection-more-2d-collisions/
                    if (!gxtGJKCollider.Intersects(ref geomPolygon, hitGeoms[i].GetWorldCentroid(), sphere))
                        hitGeoms.RemoveAt(i);
                }
            }

            return hitGeoms.Count != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="hitGeoms"></param>
        /// <param name="collisionGroupName"></param>
        /// <returns></returns>
        public bool SphereCastAll(gxtSphere sphere, out List<gxtGeom> hitGeoms, string collisionGroupName)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return SphereCastAll(sphere, out hitGeoms, cgroup);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="hitGeom"></param>
        /// <param name="collisionGroup"></param>
        /// <returns></returns>
        public bool SphereCast(gxtSphere sphere, out gxtGeom hitGeom, gxtCollisionGroup collisionGroup = gxtCollisionGroup.ALL)
        {
            hitGeom = null;
            List<gxtGeom> geoms;
            if (SphereCastAll(sphere, out geoms, collisionGroup))
            {
                float minDist = float.MaxValue;
                float tmpDist;
                for (int i = 0; i < geoms.Count; i++)
                {
                    tmpDist = (sphere.Position - geoms[i].GetWorldCentroid()).LengthSquared();
                    if (tmpDist < minDist)
                    {
                        hitGeom = geoms[i];
                        minDist = tmpDist;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="hitGeom"></param>
        /// <param name="collisionGroupName"></param>
        /// <returns></returns>
        public bool SphereCast(gxtSphere sphere, out gxtGeom hitGeom, string collisionGroupName)
        {
            gxtDebug.Assert(collisionGroupsMap.ContainsKey(collisionGroupName), "Named Collision Group: {0} Does Not Exist In Physics World");
            gxtCollisionGroup cgroup = collisionGroupsMap[collisionGroupName];
            return SphereCast(sphere, out hitGeom, cgroup);
        }



        // undecided on if I want polygon casting to return collision results or the objects hit
        // have booleans to debug draw the contacts, and colors too

        /// <summary>
        /// Debug draws all the geoms in the physics world
        /// Takes parameters for what is drawn, colors, and render depths
        /// </summary>
        /// <param name="geomColor"></param>
        /// <param name="geomCollisionColor"></param>
        /// <param name="aabbColor"></param>
        /// <param name="contactColor"></param>
        /// <param name="geomRenderDepth"></param>
        /// <param name="aabbRenderDepth"></param>
        /// <param name="contactRenderDepth"></param>
        /// <param name="drawGeoms"></param>
        /// <param name="drawBoundingBoxes"></param>
        /// <param name="drawContacts"></param>
        // TODO: HAVE GEOM ASLEEP COLOR
        public void DebugDrawGeoms(Color geomColor, Color geomCollisionColor, Color geomSleepColor, Color aabbColor, Color contactColor, float geomRenderDepth, float aabbRenderDepth, float contactRenderDepth, bool drawGeoms, bool drawBoundingBoxes, bool drawContacts)
        {
            if (!gxtDebugDrawer.SingletonIsInitialized)
            {
                if (gxtLog.SingletonIsInitialized)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot debug draw physics world if the debug drawer isn't initialized!");
                return;
            }

            // use physics scaling, reset back at the end
            float prevdebugDrawerScale = gxtDebugDrawer.Singleton.CurrentScale;
            gxtDebugDrawer.Singleton.CurrentScale = PHYSICS_SCALE;
            if (drawBoundingBoxes)
            {
                for (int i = 0; i < geomList.Count; i++)
                {
                    gxtDebugDrawer.Singleton.AddAABB(geomList[i].AABB, aabbColor, aabbRenderDepth);
                }
            }

            if (drawGeoms)
            {
                for (int i = 0; i < geomList.Count; i++)
                {
                    Color col = geomColor;
                    if (geomList[i].HasAttachedBody() && !geomList[i].RigidBody.Awake)
                    {
                        col = geomSleepColor;
                    }
                    else
                    {
                        foreach (gxtBroadphaseCollisionPair<gxtGeom> pair in narrowphaseContacts.Keys)
                        {
                            if (pair.objA.Equals(geomList[i]) || pair.objB.Equals(geomList[i]))
                                col = geomCollisionColor;
                        }
                    }
                    gxtDebugDrawer.Singleton.AddPolygon(geomList[i].Polygon, col, geomRenderDepth);
                    gxtDebugDrawer.Singleton.AddPt(geomList[i].GetWorldCentroid(), col, geomRenderDepth);
                }
            }

            if (drawContacts)
            {
                foreach (gxtContactPair contactPair in narrowphaseContacts.Values)
                {
                    if (contactPair.Enabled)
                    {
                        /*
                        if (contactPair.ContactA.Position.LengthSquared() < 0.3f)
                        {
                            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Why are contacts here?");
                        }
                        */
                        gxtDebugDrawer.Singleton.AddPt(contactPair.ContactA.Position, contactColor, contactRenderDepth);
                        gxtDebugDrawer.Singleton.AddPt(contactPair.ContactB.Position, contactColor, contactRenderDepth);
                    }
                }
            }
            gxtDebugDrawer.Singleton.CurrentScale = prevdebugDrawerScale;
        }
        
        // todo: a debug draw method that takes a dictionary of different colors for each collision group

        // change to simply show velocity vector and position
        // maybe show the axis too
        public void DebugDrawRigidBodies(Color msgColor)
        {
            gxtRigidBody tmp;

            bool cacheFill = gxtDebugDrawer.Singleton.FillGeometry;
            gxtDebugDrawer.Singleton.FillGeometry = true;
            for (int i = 0; i < bodyList.Count; ++i)
            {
                tmp = bodyList[i];
                Vector2 bodyPos = tmp.Position * PHYSICS_SCALE;
                Color col = new Color(0.0f, 0.0f, 0.85f, 0.6f);
                gxtDebugDrawer.Singleton.AddCircle(bodyPos, 10.0f, col, 1.0f);
                //string bodyMsg = string.Format("Force: {0}\nTorque: {1}\nAccel: {2}\nVel: {3}", tmp.Force.ToString(), tmp.Torque.ToString(), tmp.Acceleration.ToString(), tmp.Velocity.ToString());
                //gxtDebugDrawer.Singleton.AddString(bodyMsg, bodyPos, msgColor, 0.0f);
            }
            gxtDebugDrawer.Singleton.FillGeometry = cacheFill;
        }
    }
}