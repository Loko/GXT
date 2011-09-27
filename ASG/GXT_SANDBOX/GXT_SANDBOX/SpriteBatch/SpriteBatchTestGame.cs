using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpriteBatchTestGame : gxtGame
    {
        //gxtProcessManager processManager;
        Texture2D grassTexture;
        Texture2D scrapTexture;
        Texture2D brickTexture;
        SpriteFont font;

        IndexBuffer indexBuffer;
        VertexBuffer vertexBuffer;
        VertexPositionColorTexture[] vertices;
        int[] indicesArray;

        gxtCamera camera;
        gxtSceneGraph sceneGraph;
        gxtSpriteBatch spriteBatch;

        public SpriteBatchTestGame()
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
            camera = new gxtCamera(Vector2.Zero, 0.0f, 0.0f, false);
            gxtDisplayManager.Singleton.RegisterCamera(camera);
            gxtDisplayManager.Singleton.SetResolution(800, 600, false);
            gxtDisplayManager.Singleton.WindowTitle = "Custom SpriteBatch Test";
            base.IsMouseVisible = true;

            grassTexture = Content.Load<Texture2D>("Textures\\grass");
            scrapTexture = Content.Load<Texture2D>("Textures\\scratched_metal");
            brickTexture = Content.Load<Texture2D>("Textures\\seamless_brick");
            font = Content.Load<SpriteFont>("Fonts\\debug_font");

            sceneGraph = new gxtSceneGraph();
            sceneGraph.Initialize();

            spriteBatch = new gxtSpriteBatch();
            spriteBatch.Initialize(gxtRoot.Singleton.XNASpriteBatch, gxtRoot.Singleton.Graphics);

            gxtPolygon poly = gxtGeometry.CreateCirclePolygon(200, 11);
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), poly.NumVertices, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), 3 + ((poly.NumVertices - 3) * 3), BufferUsage.WriteOnly);
            Triangulate(poly);
            CalculateUVCoords(poly);

            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                int debugDrawId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawId, sceneGraph);
            }
        }

        private void Triangulate(gxtPolygon polygon)
        {
            // setup vertex buffer
            vertices = new VertexPositionColorTexture[polygon.NumVertices];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new VertexPositionColorTexture(new Vector3(polygon.v[i].X, polygon.v[i].Y, 0.0f), Color.White, new Vector2(0.0f, 0.0f));  // coords will be figured out later
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            // setup index buffer, uses proper triangulation
            List<int> indices = new List<int>(3 + ((vertices.Length - 3) * 3));
            for (int i = 1, j = 2; j < vertices.Length; i = j, j++)
            {
                indices.Add(0);
                indices.Add(i);
                indices.Add(j);
            }
            indicesArray = indices.ToArray();
            indexBuffer.SetData<int>(indicesArray);
        }

        public void CalculateUVCoords(gxtPolygon polygon)
        {
            Vector2 topLeft = new Vector2(-brickTexture.Width * 0.5f, -brickTexture.Height * 0.5f);
            Vector2 oneOverSizeVector = new Vector2(1.0f / brickTexture.Width, 1.0f / brickTexture.Height);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].TextureCoordinate = Vector2.Multiply(polygon.v[i] - topLeft, oneOverSizeVector);
                vertices[i].TextureCoordinate = new Vector2(gxtMath.Clamp(vertices[i].TextureCoordinate.X, 0.0f, 1.0f), gxtMath.Clamp(vertices[i].TextureCoordinate.Y, 0.0f, 1.0f));
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
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
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            const float CAMERA_SPEED = 3.0f;
            const float CAMERA_ROTATION_SPEED = 0.005f;
            const float CAMERA_ZOOM_SPEED = 0.0025f;
            if (kb.GetState(Keys.A) == gxtControlState.DOWN)
                camera.TranslateLocal(-CAMERA_SPEED, 0.0f);
            if (kb.GetState(Keys.D) == gxtControlState.DOWN)
                camera.TranslateLocal(CAMERA_SPEED, 0.0f);
            if (kb.IsDown(Keys.W))
                camera.Translate(0.0f, -CAMERA_SPEED);
            if (kb.IsDown(Keys.S))
                camera.Translate(0.0f, CAMERA_SPEED);
            if (kb.IsDown(Keys.E))
                camera.Rotation += CAMERA_ROTATION_SPEED;
            if (kb.IsDown(Keys.Q))
                camera.Rotation -= CAMERA_ROTATION_SPEED;
            if (kb.IsDown(Keys.C))
                camera.Zoom += CAMERA_ZOOM_SPEED;
            if (kb.IsDown(Keys.Z))
                camera.Zoom -= CAMERA_ZOOM_SPEED;
            if (kb.GetState(Keys.Space) == gxtControlState.FIRST_PRESSED)
                camera.Rotation = gxtMath.PI_OVER_FOUR;
            sceneGraph.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            base.Draw(gameTime);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, camera.GetTransform().ToString());
            spriteBatch.Begin(camera.GetTransform());
            //spriteBatch.DrawSprite(grassTexture, Vector2.Zero, null, Color.White, 0.0f, new Vector2(grassTexture.Width * 0.5f, grassTexture.Height * 0.5f), Vector2.One, SpriteEffects.None, 0.0f);
            //spriteBatch.DrawSprite(scrapTexture, new Vector2(200, 200), null, Color.White, 0.0f, new Vector2(scrapTexture.Width * 0.5f, scrapTexture.Height * 0.5f), Vector2.One, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(font, "Hello, World!", new Vector2(-100, - 100), Color.Red, gxtMath.PI_OVER_FOUR, font.MeasureString("Hello, World") * 0.5f, Vector2.One, SpriteEffects.None, 0.3f);
            //spriteBatch.DrawSprite(brickTexture, new Vector2(-150.0f, 150.0f), null, new Color(255, 255, 255, 100), 0.0f, new Vector2(brickTexture.Width * 0.5f, brickTexture.Height * 0.5f), Vector2.One, SpriteEffects.None, 0.65f);
            spriteBatch.DrawPolygon(brickTexture, vertexBuffer, indexBuffer, new Vector2(-100, -100), gxtMath.DegreesToRadians(10.0f), Vector2.One, SpriteEffects.None, 1.0f);
            spriteBatch.DrawSprite(grassTexture, new Vector2(50, -75), Color.White, 0.0f, new Vector2(grassTexture.Width * 0.5f, grassTexture.Height * 0.5f), Vector2.One, SpriteEffects.None, 0.0f);
            spriteBatch.DrawPolygon(null, vertexBuffer, indexBuffer, new Vector2(100, 200), 0.0f, Vector2.One, SpriteEffects.None, 0.0f);
            spriteBatch.End();
            
            gxtOBB camOBB = camera.GetViewOBB();
            gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Matrix Right: {0}", camera.GetTransform().Right.ToString());
            //camOBB.Extents *= 0.85f;
            //gxtDebugDrawer.Singleton.AddOBB(camOBB, Color.Yellow, 0.0f);
            gxtRoot.Singleton.XNASpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, camera.GetTransform());
            gxtPolygon obbPoly = gxtGeometry.ComputePolygonFromOBB(camOBB);
            for (int j = obbPoly.NumVertices - 1, i = 0; i < obbPoly.NumVertices; j = i, i++)
            {
                Vector2 d = obbPoly.v[i] - obbPoly.v[j];
                float dist = d.Length();
                float ang = gxtMath.Atan2(d.Y, d.X);
                gxtRoot.Singleton.XNASpriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, obbPoly.v[j], null, Color.Yellow, ang, new Vector2(0.0f, 0.5f), new Vector2(dist, 2.0f), SpriteEffects.None, 0.0f);
            }
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "OBB Rot: {0}", camOBB.Rotation);
            gxtMouse m = gxtMouseManager.Singleton.GetMouse();
            gxtRoot.Singleton.XNASpriteBatch.Draw(gxtPrimitiveManager.Singleton.CircleTexture, camera.GetVirtualMousePosition(m.GetPosition()), null, Color.Yellow, 0.0f, new Vector2(gxtPrimitiveManager.Singleton.CircleTextureRadius), 0.1f, SpriteEffects.None, 0.0f);
            gxtRoot.Singleton.XNASpriteBatch.Draw(grassTexture, Vector2.Zero, null, Color.Red, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
            gxtRoot.Singleton.XNASpriteBatch.End();
        }
    }
}
