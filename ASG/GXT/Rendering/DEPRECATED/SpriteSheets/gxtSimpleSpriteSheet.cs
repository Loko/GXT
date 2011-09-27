/*
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// Combines many components into a spritesheet
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSimpleSpriteSheet : gxtIDraw
    {
        #region Fields
        /// <summary>
        /// 
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Texture
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Sequence of frames
        /// </summary>
        public gxtIFrameSequence FrameSequence { get; set; }

        /// <summary>
        /// Origin of current rectangle
        /// </summary>
        public Vector2 Origin { get { return FrameSequence.CurrentFrameOrigin; } }

        /// <summary>
        /// Dimensions
        /// </summary>
        public virtual float Width { get { return FrameSequence.CurrentFrameRect.Width; } }
        public virtual float Height { get { return FrameSequence.CurrentFrameRect.Height; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Parameterless constrcutor.  XML only!
        /// </summary>
        public gxtSimpleSpriteSheet() : base() { }

        /// <summary>
        /// Takes texture, a frame sequence, and an animation behavoir
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="frameSequence">FrameSequence</param>
        /// <param name="animBehavoir">AnimBehavior</param>
        public gxtSimpleSpriteSheet(Texture2D texture, gxtIFrameSequence frameSequence)
        {
            Visible = true;
            Position = Vector2.Zero;
            Depth = 1.0f;
            Texture = texture;
            FrameSequence = frameSequence;
        }

        /// <summary>
        /// Same as above, but takes position
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="position">Position</param>
        /// <param name="frameSequence">FrameSequence</param>
        /// <param name="animBehavoir">AnimBehavoir</param>
        public gxtSimpleSpriteSheet(Texture2D texture, Vector2 position, gxtIFrameSequence frameSequence)
        {
            Visible = true;
            Position = Vector2.Zero;
            Depth = 1.0f;
            Texture = texture;
            FrameSequence = frameSequence;
        }
        #endregion Constructors

        #region Draw
        /// <summary>
        /// Draws the current frame of the spritesheet
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public virtual void Draw(ref SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, FrameSequence.CurrentFrameRect, Color.White,
                0, FrameSequence.CurrentFrameOrigin, 1, SpriteEffects.None, Depth);
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
