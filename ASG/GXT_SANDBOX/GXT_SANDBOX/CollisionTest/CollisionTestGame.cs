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
    public class CollisionTestGame : gxtGame
    {
        gxtGeom geomA;
        gxtGeom geomB;
        //gxtRigidBody bodyA;
        //gxtRigidBody bodyB;
        gxtWorld world;
        int debugDrawerId;

        public CollisionTestGame()
            : base()
        {
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
            Root.DisplayManager.WindowTitle = "GJK Collision Test";
            //world.Load();
            InitGeoms();
            //InitBodies();
            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                debugDrawerId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.CurrentSceneId = debugDrawerId;
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawerId, world.SceneGraph);
                //gxtDebugDrawer.Singleton.SetTargetDrawManager(world.DrawManager);
                gxtDebugDrawer.Singleton.DebugFont = Content.Load<SpriteFont>("Fonts\\debug_font");
                //gxtDebugDrawer.Singleton.SetDebugFont(Root);
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
            gxtPolygon worldPolyA = gxtGeometry.CreateCirclePolygon(100, 6);
            geomA.Polygon = worldPolyA;
            geomA.SetPosition(new Vector2(150, -200));

            geomB = new gxtGeom();
            geomB.CollisionEnabled = true;
            geomB.CollidesWithGroups = gxtCollisionGroup.ALL;
            geomB.CollisionGroups = gxtCollisionGroup.ALL;
            geomB.CollisionResponseEnabled = true;
            geomB.RigidBody = null;
            geomB.Tag = null;
            gxtPolygon worldPolyB = gxtGeometry.CreateCirclePolygon(75, 12);
            geomB.Polygon = worldPolyB;
            geomB.SetPosition(new Vector2(0, 0));

            world.AddGeom(geomA);
            world.AddGeom(geomB);
        }

        /*
        private void InitBodies()
        {
            bodyA = new gxtRigidBody();
            bodyA.CanSleep = false;
            bodyA.Awake = true;
            bodyA.IgnoreGravity = true;
            bodyA.MotionType = gxtRigidyBodyMotion.FIXED;
            bodyA.Mass = 5.0f;
            geomA.RigidBody = bodyA;
            bodyA.SetPosition(new Vector2(150, -100));

            bodyB = new gxtRigidBody();
            bodyB.CanSleep = false;
            bodyB.Awake = true;
            bodyB.IgnoreGravity = true;
            bodyB.MotionType = gxtRigidyBodyMotion.FIXED;
            bodyB.Mass = 5.0f;
            geomB.RigidBody = bodyB;
            bodyB.SetPosition(new Vector2(-150, 125));
        }
        */

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

            world.Update(gameTime);


            float xA = 0.0f, yA = 0.0f, rA = 0.0f;
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();

            if (kb.GetState(Keys.C) == gxtControlState.FIRST_PRESSED)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "first pressed");
                if (geomA.CollisionResponseEnabled)
                {
                    geomA.CollisionResponseEnabled = false;
                    geomB.CollisionResponseEnabled = false;
                }
                else
                {
                    geomA.CollisionResponseEnabled = true;
                    geomB.CollisionResponseEnabled = true;
                }
            }
            if (kb.IsDown(Keys.Left))
                xA -= 3.0f;
            if (kb.IsDown(Keys.Right))
                xA += 3.0f;
            if (kb.IsDown(Keys.Up))
                yA -= 3.0f;
            if (kb.IsDown(Keys.Down))
                yA += 3.0f;
            if (kb.IsDown(Keys.OemOpenBrackets))
                rA -= 0.05f;
            if (kb.IsDown(Keys.OemCloseBrackets))
                rA += 0.05f;

            geomA.Translate(new Vector2(xA, yA));
            geomA.SetRotation(geomA.Rotation + rA);

            float xB = 0.0f, yB = 0.0f, rB = 0.0f;
            if (kb.IsDown(Keys.A))
                xB -= 3.0f;
            if (kb.IsDown(Keys.D))
                xB += 3.0f;
            if (kb.IsDown(Keys.W))
                yB -= 3.0f;
            if (kb.IsDown(Keys.S))
                yB += 3.0f;
            if (kb.IsDown(Keys.Q))
                rB -= 0.05f;
            if (kb.IsDown(Keys.E))
                rB += 0.05f;

            geomB.Translate(new Vector2(xB, yB));
            geomB.SetRotation(geomB.Rotation + rB);


            Color color = Color.Yellow;
            if (gxtGeom.Intersects(geomA, geomB))
                color = Color.Red;

            Vector2 cpA, cpB;
            Vector2 nd = geomB.Position - geomA.Position;
            gxtPolygon polyA = geomA.Polygon;
            gxtPolygon polyB = geomB.Polygon;
            float dist = gxtGJKCollider.Distance(ref polyA, geomA.Position, ref polyB, geomB.Position, out cpA, out cpB);
            //gxtGJKCollider.D
            gxtDebugDrawer.Singleton.PtSize = 5.0f;
            gxtDebugDrawer.Singleton.AddPt(cpA, Color.White, 0.0f);
            gxtDebugDrawer.Singleton.AddPt(cpB, Color.White, 0.0f);


            gxtDebugDrawer.Singleton.AddPolygon(geomA.Polygon, color, 0.5f);
            gxtDebugDrawer.Singleton.AddPolygon(geomB.Polygon, color, 0.5f);

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "{0}", float.Epsilon);

            string fpsString = "FPS: " + gxtDebug.GetFPS().ToString();
            Vector2 strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(fpsString);
            strSize *= 0.5f; 
            Vector2 topLeftCorner = new Vector2(-gxtDisplayManager.Singleton.ResolutionWidth * 0.5f, -gxtDisplayManager.Singleton.ResolutionHeight * 0.5f);
            //gxtDebugDrawer.Singleton.AddString("FPS: " + gxtDebug.GetFPS(), topLeftCorner + strSize, Color.White, 0.0f);
            string infoString = "Use WASD/Arrow Keys To Move Polygons";
            topLeftCorner += new Vector2(0.0f, strSize.Y);
            strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(infoString);
            strSize *= 0.5f;
            //gxtDebugDrawer.Singleton.AddString(infoString, topLeftCorner + strSize, Color.White, 0.0f);
            string infoString2 = "Press C to enable/disable simple collsion response";
            topLeftCorner += new Vector2(0.0f, strSize.Y);
            strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(infoString2);
            strSize *= 0.5f;

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, gxtDebugDrawer.Singleton.NumDebugNodes);
            //gxtDebugDrawer.Singleton.AddString(infoString2, topLeftCorner + strSize, Color.White, 0.0f);


            //gxtDebugDrawer.Singleton.AddString("");
            //xna reach profile has no alpha transparency!
            //world.Physics.DebugDrawGeoms(Color.Yellow, 0.5f, new Color(0.0f, 0.0f, 1.0f, 0.25f), 0.51f, true);
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
