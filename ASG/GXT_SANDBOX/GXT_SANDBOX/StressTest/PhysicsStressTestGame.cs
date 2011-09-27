using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Physics;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PhysicsStressTestGame : gxtGame
    {
        gxtGeom playerGeom;
        gxtRigidBody playerBody;
        private List<gxtGeom> geoms;
        private List<gxtRigidBody> bodies;
        
        bool gravityOn = false;
        gxtWorld world;
        int debugDrawerId;

        Vector2 mouseDownPt;

        public PhysicsStressTestGame()
            : base()
        {
        }

        public bool OnCollision(gxtGeom geomA, gxtGeom geomB, gxtCollisionResult collisionResult)
        {
            float scalar = 35.0f;
            gxtDebugDrawer.Singleton.AddLine(geomA.Position, geomA.Position + collisionResult.Normal * scalar, Color.Orange, 0.0f);
            gxtDebugDrawer.Singleton.AddLine(geomB.Position, geomB.Position + collisionResult.Normal * scalar, Color.Orange, 0.0f);
            return true;
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
            IsMouseVisible = true;
            Root.DisplayManager.WindowTitle = "Physics Stress Test";
            //world.Load();
            Root.DisplayManager.RegisterCamera(world.Camera);
            //Root.DisplayManager.SetResolution(1280, 720, false);
            world.Camera.Zoom = 0.0f;
            InitGeoms();
            InitBodies();
            //gxtSolver.frictionType = gxtFrictionType.MINIMUM;
            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                debugDrawerId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawerId, world.SceneGraph);
                //gxtDebugDrawer.Singleton.SetTargetDrawManager(world.DrawManager);
                gxtDebugDrawer.Singleton.DebugFont = Content.Load<SpriteFont>("Fonts\\debug_font");
            }
        }

        private void InitGeoms()
        {
            geoms = new List<gxtGeom>();

            playerGeom = new gxtGeom();
            playerGeom.CollisionEnabled = true;
            playerGeom.CollidesWithGroups = gxtCollisionGroup.ALL;
            playerGeom.CollisionGroups = gxtCollisionGroup.ALL;
            playerGeom.CollisionResponseEnabled = true;
            playerGeom.RigidBody = null;
            playerGeom.Tag = null;
            gxtPolygon worldPolyA = gxtGeometry.CreateCirclePolygon(100, 9);
            playerGeom.Polygon = worldPolyA;
            playerGeom.SetPosition(new Vector2(150, -200));
            playerGeom.OnCollision += OnCollision;

            gxtGeom floorGeom = new gxtGeom();
            floorGeom.CollisionEnabled = true;
            floorGeom.CollidesWithGroups = gxtCollisionGroup.ALL;
            floorGeom.CollisionGroups = gxtCollisionGroup.ALL;
            floorGeom.CollisionResponseEnabled = true;
            floorGeom.RigidBody = null;
            floorGeom.Tag = null;
            gxtPolygon floorPoly = gxtGeometry.CreateRectanglePolygon(750, 30);
            floorGeom.Polygon = floorPoly;
            floorGeom.SetPosition(new Vector2(0, 200));
            floorGeom.OnCollision += OnCollision;
            floorGeom.Material = new gxtPhysicsMaterial(0.7f, 0.5f);

            world.AddGeom(playerGeom);
            world.AddGeom(floorGeom);
            geoms.Add(floorGeom);
        }

        private void InitBodies()
        {
            bodies = new List<gxtRigidBody>();

            playerBody = new gxtRigidBody();
            playerBody.CanSleep = true;
            playerBody.Awake = true;
            playerBody.IgnoreGravity = false;
            playerBody.MotionType = gxtRigidyBodyMotion.DYNAMIC;
            playerBody.Mass = 3.0f;
            playerBody.Inertia = gxtRigidBody.GetInertiaForRectangle(playerGeom.LocalAABB.Width, playerGeom.LocalAABB.Height, playerBody.Mass);
            playerGeom.RigidBody = playerBody;
            playerBody.Position = new Vector2(150, -100);
            playerBody.Damping = 1.0f;

            gxtRigidBody floorBody = new gxtRigidBody();
            floorBody.CanSleep = true;
            floorBody.Awake = false;
            floorBody.IgnoreGravity = true;
            floorBody.MotionType = gxtRigidyBodyMotion.FIXED;
            floorBody.Mass = 20.0f;
            floorBody.Inertia = gxtRigidBody.GetInertiaForRectangle(playerGeom.LocalAABB.Width, playerGeom.LocalAABB.Height, playerBody.Mass);
            geoms[0].RigidBody = floorBody;
            floorBody.Position = new Vector2(0, 200);
            floorBody.Damping = 1.0f;
            //floorBody.Material = new gxtPhysicsMaterial(0.65f, 0.1f);

            world.AddRigidBody(playerBody);
            world.AddRigidBody(floorBody);
            bodies.Add(floorBody);
        }

        private void CreateGeom(Vector2 min, Vector2 max, float density)
        {
            gxtGeom g = new gxtGeom();
            g.CollidesWithGroups = gxtCollisionGroup.ALL;
            g.CollisionEnabled = true;
            g.CollisionGroups = gxtCollisionGroup.ALL;
            g.CollisionResponseEnabled = true;
            g.Polygon = gxtGeometry.CreateRectanglePolygon(max.X - min.X, max.Y - min.Y);
            g.Material = new gxtPhysicsMaterial(0.85f, 0.45f);

            gxtRigidBody b = new gxtRigidBody();
            b.IgnoreGravity = false;
            b.Mass = density * g.Polygon.GetArea();
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "mass: {0}", b.Mass);
            b.Inertia = gxtRigidBody.GetInertiaForRectangle(max.X - min.X, max.Y - min.Y, b.Mass);
            b.MotionType = gxtRigidyBodyMotion.DYNAMIC;
            //b.Material = new gxtPhysicsMaterial(0.85f, 0.45f);
            b.Position = ((min + max) * 0.5f);
            b.Damping = 0.9995f;

            g.RigidBody = b;

            geoms.Add(g);
            bodies.Add(b);
            world.AddGeom(g);
            world.AddRigidBody(b);
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
            world.Initialize();
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

            // assume gravity on
            float force = 2000;
            float torque = 2000.0f;
            playerBody.IgnoreGravity = false;

            // otherwise switch
            if (!gravityOn)
            {
                playerBody.IgnoreGravity = true;
                force = 150.0f;
                torque = 250.0f;
            }

            if (kb.GetState(Keys.G) == gxtControlState.FIRST_PRESSED)
                gravityOn = !gravityOn;

            Vector2 mousePos = world.Camera.GetVirtualMousePosition(mouse.GetPosition());
            if (mouse.GetState(gxtMouseButton.LEFT) == gxtControlState.FIRST_PRESSED)
            {
                mouseDownPt = mousePos;
            }
            else if (mouse.GetState(gxtMouseButton.LEFT) == gxtControlState.DOWN)
            {
                gxtDebugDrawer.Singleton.AddAABB(mouseDownPt, mousePos, Color.Gray, 0.0f);
            }
            if (mouse.GetState(gxtMouseButton.LEFT) == gxtControlState.FIRST_RELEASED)
            {
                Vector2 mouseUpPoint = mousePos;
                CreateGeom(mouseDownPt, mouseUpPoint, 0.000001f);
            }

            /*
            if (kb.GetState(Keys.D) == gxtControlState.FIRST_PRESSED)
            {
                if (damping == 1.0f)
                    damping = 0.9f;
                else
                    damping = 1.0f;
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Damping Value Set To: {0}", damping);
            }
            */

            //bodyA.Damping = damping;

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
                playerBody.ApplyForce(new Vector2(xA, yA));
            if (rA != 0.0f)
                playerBody.ApplyTorque(rA);
            //bodyA.Translate(new Vector2(xA, yA));
            //bodyA.SetRotation(bodyA.Rotation + rA);

            Vector2 forceBeforeClear = playerBody.Force;

            world.Update(gameTime);

            gxtDebugDrawer.Singleton.AddPolygon(playerGeom.Polygon, Color.Yellow, 0.5f);

            for (int i = 0; i < geoms.Count; i++)
            {
                gxtDebugDrawer.Singleton.AddPolygon(geoms[i].Polygon, Color.Yellow, 0.05f);
            }
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, bodyA.Acceleration.ToString());

            /*
            Color color = Color.Yellow;
            if (gxtGeom.Intersects(geomA, geomB))
                color = Color.Red;
            float scalar = 0.85f;
            gxtDebugDrawer.Singleton.AddPolygon(geomA.Polygon, color, 0.5f);
            gxtDebugDrawer.Singleton.AddPolygon(geomB.Polygon, color, 0.5f);
            gxtDebugDrawer.Singleton.AddLine(bodyA.Position, bodyA.Position + forceBeforeClear * scalar, Color.Blue, 0.0f);
            gxtDebugDrawer.Singleton.AddPolygon(geomC.Polygon, Color.Yellow, 0.0f);
            gxtDebugDrawer.Singleton.AddPolygon(geomD.Polygon, Color.Yellow, 0.0f);
            gxtDebugDrawer.Singleton.AddPolygon(geomE.Polygon, Color.Yellow, 0.0f);
            */
            //gxtDebugDrawer.Singleton.AddPolygon(geomB.Polygon, color, 0.5f);
            //gxtDebugDrawer.Singleton.AddString(geomB.Position.ToString(), geomB.Position, Color.White, 0.0f);

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "{0}", float.Epsilon);

            string fpsString = "FPS: " + gxtDebug.GetFPS().ToString();
            Vector2 strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(fpsString);
            strSize *= 0.5f;
            Vector2 topLeftCorner = new Vector2(-gxtDisplayManager.Singleton.ResolutionWidth * 0.5f, -gxtDisplayManager.Singleton.ResolutionHeight * 0.5f);
            gxtDebugDrawer.Singleton.AddString("FPS: " + gxtDebug.GetFPS(), topLeftCorner + strSize, Color.White, 0.0f);
            //gxtDebugDrawer.Singleton.AddString("");
            //xna reach profile has no alpha transparency!
            //world.Physics.DebugDrawGeoms(Color.Yellow, 0.5f, new Color(0.0f, 0.0f, 1.0f, 0.25f), 0.51f, true);
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
