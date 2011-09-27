using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TextureMappingTestGame2 : gxtGame
    {
        //gxtProcessManager processManager;
        Texture2D grassTexture;
        //TexturedPolygon texturedPolygon;
        //TexturedPolygon texturedPolygon2;
        gxtSceneGraph sceneGraph;
        gxtCamera camera;
        gxtCircle referenceCircle;
        gxtPolygon polygon;
        gxtPolygon polygon2;
        int debugDrawId;

        public TextureMappingTestGame2()
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

            polygon = gxtGeometry.CreateCirclePolygon(100.0f, 3); //gxtGeometry.CreateCirclePolygon(75.0f, 5);
            
            texturedPolygon = new TexturedPolygon(polygon, gxtRoot.Singleton.Graphics, -0.5f);
            texturedPolygon.ColorOverlay = new Color(255, 0, 0, 100);
            grassTexture = Content.Load<Texture2D>("Textures\\grass");
            texturedPolygon.Texture = grassTexture;
            texturedPolygon.TextureEnabled = false;
            texturedPolygon.CalculateUVCoords();

            polygon2 = gxtGeometry.CreateCirclePolygon(100.0f, 7);

            texturedPolygon2 = new TexturedPolygon(polygon2, gxtRoot.Singleton.Graphics);
            texturedPolygon2.Texture = grassTexture;
            texturedPolygon2.TextureEnabled = false;
            texturedPolygon2.CalculateUVCoords();
            texturedPolygon2.Position = new Vector2(-50, -50);

            //texturedPolygon.Scale(1.5f, 1.5f);
            
            //referenceCircle = new gxtCircle(Vector2.Zero, 200.0f, Color.Blue, 0.0f);


            sceneGraph = new gxtSceneGraph();
            sceneGraph.Initialize();

            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                debugDrawId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawId, sceneGraph);
                gxtDebugDrawer.Singleton.SetId(debugDrawId);
            }
            //manager.Add(referenceCircle);
            camera = new gxtCamera(Vector2.Zero, 0.0f, 0.0f, gxtDisplayManager.Singleton);
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
            grassTexture = Content.Load<Texture2D>("Textures\\grass");
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
            gxtDebugDrawer.Singleton.SetId(debugDrawId);
            gxtDebugDrawer.Singleton.AddPolygon(polygon, Color.Yellow, 1.0f);
            gxtDebugDrawer.Singleton.AddPolygon(polygon2, Color.Yellow, 1.0f);
            //gxtDebugDrawer.Singleton.AddSphere(new gxtSphere(Vector2.Zero, 100.0f), Color.Yellow, 0.0f);
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            if (kb.IsDown(Keys.D))
                texturedPolygon.Position += new Vector2(10.0f, 0.0f);
            if (kb.IsDown(Keys.A))
                texturedPolygon.Position -= new Vector2(10.0f, 0.0f);
            if (kb.IsDown(Keys.S))
                texturedPolygon.Position += new Vector2(0.0f, 10.0f);
            if (kb.IsDown(Keys.W))
                texturedPolygon.Position -= new Vector2(0.0f, 10.0f);
            if (kb.IsDown(Keys.Q))
                texturedPolygon.SetRotation(texturedPolygon.Rotation - 0.05f);
            if (kb.IsDown(Keys.E))
                texturedPolygon.SetRotation(texturedPolygon.Rotation + 0.05f);
            if (kb.IsDown(Keys.Left))
            {
                camera.Translate(-1.0f, 0.0f);
                texturedPolygon.effect.View *= Matrix.CreateTranslation(1.0f, 0.0f, 0.0f);
            }
            if (kb.IsDown(Keys.Right))
                camera.Translate(1.0f, 0.0f);
                //camera.Translate(1.0f, 0.0f);
            if (kb.GetState(Keys.Space) == gxtControlState.FIRST_PRESSED)
                texturedPolygon.Scale(-1.0f, 1.0f);
            gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "Cam Mat: {0}", camera.GetTransformation());
            gxtLog.WriteLineV(VerbosityLevel.WARNING, "Effect Mat: {0}", texturedPolygon.effect.View);
            //texturedPolygon.effect.View *= camera.GetTransformation() * Matrix.CreateTranslation(-400.0f, -300.0f, 0.0f);
            gxtMouse mouse = gxtMouseManager.Singleton.GetMouse();
            //camera.Zoom += mouse.GetDeltaScroll() * 0.05f;
            //camera.GetTransformation();
            //manager.Update();


        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //texturedPolygon.SpecialDraw(gxtRoot.Singleton.Graphics);
            //gxtRoot.Singleton.Graphics.Reset();
            //texturedPolygon.SpecialDraw(gxtRoot.Singleton.Graphics);

            //texturedPolygon2.SpecialDraw(gxtRoot.Singleton.Graphics);
            
            gxtSpriteBatch spriteBatch = gxtRoot.Singleton.SpriteBatch;
            spriteBatch.Begin(camera);
            sceneGraph.DrawScene(spriteBatch);
            
            texturedPolygon2.SpecialDraw(gxtRoot.Singleton.Graphics);
            texturedPolygon.SpecialDraw(gxtRoot.Singleton.Graphics);

            spriteBatch.End();
            //sceneGraph.DrawScene();
            
            //SpriteBatch sb = gxtRoot.Singleton.SpriteBatch;
            //sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, camera.GetTransformation());
            //manager.Draw(ref sb, camera.GetViewAABB());
            //gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "Graphics Projection: {0}", gxtRoot.Singleton.Graphics.PresentationParameters.
            //sb.End();
        }
    }

}
