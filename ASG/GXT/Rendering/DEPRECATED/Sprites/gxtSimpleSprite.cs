/*
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GXT.Rendering
{
    /// <summary>
    /// Class for a basic sprite with a static size.  Gives control
    /// over position, the texture, and the render depth.  Origin will
    /// always be in the center of the sprite.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSimpleSprite : gxtIDraw
    {
        #region Fields
        /// <summary>
        /// Visible?
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Position
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Render Depth
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Texture
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Dimensions.
        /// </summary>
        public virtual float Width { get { return Texture.Width; } }
        public virtual float Height { get { return Texture.Height; } }

        /// <summary>
        /// Set origin in sprite center
        /// </summary>
        public Vector2 Origin { get { return new Vector2(Texture.Width * 0.5f, Texture.Height * 0.5f); } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Paramterless constructor.  XML only!
        /// </summary>
        public gxtSimpleSprite() { }

        /// <summary>
        /// Takes a texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public gxtSimpleSprite(Texture2D texture)
        {
            Visible = true;
            Texture = texture;
            Position = Vector2.Zero;
            Depth = 1;

        }

        /// <summary>
        /// Takes a texture and a position
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        public gxtSimpleSprite(Texture2D texture, Vector2 position)
        {
            Visible = true;
            Texture = texture;
            Position = position;
            Depth = 1;
        }

        /// <summary>
        /// Takes a texture, a position, and a render depth
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        /// <param name="depth">Depth</param>
        public gxtSimpleSprite(Texture2D texture, Vector2 position, float depth)
        {
            Visible = true;
            Texture = texture;
            Position = position;
            Depth = depth;
        }
        #endregion Constructors

        #region Draw
        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public virtual void Draw(ref SpriteBatch spriteBatch)
        {
            if (Visible)
                spriteBatch.Draw(Texture, Position, null, Color.White, 0, Origin, 1, SpriteEffects.None, Depth);
        }

        public static void Draw(ref SpriteBatch spriteBatch, Texture2D texture, Vector2 position, float depth)
        {
            Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;
            spriteBatch.Draw(texture, position, null, Color.White, 0, origin, 1, SpriteEffects.None, depth);
        }
        #endregion Draw

        #region Culling
        public virtual gxtAABB GetAABB()
        {
            return new gxtAABB(Position, Origin);
        }

        public virtual gxtOBB GetOBB()
        {
            return new gxtOBB(Position, Origin);
        }
        #endregion Culling
    }
}
*/
