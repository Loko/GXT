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
    public class TextureMappingTestGame : gxtGame
    {
        //gxtProcessManager processManager;
        Texture2D grassTexture;
        IndexBuffer indexBuffer;
        VertexBuffer vertexBuffer;
        VertexPositionColorTexture[] vertices;
        BasicEffect effect;
        gxtCamera camera;
        gxtSceneGraph sceneGraph;

        public TextureMappingTestGame()
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
            camera = new gxtCamera(Vector2.Zero, 0.0f, 0.0f, gxtDisplayManager.Singleton);
            gxtDisplayManager.Singleton.RegisterCamera(camera);
            gxtRoot.Singleton.DisplayManager.SetResolution(800, 600, false);
            vertices = new VertexPositionColorTexture[3];
            
            /*
            vertices[0] = new VertexPositionColorTexture(new Vector3(-0.1f, 0.0f, 1.0f), new Color(255, 255, 255, 255), new Vector2(0.0f, 1.0f));
            vertices[2] = new VertexPositionColorTexture(new Vector3(0.35f, 0.1f, 1.0f), new Color(255, 255, 255, 255), new Vector2(1.0f, 0.0f));
            vertices[1] = new VertexPositionColorTexture(new Vector3(0.0f, 0.45f, 1.0f), new Color(255, 255, 255, 255), new Vector2(0.0f, 0.0f));
            */
            /*
            vertices[0] = new VertexPositionColorTexture(new Vector3(-10.0f, 0.0f, 1.0f), new Color(255, 255, 255, 255), new Vector2(0.0f, 1.0f));
            vertices[2] = new VertexPositionColorTexture(new Vector3(3.5f, -2.5f, 1.0f), new Color(255, 255, 255, 255), new Vector2(1.0f, 0.0f));
            vertices[1] = new VertexPositionColorTexture(new Vector3(0.0f, 4.5f, 1.0f), new Color(255, 255, 255, 255), new Vector2(0.0f, 0.0f));
            */
            vertices[0] = new VertexPositionColorTexture(new Vector3(0.0f, 0.0f, 0.0f), new Color(255, 255, 255, 255), new Vector2(0.0f, 1.0f));
            vertices[1] = new VertexPositionColorTexture(new Vector3(250.0f, 300.0f, 0.0f), new Color(255, 255, 255, 255), new Vector2(1.0f, 0.0f));
            vertices[2] = new VertexPositionColorTexture(new Vector3(25.0f, -300.0f, 0.0f), new Color(255, 255, 255, 255), new Vector2(0.0f, 0.0f));
            
           

            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            // effect.world is object centroid
            // view up is negated, position is -1, target is camX, camY, 0.0f

            effect = new BasicEffect(gxtRoot.Singleton.Graphics);
            effect.TextureEnabled = true;
            effect.Texture = grassTexture;
            effect.LightingEnabled = false;
            
            effect.World = Matrix.CreateWorld(new Vector3(275.0f / 3.0f, 250.0f / 3.0f, 0.0f), -Vector3.UnitZ, Vector3.Up);
            gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "World: {0}", effect.World.ToString());
            effect.View = Matrix.CreateLookAt(new Vector3(0, 0, -1.0f), new Vector3(0.0f, 0.0f, 0.0f), -Vector3.Up);
            //effect.View *= Matrix.CreateTranslation(0.0f, -40.0f, 0.0f);
            gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "View: {0}", effect.View.ToString());
            
            effect.Projection = Matrix.CreateOrthographic(800.0f, 600.0f, 0.0f, 1.0f);
            //effect.Projection = Matrix.CreateTranslation(0.0f, 0.0f, 1.0f);
            //effect.Projection = Matrix.CreatePerspectiveFieldOfView(gxtMath.PI * 0.25f, 4.0f / 3.0f, 0.2f, 100.0f);
            //gxtRoot.Singleton.SpriteBatch.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "Projection: {0}", effect.Projection.ToString());
            gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "Camera: {0}", camera.GetTransformation().ToString());
            gxtRoot.Singleton.Graphics.RasterizerState = RasterizerState.CullNone;

            sceneGraph = new gxtSceneGraph();
            //manager.Initialize();
            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                int debugDrawId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawId, sceneGraph);
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
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            if (kb.IsDown(Keys.D))
                camera.Translate(1.0f, 0.0f);
            gxtMouse mouse = gxtMouseManager.Singleton.GetMouse();
            camera.Zoom += mouse.GetDeltaScroll() * 0.05f;
            //camera.GetTransformation();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            gxtRoot.Singleton.Graphics.SetVertexBuffer(vertexBuffer);
            camera.GetTransformation();
            //effect.Projection = Matrix.CreateOrthographic(800.0f, 600.0f, 0.05f, 3.0f);
            //effect.
            //gxtRoot.Singleton.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.CreateScale(1, -1, 1)); 
            //gxtRoot.Singleton.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, DepthStencilState.Default, RasterizerState.CullNone, effect, Matrix.CreateScale(0.0f, -1.0f, 0.0f));

            //effect.World = Matrix.CreateTranslation(400.0f, 300.0f, 0.0f);
            //effect.View = Matrix.CreateTranslation(400.0f, 300.0f, 0.0f);
            //effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 5.0f), Vector3.Zero, Vector3.Up);
            //effect.Projection = Matrix.CreatePerspectiveFieldOfView(gxtMath.PI_OVER_TWO / 2.0f, 4.0f / 3.0f, 0.25f, 100.0f);
            //effect.View = Matrix.CreateLookAt(new Vector3(0, 0, -5), Vector3.Zero, Vector3.Up);// camera.GetTransformation();
            //effect.Projection = Matrix.CreatePerspectiveFieldOfView(gxtMath.PI_OVER_TWO * 0.5f, 4.0f / 3.0f, 0.2f, 100.0f);
            //gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "Effect View: {0}", effect.View.ToString());
            //gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "Effect World: {0}", effect.World.ToString());
            //gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "Effect Projection: {0}", effect.Projection.ToString());
            //gxtLog.WriteLineV(VerbosityLevel.CRITICAL, "Camera View: {0}", camera.GetTransformation().ToString());
            //effect.Projection = camera.GetTransformation();
            //effect.World = camera.GetTransformation();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gxtRoot.Singleton.SpriteBatch.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            //effect.CurrentTechnique.Passes[0].Apply();
            }
            //gxtRoot.Singleton.SpriteBatch.End();

            //gxtRoot.Singleton.SpriteBatch.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            //gxtRoot.Singleton.SpriteBatch.End();
            //gxtRoot.Singleton.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null);
            //gxtRoot.Singleton.SpriteBatch.Draw(grassTexture, new Vector2(400, 300), Color.White);
            
            //gxtRoot.Singleton.SpriteBatch.End();
        }
    }

}
