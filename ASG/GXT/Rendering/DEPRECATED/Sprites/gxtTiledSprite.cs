/*
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GXT.Rendering
{
    /// <summary>
    /// Tiles a sprite to fit to a surface larger than the sprite itself
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtTiledSprite : gxtIDraw
    {
        #region Fields
        public Vector2 Position { get; set; }

        public bool Visible { get; set; }

        public float Depth { get; set; }

        [ContentSerializer]
        private Vector2 fillSize;
        [ContentSerializer]
        private float rows;
        [ContentSerializer]
        private float cols;

        [ContentSerializerIgnore]
        public Texture2D Texture { get; set; }

        public float Width { get { return fillSize.X; } }
        public float Height { get { return fillSize.Y; } }
        public float SrcWidth { get { return Texture.Width; } }
        public float SrcHeight { get { return Texture.Height; } }
        public Vector2 Origin { get { return Vector2.Zero; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Paramterless constructor.  XML only!
        /// </summary>
        public gxtTiledSprite()
            : base()
        {

        }

        public gxtTiledSprite(Texture2D texture, Vector2 size)
        {
            Visible = true;
            Position = Vector2.Zero;
            Depth = 1;
            Texture = texture;
            fillSize = size;
            cols = Width / SrcWidth;
            rows = Height / SrcHeight;
        }

        public gxtTiledSprite(Texture2D texture, Vector2 size, Vector2 position)
        {
            Visible = true;
            Position = position;
            Texture = texture;
            fillSize = size;
            cols = Width / SrcWidth;
            rows = Height / SrcHeight;
        }
        #endregion Constructors

        #region Draw
        public void Draw(ref SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            Vector2 pos = Position - fillSize * 0.5f;
            float tmpRows = rows;
            float tmpCols = cols;
            float x = 0, y = 0;

            for (float r = 0; r < rows; r += 1.0f, tmpRows -= 1)
            {
                y = SrcHeight * r;
                for (float c = 0; c < cols; c += 1.0f, tmpCols -= 1)
                {
                    x = SrcWidth * c;
                    if (tmpRows * SrcHeight >= SrcHeight && tmpCols * SrcWidth >= SrcWidth)
                        DrawFull(ref spriteBatch, pos, x, y);
                    else
                        DrawPartial(ref spriteBatch, pos, x, y, new Vector2(tmpCols * SrcWidth, tmpRows * SrcHeight));
                }
                tmpCols = cols;
            }
        }

        private void DrawFull(ref SpriteBatch spriteBatch, Vector2 pos, float x, float y)
        {
            spriteBatch.Draw(Texture, pos + new Vector2(x, y), null, Color.White, 0,
                        Vector2.Zero, Vector2.One, SpriteEffects.None, Depth);
        }

        private void DrawPartial(ref SpriteBatch spriteBatch, Vector2 pos, float x, float y, Vector2 size)
        {
            spriteBatch.Draw(Texture, pos + new Vector2(x, y), new Rectangle(0, 0, (int)size.X, (int)size.Y), Color.White, 0, Origin, 1, SpriteEffects.None, Depth);
        }

        #endregion Draw

        public gxtAABB GetAABB()
        {
            return new gxtAABB(Position, fillSize * 0.5f);
        }

        public gxtOBB GetOBB()
        {
            return new gxtOBB(Position, fillSize * 0.5f);
        }
    }
}
*/
