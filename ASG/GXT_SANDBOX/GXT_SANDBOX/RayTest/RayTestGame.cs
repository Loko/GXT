#define NARROWPHASE_TEST
#define BROADPHASE_TEST
//#undef NARROWPHASE_TEST
#undef BROADPHASE_TEST
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
    public class RayTestGame : gxtGame
    {
        gxtGeom geomA, geomB;
        gxtRigidBody bodyA, bodyB;
        gxtWorld world;
        Vector2 rayOrigin;
        int sceneId;

        public RayTestGame()
            : base()
        {
        }

        public bool OnCollision(gxtGeom geomA, gxtGeom geomB, gxtCollisionResult collisionResult)
        {
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
            Root.DisplayManager.WindowTitle = "RayCast Testing";
            rayOrigin = new Vector2(-225, -100);
            //world.Load();
            InitGeoms();
            InitBodies();
            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                sceneId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(sceneId, world.SceneGraph);
                gxtDebugDrawer.Singleton.CurrentSceneId = sceneId;
                //gxtDebugDrawer.Singleton.SetTargetDrawManager(world.DrawManager);
                gxtDebugDrawer.Singleton.DebugFont = Content.Load<SpriteFont>("Fonts\\debug_font");
            }
        }

        private void InitGeoms()
        {
            geomA = new gxtGeom();
            geomA.CollisionEnabled = true;
            geomA.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomA.CollisionGroups = gxtCollisionGroup.ALL;
            geomA.CollisionResponseEnabled = true;
            geomA.RigidBody = null;
            geomA.Tag = null;
            gxtPolygon worldPolyA = gxtGeometry.CreateCirclePolygon(100, 9);
            geomA.Polygon = worldPolyA;
            geomA.SetPosition(new Vector2(150, -200));
            geomA.OnCollision += OnCollision;


            geomB = new gxtGeom();
            geomB.CollisionEnabled = true;
            geomB.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomB.CollisionGroups = gxtCollisionGroup.ALL;
            geomB.CollisionResponseEnabled = true;
            geomB.RigidBody = null;
            geomB.Tag = null;
            gxtPolygon worldPolyB = gxtGeometry.CreateRectanglePolygon(800, 50);
            geomB.Polygon = worldPolyB;
            geomB.SetPosition(new Vector2(0, 200));

            world.AddGeom(geomA);
            world.AddGeom(geomB);
        }

        private void InitBodies()
        {
            bodyA = new gxtRigidBody();
            bodyA.CanSleep = true;
            bodyA.Awake = true;
            bodyA.IgnoreGravity = true;
            bodyA.MotionType = gxtRigidyBodyMotion.DYNAMIC;
            bodyA.Mass = 1.0f;
            bodyA.Inertia = gxtRigidBody.GetInertiaForRectangle(geomA.LocalAABB.Width, geomA.LocalAABB.Height, bodyA.Mass);
            geomA.RigidBody = bodyA;
            bodyA.Position = new Vector2(150, -100);


            bodyB = new gxtRigidBody();
            bodyB.CanSleep = false;
            bodyB.Awake = true;
            bodyB.IgnoreGravity = true;
            bodyB.MotionType = gxtRigidyBodyMotion.FIXED;
            bodyB.Mass = 5.0f;
            geomB.RigidBody = bodyB;
            bodyB.Position = new Vector2(0, 250);

            world.AddRigidBody(bodyA);
            world.AddRigidBody(bodyB);
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

            // assume gravity off
            float force = 150;
            float torque = 300.0f;

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
            //bodyA.Translate(new Vector2(xA, yA));
            //bodyA.SetRotation(bodyA.Rotation + rA);

            Vector2 forceBeforeClear = bodyA.Force;

            world.Update(gameTime);

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, bodyA.Acceleration.ToString());


            Color geomAColor = Color.Yellow;
            Color geomBColor = Color.Yellow;

            gxtRay ray = new gxtRay();
            ray.Origin = rayOrigin;


            if (kb.GetState(Keys.C) == gxtControlState.FIRST_PRESSED)
            {
                if (rayOrigin == new Vector2(-225, -100))
                    rayOrigin = new Vector2(250, 200);
                else
                    rayOrigin = new Vector2(-225, -100);
            }

            Vector2 d = world.Camera.GetVirtualMousePosition(mouse.GetPosition()) - ray.Origin;
            ray.Direction = Vector2.Normalize(d);

            gxtDebugDrawer.Singleton.PtSize = 10.0f;
            gxtDebugDrawer.Singleton.AddPt(ray.Origin, Color.Green, 0.0f);
            //gxtDebugDrawer.Singleton.AddLine(ray.Origin, ray.Origin + d, Color.Green, 0.0f);
            //gxtDebugDrawer.Singleton.AddRay(ray, Color.Green, 0.0f);
            float t = 0.0f;

            #if NARROWPHASE_TEST
            Vector2 polyIntersectionPt;
            Vector2 polyRayNormal;
            if (gxtGJKCollider.RayCast(ray, geomA.Polygon, float.MaxValue, out t, out polyIntersectionPt, out polyRayNormal))
            {
                geomAColor = Color.Red;
                gxtDebugDrawer.Singleton.AddLine(polyIntersectionPt, polyIntersectionPt + polyRayNormal * 50, Color.Gray, 0.0f);
                gxtDebugDrawer.Singleton.AddPt(polyIntersectionPt, Color.YellowGreen, 0.0f);
                gxtDebugDrawer.Singleton.AddRay(ray, t, Color.Green, 0.0f, System.TimeSpan.Zero);
                //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "t: {0}", t);
            }
            else
            {
                gxtDebugDrawer.Singleton.AddRay(ray, Color.Green, 0.0f);
            }
            #elif BROADPHASE_TEST
            Vector2 aabbIntersectionPt;
            if (ray.IntersectsAABB(geomA.AABB, out t, out aabbIntersectionPt, float.MaxValue, true))
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "aabb intersection");
                gxtDebugDrawer.Singleton.AddRay(ray, t, Color.Green, 0.0f, System.TimeSpan.Zero);
                gxtDebugDrawer.Singleton.AddPt(aabbIntersectionPt, Color.YellowGreen, 0.0f);
            }
            else
            {
                gxtDebugDrawer.Singleton.AddRay(ray, Color.Green, 0.0f);
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "no intersection");
            }

            Color aabbColor = new Color(100, 0, 0, 100); //new Color(1.0f, 0.0f, 0.0f, 1.0f);
            gxtDebugDrawer.Singleton.AddAABB(geomA.AABB, aabbColor, 1.0f);


            #endif



            gxtDebugDrawer.Singleton.AddPolygon(geomA.Polygon, geomAColor, 0.5f);
            gxtDebugDrawer.Singleton.AddPolygon(geomB.Polygon, geomBColor, 0.5f);

            float scalar = 0.85f;
            gxtDebugDrawer.Singleton.AddLine(bodyA.Position, bodyA.Position + forceBeforeClear * scalar, Color.Blue, 0.0f);
            //gxtDebugDrawer.Singleton.AddPolygon(geomB.Polygon, color, 0.5f);
            //gxtDebugDrawer.Singleton.AddString(geomB.Position.ToString(), geomB.Position, Color.White, 0.0f);

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "{0}", float.Epsilon);

            string fpsString = "FPS: " + gxtDebug.GetFPS().ToString();
            Vector2 strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(fpsString);
            strSize *= 0.5f;
            Vector2 topLeftCorner = new Vector2(-gxtDisplayManager.Singleton.ResolutionWidth * 0.5f, -gxtDisplayManager.Singleton.ResolutionHeight * 0.5f);
            gxtDebugDrawer.Singleton.AddString("FPS: " + gxtDebug.GetFPS(), topLeftCorner + strSize, Color.White, 0.0f);
            topLeftCorner += new Vector2(0.0f, strSize.Y);
            string infoString = "Use Arrow Keys To Move The Polygon";
            strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(infoString);
            strSize *= 0.5f;
            gxtDebugDrawer.Singleton.AddString(infoString, topLeftCorner + strSize + new Vector2(0, 1.0f), Color.White, 0.0f);
            topLeftCorner += new Vector2(0.0f, strSize.Y);
            string infoString2 = "Use Mouse To Aim the Ray, C To Change Its Position";
            strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(infoString2);
            strSize *= 0.5f;
            gxtDebugDrawer.Singleton.AddString(infoString2, topLeftCorner + strSize + new Vector2(0, 2.0f), Color.White, 0.0f);
            //topLeftCorner += new Vector2(0.0f, gxtDebugDrawer.Singleton.DebugFont.MeasureString("FPS").Y);
            //gxtDebugDrawer.Singleton.AddString("Use Mouse To Aim the Ray", topLeftCorner, Color.White, 0.0f);
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
