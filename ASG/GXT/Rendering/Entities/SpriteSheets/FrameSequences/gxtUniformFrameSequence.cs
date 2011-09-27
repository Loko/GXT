using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GXT.Rendering
{
    /// <summary>
    /// Defines a frame sequence for a sheet with uniformily sized frames
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtUniformFrameSequence : gxtIFrameSequence
    {
        #region Fields
        [ContentSerializer]
        int pos;    // Position in the sequence
        [ContentSerializer]
        Rectangle[] sourceRectangles;   // Collection of rectangles

        /// <summary>
        /// Current Rectangle at position
        /// </summary>
        public Rectangle CurrentFrameRect { get { return sourceRectangles[pos]; } }

        /// <summary>
        /// Constant origin
        /// </summary>
        [ContentSerializer]
        private Vector2 sequenceFrameOrigin;
        public Vector2 CurrentFrameOrigin { get { return sequenceFrameOrigin; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Paramterless constructor.  XML only!
        /// </summary>
        public gxtUniformFrameSequence() { }

        /// <summary>
        /// Similiar to above, but uses ints rather than a point struct
        /// </summary>
        /// <param name="frameWidth">Width of each frame</param>
        /// <param name="frameHeight">Height of each frame</param>
        /// <param name="sheetRows">Rows in the sheet</param>
        /// <param name="sheetCols">Columns in the sheet</param>
        public gxtUniformFrameSequence(int frameWidth, int frameHeight, int sheetRows, int sheetCols)
        {
            createRectangles(frameWidth, frameHeight, sheetRows, sheetCols);
            pos = 0;
            sequenceFrameOrigin = new Vector2(frameWidth * 0.5f, frameHeight * 0.5f);
        }

        public gxtUniformFrameSequence(int frameWidth, int frameHeight, int sheetRows, int sheetCols, Vector2 startPosition)
        {
            createRectangles(frameWidth, frameHeight, sheetRows, sheetCols, startPosition);
            pos = 0;
            sequenceFrameOrigin = new Vector2(frameWidth * 0.5f, frameHeight * 0.5f);
        }
        #endregion Constructors

        #region RectCreation
        /// <summary>
        /// Overload of above
        /// </summary>
        /// <param name="frameWidth">Width of each frame</param>
        /// <param name="frameHeight">Height of each frame</param>
        /// <param name="sheetRows">Sheet Rows</param>
        /// <param name="sheetCols">Sheet Height</param>
        private void createRectangles(int frameWidth, int frameHeight, int sheetRows, int sheetCols)
        {
            sourceRectangles = new Rectangle[sheetRows * sheetCols];

            for (int row = 0; row < sheetRows; row++)
            {
                for (int col = 0; col < sheetCols; col++)
                {
                    sourceRectangles[row * sheetCols + col] = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
                }
            }
        }

        private void createRectangles(int frameWidth, int frameHeight, int sheetRows, int sheetCols, Vector2 startPos)
        {
            sourceRectangles = new Rectangle[sheetRows * sheetCols];

            for (int row = 0; row < sheetRows; row++)
            {
                for (int col = 0; col < sheetCols; col++)
                {
                    sourceRectangles[row * sheetCols + col] = new Rectangle(((int)startPos.X) + col * frameWidth, ((int)startPos.Y) + row * frameHeight, frameWidth, frameHeight);
                }
            }
        }
        #endregion RectCreation

        #region Iterator
        /// <summary>
        /// Moves forwards in sequence
        /// </summary>
        public void Forwards()
        {
            if (AtEnd())
                ToStart();
            else
                pos++;
        }

        /// <summary>
        /// Moves backwards in sequence
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
            pos = sourceRectangles.Length - 1;
        }

        /// <summary>
        /// Puts position at given index
        /// </summary>
        /// <param name="index">index</param>
        public void ToPosition(int index)
        {
            this.pos = index;
        }

        public int GetPosition()
        {
            return this.pos;
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
        /// Tells if the sheet is at the end
        /// </summary>
        /// <returns>At end?</returns>
        public bool AtEnd()
        {
            return pos == sourceRectangles.Length - 1;
        }
        #endregion Iterator
    }
}
