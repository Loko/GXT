using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtLine : gxtIEntity
    {
        private Vector2 start, end;
        
        private Vector2 lineScale;
        private float angle;

        public Vector2 Start { get { return start; } set { start = value; UpdateCachedValues(); } }
        public Vector2 End { get { return end; } set { end = value; UpdateCachedValues(); } }

        public float LineThickness { get { return lineScale.Y; } set { gxtDebug.Assert(value > 0.0f); lineScale = new Vector2(lineScale.X, value); } }

        public static readonly Vector2 ORIGIN = new Vector2(0.0f, 0.5f);

        public gxtLine(float lineThickness = 2.0f)
        {
            //start = Vector2.Zero;
            //end = Vector2.Zero;
            LineThickness = lineThickness;
        }

        public gxtLine(Vector2 start, Vector2 end, float lineThickness = 2.0f)
        {
            // line thickness should be some constant, at least to start
            // then, if we want something more flexible, a scale multiplier for all lines that 
            // adjusts when the resolution changes, or the camera zooms out

            // lines can be cut out and simply not rendered by direct x if the line thickness is too small
            this.start = start;
            this.end = end;
            LineThickness = lineThickness;
            UpdateCachedValues();
        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            Vector2 c = (Start + End) * 0.5f;
            float rX = gxtMath.Abs((End.X - c.X) * scale.X);
            float rY = gxtMath.Abs((End.Y - c.Y) * scale.Y);
            gxtAABB localAABB = new gxtAABB(c, new Vector2(rX, rY));
            return gxtAABB.Update(position, rotation, localAABB);
        }

        public gxtAABB GetLocalAABB()
        {
            Vector2 c = (Start + End) * 0.5f;
            float rX = gxtMath.Abs(End.X - c.X);
            float rY = gxtMath.Abs(End.Y - c.Y);
            return new gxtAABB(c, new Vector2(rX, rY));
        }

        private void UpdateCachedValues()
        {
            Vector2 d = end - start;
            angle = gxtMath.Atan2(d.Y, d.X);
            float dist = d.Length() + 1.0f;
            lineScale = new Vector2(dist, lineScale.Y);
        }

        public void Dispose()
        {

        }

        public void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            gxtDebug.Assert(gxtPrimitiveManager.SingletonIsInitialized);

            Matrix rotMat = Matrix.CreateRotationZ(rotation);
            Vector2 tStart = Vector2.Transform(Vector2.Multiply(start, scale), rotMat) + position;

            spriteBatch.DrawSprite(gxtPrimitiveManager.Singleton.PixelTexture, tStart, colorOverlay, angle + rotation, ORIGIN, 
                Vector2.Multiply(scale, lineScale), spriteEffects, renderDepth);
        }
    }
}
