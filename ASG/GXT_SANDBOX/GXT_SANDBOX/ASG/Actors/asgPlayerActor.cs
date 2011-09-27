using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GXT;
using GXT.IO;
using GXT.Rendering;
using GXT.Physics;
using GXT.Processes;

namespace ASG
{
    // delegates will inevitably go here
    
    public enum asgDamageMode
    {
        NORMAL = 0,
        GOD = 1
    };

    public enum asgClipMode
    {
        NORMAL = 0,
        NOCLIP = 1
    };

    // TODO: COOL DOWN WHEN YOU HIT THE GROUND SO YOU CAN'T INSTANTLY JUMP AGAIN
    public class asgPlayerActor : gxtIActor
    {
        /* gxtIActor Members */
        protected bool enabled;
        protected gxtWorld world;
        protected Vector2 position;
        protected float rotation;
        protected gxtHashedString hashedString;

        // will need to do a lot more than this when actually enabled/disabled
        public bool Enabled { get { return enabled; } set { enabled = value; } }
        public gxtWorld World { get { return world; } }
        public Vector2 Position { get { return position; } set { SetWorldPosition(value); } }
        public float Rotation { get { return rotation; } set { SetWorldRotation(value); } }
        public gxtHashedString Type { get { return hashedString; } }

        private float maxSpeed;
        private float moveSpeed;
        private float airMoveSpeed;

        public float MaxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }
        public float MoveSpeed { get { return moveSpeed; } set { gxtDebug.Assert(value > float.Epsilon); moveSpeed = value; } }
        public float AirMoveSpeed { get { return airMoveSpeed; } set { gxtDebug.Assert(value >= 0.0f); airMoveSpeed = value; } }
        
        // rendering
        gxtPolygon scenePoly;
        gxtIDrawable playerDrawable;
        gxtISceneNode playerSceneNode;

        // geometry
        // there will likely be more over time
        gxtPolygon playerPolygon;
        gxtGeom playerGeom;

        gxtOBB localOBB;
        gxtOBB worldOBB;

        // bodies
        gxtRigidBody playerBody;
        public gxtRigidBody Body { get { return playerBody; } }

        // timers
        gxtStopWatch rayCastTimer;
        private List<gxtRayHit> raycasts;
        private Vector2 surfaceNormal;

        private float halfHeight;

        public List<gxtRayHit> RayCasts { get { return raycasts; } }
        public Vector2 SurfaceNormal { get { return surfaceNormal; } }

        private Vector2 fwd = Vector2.UnitX;
        public Vector2 Forward { get { return fwd; } protected set { fwd = value; } }
        public Vector2 Right { get { return gxtMath.RightPerp(fwd); } }
        public Vector2 Left { get { return gxtMath.LeftPerp(fwd); } }

        private asgClipMode clipMode;
        public asgClipMode ClipMode
        {
            get { return clipMode; }
            set
            {
                gxtDebug.Assert(clipMode == asgClipMode.NORMAL || clipMode == asgClipMode.NOCLIP);
                if (clipMode == value)
                    return;

                clipMode = value;
                if (clipMode == asgClipMode.NORMAL)
                {
                    playerGeom.CollisionResponseEnabled = true;
                    playerGeom.CollisionEnabled = true;
                    playerBody.IgnoreGravity = false;
                }
                else
                {
                    playerGeom.CollisionEnabled = false;
                    playerBody.IgnoreGravity = true;
                    // could set it's collision groups to NONE here
                }
                playerBody.ClearKinematics();
                playerBody.ClearForceAndTorque();
            }
        }

        private bool collidingWithWalkableGeom = false;
        public bool OnGround { get { return collidingWithWalkableGeom; } private set { collidingWithWalkableGeom = value; } }

        // it's better practice NOT to increment this here
        private TimeSpan seperationTime = TimeSpan.Zero;
        private bool incrementSeperationTime = false;

        public asgPlayerActor(gxtWorld world)
        {
            this.world = world;
            this.enabled = true;
        }

        public void Initialize(Vector2 initPos, float speed = 3.0f, float maxSpeed = 500.0f)
        {
            hashedString = new gxtHashedString("player_actor");
            this.position = initPos;
            this.rotation = 0.0f; // if we were to take a rotation be sure to use gxtMath.WrapAngle(initRot)
            MoveSpeed = speed;
            MaxSpeed = maxSpeed;

            this.clipMode = asgClipMode.NORMAL;
            // in physics world units
            // setup body
            playerBody = new gxtRigidBody();
            playerBody.Mass = 2.0f;
            playerBody.CanSleep = false;    // should NEVER go to sleep
            playerBody.Awake = true;
            playerBody.FixedRotation = true;
            playerBody.IgnoreGravity = false;
            playerBody.Position = position;
            playerBody.Rotation = rotation;
            world.AddRigidBody(playerBody);

            // setup geom
            //playerPolygon = gxtGeometry.CreateRectanglePolygon(2, 3.5f);
            playerPolygon = gxtGeometry.CreateRoundedRectanglePolygon(2.0f, 3.5f, 0.45f, 0.05f);
            //playerPolygon = gxtGeometry.CreateEllipsePolygon(1.0f, 1.75f, 20);
            //playerPolygon = gxtGeometry.CreateCapsulePolygon(3.0f, 1.0f, 8);
            //playerPolygon = gxtGeometry.CreateCirclePolygon(3.0f, 3);
            playerGeom = new gxtGeom(playerPolygon, position, rotation);
            playerGeom.Tag = this;
            playerGeom.CollidesWithGroups = world.PhysicsWorld.GetCollisionGroup("traversable_world_geometry");
            playerGeom.CollisionGroups = world.PhysicsWorld.GetCollisionGroup("player");
            playerGeom.RigidBody = playerBody;
            playerGeom.OnCollision += OnCollision;
            playerGeom.OnSeperation += OnSeperation;
            world.PhysicsWorld.AddGeom(playerGeom);

            // setup scene node
            // for now we'll just use a line loop but programming it 
            // this way makes it easy to swap out for something like a skeleton 
            // or a texture later down the road
            scenePoly = gxtPolygon.Copy(playerPolygon);
            scenePoly.Scale(gxtPhysicsWorld.PHYSICS_SCALE);
            //playerEntity = new gxtLineLoop(scenePoly.v);

            // setup drawable
            //playerDrawable = new gxtDrawable(playerEntity, Color.Yellow, true, 0.1f);
            playerSceneNode = new gxtSceneNode();
            playerSceneNode.Position = playerBody.Position;
            //playerSceneNode.AttachDrawable(playerDrawable);
            //world.AddSceneNode(playerSceneNode);

            // setup raycatsing logic
            rayCastTimer = new gxtStopWatch(true);
            world.AddProcess(rayCastTimer);
            raycasts = new List<gxtRayHit>();

            clipMode = asgClipMode.NORMAL;
            this.halfHeight = playerGeom.LocalAABB.Height * 0.5f;
        }

