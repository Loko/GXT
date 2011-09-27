using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtCircleShell : gxtCircle
    {
        public gxtCircleShell(float radius) : base(radius)
        {

        }

        public override void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            gxtDebug.Assert(gxtPrimitiveManager.SingletonIsInitialized);

            //float radiusOverDefault = radius / gxtPrimitiveManager.Singleton.CircleTextureRadius;
            scale = new Vector2(scale.X * radiusOverTextureRadius, scale.Y * radiusOverTextureRadius);
            //scale = new Vector2(scale.X * radiusOverDefault, scale.Y * radiusOverDefault);

            spriteBatch.DrawSprite(gxtPrimitiveManager.Singleton.CircleShellTexture, position, colorOverlay, 0, new Vector2(gxtPrimitiveManager.Singleton.CircleTextureRadius),
                scale, SpriteEffects.None, renderDepth);
        }
    }
}
