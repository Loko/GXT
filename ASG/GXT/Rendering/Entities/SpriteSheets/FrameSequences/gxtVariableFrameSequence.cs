using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    /// <summary>
    /// Defines a sheet with variable frames and does not need a rectangular sheet
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtVariableFrameSequence : gxtIFrameSequence
    {
        #region Fields
        [ContentSerializer]
        int pos;    // Position in the sequence
        [ContentSerializer]
        gxtSpriteFrame[] frames; // Sequence, rectangles and origins

        /// <summary>
        /// Current rectangle
        /// </summary>
        public Rectangle CurrentFrameRect { get { return frames[pos].FrameRectangle; } }

        /// <summary>
        /// Current frame origin
        /// </summary>
        public Vector2 CurrentFrameOrigin { get { return frames[pos].FrameOrigin; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Parameterless constructor.  XML only!
        /// </summary>
        public gxtVariableFrameSequence() { }

        /// <summary>
        /// Takes collection of frames
        /// </summary>
        /// <param name="frames">Frames</param>
        public gxtVariableFrameSequence(gxtSpriteFrame[] frames)
        {
            this.frames = frames;
            pos = 0;
        }
        #endregion Constructors

        #region Iterator
        /// <summary>
        /// Moves forward in the sequence
        /// </summary>
        public void Forwards()
        {
            if (AtEnd())
                ToStart();
            else
                pos++;
        }

        /// <summary>
        /// Moves backwards in the sequence
        /// </summary>
        public void Backwards()
        {
            if (AtStart())
                ToEnd();
            else
                pos--;
        }

        /// <summary>
        /// Puts position at start
        /// </summary>
        public void ToStart()
        {
            pos = 0;
        }

        /// <summary>
        /// Puts position at end
        /// </summary>
        public void ToEnd()
        {
            pos = frames.Length - 1;
        }

        public int GetPosition()
        {
            return this.pos;
        }

        /// <summary>
        /// Puts position at given index
        /// </summary>
        /// <param name="index">index</param>
        public void ToPosition(int index)
        {
            this.pos = index;
        }

        /// <summary>
        /// Tells if the sheet is at the start
        /// </summary>
        /// <returns>At start?</returns>
        public bool AtStart()
        {
            return pos == 0;
        }

        /// <summary>
        /// Tells if the sheet is at end
        /// </summary>
        /// <returns>At end?</returns>
        public bool AtEnd()
        {
            return pos == frames.Length - 1;
        }
        #endregion Iterator
    }
}
