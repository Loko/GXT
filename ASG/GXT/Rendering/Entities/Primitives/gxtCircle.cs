using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public class gxtCircle : gxtIEntity
    {
        protected float radius;
        protected float radiusOverTextureRadius;
        // cache (1 / circleTextureRadius)

        public float Radius { 
            get { return radius; }
            set { gxtDebug.Assert(value > 0.0f, "Must have positive radius"); gxtDebug.Assert(gxtPrimitiveManager.SingletonIsInitialized); radius = value; radiusOverTextureRadius = radius / gxtPrimitiveManager.Singleton.CircleTextureRadius; } 
        }

        // cache (1 / circle texture radius)
        // assert if the primitive manager isn't initialized
        // circle shell inherits and just overrides the draw method

        public gxtCircle(float radius)
        {
            Radius = radius;
        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            // rotationally invariant
            Vector2 absExtents = new Vector2(gxtMath.Abs(radius * scale.X), gxtMath.Abs(radius * scale.Y));
            return new gxtAABB(position, absExtents);
        }

        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, new Vector2(radius));
        }

        public virtual void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            gxtDebug.Assert(gxtPrimitiveManager.SingletonIsInitialized);

            //float radiusOverDefault = radius / gxtPrimitiveManager.Singleton.CircleTextureRadius;
            scale = new Vector2(scale.X * radiusOverTextureRadius, scale.Y * radiusOverTextureRadius);
            
            spriteBatch.DrawSprite(gxtPrimitiveManager.Singleton.CircleTexture, position, colorOverlay, rotation, new Vector2(gxtPrimitiveManager.Singleton.CircleTextureRadius),
                scale, SpriteEffects.None, renderDepth);
        }

        public void Dispose()
        {

        }
    }
}
