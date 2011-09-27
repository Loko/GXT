/*
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// SpriteSheet but can have a color overlay, scale, rotation,
    /// and orientation
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSpriteSheet : gxtSimpleSpriteSheet
    {
        #region Fields
        /// <summary>
        /// Color Overlay
        /// </summary>
        public Color ColorOverlay { get; set; }

        /// <summary>
        /// Ajusts alpha value of color overlay
        /// </summary>
        public byte Alpha { get { return ColorOverlay.A; } set { ColorOverlay = new Color(ColorOverlay.R, ColorOverlay.G, ColorOverlay.B, value); } }

        /// <summary>
        /// Will scale all frames!
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Will rotate all frames!
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Will orient all frames!
        /// </summary>
        public SpriteEffects Orientation { get; set; }

        /// <summary>
        /// Dimensions with respect to scale
        /// </summary>
        public override float Width { get { return Texture.Width * Scale.X; } }
        public override float Height { get { return Texture.Height * Scale.Y; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Parameterless constructor.  XML only!
        /// </summary>
        public gxtSpriteSheet() : base() { }

        /// <summary>
        /// Takes same parameters as base class
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="frameSequence">FrameSequence</param>
        public gxtSpriteSheet(Texture2D texture, gxtIFrameSequence frameSequence)
            : base(texture, frameSequence)
        {
            ColorOverlay = Color.White;
            Scale = Vector2.One;
            Rotation = 0;
            Orientation = SpriteEffects.None;
        }
        #endregion Constructors

        #region Draw
        /// <summary>
        /// Draws the spritesheet
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public override void Draw(ref SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, FrameSequence.CurrentFrameRect, ColorOverlay, Rotation,
                FrameSequence.CurrentFrameOrigin, Scale, Orientation, Depth);
        }
        #endregion Draw

        #region Culling
        public override gxtAABB GetAABB()
        {
            Vector2 r = Vector2.Multiply(Origin, Scale); //Origin;
            float cos = gxtMath.Abs(gxtMath.Cos(Rotation));
            float sin = gxtMath.Abs(gxtMath.Sin(Rotation));
            return new gxtAABB(Position, new Vector2(r.X * cos + r.Y * sin, r.X * sin + r.Y * cos));
        }

        public override gxtOBB GetOBB()
        {
            Vector2 r = Vector2.Multiply(Origin, Scale);
            return new gxtOBB(Position, r, Rotation);
        }
        #endregion Culling
    }
}
*/
