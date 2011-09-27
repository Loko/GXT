using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// Helps manage primitves by keeping the code generated textures
    /// global, and in one place.  Also stores a quad index buffer usable 
    /// for drawing these textures and sprites.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: GET RID OF EXPENSIVE MOD AND SQRT CALLS IF POSSIBLE
    public class gxtPrimitiveManager : gxtSingleton<gxtPrimitiveManager>
    {
        private Texture2D pixelTexture;
        private Texture2D circleTexture;
        private Texture2D circleShellTexture;
        private float circleTextureRadius;
        private int[] quadIndices;
        private IndexBuffer quadIndexBuffer;

        /// <summary>
        /// 1x1 White Pixel Texture
        /// </summary>
        public Texture2D PixelTexture { get { return pixelTexture; } }

        /// <summary>
        /// Circle Texture
        /// </summary>
        public Texture2D CircleTexture { get { return circleTexture; } }

        /// <summary>
        /// Circle Shell Texture
        /// </summary>
        public Texture2D CircleShellTexture { get { return circleShellTexture; } }

        /// <summary>
        /// Radius of Circle Texture
        /// Speeds up calculations by keeping it here
        /// </summary>
        public float CircleTextureRadius { get { return circleTextureRadius; } }

        /// <summary>
        /// Indices usable with a typical quad sprite/billboard
        /// </summary>
        public int[] QuadIndices { get { return quadIndices; } }

        /// <summary>
        /// Index buffer useable with a typical quad sprite/billboard
        /// </summary>
        public IndexBuffer QuadIndexBuffer { get { return quadIndexBuffer; } }

        /// <summary>
        /// Determines if the primitive manager has been initialized
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return pixelTexture != null && circleTexture != null && 
                circleShellTexture != null && quadIndices != null && 
                quadIndexBuffer != null;
        }

        /// <summary>
        /// Initializes the PrimitveBatch.  Creates pixel and circle texture
        /// given a graphics device.
        /// </summary>
        /// <param name="graphicsDevice">Graphics</param>
        /// <param name="circleRadius">Radius of the circle texture (it looks worse with scaling)</param>
        /// <param name="circleBorderWidth">Border width of the circle texture</param>
        /// <param name="circleShellBorderWidth">Border width of the circle shell texture</param>
        public void Initialize(GraphicsDevice graphicsDevice, int circleRadius = 400, int circleBorderWidth = 1, int circleShellBorderWidth = 7)
        {
            gxtDebug.Assert(!IsInitialized(), "The Primitive Manager has already been initialized!");
            gxtDebug.Assert(circleRadius >= 1, "Circle Radius Must Be Positive!");
            gxtDebug.Assert(circleBorderWidth >= 1 && circleBorderWidth < circleRadius, "Circle Border Width Must Be Positive and Less Than the Circle Radius!");
            gxtDebug.Assert(circleShellBorderWidth >= 1 && circleShellBorderWidth < circleRadius, "Circle Shell Border Width Must Be Positive and Less Than the Circle Radius!");

            circleTextureRadius = circleRadius;
            CreatePixelTexture(graphicsDevice);
            CreateCircleTexture(graphicsDevice, circleRadius, circleBorderWidth);
            CreateCircleShellTexture(graphicsDevice, circleRadius, circleShellBorderWidth);
            CreateQuadIndexBuffer(graphicsDevice);
        }

        /// <summary>
        /// Unloads resources allocated by the primitive manager
        /// </summary>
        public void Unload()
        {
            if (pixelTexture != null)
                pixelTexture.Dispose();
            if (circleTexture != null)
                circleTexture.Dispose();
            if (circleShellTexture != null)
                circleShellTexture.Dispose();
            if (quadIndexBuffer != null)
                quadIndexBuffer.Dispose();
        }

        /// <summary>
        /// Creates the 1x1 pixel texture
        /// </summary>
        /// <param name="graphicsDevice">Graphics</param>
        private void CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            if (pixelTexture != null)
                return;

            pixelTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixelTexture.SetData<Color>(new Color[] { Color.White });
        }

        /// <summary>
        /// Creates the circle texture at the given radius
        /// Note: This method is heavily influenced and similiar to the
        /// one seen in the Farseer Physics 
        /// </summary>
        /// <param name="graphicsDevice">Graphics</param>
        /// <param name="maxCircleRadius">Max Radius</param>
        private void CreateCircleTexture(GraphicsDevice graphicsDevice, int maxCircleRadius, int borderWidth)
        {
            if (circleTexture != null)
                return;

            int y = -1;
            int diameter = maxCircleRadius * 2;
            Vector2 center = new Vector2(maxCircleRadius);

            circleTexture = new Texture2D(graphicsDevice, diameter, diameter, false, SurfaceFormat.Color);
            Color[] colors = new Color[diameter * diameter];

            for (int i = 0; i < colors.Length; i++)
            {
                int x = i % diameter;

                if (x == 0)
                    y += 1;

                Vector2 diff = new Vector2(x - center.X, y - center.Y);
                float length = diff.Length();

                if (length > maxCircleRadius - borderWidth)
                    colors[i] = Color.Transparent;
                else
                    colors[i] = Color.White;
            }
            circleTexture.SetData(colors);
        }

        /// <summary>
        /// Checks the ring texture to complement the default circle
        /// </summary>
        /// <param name="graphicsDevice">Graphics</param>
        /// <param name="maxCircleRadius">Max Radius</param>
        /// <param name="borderWidth">Width of the ring</param>
        private void CreateCircleShellTexture(GraphicsDevice graphicsDevice, int maxCircleRadius, int borderWidth)
        {
            if (circleShellTexture != null)
                return;

            int y = -1;
            int diameter = maxCircleRadius * 2;
            Vector2 center = new Vector2(maxCircleRadius);

            circleShellTexture = new Texture2D(graphicsDevice, diameter, diameter, false, SurfaceFormat.Color);
            Color[] colors = new Color[diameter * diameter];

            for (int i = 0; i < colors.Length; i++)
            {
                int x = i % diameter;

                if (x == 0)
                    y += 1;

                Vector2 diff = new Vector2(x, y) - center;
                float length = diff.Length();

                if (length > maxCircleRadius)
                    colors[i] = Color.Transparent;
                else if (length > maxCircleRadius - borderWidth)
                    colors[i] = new Color(255, 255, 255, 255);  // semi transparent white instead?
                else
                    colors[i] = Color.Transparent;
            }
            circleShellTexture.SetData(colors);
        }

        /// <summary>
        /// Creates the quad indices and index buffer
        /// </summary>
        /// <param name="graphicsDevice">Graphics</param>
        private void CreateQuadIndexBuffer(GraphicsDevice graphicsDevice)
        {
            quadIndices = new int[] {0, 1, 2, 0, 2, 3 };
            quadIndexBuffer = new IndexBuffer(graphicsDevice, typeof(int), 6, BufferUsage.WriteOnly);
            quadIndexBuffer.SetData<int>(quadIndices);
        }
    }
}

