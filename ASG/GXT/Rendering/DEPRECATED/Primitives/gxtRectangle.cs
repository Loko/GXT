/*
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    //TODO: JEFF
    // Position is always at the center of the rectangle, but constructors
    // always take full width and height arguments
    // is this confusing?  does it warrant a change?
    /// <summary>
    /// Class for a drawable rectangular primitive.  Gives control over position,
    /// width, height, rotation, and color.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtRectangle : gxtIDraw
    {
        #region Fields
        /// <summary>
        /// Draw It?
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Position of rect
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Depth
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Width and Height of Rectangle
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Rotation of rect in radians
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Accessors for width and height
        /// </summary>
        public float Width { get { return Size.X; } }
        public float Height { get { return Size.Y; } }

        /// <summary>
        /// Color/Alpha
        /// </summary>
        public Color ColorOverlay { get; set; }
        public byte Alpha { get { return ColorOverlay.A; } set { ColorOverlay = new Color(ColorOverlay.R, ColorOverlay.G, ColorOverlay.B, value); } }

        // make this const and static?
        public Vector2 Origin { get { return new Vector2(0.5f); } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Parameterless constructor.  XML only!
        /// </summary>
        public gxtRectangle() { }

        /// <summary>
        /// Takes a position and size
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="size">Size</param>
        public gxtRectangle(Vector2 position, Vector2 size)
        {
            Position = position;
            Depth = 0;
            Visible = true;
            Size = size;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes position and scalars for each component
        /// of the size
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public gxtRectangle(Vector2 position, float width, float height)
        {
            Position = position;
            Depth = 0;
            Visible = true;
            Size = new Vector2(width, height);
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes position, size, and color
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="size">Size</param>
        /// <param name="colorOverlay">Color</param>
        public gxtRectangle(Vector2 position, Vector2 size, Color colorOverlay)
        {
            Position = position;
            Depth = 0;
            Visible = true;
            Size = size;
            ColorOverlay = colorOverlay;
        }

        /// <summary>
        /// Takes position, size, color, and render depth
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="size">Size</param>
        /// <param name="colorOverlay">Color</param>
        /// <param name="depth">Depth</param>
        public gxtRectangle(Vector2 position, Vector2 size, Color colorOverlay, float depth)
        {
            Position = position;
            Depth = depth;
            Visible = true;
            Size = size;
            ColorOverlay = colorOverlay;
        }
        #endregion Constructors

        #region Culling
        public gxtAABB GetAABB()
        {
            Vector2 r = Vector2.Multiply(Origin, Size); //Origin;
            float cos = gxtMath.Abs(gxtMath.Cos(Rotation));
            float sin = gxtMath.Abs(gxtMath.Sin(Rotation));
            return new gxtAABB(Position, new Vector2(r.X * cos + r.Y * sin, r.X * sin + r.Y * cos));
        }

        public gxtOBB GetOBB()
        {
            Vector2 r = Vector2.Multiply(Origin, Size);
            return new gxtOBB(Position, r, Rotation);
        }
        #endregion Culling

        #region Draw
        /// <summary>
        /// Draws the rectangle primitve
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public void Draw(ref SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, Position, null, ColorOverlay, Rotation,
                Origin, Size, SpriteEffects.None, Depth);
        }

        public static void Draw(ref SpriteBatch spriteBatch, Vector2 position, Vector2 size, Color color, float depth, float rotation, Vector2 origin)
        {
            spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, position, null, color, rotation, origin, size, SpriteEffects.None, depth);
        }
        #endregion Draw
    }
}
*/
