using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GXT.IO;

namespace GXT.Rendering
{
    public class gxtSprite : gxtIEntity
    {
        private Texture2D texture;
        public Texture2D Texture { get { return texture; } set { texture = value; UpdateOrigin(); } }
        
        private Vector2 origin;

        public gxtSprite(Texture2D texture)
        {
            Texture = texture;
        }

        public bool LoadTexture(string name)
        {
            return gxtResourceManager.Singleton.LoadTexture(name, out texture);
        }

        private void UpdateOrigin()
        {
            if (texture != null && !texture.IsDisposed)
                origin = new Vector2(Texture.Width * 0.5f, Texture.Height * 0.5f);
        }

        public void Dispose()
        {
            if (texture != null)
                texture.Dispose();
        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            float rx = gxtMath.Abs(origin.X * scale.X);
            float ry = gxtMath.Abs(origin.Y * scale.Y);
            return gxtAABB.Update(position, rotation, new gxtAABB(Vector2.Zero, new Vector2(rx, ry)));
        }

        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, new Vector2(origin.X, origin.Y));
        }

        public void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            if (texture != null)
                spriteBatch.DrawSprite(Texture, position, colorOverlay, rotation, origin, scale, spriteEffects, renderDepth);
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Drawable Sprite Does Not Have A Texture (Position: " + position.ToString() + " )");
                if (gxtDebugDrawer.SingletonIsInitialized)
                {
                    //if (!gxtDebugDrawer.Singleton.IsSet(true))
                    //    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The Debug Drawer Is Not Set: A \"NO TEXTURE\" string will not be drawn");
                    gxtDebugDrawer.Singleton.AddString("NO TEXTURE", position, Color.Magenta, 0.0f);
                }
            }
        }
    }
}
