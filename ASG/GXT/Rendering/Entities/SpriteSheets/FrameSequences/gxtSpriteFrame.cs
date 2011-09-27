using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    /// <summary>
    /// Frame, used for variable frame sequences
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSpriteFrame
    {
        #region Fields
        /// <summary>
        /// Rectangle for this frame
        /// </summary>
        public Rectangle FrameRectangle { get; set; }

        /// <summary>
        /// Origin for this frame
        /// </summary>
        public Vector2 FrameOrigin { get; set; }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public gxtSpriteFrame()
        {
            FrameRectangle = new Rectangle();
            FrameOrigin = new Vector2(FrameRectangle.Width / 2.0f, FrameRectangle.Height / 2.0f);
        }

        /// <summary>
        /// Defines frame rectangle
        /// </summary>
        /// <param name="frameRectangle">Rect</param>
        public gxtSpriteFrame(Rectangle frameRectangle)
        {
            FrameRectangle = frameRectangle;
            FrameOrigin = new Vector2(FrameRectangle.Width / 2.0f, FrameRectangle.Height / 2.0f);
        }

        /// <summary>
        /// Defines frame rect and origin
        /// </summary>
        /// <param name="frameRectangle">Rect</param>
        /// <param name="frameOrigin">Origin</param>
        public gxtSpriteFrame(Rectangle frameRectangle, Vector2 frameOrigin)
        {
            FrameRectangle = frameRectangle;
            FrameOrigin = frameOrigin;
        }
        #endregion Constructors
    }
}
