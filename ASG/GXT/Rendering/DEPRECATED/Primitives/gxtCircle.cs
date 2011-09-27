/*
using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GXT.Rendering
{
    /// <summary>
    /// Class for drawable circle primitive.  Gives control over position,
    /// radius, and color.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    
    public class gxtCircle : gxtIDraw
    {
        #region Fields
        /// <summary>
        /// Draw It?
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Center position
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Depth
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Radius of the circle
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Color/Alpha
        /// </summary>
        public Color ColorOverlay { get; set; }
        public byte Alpha { get { return ColorOverlay.A; } set { ColorOverlay = new Color(ColorOverlay.R, ColorOverlay.G, ColorOverlay.B, value); } }

        /// <summary>
        /// Cenetered origin
        /// </summary>
        //public Vector2 Origin { get { return new Vector2(PrimitiveManager.CircleTextureRadius); } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Parameterless constructor.  XML only!
        /// </summary>
        public gxtCircle() : base() { }

        /// <summary>
        /// Takes position and radius
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        public gxtCircle(Vector2 position, float radius)
        {
            Position = position;
            Depth = 0;
            Visible = true;
            Radius = radius;
            ColorOverlay = Color.White;
        }

        /// <summary>
        /// Takes position, radius, and color
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="colorOverlay">Color</param>
        public gxtCircle(Vector2 position, float radius, Color colorOverlay)
        {
            Position = position;
            Depth = 0;
            Visible = true;
            Radius = radius;
            ColorOverlay = colorOverlay;
        }

        /// <summary>
        /// Takes position, radius, color, and depth
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="colorOverlay">Color</param>
        /// <param name="depth">Depth</param>
        public gxtCircle(Vector2 position, float radius, Color colorOverlay, float depth)
        {
            Position = position;
            Depth = depth;
            Visible = true;
            Radius = radius;
            ColorOverlay = colorOverlay;
        }
        #endregion Constructors

        #region Culling
        public gxtAABB GetAABB()
        {
            return new gxtAABB(Position, new Vector2(Radius));
        }

        public gxtOBB GetOBB()
        {
            return new gxtOBB(Position, new Vector2(Radius));
        }
        #endregion Culling

        #region Draw
        /// <summary>
        /// Draws the circle primitve
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public void Draw(ref SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            spriteBatch.Draw(gxtPrimitiveManager.Singleton.CircleTexture, Position, null, ColorOverlay, 0, new Vector2(gxtPrimitiveManager.Singleton.CircleTextureRadius),
                new Vector2(Radius / gxtPrimitiveManager.Singleton.CircleTextureRadius), SpriteEffects.None, Depth);
        }

        public static void Draw(ref SpriteBatch spriteBatch, Vector2 position, float radius, float depth, Color color)
        {
            spriteBatch.Draw(gxtPrimitiveManager.Singleton.CircleTexture, position, null, color, 0, new Vector2(gxtPrimitiveManager.Singleton.CircleTextureRadius),
                new Vector2(radius / gxtPrimitiveManager.Singleton.CircleTextureRadius), SpriteEffects.None, depth);
        }
        #endregion Draw
    }
}
*/