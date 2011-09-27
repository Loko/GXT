using System;
using System.Collections.Generic;
using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Physics;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KinematicTestGame : gxtGame
    {
        gxtGeom geomA, geomB, geomC, geomD, /*geomE,*/ geomF;
        gxtRigidBody bodyA, /*bodyB,*/ bodyC, bodyD, /*bodyE,*/ bodyF;
        gxtRigidBody floorRB;
        gxtGeom floorG;
        bool gravityOn = false;
        gxtWorld world;
        int debugDrawerId;
        float damping = 1.0f;

        public KinematicTestGame()
            : base()
        {
        }

        public bool OnCollision(gxtGeom ga, gxtGeom gb, gxtCollisionResult collisionResult)
        {
            if (ga == geomF)
            {

            }
            //gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Collision between {0} and {1}", geomA.Id, geomB.Id);
            return true;
            /*
            float scalar = 35.0f;
            gxtDebugDrawer.Singleton.AddLine(geomA.Position, geomA.Position + collisionResult.Normal * scalar, Color.Orange, 0.0f);
            gxtDebugDrawer.Singleton.AddLine(geomB.Position, geomB.Position + collisionResult.Normal * scalar, Color.Orange, 0.0f);
            gxtDebugDrawer.Singleton.AddString(geomA.Id.ToString(), geomA.Position, Color.White, 0.0f);
            gxtDebugDrawer.Singleton.AddString(geomB.Id.ToString(), geomB.Position, Color.White, 0.0f);
            */
        }

        public void OnSeperation(gxtGeom geomA, gxtGeom geomB)
        {
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Seperation between {0} and {1}", geomA.Id, geomB.Id);            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            world.Initialize();
            gxtDisplayManager.Singleton.WindowTitle = "Rigid Body Testing";
            //world.Load();
            InitGeoms();
            InitBodies();
            gxtRoot.Singleton.Game.IsMouseVisible = true;
            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                debugDrawerId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawerId, world.SceneGraph);
                //gxtDebugDrawer.Singleton.SetTargetDrawManager(world.DrawManager);
                gxtDebugDrawer.Singleton.DebugFont = Content.Load<SpriteFont>("Fonts\\debug_font");
            }

            //VectorTest();
        }

        private void VectorTest()
        {
            int iterations = 8;
            float step = gxtMath.TWO_PI / (float) iterations;
            for (int i = 0; i < iterations; ++i)
            {
                float ang = step * i;
                float cos = gxtMath.Cos(ang);
                float sin = gxtMath.Sin(ang);
                Vector2 v = new Vector2(cos, sin);
                PrintVector(v, Vector2.UnitX, 0.0f);
            }

        }

        private void PrintVector(Vector2 v, Vector2 comp, float tolerance)
        {
            float scalar = 45.0f;
            float dot = Vector2.Dot(v, comp);
            float absDot = gxtMath.AbsDot(v, comp);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "V: {0}\nDot: {1}\nAbsDot: {2}", v.ToString(), dot, absDot);
            Color col = Color.White;
            gxtDebugDrawer.Singleton.AddLine(Vector2.Zero, v * scalar, col, 0.0f, TimeSpan.FromSeconds(120.0));
            gxtDebugDrawer.Singleton.AddString("Dot: " + dot.ToString() + "\nAbsDot: " + absDot.ToString(), v * (scalar + 130.0f), Color.Yellow, 0.0f, TimeSpan.FromSeconds(120.0));
        }

        private bool OnOneWayPlatformCollision(gxtGeom ga, gxtGeom gb, gxtCollisionResult cr)
        {
            float tolerance = 0.5f;
            if (ga == geomC)
            {
                //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "here");
                float dot = Vector2.Dot(cr.Normal, Vector2.UnitY);
                if (dot >= tolerance)
                {
                    //if (gb.HasAttachedBody() && Vector2.Dot(gb.RigidBody.PrevAcceleration, Vector2.UnitY) <= 0.0f)
                        return false;
                }
                else
                {
                    gxtRayHit rayHit;
                    gxtRay downRay = new gxtRay();
                    downRay.Origin = gb.GetWorldCentroid() - new Vector2(0.0f, geomF.LocalAABB.Height * 0.45f);
                    downRay.Direction = Vector2.UnitY;
                    float halfHeight = geomF.LocalAABB.Height * 0.65f;
                    if (world.PhysicsWorld.RayCast(downRay, out rayHit, gxtCollisionGroup.ALL, halfHeight))
                    {
                        
                        gxtRay adjRay = new gxtRay(downRay.Origin * gxtPhysicsWorld.PHYSICS_SCALE, downRay.Direction);
                        gxtDebugDrawer.Singleton.AddRay(adjRay, rayHit.Distance * gxtPhysicsWorld.PHYSICS_SCALE, Color.Green, 0.0f, TimeSpan.FromSeconds(5.0));
                        return false;
                    }
                    else
                    {
                        //return false;
                        gxtRay adjRay = new gxtRay(downRay.Origin * gxtPhysicsWorld.PHYSICS_SCALE, downRay.Direction);
                        gxtDebugDrawer.Singleton.AddRay(adjRay, halfHeight * gxtPhysicsWorld.PHYSICS_SCALE, Color.Green, 0.0f, TimeSpan.FromSeconds(5.0));
                        //return false;
                    }
                }
                //else if (gb.HasAttachedBody() && Vector2.Dot(gb.RigidBody.PrevAcceleration, Vector2.UnitY))
            } else
            {
                gxtDebug.Assert(false, "You didn't set up the swap properly");
            }
            /*
            else if (gb == geomC)
            {
                float dot = Vector2.Dot(cr.Normal, new Vector2(0.0f, -1.0f));
                if (dot >= tolerance)
                {
                    if (gb.HasAttachedBody() && Vector2.Dot(gb.RigidBody.PrevAcceleration, new Vector2(0.0f, -1.0f)) >= 0.0f)
                    return false;
                }
            }
            */
            return true;
        }

        private void InitGeoms()
        {

            gxtPhysicsMaterial material = new gxtPhysicsMaterial(0.7f, 0.2f);
            gxtPhysicsMaterial fMaterial = new gxtPhysicsMaterial(0.3f, 0.01f);
            gxtPhysicsMaterial cMaterial = new gxtPhysicsMaterial(0.01f, 0.5f);
            gxtPhysicsMaterial dMaterial = new gxtPhysicsMaterial(0.25f, 0.55f);

            geomA = new gxtGeom();
            geomA.CollisionEnabled = true;
            geomA.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomA.CollisionGroups = gxtCollisionGroup.ALL;
            geomA.CollisionResponseEnabled = true;
            geomA.RigidBody = null;
            geomA.Tag = null;
            //gxtPolygon worldPolyA = gxtGeometry.CreateRectanglePolygon(5.5f, 4.15f);
            //gxtPolygon worldPolyA = gxtGeometry.CreateRectanglePolygon(5.35f, 3.0f);
            gxtPolygon worldPolyA = gxtGeometry.CreateCirclePolygon(2.65f, 5);
            geomA.Polygon = worldPolyA;
            geomA.SetPosition(new Vector2(0, 0));
            //geomA.OnCollision += OnCollision;
            geomA.OnSeperation += OnSeperation;
            geomA.Material = material;

            world.PhysicsWorld.Gravity = new Vector2(0.0f, 9.8f);
            
            // A and B will be attached to the came rigid body
            geomB = new gxtGeom();
            geomB.CollisionEnabled = true;
            geomB.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomB.CollisionGroups = gxtCollisionGroup.ALL;
            geomB.CollisionResponseEnabled = true;
            geomB.RigidBody = null;
            geomB.Tag = null;
            //gxtPolygon worldPolyB = gxtGeometry.CreateRectanglePolygon(2.35f, 3.3f);
            gxtPolygon worldPolyB = gxtGeometry.CreateRectanglePolygon(2.35f, 5);
            geomB.SetupFromLocalPolygon(worldPolyB, new Vector2(0.0f, -4.0f), 0.0f);
            geomB.SetPosition(new Vector2(0, 0));
            geomB.Material = material;
            
            
            geomC = new gxtGeom();
            geomC.CollisionEnabled = true;
            geomC.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomC.CollisionGroups = gxtCollisionGroup.ALL;
            geomC.CollisionResponseEnabled = true;
            geomC.RigidBody = null;
            geomC.OnCollision += OnOneWayPlatformCollision;
            geomC.Tag = null;
            gxtPolygon worldPolyC = gxtGeometry.CreateRectanglePolygon(7.5f, 1.35f);
            geomC.Polygon = worldPolyC;
            geomC.SetPosition(new Vector2(4.5f, 5.25f));
            geomC.Material = cMaterial;

            geomD = new gxtGeom();
            geomD.CollisionEnabled = true;
            geomD.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomD.CollisionGroups = gxtCollisionGroup.ALL;
            geomD.CollisionResponseEnabled = true;
            geomD.RigidBody = null;
            geomD.Tag = null;
            gxtPolygon worldPolyD = gxtGeometry.CreateRectanglePolygon(3.0f, 3.0f);
            geomD.Polygon = worldPolyD;
            geomD.Material = dMaterial;
            //geomD.SetPosition(new Vector2(400, 0));

            /*
            geomE = new gxtGeom();
            geomE.CollisionEnabled = true;
            geomE.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomE.CollisionGroups = gxtCollisionGroup.ALL;
            geomE.CollisionResponseEnabled = true;
            geomE.RigidBody = null;
            geomE.Tag = null;
            gxtPolygon worldPolyE;
            Vector2[] tri = new Vector2[3];
            tri[0] = new Vector2(-400, -300);
            tri[1] = new Vector2(400, -75);
            tri[2] = new Vector2(400, -300);
            worldPolyE = new gxtPolygon(tri);
            geomE.Polygon = worldPolyE;
            geomE.SetPosition(new Vector2(0, 0));
            */

            geomF = new gxtGeom();
            geomF.CollisionEnabled = true;
            geomF.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomF.CollisionGroups = gxtCollisionGroup.ALL;
            geomF.CollisionResponseEnabled = true;
            geomF.RigidBody = null;
            geomF.Tag = null;
            //geomF.Polygon = gxtGeometry.CreateCirclePolygon(2.0f, 12);
            geomF.Polygon = gxtGeometry.CreateRectanglePolygon(3.0f, 4.5f);
            //geomF.Polygon = gxtGeometry.CreateRectanglePolygon(4.35f, 4);
            geomF.SetPosition(new Vector2(-5.5f, -3.15f));
            geomF.OnCollision += OnCollision;
            geomF.Material = fMaterial;

            floorG = new gxtGeom();
            floorG.CollisionEnabled = true;
            floorG.CollidesWithGroups = gxtCollisionGroup.ALL;
            floorG.CollisionGroups = gxtCollisionGroup.ALL;
            floorG.CollisionResponseEnabled = true;
            floorG.RigidBody = null;
            floorG.Tag = null;
            floorG.Polygon = gxtGeometry.CreateRectanglePolygon(30, 2.5f);
            floorG.SetPosition(new Vector2(0.0f, 10.25f));
            floorG.Material = material;

            world.AddGeom(geomA);
            world.AddGeom(floorG);
            //world.AddGeom(geomB);
            
            world.AddGeom(geomC);
            world.AddGeom(geomD);
            /*
            world.AddGeom(geomE);
            */
            world.AddGeom(geomF);
            
        }

        private void InitBodies()
        {
            

            bodyA = new gxtRigidBody();
            bodyA.CanSleep = false;
            bodyA.Awake = true;
            bodyA.IgnoreGravity = false;
            bodyA.MotionType = gxtRigidyBodyMotion.DYNAMIC;
            bodyA.Mass = 5.15f;
            bodyA.Inertia = gxtRigidBody.GetInertiaForPolygon(geomA.Polygon, bodyA.Mass);//gxtRigidBody.GetInertiaForRectangle(geomA.LocalAABB.Width, geomA.LocalAABB.Height, bodyA.Mass) + gxtRigidBody.GetInertiaForRectangle(geomB.LocalAABB.Width, geomB.LocalAABB.Height, bodyA.Mass); //gxtRigidBody.GetInertiaForPolygon(geomA.Polygon, bodyA.Mass);
            geomA.RigidBody = bodyA;
            //geomB.RigidBody = bodyA;
            bodyA.Position = new Vector2(0, 0);
            bodyA.Damping = 0.999815f;
            bodyA.AngularDamping = 0.99615f;
            //bodyA.Material = material;

            
            bodyF = new gxtRigidBody();
            bodyF.CanSleep = false;
            bodyF.Awake = true;
            bodyF.IgnoreGravity = true;
            bodyF.MotionType = gxtRigidyBodyMotion.DYNAMIC;
            bodyF.Mass = 1.0f;
            float inertiaRect = gxtRigidBody.GetInertiaForRectangle(geomF.LocalAABB.Width, geomF.LocalAABB.Height, bodyF.Mass); //gxtRigidBody.GetInertiaForPolygon(geomA.Polygon, bodyA.Mass);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Rectangle Inertia: {0}", inertiaRect);
            float polyInertia = gxtRigidBody.GetInertiaForPolygon(geomF.Polygon, bodyF.Mass);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Polygon Inertia: {0}", polyInertia);
            float circleInertia = gxtRigidBody.GetInertiaForCircle(2.0f, bodyF.Mass);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Circle Inertia: {0}", circleInertia);
            bodyF.Inertia = polyInertia;
            bodyF.FixedRotation = true;
            //bodyF.Inertia = 0.0f;
            geomF.RigidBody = bodyF;
            bodyF.Position = new Vector2(-7.5f, -3.15f);
            bodyF.Damping = 0.999815f;
            bodyF.AngularDamping = 0.99615f;
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Body F I: {0}, invI: {1}", bodyF.Inertia, bodyF.InverseInertia);
            //bodyF.Material = material;

            floorRB = new gxtRigidBody();
            floorRB.MotionType = gxtRigidyBodyMotion.FIXED;
            floorRB.CanSleep = false;
            floorRB.Awake = true;
            floorRB.IgnoreGravity = true;
            floorRB.Mass = 2.0f;
            floorRB.Position = new Vector2(0.0f, 10.25f);
            //floorRB.Material = material;
            floorRB.Inertia = gxtRigidBody.GetInertiaForRectangle(20, 4.5f, 2.0f);
            //floorG.RigidBody = floorRB;

            
            /*
            bodyB = new gxtRigidBody();
            bodyB.CanSleep = false;
            bodyB.Awake = true;
            bodyB.IgnoreGravity = true;
            bodyB.MotionType = gxtRigidyBodyMotion.FIXED;
            bodyB.Mass = 5.0f;
            geomB.RigidBody = bodyB;
            bodyB.Position = new Vector2(0, 250);
            bodyB.Material = material;
            */
            
            bodyC = new gxtRigidBody();
            bodyC.CanSleep = false;
            bodyC.Awake = true;
            bodyC.IgnoreGravity = true;
            bodyC.MotionType = gxtRigidyBodyMotion.FIXED;
            bodyC.Mass = 5.0f;
            bodyC.FixedRotation = true;
            geomC.RigidBody = bodyC;
            bodyC.Position = new Vector2(4.5f, 2.85f);

            
            bodyD = new gxtRigidBody();
            bodyD.CanSleep = false;
            bodyD.Awake = true;
            bodyD.IgnoreGravity = false;
            bodyD.MotionType = gxtRigidyBodyMotion.DYNAMIC;
            bodyD.Mass = 2.0f;
            geomD.RigidBody = bodyD;
            bodyD.Inertia = gxtRigidBody.GetInertiaForRectangle(2.0f, 2.0f, 2.0f);
            bodyD.Position = new Vector2(0.0f, 4.65f);
            //bodyD.Material = material;

            /*
            bodyE = new gxtRigidBody();
            bodyE.CanSleep = false;
            bodyE.Awake = true;
            bodyE.IgnoreGravity = true;
            bodyE.MotionType = gxtRigidyBodyMotion.FIXED;
            bodyE.Mass = 5.0f;
            geomE.RigidBody = bodyE;
            bodyE.Position = new Vector2(110, -175);
            bodyE.Material = material;
            */
            world.AddRigidBody(bodyA);
            world.AddRigidBody(floorRB);
            
            /*
            world.AddRigidBody(bodyB);
            world.AddRigidBody(bodyC);
            */
            world.AddRigidBody(bodyD);
            //world.AddRigidBody(bodyE);
           
            world.AddRigidBody(bodyF);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //root.Initialize(this);
            //spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            base.LoadContent();
            world = new gxtWorld();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float xA = 0.0f, yA = 0.0f, rA = 0.0f;
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            gxtMouse mouse = gxtMouseManager.Singleton.GetMouse();

            //if (kb.IsDown(Keys.Down))
                //world.Camera.Zoom -= 0.0005f;
            //else if (kb.IsDown(Keys.Up))
            //    world.Camera.Zoom += 0.0005f;

            int dScroll = mouse.GetDeltaScroll();
            if (dScroll != 0)
            {
                world.GameSpeed = gxtMath.Clamp(world.GameSpeed + (dScroll * 0.05f), 0.05f, 1.0f);
            }

            // assume gravity on
            float force = 10.5f;
            float torque = 3.5f;
            bodyA.IgnoreGravity = false;
            bodyF.IgnoreGravity = false;

            // otherwise switch
            if (!gravityOn)
            {
                bodyA.IgnoreGravity = true;
                bodyF.IgnoreGravity = true;
                force = 6.5f;
                torque = 1.25f;
            }

            if (kb.GetState(Keys.G) == gxtControlState.FIRST_PRESSED)
                gravityOn = !gravityOn;

            if (kb.GetState(Keys.C) == gxtControlState.FIRST_PRESSED)
            {
                geomA.CollisionResponseEnabled = !geomA.CollisionResponseEnabled;
                geomF.CollisionResponseEnabled = !geomF.CollisionResponseEnabled;
            }

            if (kb.GetState(Keys.F) == gxtControlState.FIRST_PRESSED)
            {
                bodyF.FixedRotation = !bodyF.FixedRotation;
                if (!bodyF.FixedRotation)
                {
                    bodyF.Inertia = gxtRigidBody.GetInertiaForPolygon(geomF.Polygon, bodyF.Mass);
                }
            }

            Vector2 virtualMousePos = world.Camera.GetVirtualMousePosition(mouse.GetPosition());
            if (kb.GetState(Keys.T) == gxtControlState.FIRST_PRESSED)
            {
                Vector2 physicsWorldPos = virtualMousePos * gxtPhysicsWorld.ONE_OVER_PHYSICS_SCALE;
                //physicsWorldPos = (world.Camera.Position + physicsWorldPos) * gxtPhysicsWorld.ONE_OVER_PHYSICS_SCALE; // + (physicsWorldPos * gxtPhysicsWorld.ONE_OVER_PHYSICS_SCALE);
                float explosionForce = 750.0f;
                float sphereRadius = 4.5f;
                List<gxtGeom> hitGeoms;
                gxtSphere sphere = new gxtSphere(physicsWorldPos, sphereRadius);
                //gxtAABB box = new gxtAABB(physicsWorldPos, new Vector2(sphereRadius));
                float adjustedRadius = sphereRadius * gxtPhysicsWorld.PHYSICS_SCALE;
                gxtSphere adjustedSphere = new gxtSphere(virtualMousePos, adjustedRadius);
                Color sphereColor = Color.Blue;
                if (world.PhysicsWorld.PointCastAll(physicsWorldPos, out hitGeoms))
                {
                    //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Something was hit...");
                    sphereColor = Color.Red;
                    for (int i = 0; i < hitGeoms.Count; ++i)
                    {
                        if (hitGeoms[i].HasAttachedBody())
                        {
                            Vector2 forceToApply = Vector2.Normalize(hitGeoms[i].GetWorldCentroid() - physicsWorldPos) * explosionForce;
                            hitGeoms[i].RigidBody.ApplyForceAtPoint(forceToApply, physicsWorldPos);
                        }
                    }
                }
                gxtDebugDrawer.Singleton.AddPt(physicsWorldPos * gxtPhysicsWorld.PHYSICS_SCALE, Color.Blue, 0.0f, System.TimeSpan.FromSeconds(3.0));
                gxtDebugDrawer.Singleton.AddSphere(adjustedSphere, sphereColor, 0.5f, TimeSpan.FromSeconds(3.0));
            }
            gxtDebugDrawer.Singleton.AddPt(virtualMousePos, Color.White, 0.0f);

            bodyA.Damping = damping;

            if (kb.IsDown(Keys.Left))
                xA -= force;
            if (kb.IsDown(Keys.Right))
                xA += force;
            if (kb.IsDown(Keys.Up))
                yA -= force;
            if (kb.IsDown(Keys.Down))
                yA += force;
            if (kb.IsDown(Keys.OemOpenBrackets))
                rA -= torque;
            if (kb.IsDown(Keys.OemCloseBrackets))
                rA += torque;

            if (xA != 0.0f || yA != 0.0f)
                bodyA.ApplyForce(new Vector2(xA, yA));
            if (rA != 0.0f)
                bodyA.ApplyTorque(rA);

            float xF = 0.0f, yF = 0.0f, rF = 0.0f;

            if (kb.IsDown(Keys.A))
                xF -= force;
            if (kb.IsDown(Keys.D))
                xF += force;
            if (kb.IsDown(Keys.W))
                yF -= force;
            if (kb.IsDown(Keys.S))
                yF += force;
            if (kb.IsDown(Keys.Q))
                rF -= torque;
            if (kb.IsDown(Keys.E))
                rF += torque;

            if (kb.GetState(Keys.Space) == gxtControlState.FIRST_PRESSED)
            {
                bodyF.Velocity -= new Vector2(0.0f, 8.0f);
                bodyF.ApplyForce(new Vector2(0.0f, -100.0f));
            }

            if (xF != 0.0f || yF != 0.0f)
                bodyF.ApplyForce(new Vector2(xF, yF));
            if (rF != 0.0f)
                bodyF.ApplyTorque(rF);

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Velocity F: {0}", bodyF.Velocity.ToString());
            //bodyA.Translate(new Vector2(xA, yA));
            //bodyA.SetRotation(bodyA.Rotation + rA);

            Vector2 forceBeforeClearA = bodyA.Force;
            //Vector2 forceBeforeClearF = bodyF.Force;

            world.PhysicsWorld.DebugDrawGeoms(Color.Yellow, Color.Salmon, Color.Gray, Color.Blue, Color.Red, 0.5f, 0.55f, 0.5f, true, true, false);
            //world.PhysicsWorld.DebugDrawRigidBodies(Color.White);



            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, bodyA.Acceleration.ToString());

            //float scalar = 0.85f;


            /*
            gxtDebugDrawer.Singleton.AddPolygon(geomA.Polygon, Color.Yellow, 0.5f);
            gxtDebugDrawer.Singleton.AddPolygon(geomB.Polygon, Color.Yellow, 0.5f);
            gxtDebugDrawer.Singleton.AddLine(bodyA.Position, bodyA.Position + forceBeforeClearA * scalar, Color.Blue, 0.0f);
            gxtDebugDrawer.Singleton.AddLine(bodyF.Position, bodyF.Position + forceBeforeClearF * scalar, Color.Blue, 0.0f);
            gxtDebugDrawer.Singleton.AddPolygon(geomC.Polygon, Color.Yellow, 0.0f);
            gxtDebugDrawer.Singleton.AddPolygon(geomD.Polygon, Color.Yellow, 0.0f);
            gxtDebugDrawer.Singleton.AddPolygon(geomE.Polygon, Color.Yellow, 0.0f);
            gxtDebugDrawer.Singleton.AddPolygon(geomF.Polygon, Color.Yellow, 0.0f);
            */
            //gxtDebugDrawer.Singleton.AddPolygon(geomB.Polygon, color, 0.5f);
            //gxtDebugDrawer.Singleton.AddString(geomB.Position.ToString(), geomB.Position, Color.White, 0.0f);

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "{0}", float.Epsilon);

            string fpsString = "FPS: " + gxtDebug.GetFPS().ToString();
            Vector2 strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(fpsString);
            strSize *= 0.5f;
            Vector2 topLeftCorner = new Vector2(-gxtDisplayManager.Singleton.ResolutionWidth * 0.5f, -gxtDisplayManager.Singleton.ResolutionHeight * 0.5f);
            gxtDebugDrawer.Singleton.AddString("FPS: " + gxtDebug.GetFPS(), topLeftCorner + strSize, Color.White, 0.0f);
            
            //gxtDebugDrawer.Singleton.AddString("Velocity: " + bodyF.Velocity.ToString(), Vector2.Zero, Color.White, 0.0f);
            //gxtDebugDrawer.Singleton.AddString("");
            //xna reach profile has no alpha transparency!
            //world.Physics.DebugDrawGeoms(Color.Yellow, 0.5f, new Color(0.0f, 0.0f, 1.0f, 0.25f), 0.51f, true);
            world.Update(gameTime);
            world.LateUpdate(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            world.Draw(Root.GraphicsBatch);
        }
    }

}
