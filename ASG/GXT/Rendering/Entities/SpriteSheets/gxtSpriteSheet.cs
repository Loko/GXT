using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtSpriteSheet
    {
        private float renderDepth;
        public float RenderDepth { get { return renderDepth; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f, "Render Depths Must Be Between 0 and 1"); renderDepth = value; } }

        private Color colorOverlay;
        public Color ColorOverlay { get { return colorOverlay; } set { colorOverlay = value; } }

        private Texture2D texture;
        public Texture2D Texture { get { return texture; } set { texture = value; } }

        private gxtIFrameSequence frameSequence;
        public gxtIFrameSequence FrameSequence { get { return frameSequence; } set { frameSequence = value; } }

        private Vector2 Origin { get { return FrameSequence.CurrentFrameOrigin; } }

        public gxtSpriteSheet(float renderDepth = 0.5f)
        {
            RenderDepth = renderDepth;
            ColorOverlay = Color.White;
        }

        public gxtSpriteSheet(Texture2D texture, gxtIFrameSequence frameSequence)
        {
            this.texture = texture;
            this.frameSequence = frameSequence;
            ColorOverlay = Color.White;
        }

        public void Dispose()
        {
            if (texture != null && !texture.IsDisposed)
                texture.Dispose();
        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            float rX = gxtMath.Abs(Origin.X * scale.X);
            float rY = gxtMath.Abs(Origin.Y * scale.Y);
            gxtAABB aabb = new gxtAABB(Vector2.Zero, new Vector2(rX, rY));
            aabb = aabb.GetRotatedAABB(rotation);
            aabb.Translate(position);
            return aabb; 
        }

        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, Origin.X * 0.5f, Origin.Y * 0.5f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            if (texture != null && frameSequence != null)
            {
                    spriteBatch.Draw(texture, position, FrameSequence.CurrentFrameRect, colorOverlay, rotation,
                        FrameSequence.CurrentFrameOrigin, scale, spriteEffects, renderDepth);
            }
            else
            {
                if (texture == null)
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Drawable Sprite Sheet Does Not Have A Texture (Position: {0})", position.ToString());
                    if (gxtDebugDrawer.SingletonIsInitialized)
                    {
                        // check if debug drawer has a spritefont??
                        gxtDebugDrawer.Singleton.AddString("NO SPRITE SHEET TEXTURE", position, Color.Magenta, 0.0f);
                    }
                }
                if (frameSequence == null)
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Drawable Sprite Sheet Does Not Have A Frame Sequence (Position: {0}, Texture: {1})", position.ToString(), texture.Name);
                    {
                        if (gxtDebugDrawer.SingletonIsInitialized)
                        {
                            gxtDebugDrawer.Singleton.AddString("NO SPRITE SHEET FRAME SEQUENCE", position, Color.Magenta, 0.0f);
                        }
                    }
                }
            }
        }
    }
}
