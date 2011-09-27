using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GXT.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public class gxtRectangle : gxtIEntity
    {
        private static readonly Vector2 ORIGIN = new Vector2(0.5f, 0.5f);

        private Vector2 size;
        public Vector2 Size { get { return size; } set { gxtDebug.Assert(size.X > 0.0f && size.Y > 0.0f, "Rectangles must have positive size values"); size = value; } }

        public float Width { get { return Size.X; } set { size = new Vector2(value, size.Y); } }
        public float Height { get { return Size.Y; } set { size = new Vector2(size.X, value); } }

        public void SetFromHalfWidths(float rx, float ry)
        {
            size = new Vector2(rx * 2.0f, ry * 2.0f);
        }

        public void SetSize(float width, float height)
        {
            size = new Vector2(width, height);
        }

        public gxtRectangle()
        {

        }

        public gxtRectangle(float width, float height)
        {
            size = new Vector2(width, height);
        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            float rx = gxtMath.Abs((size.X * 0.5f) * scale.X);
            float ry = gxtMath.Abs((size.Y * 0.5f) * scale.Y);
            float cos = gxtMath.Abs(gxtMath.Cos(rotation));
            float sin = gxtMath.Abs(gxtMath.Sin(rotation));
            Vector2 r = new Vector2(rx * cos + ry * sin, rx * sin + ry * cos);
            return new gxtAABB(position, r);
            //return gxtAABB.Update(position, rotation, new gxtAABB(Vector2.Zero, new Vector2(rx, ry)));
        }

        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, new Vector2(size.X * 0.5f, size.Y * 0.5f));
        }

        public void Dispose()
        {

        }

        public void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            gxtDebug.Assert(gxtPrimitiveManager.SingletonIsInitialized);

            spriteBatch.DrawSprite(gxtPrimitiveManager.Singleton.PixelTexture, position, colorOverlay, rotation, ORIGIN, Vector2.Multiply(size, scale), spriteEffects, renderDepth);
        }
    }
}
