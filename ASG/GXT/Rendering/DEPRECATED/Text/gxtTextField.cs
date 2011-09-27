/*
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// TextField, with additional properties for rotation, scale, and alignment
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtTextField : gxtSimpleTextField
    {
        #region Fields
        /// <summary>
        /// Rotation of the text
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Scale of the text
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Takes scale into account
        /// </summary>
        public override float Width { get { return Font.MeasureString(Text).X * Scale.X; } }
        public override float Height { get { return Font.MeasureString(Text).Y * Scale.Y; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Paramterless constructor.  XML only!
        /// </summary>
        public gxtTextField() : base() { }

        /// <summary>
        /// Takes font
        /// </summary>
        /// <param name="font">Font</param>
        public gxtTextField(SpriteFont font)
            : base(font, String.Empty, Vector2.Zero, Color.White, 0)
        {
            Rotation = 0;
            Scale = Vector2.One;
        }

        /// <summary>
        /// Takes font and text
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        public gxtTextField(SpriteFont font, string text)
            : base(font, text, Vector2.Zero, Color.White, 0)
        {
            Rotation = 0;
            Scale = Vector2.One;
        }

        /// <summary>
        /// Takes font, text, position, and an alignment preset
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        /// <param name="position">Position</param>
        /// <param name="textAlignment">Alignment</param>
        public gxtTextField(SpriteFont font, string text, Vector2 position)
            : base(font, text, position, Color.White, 0)
        {
            Rotation = 0;
            Scale = Vector2.One;
        }

        /// <summary>
        /// Takes all values except ones for scale and rotation
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        /// <param name="position">Position</param>
        /// <param name="textAlignment">Alignment</param>
        /// <param name="colorOverlay">Color</param>
        /// <param name="depth">Render Depth</param>
        public gxtTextField(SpriteFont font, string text, Vector2 position, Color colorOverlay, float depth)
            : base(font, text, position, colorOverlay, depth)
        {
            Rotation = 0;
            Scale = Vector2.One;
        }

        /// <summary>
        /// Takes all values
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        /// <param name="position">Position</param>
        /// <param name="textAlignment">Alignment</param>
        /// <param name="colorOverlay">Color</param>
        /// <param name="depth">Depth</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="scale">Scale</param>
        /// <param name="rotation">Rotation</param>
        public gxtTextField(SpriteFont font, string text, Vector2 position, Color colorOverlay, float depth,
            byte alpha, Vector2 scale, float rotation)
            : base(font, text, position, colorOverlay, depth)
        {
            Rotation = rotation;
            Scale = scale;
        }
        #endregion Constructors

        #region Draw
        /// <summary>
        /// Draws the modular textfield
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public override void Draw(ref SpriteBatch spriteBatch)
        {
            if (Visible)
                spriteBatch.DrawString(Font, Text, Position, ColorOverlay, Rotation, Origin, Scale, SpriteEffects.None, Depth);
        }

        public static void Draw(ref SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 scale, float depth)
        {
            Vector2 origin = font.MeasureString(text) * 0.5f;
            spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, SpriteEffects.None, depth);
        }
        #endregion Draw

        #region Culling
        public override gxtAABB GetAABB()
        {
            float cos = gxtMath.Abs(gxtMath.Cos(Rotation));
            float sin = gxtMath.Abs(gxtMath.Sin(Rotation));
            Vector2 r = Vector2.Multiply(Origin, Scale);
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