/*
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GXT.Rendering
{
    /// <summary>
    /// Class for basic ingame text.  Can control the text string, position,
    /// and color.  Origin for this is always centered.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSimpleTextField : gxtIDraw
    {
        #region Fields
        /// <summary>
        /// Visible?
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Spritefont
        /// </summary>
        [ContentSerializerIgnore]
        public SpriteFont Font { get; set; }

        /// <summary>
        /// Position, relative to center
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Render depth
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Text to display
        /// </summary>
        protected string text;
        public string Text { get { return text; } set { text = value; UpdateOrigin(); } }

        /// <summary>
        /// Color of the text
        /// </summary>
        public Color ColorOverlay { get; set; }

        /// <summary>
        /// Ajusts alpha value of color overlay
        /// </summary>
        public byte Alpha { get { return ColorOverlay.A; } set { ColorOverlay = new Color(ColorOverlay.R, ColorOverlay.G, ColorOverlay.B, value); } }

        /// <summary>
        /// Origin, always centered
        /// </summary>
        private Vector2 origin;
        public Vector2 Origin { get { return origin; } }

        /// <summary>
        /// Dimensions.
        /// </summary>
        public virtual float Width { get { return Font.MeasureString(Text).X; } }
        public virtual float Height { get { return Font.MeasureString(Text).Y; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Paramterless constructor.  XML only!
        /// </summary>
        public gxtSimpleTextField() : base() { }

        /// <summary>
        /// Takes font
        /// </summary>
        /// <param name="font">Font</param>
        public gxtSimpleTextField(SpriteFont font)
        {
            Position = Vector2.Zero;
            Depth = 0;
            Visible = true;
            Font = font;
            Text = String.Empty;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes font and text
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        public gxtSimpleTextField(SpriteFont font, string text)
        {
            Position = Vector2.Zero;
            Depth = 0;
            Visible = true;
            Font = font;
            Text = text;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Constructor, takes font, text, and a position
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        /// <param name="position">Position</param>
        public gxtSimpleTextField(SpriteFont font, string text, Vector2 position)
        {
            Position = Vector2.Zero;
            Depth = 0;
            Visible = true;
            Font = font;
            Text = text;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes font, text, position, and color
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        /// <param name="position">Position</param>
        /// <param name="colorOverlay">Color</param>
        public gxtSimpleTextField(SpriteFont font, string text, Vector2 position, Color colorOverlay)
        {
            Position = Vector2.Zero;
            Depth = 0;
            Visible = true;
            Font = font;
            Text = text;
            ColorOverlay = colorOverlay; ;
        }

        /// <summary>
        /// Takes font, text, position, color and depth.
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        /// <param name="position">Position</param>
        /// <param name="colorOverlay">Color</param>
        /// <param name="depth">Render Depth</param>
        public gxtSimpleTextField(SpriteFont font, string text, Vector2 position, Color colorOverlay, float depth)
        {
            Position = position;
            Depth = depth;
            Visible = true;
            Font = font;
            Text = text;
            ColorOverlay = colorOverlay;
        }

        /// <summary>
        /// Takes all values
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        /// <param name="position">Position</param>
        /// <param name="colorOverlay">Color</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="depth">Depth</param>
        public gxtSimpleTextField(SpriteFont font, string text, Vector2 position, Color colorOverlay, byte alpha, float depth)
        {
            Position = Vector2.Zero;
            Depth = 0;
            Visible = true;
            Font = font;
            Text = text;
            ColorOverlay = new Color(colorOverlay.R, colorOverlay.G, colorOverlay.B, alpha);
        }
        #endregion Constructors

        protected virtual void UpdateOrigin()
        {
            gxtDebug.Assert(Font != null);
            origin = Font.MeasureString(Text) * 0.5f;
        }

        #region Draw
        /// <summary>
        /// Draws the textfield
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public virtual void Draw(ref SpriteBatch spriteBatch)
        {
            if (Visible)
                spriteBatch.DrawString(Font, Text, Position, ColorOverlay, 0, Origin, 1, SpriteEffects.None, Depth);
        }

        /// <summary>
        /// Draws a simple textfield with the given parameters
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="depth"></param>
        public static void Draw(ref SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float depth)
        {
            Vector2 textOrigin = font.MeasureString(text) * 0.5f;
            spriteBatch.DrawString(font, text, position, color, 0, textOrigin, 1, SpriteEffects.None, depth);
        }
        #endregion Draw

        public virtual gxtAABB GetAABB()
        {
            return new gxtAABB(Position, Origin);
        }

        public virtual gxtOBB GetOBB()
        {
            return new gxtOBB(Position, Origin);
        }
    }
}
*/