        public void SetWorldPosition(Vector2 position)
        {
            playerBody.Position = position;
        }

        public void SetWorldRotation(float rotation)
        {
            playerBody.Rotation = rotation;
        }

        public void Translate(Vector2 t)
        {
            playerBody.Translate(t);
        }

        public void Move(Vector2 t)
        {
            playerBody.Velocity += Forward * Vector2.Dot(Forward, t) * moveSpeed;
        }

        public gxtAABB GetAABB()
        {
            return playerGeom.AABB;
            //return playerG
        }

        public gxtOBB GetOBB()
        {
            return new gxtOBB();
        }

        public void Update(GameTime gameTime)
        {
            if (!enabled)
                return;

            // state based logic here only!
            if (incrementSeperationTime)
            {
                seperationTime += gameTime.ElapsedGameTime;
                if (seperationTime >= TimeSpan.FromSeconds(0.05))
                    OnGround = false;
            }

            // make a variable for raycast distance
            if (clipMode == asgClipMode.NORMAL && PerformRaycast(10.0f))
            {
                // will likely want to search for the first one that is a walkable platform
                // instead of just naively making it the first one found (it could be a bullet, projectile, etc. etc.)
                gxtDebug.Assert(raycasts != null && raycasts.Count > 0);
                surfaceNormal = raycasts[0].Normal;
                //if (incollisionWithGround)
                float dH = raycasts[0].Distance - halfHeight;
                if (dH <= 0.0f)
                {
                    playerBody.Translate(0.0f, dH);
                    OnGround = true;
                }
            }
            else
            {
                // default surface normal, works even if in the air
                // if rotation is allowed, would need to make it equal the y axis vector
                surfaceNormal = -Vector2.UnitY;
            }

            /*
            if (rayCastTimer.ElapsedTime >= TimeSpan.FromSeconds(0.5))
            {
                gxtRay downRayCast = new gxtRay(position, Vector2.UnitY);
                gxtRayHit rayResult;
                if (world.PhysicsWorld.RayCast(downRayCast, out rayResult, "platforms", 10.0f))
                {
                    // assuming the player is grounded
                    Vector2 normalRight = gxtMath.RightPerp(rayResult.Normal);
                    this.Forward = normalRight;
                }

                rayCastTimer.ElapsedTime -= TimeSpan.FromSeconds(0.5);
            }
            */
            
            // update final positions, update scene node
            position = playerBody.Position;
            rotation = playerBody.Rotation;
            playerSceneNode.Position = position * gxtPhysicsWorld.PHYSICS_SCALE;
        }

        private bool PerformRaycast(float t)
        {
            raycasts.Clear();

            gxtRay downRay = new gxtRay();
            downRay.Origin = position;
            downRay.Direction = Vector2.UnitY;

            // may want to raycast against more than just platforms
            bool anyHits = world.PhysicsWorld.RayCastAll(downRay, out raycasts, "traversable_world_geometry", 15.0f);
            gxtDebugDrawer.Singleton.CurrentScale = gxtPhysicsWorld.PHYSICS_SCALE;
            if (anyHits)
            {
                // insertion sort
                int j, i;
                gxtRayHit hit;
                for (j = 1; j < raycasts.Count - 1; ++j)
                {
                    hit = raycasts[j];
                    i = j - 1;
                    while ((i >= 0) && (hit.Distance < raycasts[i].Distance))
                    {
                        raycasts[i + 1] = raycasts[i];
                        --i;
                    }
                    raycasts[i + 1] = hit;
                }

                gxtDebugDrawer.Singleton.AddRay(downRay, raycasts[0].Distance, Color.Red);
            }
            else
            {
                gxtDebugDrawer.Singleton.AddRay(downRay, t, Color.Green);
            }

            return anyHits;
        }

        public bool OnCollision(gxtGeom playerGeom, gxtGeom otherGeom, gxtCollisionResult collisionResult)
        {
            incrementSeperationTime = false;
            //collidingWithWalkableGeom = true;
            return true;
        }

        public void OnSeperation(gxtGeom playerGeom, gxtGeom otherGeom)
        {
            incrementSeperationTime = true;
            //collidingWithWalkableGeom = false;
        }

        public void UnloadActor()
        {
            // remove node and drawable
            //playerSceneNode.DetachDrawable(playerDrawable);
            playerDrawable.Dispose();
            world.RemoveSceneNode(playerSceneNode);

            // remove rigid bodies

            // remove geoms
        }
    }
}
