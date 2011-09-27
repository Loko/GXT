/*
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// Class for modular sprites.  Gives control over
    /// the origin, scale, rotation, orientation, color,
    /// alpha.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSprite : gxtSimpleSprite
    {
        #region Fields
        /// <summary>
        /// Takes scale into account
        /// </summary>
        public override float Width { get { return Texture.Width * Scale.X; } }
        public override float Height { get { return Texture.Height * Scale.Y; } }

        /// <summary>
        /// Scale.  Default is Vector2.One
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Rotation in radians
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Orientation
        /// </summary>
        public SpriteEffects Orientation { get; set; }

        /// <summary>
        /// Color Overlay
        /// </summary>
        public Color ColorOverlay { get; set; }

        /// <summary>
        /// Ajusts alpha value of color overlay
        /// </summary>
        public byte Alpha { get { return ColorOverlay.A; } set { ColorOverlay = new Color(ColorOverlay.R, ColorOverlay.G, ColorOverlay.B, value); } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Paramterless constructor.  XML only!
        /// </summary>
        public gxtSprite() : base() { }

        /// <summary>
        /// Takes a texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public gxtSprite(Texture2D texture)
            : base(texture)
        {
            Scale = Vector2.One;
            Rotation = 0;
            Orientation = SpriteEffects.None;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes a texture and position
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        public gxtSprite(Texture2D texture, Vector2 position)
            : base(texture, position)
        {
            Scale = Vector2.One;
            Rotation = 0;
            Orientation = SpriteEffects.None;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes a texture, position, and render depth
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        /// <param name="depth">Render Depth</param>
        public gxtSprite(Texture2D texture, Vector2 position, float depth)
            : base(texture, position, depth)
        {
            Scale = Vector2.One;
            Rotation = 0;
            Orientation = SpriteEffects.None;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes a texture, position, render depth, and overlaying color
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        /// <param name="depth">Render Depth</param>
        /// <param name="colorOverlay">Color Overlay</param>
        public gxtSprite(Texture2D texture, Vector2 position, float depth, Color colorOverlay)
            : base(texture, position, depth)
        {
            Scale = Vector2.One;
            Rotation = 0;
            Orientation = SpriteEffects.None;
            ColorOverlay = colorOverlay;
        }

        /// <summary>
        /// Takes parameters for everything except scale and orientation
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        /// <param name="depth">Render Depth</param>
        /// <param name="colorOverlay">Color Overlay</param>
        /// <param name="origin">Origin</param>
        /// <param name="rotation">Rotation</param>
        public gxtSprite(Texture2D texture, Vector2 position, float depth, Color colorOverlay, Vector2 origin, float rotation)
            : base(texture, position, depth)
        {
            Scale = Vector2.One;
            Rotation = rotation;
            Orientation = SpriteEffects.None;
            ColorOverlay = colorOverlay;
        }

        /// <summary>
        /// Constructor that takes every value
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        /// <param name="depth">Render Depth</param>
        /// <param name="colorOverlay">Color Overlay</param>
        /// <param name="origin">Origin</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        /// <param name="orientation">Orientation</param>
        /// <param name="alpha">Alpha</param>
        public gxtSprite(Texture2D texture, Vector2 position, float depth, Color colorOverlay, Vector2 origin, float rotation, Vector2 scale,
            SpriteEffects orientation, byte alpha)
            : base(texture, position, depth)
        {
            Scale = scale;
            Rotation = rotation;
            Orientation = orientation;
            ColorOverlay = new Color(colorOverlay.R, colorOverlay.G, colorOverlay.B, alpha);
        }
        #endregion Constructors

        #region Culling
        /// <summary>
        /// Sets color and preserves existing alpha
        /// </summary>
        /// <param name="color"></param>
        public void SetColorKeepAlpha(Color color)
        {
            ColorOverlay = new Color(color.R, color.G, color.B, Alpha);
        }

        public override gxtAABB GetAABB()
        {
            Vector2 r = Vector2.Multiply(Origin, Scale); // Virtual center;
            float cos = gxtMath.Abs(gxtMath.Cos(Rotation));
            float sin = gxtMath.Abs(gxtMath.Sin(Rotation));
            return new gxtAABB(Position, new Vector2(r.X * cos + r.Y * sin, r.X * sin + r.Y * cos));
        }

        public override gxtOBB GetOBB()
        {
            return new gxtOBB(Position, Vector2.Multiply(Origin, Scale), Rotation);
        }
        #endregion Culling

        #region Draw
        /// <summary>
        /// Draws modular sprite
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public override void Draw(ref SpriteBatch spriteBatch)
        {
            if (Visible)
                spriteBatch.Draw(Texture, Position, null, ColorOverlay, Rotation, Origin, Scale, Orientation, Depth);
        }

        public static void Draw(ref SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, float rotation, float depth)
        {
            Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;
            spriteBatch.Draw(texture, position, null, Color.White, rotation, origin, scale, SpriteEffects.None, depth);
        }
        #endregion Draw
    }
}
*/