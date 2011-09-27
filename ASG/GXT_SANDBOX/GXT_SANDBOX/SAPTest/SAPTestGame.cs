#define RAY_CAST_TEST
//#undef RAY_CAST_TEST
using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Physics;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SAPTestGame : gxtGame
    {
        List<gxtISceneNode> nodes;
        gxtSortAndSweepCollider<gxtISceneNode> drawableCollider;
        // we will debug draw the extents
        int debugDrawerId;
        gxtSceneGraph sceneGraph;
        gxtCamera camera;

        const int NUM_RECTANGLES = 10;
        bool drawBoundingBoxes = false;
        bool drawIntervals = false;

        gxtISceneNode currentSelection = null;



        public SAPTestGame() : base()
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
            IsMouseVisible = true;
            nodes = new List<gxtISceneNode>();
            camera = new gxtCamera(Vector2.Zero, 0.0f, 0.0f, false);
            gxtDisplayManager.Singleton.RegisterCamera(camera);
            gxtDisplayManager.Singleton.SetResolution(800, 600, false);
            Root.DisplayManager.WindowTitle = "Sweep and Prune TestBed";
            sceneGraph = new gxtSceneGraph();
            sceneGraph.Initialize();
            //drawManager.Initialize();
            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                debugDrawerId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawerId, sceneGraph);
                gxtDebugDrawer.Singleton.DebugFont = Content.Load<SpriteFont>("Fonts\\debug_font");
                gxtDebugDrawer.Singleton.FillGeometry = false;
            }
            drawableCollider = new gxtSortAndSweepCollider<gxtISceneNode>();
            drawableCollider.Initialize();
            InitializeRectangles();
        }

        private void InitializeRectangles()
        {
            float spacing = 100.0f;
            //float sign = 1.0f;
            int verticalVariation = 35;

            gxtRandom rng = new gxtRandom();

            gxtISceneNode node;
            gxtRectangle rect = new gxtRectangle(65, 35);
            
            for (int i = 0; i < NUM_RECTANGLES; i++)
            {
                //gxtIDrawable drawable = new gxtDrawable(Color.Blue);
                //drawable.Entity = rect;

                float x = (float)(spacing * i);
                float y = rng.Next(-verticalVariation, verticalVariation);
                float rot = (float)rng.NextFloat() * gxtMath.TWO_PI;

                node = new gxtSceneNode();
                node.Position = new Vector2(x, y);
                node.Rotation = rot;
                
                //node.ColorOverlay = Color.Blue;
                //node.AttachDrawable(drawable);
                sceneGraph.AddNode(node);
                gxtAABB aabb = node.GetAABB();
                nodes.Add(node);
                drawableCollider.AddObject(node, ref aabb);
                //gxtRectangle rect = new gxtRectangle(new Vector2(x, y), new Vector2(65, 35), Color.Blue, 0.5f);
                //rect.Rotation = rot;
                //drawables.Add(rect);
                //drawManager.Add(rect);
                //gxtAABB rectAABB = rect.GetAABB();
                //drawableCollider.AddObject(rect, ref rectAABB);
            }
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
            gxtDebugDrawer.Singleton.CurrentSceneId = debugDrawerId;

            foreach (gxtISceneNode r in nodes)
            {
                r.SetColor(Color.Blue);
            }

            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            float tx = 0.0f, ty = 0.0f;
            float cameraSpeed = 3.0f;
            if (kb.IsDown(Keys.A))
                tx -= cameraSpeed;
            if (kb.IsDown(Keys.W))
                ty -= cameraSpeed;
            if (kb.IsDown(Keys.D))
                tx += cameraSpeed;
            if (kb.IsDown(Keys.S))
                ty += cameraSpeed;
            if (kb.IsDown(Keys.Q))
                camera.Rotation -= 0.005f;
            if (kb.IsDown(Keys.E))
                camera.Rotation += 0.005f;
            if (kb.IsDown(Keys.Z))
                camera.Zoom += 0.01f;
            if (kb.IsDown(Keys.C))
                camera.Zoom = gxtMath.Min(gxtCamera.MIN_CAMERA_SCALE, camera.Zoom - 0.01f);
            if (kb.GetState(Keys.Tab) == gxtControlState.FIRST_PRESSED)
                gxtDisplayManager.Singleton.SetResolution(1280, 720, false);
            if (kb.GetState(Keys.CapsLock) == gxtControlState.FIRST_PRESSED)
                gxtDisplayManager.Singleton.SetResolution(800, 600, false);
                
            if (kb.GetState(Keys.T) == gxtControlState.FIRST_PRESSED)
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, drawableCollider.DebugTrace());
            if (kb.GetState(Keys.I) == gxtControlState.FIRST_PRESSED)
                drawIntervals = !drawIntervals;
            if (kb.GetState(Keys.B) == gxtControlState.FIRST_PRESSED)
                drawBoundingBoxes = !drawBoundingBoxes;
            if (kb.GetState(Keys.P) == gxtControlState.FIRST_PRESSED)
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, drawableCollider.DebugTracePairs());

            camera.TranslateLocal(tx, ty);

            gxtOBB camOBB = camera.GetViewOBB();
            camOBB.Extents *= 0.95f;

            gxtAABB camAABB = camera.GetViewAABB();
            camAABB.Extents *= 0.95f;

            gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Zoom: {0}", camera.Zoom); 
            gxtDebugDrawer.Singleton.AddOBB(camOBB, Color.Yellow, 0.0f);
            gxtDebugDrawer.Singleton.AddAABB(camAABB, Color.Red, 0.0f);
            gxtDebugDrawer.Singleton.AddAxes(camera.Position, camera.Rotation, Color.Red, Color.Green, 0.0f);
            //gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Camera Rotation: {0}", camera.Rotation.ToString());
            gxtAABB cameraAABB = camera.GetViewAABB();
            float cameraYBoundary = cameraAABB.Max.Y;
            float cameraXBoundary = cameraAABB.Max.X;

            gxtMouse mouse = gxtMouseManager.Singleton.GetMouse();
            Vector2 virtualMousePos = camera.GetVirtualMousePosition(mouse.GetPosition());
            gxtDebugDrawer.Singleton.AddPt(virtualMousePos, Color.Yellow, 0.0f);
            if (mouse.GetState(gxtMouseButton.LEFT) == gxtControlState.FIRST_PRESSED)
            {
                foreach (gxtISceneNode drawable in nodes)
                {
                    gxtAABB aabb = drawable.GetAABB();
                    if (aabb.Contains(virtualMousePos))
                    {
                        currentSelection = drawable;
                        currentSelection.SetColor(Color.Green);
                        //currentSelection.ColorOverlay = Color.Green;
                        //currentSelection = drawable as gxtRectangle;
                        //currentSelection.ColorOverlay = Color.Green;
                    }
                }
            }
            else if (mouse.GetState(gxtMouseButton.LEFT) == gxtControlState.DOWN)
            {
                if (currentSelection != null)
                {
                    if (kb.GetState(Keys.Delete) == gxtControlState.FIRST_PRESSED)
                    {
                        drawableCollider.RemoveObject(currentSelection);
                        sceneGraph.RemoveNode(currentSelection);
                        nodes.Remove(currentSelection);
                        currentSelection = null;
                        //drawManager.Remove(currentSelection);
                        //drawables.Remove(currentSelection);
                        //currentSelection = null;
                    }
                    else
                    {
                        Vector2 prevVirtualMousePos = camera.GetVirtualMousePosition(new Vector2(mouse.PrevState.X, mouse.PrevState.Y));
                        Vector2 d = virtualMousePos - prevVirtualMousePos;
                        currentSelection.Position += d;
                        gxtAABB aabb = currentSelection.GetAABB();
                        drawableCollider.UpdateObject(currentSelection, ref aabb);
                        currentSelection.SetColor(Color.Green);
                        //currentSelection.ColorOverlay = Color.Green;
                    }
                }

            }
            else if (mouse.IsUp(gxtMouseButton.LEFT))
            {
                if (currentSelection != null)
                {
                    currentSelection.SetColor(Color.Blue);
                    currentSelection = null;
                }
            }


            
            foreach (gxtISceneNode drawable in nodes)
            {
                if (drawBoundingBoxes)
                    gxtDebugDrawer.Singleton.AddAABB(drawable.GetAABB(), new Color(255, 0, 0, 100), 1.0f);
            }

            if (drawIntervals)
                drawableCollider.DebugDraw(Color.White, Color.Red, cameraYBoundary, cameraXBoundary);

            #if RAY_CAST_TEST
            Vector2 rayOrigin = new Vector2(-100.0f, 0.0f);
            Vector2 direction = virtualMousePos - rayOrigin;
            direction.Normalize();

            gxtRay ray = new gxtRay(rayOrigin, direction);
            List<gxtISceneNode> rayHits = drawableCollider.RayCastAll(ray);
            if (rayHits.Count != 0)
            {
                for (int i = 0; i < rayHits.Count; i++)
                {
                    rayHits[i].SetColor(Color.Orange, false);
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Num Ray Hits: {0}", rayHits.Count);
                    //gxtRectangle rect = rayHits[i] as gxtRectangle;
                    //rect.ColorOverlay = Color.Orange;
                }
                gxtDebugDrawer.Singleton.AddRay(ray, float.MaxValue, Color.Green, 0.0f);
            }
            else
            {
                gxtDebugDrawer.Singleton.AddRay(ray, Color.Green, 0.0f);
            }
            #else
            gxtBroadphaseCollisionPair<gxtISceneNode>[] rectPairs = drawableCollider.GetCollisionPairs();
            foreach (gxtBroadphaseCollisionPair<gxtISceneNode> rectPair in rectPairs)
            {
                rectPair.objA.ColorOverlay = Color.Orange;
                rectPair.objB.ColorOverlay = Color.Orange;
                gxtDebugDrawer.Singleton.AddPt(rectPair.objA.GetAABB().Position, Color.Yellow, 0.0f);
            }
            #endif


            //drawManager.Update();
            // fps at top left
            /*
            string fpsString = "FPS: " + gxtDebug.GetFPS().ToString();
            Vector2 strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(fpsString);
            strSize *= 0.5f;
            Vector2 topLeftCorner = new Vector2(-gxtDisplayManager.Singleton.WindowWidth * 0.5f, -gxtDisplayManager.Singleton.WindowHeight * 0.5f);
            topLeftCorner += camera.Position;
            gxtDebugDrawer.Singleton.AddString("FPS: " + gxtDebug.GetFPS(), topLeftCorner + strSize, Color.White, 0.0f);
            */
            sceneGraph.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            gxtSpriteBatch spriteBatch = gxtRoot.Singleton.SpriteBatch;
            spriteBatch.Begin(camera.GetTransform());
            //drawManager.Draw(ref spriteBatch, camera.GetViewAABB());
            //sceneGraph.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
