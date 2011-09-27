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
    public class SceneGraphTestGame : gxtGame
    {
        gxtCamera camera;
        gxtSceneGraph sceneGraph;

        gxtISceneNode baseNode;
        gxtISceneNode childNode;
        gxtISceneNode childNode2;

        bool baseNodeVisible = true;

        gxtTransformSpace childTransformSpace = gxtTransformSpace.LOCAL;

        //float worldRotation = 0;
        Vector2 prevVirtualMousePos = Vector2.Zero;

        public SceneGraphTestGame()
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
            IsMouseVisible = true;
            camera = new gxtCamera(Vector2.Zero, 0.0f, 0.0f, false);
            gxtDisplayManager.Singleton.WindowTitle = "Scene Graph Test";
            //gxtDisplayManager.Singleton.SetResolution(800, 600, false);
            sceneGraph = new gxtSceneGraph();
            sceneGraph.Initialize();

            childNode2 = new gxtSceneNode();
            baseNode = new gxtSceneNode();
            childNode = new gxtSceneNode();

            /*
            gxtIDrawable blueRectDrawable = new gxtDrawable(Color.Blue);
            gxtRectangle blueRect = new gxtRectangle(200.0f, 100.0f);
            blueRectDrawable.Entity = blueRect;

            gxtIDrawable yellowRectDrawable = new gxtDrawable(Color.Yellow, true, 0.45f);
            gxtRectangle yellowRect = new gxtRectangle(85.0f, 45.0f);
            yellowRectDrawable.Entity = yellowRect;


            gxtIDrawable grassDrawable = new gxtDrawable(new Color(255, 255, 255, 255), true, 0.0f);
            gxtPolygon grassPoly = gxtGeometry.CreateRectanglePolygon(150, 150);
            gxtDrawablePolygon grassPolygon = new gxtDrawablePolygon(grassPoly.v);
            Texture2D grassTexture = Content.Load<Texture2D>("Textures\\grass");
            grassPolygon.SetupTexture(grassTexture, gxtTextureCoordinateType.CLAMP);
            grassDrawable.Entity = grassPolygon;

            gxtIDrawable gridDrawable = new gxtDrawable(new Color(30, 100, 255, 255), true, 0.5f);
            Texture2D gridTexture = Content.Load<Texture2D>("Textures\\grid");
            gxtPolygon gridPoly = gxtGeometry.CreateRectanglePolygon(247, 250);
            gxtDrawablePolygon gridPolygon = new gxtDrawablePolygon(gridPoly.v);
            gridPolygon.SetupTexture(gridTexture, gxtTextureCoordinateType.CLAMP);
            gridDrawable.Entity = gridPolygon;

            gxtIDrawable textDrawable = new gxtDrawable(Color.White, true, 0.0f);
            SpriteFont font = Content.Load<SpriteFont>("Fonts\\debug_font");
            gxtTextField textEntity = new gxtTextField(font, "1.0f");
            textDrawable.Entity = textEntity;
            */
            //Vector2[] grassVertices2 = gxtGeometry.CreateCircleVertices(225, 7);
            Vector2[] grassVertices2 = gxtGeometry.CreateRectangleVertices(350, 350);
            //gxtMesh grassMesh = new gxtMesh(grassVertices);
            gxtMesh grassMesh = new gxtMesh();
            //grassMesh.SetVertices(grassVertices);
            Texture2D grassTexture = Content.Load<Texture2D>("Textures\\grass");
            grassMesh.SetVertices(grassVertices2);
            grassMesh.ApplyTexture(grassTexture, gxtTextureCoordinateType.WRAP);
            gxtIMaterial material = new gxtMaterial();
            grassMesh.Material = material;
            material.RenderDepth = 0.0f;
            material.ColorOverlay = new Color(255, 255, 255, 255);

            gxtIMaterial fontMaterial = new gxtMaterial();
            fontMaterial.ColorOverlay = Color.Red;
            //fontMaterial.RenderDepth = 0.75f;
            SpriteFont font = gxtResourceManager.Singleton.Load<SpriteFont>("Fonts\\debug_font");
            gxtTextField tf = new gxtTextField();
            tf.Text = "LOLOL";
            tf.SpriteFont = font;
            tf.Material = fontMaterial;
            tf.Material.Visible = true;
            //gxtSprite sprite = new gxtSprite(Content.Load<Texture2D>("Textures\\grass"));
            //grassDrawable.Entity = sprite;

            //transformNode.AttachDrawable(blueRect);
            //transformNode.AttachDrawable(yellowRect);
            baseNode.Scale = new Vector2(1.0f, 1.0f);

            baseNode.AttachDrawable(grassMesh);
            baseNode.AttachDrawable(tf);

            //Vector2[] lines = gxtGeometry.CreateRectangleVertices(150, 150);

            gxtIMaterial lineMaterial = new gxtMaterial();
            lineMaterial.ColorOverlay = Color.White;
            lineMaterial.RenderDepth = 0.0f;
            //gxtDynamicIndexedPrimitive dynamicLine = new gxtDynamicIndexedPrimitive(lines, PrimitiveType.LineList);
            gxtLine line = new gxtLine();
            line.Start = new Vector2(-75, -75);
            line.End = new Vector2(75, 75);
            line.Material = lineMaterial;
            //dynamicLine.Texture = grassTexture;
            //baseNode.AttachDrawable(dynamicLine);

            Texture2D metalTexture = gxtResourceManager.Singleton.LoadTexture("Textures\\scratched_metal");
            gxtIMaterial metalMaterial = new gxtMaterial();
            metalMaterial.SetDefaults();
            gxtSprite metalSprite = new gxtSprite(metalTexture, metalMaterial);
            //childNode2.AttachDrawable(metalSprite);

            gxtIMaterial circMaterial = new gxtMaterial();
            circMaterial.RenderDepth = 0.1f;
            circMaterial.ColorOverlay = new Color(255, 0, 0, 100);

            gxtCircle circ = new gxtCircle(75.0f, gxtCircleDrawMode.CIRCLE);
            circ.Material = circMaterial;

            gxtCircle circShell = new gxtCircle(75.0f, gxtCircleDrawMode.SHELL);
            circShell.Material = lineMaterial;

            childNode.Position = new Vector2(150, 300);
            childNode.ScaleAxes(1.35f);
            //childNode.Rotation = gxtMath.PI_OVER_FOUR;
            childNode.AttachDrawable(line);
            baseNode.AddChild(childNode);
            //childNode.AttachDrawable(grassMesh);
            childNode.AttachDrawable(circ);
            childNode.AttachDrawable(circShell);
            //baseNode.AttachDrawable(circ);

            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                int id = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.DebugFont = gxtResourceManager.Singleton.Load<SpriteFont>("Fonts\\debug_font");
                gxtDebugDrawer.Singleton.AddSceneGraph(id, sceneGraph);
                gxtDebugDrawer.Singleton.CurrentSceneId = id;
            }

            childNode2.Position = new Vector2(125, 200);
            //childNode2.Rotation = -0.085f;
            gxtRectangle r = new gxtRectangle(100, 150);
            r.Material = new gxtMaterial();
            r.Material.ColorOverlay = new Color(0, 0, 255, 100);

            Vector2[] rectVerts = gxtGeometry.CreateRectangleVertices(100, 150);
            gxtLineLoop lloop = new gxtLineLoop(rectVerts, lineMaterial, true);
            childNode2.AttachDrawable(r);
            childNode2.AttachDrawable(lloop);
            childNode2.Scale = new Vector2(1.0f, 1.0f);
            childNode.AddChild(childNode2);
            /*
            baseNode.AttachDrawable(grassDrawable);
            baseNode.AttachDrawable(textDrawable);
            baseNode.AttachDrawable(gridDrawable);

            //transformNode.AddChild(baseNode);
            sceneGraph.AddNode(baseNode);

            gxtIDrawable lineLoopDrawable = new gxtDrawable(Color.Red, true, 0.3f);
            gxtLineLoop ll = new gxtLineLoop(2.0f);
            gxtAABB aabb = baseNode.GetAABB();
            gxtPolygon aabbPoly = gxtGeometry.ComputePolygonFromAABB(aabb);
            ll.SetVertices(aabbPoly.v);
            lineLoopDrawable.Entity = ll;

            gxtIDrawable circleDrawable = new gxtDrawable(true, 0.65f);
            gxtSprite circle = new gxtSprite(grassTexture);
            //gxtCircle circle = new gxtCircle(100.0f);
            circleDrawable.Entity = circle;
            childNode.Position = new Vector2(100.0f, 200.0f);
            childNode.Rotation = 0.0f;
            childNode.Scale = new Vector2(1.0f, 1.0f);
            childNode.AttachDrawable(circleDrawable);

            gxtIDrawable circleLoopDrawable = new gxtDrawable(Color.Yellow, true, 0.35f);
            gxtLineLoop circleLoop = new gxtLineLoop(2.0f);
            gxtAABB circAABB = circle.GetAABB(Vector2.Zero, 0.0f, Vector2.One);
            gxtPolygon circAABBPoly = gxtGeometry.ComputePolygonFromAABB(circAABB);
            circleLoop.SetVertices(circAABBPoly.v);
            circleLoopDrawable.Entity = circleLoop;

            baseNode.AttachDrawable(lineLoopDrawable);
            childNode.AttachDrawable(circleLoopDrawable);

            baseNode.AddChild(childNode);

            gxtIDrawable blueRectDrawable2 = new gxtDrawable(Color.Blue, true, 0.5f);
            gxtRectangle rect = new gxtRectangle(100, 65);
            blueRectDrawable2.Entity = rect;
            childNode2.Position = new Vector2(-150, 85);
            childNode2.Rotation = 0.3f;
            childNode2.AttachDrawable(blueRectDrawable2);
            childNode.AddChild(childNode2);

            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                int id = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(id, sceneGraph);
                gxtDebugDrawer.Singleton.CurrentSceneId = id;
            }
            /*
            //Texture2D grassTexture = Content.Load<Texture2D>("Textures\\grass");
            //gxtIDrawable grass = new gxtDrawableSprite(grassTexture);
            gxtRectangle rect = new gxtRectangle(200, 100);
            rect.RenderDepth = 1.0f;
            //gxtDrawableLine line = new gxtDrawableLine(new Vector2(-100, 50), new Vector2(100, -50));
            //gxtPolygon boxPoly = gxtGeometry.CreateRectanglePolygon(200, 100);
            //gxtDrawableLineLoop lineLoop = new gxtDrawableLineLoop();
            //lineLoop.SetVertices(boxPoly.v);

            gxtLineBatch lineBatch = new gxtLineBatch();
            lineBatch.Add(new Vector2(0.0f, 100.0f), new Vector2(100.0f, -150.0f));


            gxtCircle circle = new gxtCircle(100.0f);
            circle.RenderDepth = 1.0f;

            grassNode = new gxtSceneNode();
            //grassNode.AttachDrawable(lineLoop);
            //grassNode.AttachDrawable(rect);
            //grassNode.AttachDrawable(line);
            grassNode.AttachDrawable(lineBatch);
            //grassNode.Drawable = grass;

            //Texture2D scrapMetalTexture = Content.Load<Texture2D>("Textures\\grid");
            //gxtIDrawable scrapMetal = new gxtDrawableSprite(scrapMetalTexture);

            scrapMetalNode = new gxtSceneNode();
            //scrapMetalNode.AttachDrawable(scrapMetal);
            //scrapMetalNode.AttachDrawable(lineLoop);
            scrapMetalNode.AttachDrawable(circle);
            //scrapMetalNode.Drawable = scrapMetal;
            scrapMetalNode.Position = new Vector2(100.0f, 100.0f);
            //scrapMetalNode.InheritRotation = false;
            //scrapMetalNode.InheritPosition = false;

            grassNode.AddChild(scrapMetalNode);

            transformNode = new gxtSceneNode();
            //transformNode.Drawable = scrapMetal;
            transformNode.AddChild(grassNode);
            //transformNode.AttachDrawable(circle);

            sceneGraph.AddNode(transformNode);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, sceneGraph.NodeCount);
            manager = new gxtDrawManager();
            manager.Initialize();
            */
            sceneGraph.AddNode(baseNode);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Window Width: {0}", gxtDisplayManager.Singleton.ResolutionWidth);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Window Height: {0}", gxtDisplayManager.Singleton.ResolutionHeight);
            //gxtSprite sprite = new gxtSprite(texture);
            //sprite.Depth = 0.0f;
            //sprite.Alpha = 100;
            //manager.Add(sprite);
            
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
            /*
            gxtGamepad gp = gxtGamepadManager.Singleton.GetGamepad(PlayerIndex.One);
            float speed = 5.0f;
            grassNode.RelativePosition += gp.AdjLStick() * speed;
            if (gp.GetState(Buttons.B) == gxtControlState.FIRST_PRESSED)
                scrapMetalNode.RelativeVisibility = !scrapMetalNode.RelativeVisibility;
            float rz = (gp.RTrigger() - gp.LTrigger()) * 0.1f;
            if (rz != 0.0f)
                grassNode.RelativeRotation += rz;
                //grassNode.RelativeVisibility = !grassNode.RelativeVisibility;
            */
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            float speed = 5.0f;
            if (kb.IsDown(Keys.A))
                baseNode.Translate(new Vector2(-speed, 0.0f), gxtTransformSpace.WORLD);
            if (kb.IsDown(Keys.D))
                baseNode.Translate(new Vector2(speed, 0.0f), gxtTransformSpace.WORLD);
            if (kb.IsDown(Keys.S))
                baseNode.Translate(new Vector2(0.0f, speed), gxtTransformSpace.WORLD);
            if (kb.IsDown(Keys.W))
                baseNode.Translate(new Vector2(0.0f, -speed), gxtTransformSpace.WORLD);
            if (kb.IsDown(Keys.E))
                baseNode.Rotate(0.005f, gxtTransformSpace.LOCAL);
                //grassNode.RelativeRotation += 0.005f;
                //baseNode.RelativeRotation += 0.005f;
            if (kb.IsDown(Keys.Q))
                baseNode.Rotate(-0.005f, gxtTransformSpace.LOCAL);
                //grassNode.RelativeRotation -= 0.005f;
                //baseNode.RelativeRotation -= 0.005f;
            if (kb.IsDown(Keys.Z))
                baseNode.ScaleAxes(0.995f);

            if (kb.IsDown(Keys.C))
                baseNode.ScaleAxes(1.005f);

            if (kb.GetState(Keys.Enter) == gxtControlState.FIRST_PRESSED)
            {
                childTransformSpace = (gxtTransformSpace)(((int)childTransformSpace + 1) % 3);
            }

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, childTransformSpace.ToString());


            if (kb.IsDown(Keys.Left))
                childNode.Translate(new Vector2(-speed, 0.0f), childTransformSpace);
            if (kb.IsDown(Keys.Right))
                childNode.Translate(new Vector2(speed, 0.0f), childTransformSpace);
            if (kb.IsDown(Keys.Down))
                childNode.Translate(new Vector2(0.0f, speed), childTransformSpace);
            if (kb.IsDown(Keys.Up))
                childNode.Translate(new Vector2(0.0f, -speed), childTransformSpace);
            if (kb.IsDown(Keys.OemOpenBrackets))
                childNode.Rotate(-0.005f, childTransformSpace);
            //grassNode.RelativeRotation += 0.005f;
            //baseNode.RelativeRotation += 0.005f;
            if (kb.IsDown(Keys.OemCloseBrackets))
                childNode.Rotate(0.005f, childTransformSpace);
            //grassNode.RelativeRotation -= 0.005f;
            //baseNode.RelativeRotation -= 0.005f;
            if (kb.IsDown(Keys.OemComma))
                childNode.ScaleAxes(0.995f);

                //baseNode.Scale = new Vector2(baseNode.Scale.X - 0.005f, baseNode.Scale.Y - 0.005f);
                //baseNode.ScaleAll(0.985f);
                //grassNode.RelativeScale = new Vector2(grassNode.RelativeScale.X - 0.005f, grassNode.RelativeScale.Y - 0.005f);
                //baseNode.Scale = new Vector2(baseNode.Scale.X - 0.005f, baseNode.Scale.Y - 0.005f);
            if (kb.IsDown(Keys.OemPeriod))
                childNode.ScaleAxes(1.005f);
                //baseNode.Scale = new Vector2(baseNode.Scale.X + 0.005f, baseNode.Scale.Y - 0.005f);
                //baseNode.ScaleAll(1.015f);
                //grassNode.RelativeScale = new Vector2(grassNode.RelativeScale.X + 0.005f, grassNode.RelativeScale.Y + 0.005f);
                //baseNode.Scale = new Vector2(baseNode.Scale.X + 0.005f, baseNode.Scale.Y + 0.005f);
            if (kb.GetState(Keys.N) == gxtControlState.FIRST_PRESSED)
                baseNode.ScaleAxes(new Vector2(-1.0f, 1.0f));
            //if (kb.GetState(Keys.P) == gxtControlState.FIRST_PRESSED)
            //    childNode.InheritPosition = !childNode.InheritPosition;
            if (kb.GetState(Keys.V) == gxtControlState.FIRST_PRESSED)
            {
                baseNodeVisible = !baseNodeVisible;
                baseNode.SetVisibility(baseNodeVisible, false);
            }
            if (kb.GetState(Keys.Space) == gxtControlState.FIRST_PRESSED)
            {
                //baseNode.ScaleAxes(-1.0f, 1.0f);
                //baseNode.FlipHorizontal();
                //baseNode.SetColor(Color.Red);
                baseNode.FlipVertical();
            }
            if (kb.GetState(Keys.T) == gxtControlState.FIRST_PRESSED)
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, sceneGraph.DebugTrace());

            if (kb.IsDown(Keys.F2))
                camera.Rotation += 0.005f;
            if (kb.IsDown(Keys.F1))
                camera.Rotation -= 0.005f;
            if (kb.IsDown(Keys.F3))
                camera.Translate(-0.3f, 0.0f);
            if (kb.IsDown(Keys.F4))
                camera.Translate(0.3f, 0.0f);
            if (kb.IsDown(Keys.F5))
                camera.Translate(0.0f, -0.3f);
            if (kb.IsDown(Keys.F6))
                camera.Translate(0.0f, 0.3f);
            if (kb.IsDown(Keys.F7))
                camera.Zoom -= 0.005f;
            if (kb.IsDown(Keys.F8))
                camera.Zoom += 0.005f;
            
                //transformNode.ColorOverlay = Color.Red;
            /*
            if (kb.GetState(Keys.U) == gxtControlState.FIRST_PRESSED)
                childNode.InheritScale = !childNode.InheritScale;
            if (kb.IsDown(Keys.Left))
                sceneRotation -= 0.005f;
            if (kb.IsDown(Keys.Right))
                sceneRotation += 0.005f;
            if (kb.IsDown(Keys.Up))
                scenePos -= new Vector2(0.0f, 10.0f);
            */
            //sceneGraph.Update();
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, baseNode.Scale.ToString());

            gxtMouse m = gxtMouseManager.Singleton.GetMouse();
            Vector2 mousePos = camera.GetVirtualMousePosition(m.GetPosition());
            if (m.GetState(gxtMouseButton.LEFT) == gxtControlState.FIRST_PRESSED)
            {

                baseNode.SetDerivedPosition(mousePos);
            }

            if (m.GetState(gxtMouseButton.RIGHT) == gxtControlState.FIRST_PRESSED)
            {
                Vector2 mouseN = Vector2.Normalize(mousePos - baseNode.GetDerivedPosition());
                if (mouseN != Vector2.Zero)
                {
                    float ang = gxtMath.Atan2(mouseN.Y, mouseN.X);
                    //worldRotation += 0.00005f; //gxtMath.WrapAngle(worldRotation + 0.00005f);
                    baseNode.SetDerivedRotation(gxtMath.PI_OVER_TWO + baseNode.GetDerivedRotation());
                }
            }
            prevVirtualMousePos = mousePos;
            gxtDebugDrawer.Singleton.AddPt(mousePos, Color.Yellow, 0.0f);

            /*
            IEnumerator<gxtISceneNode> treeIterator = sceneGraph.Root.GetChildEnumerator();
            while (treeIterator.MoveNext())
            {
                gxtISceneNode cur = treeIterator.Current;
                IEnumerator<gxtISceneNode> curChildren = cur.GetChildEnumerator();
                while (curChildren.MoveNext())
                {
                    aabbsToAdd.Add(curChildren.Current.GetAABB());
                }
            }
            */

            sceneGraph.Update(gameTime);

            List<gxtAABB> aabbsToAdd = new List<gxtAABB>();
            aabbsToAdd.Add(baseNode.GetAABB());
            aabbsToAdd.Add(childNode.GetAABB());
            aabbsToAdd.Add(childNode2.GetAABB());

            /*
            IEnumerator<gxtISceneNode> treeIterator = sceneGraph.Root.GetChildEnumerator();
            while (treeIterator.MoveNext())
            {
                gxtISceneNode cur = treeIterator.Current;
                IEnumerator<gxtISceneNode> curChildren = cur.GetChildEnumerator();
                while (curChildren.MoveNext())
                {
                    aabbsToAdd.Add(curChildren.Current.GetAABB());
                }
            }
            */

            for (int i = 0; i < aabbsToAdd.Count; ++i)
            {
                gxtDebugDrawer.Singleton.AddAABB(aabbsToAdd[i], Color.Blue, 0.0f);
            }

            sceneGraph.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "cam pos: {0}", camera.Position);
            base.Draw(gameTime);
            gxtGraphicsBatch gb = gxtRoot.Singleton.GraphicsBatch;
            gb.Begin(gxtBatchDrawOrder.PRIMITIVES_FIRST, gxtBatchSortMode.DEFAULT, gxtBatchDepthMode.FRONT_TO_BACK, camera.GetTransform());
            sceneGraph.Draw(gb);
            gb.End();
            //gxtSpriteBatch spriteBatch = gxtRoot.Singleton.SpriteBatch;
            //spriteBatch.Begin(camera.GetTransform());
            //sceneGraph.Draw(spriteBatch);
            //gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, scenePos.ToString());
            //spriteBatch.End();
        }
    }
}
