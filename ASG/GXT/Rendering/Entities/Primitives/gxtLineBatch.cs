using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtLineBatch : gxtIEntity
    {
        private float lineThickness;

        private List<Vector2> vertices;
        private List<Vector2> cachedValues;

        public float LineThickness { get { return lineThickness; } set { lineThickness = value; } }

        public int NumLines { get { return vertices.Count / 2; } }

        public gxtLineBatch(float lineThickness = 2.0f)
        {
            vertices = new List<Vector2>();
            cachedValues = new List<Vector2>();
            LineThickness = lineThickness;
        }

        public bool IsValid()
        {
            return vertices.Count % 2 == 0 && cachedValues.Count == vertices.Count / 2;
        }

        public void Clear()
        {
            vertices.Clear();
            cachedValues.Clear();
        }

        private void RecomputeCachedValues()
        {
            int halfSize = (vertices.Count + 1) / 2;
            cachedValues = new List<Vector2>(halfSize);
            for (int i = 0; i < vertices.Count; i += 2)
            {
                Vector2 d = vertices[i + 1] - vertices[i];
                float angle = gxtMath.Atan2(d.Y, d.X);
                float dist = d.Length() + 1.0f;
                cachedValues[i / 2] = new Vector2(dist, angle);
            }
        }

        private void AddCachedValues(ref Vector2 ptA, ref Vector2 ptB)
        {
            Vector2 d = ptB - ptA;
            float angle = gxtMath.Atan2(d.Y, d.X);
            float dist = d.Length() + 1.0f;
            cachedValues.Add(new Vector2(dist, angle));
        }

        public void Add(Vector2 ptA, Vector2 ptB)
        {
            vertices.Add(ptA);
            vertices.Add(ptB);
            AddCachedValues(ref ptA, ref ptB);
        }

        public bool Remove(Vector2 ptA, Vector2 ptB)
        {
            int idx = FindIndex(ptA, ptB);

            if (idx == -1)
                return false;

            RemoveAt(idx);
            return true;
        }

        private void RemoveAt(int i)
        {
            // it shifts down so we remove both by calling it twice
            vertices.RemoveAt(i);
            vertices.RemoveAt(i);

            //int halfI = i / 2;
            cachedValues.RemoveAt(i / 2);
        }

        public bool Contains(Vector2 ptA, Vector2 ptB)
        {
            return FindIndex(ptA, ptB) != -1;
        }

        private int FindIndex(Vector2 ptA, Vector2 ptB)
        {
            for (int i = 0; i < vertices.Count; i += 2)
            {
                if (vertices[i] == ptA)
                {
                    if (vertices[i + 1] == ptB)
                        return i;
                }
                else if (vertices[i] == ptB)
                {
                    if (vertices[i + 1] == ptA)
                        return i;
                }
            }
            return -1;
        }

        public void Dispose()
        {
            Clear();
        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            gxtAABB aabb = gxtGeometry.ComputeAABB(vertices.GetEnumerator());
            aabb.Extents = new Vector2(gxtMath.Abs(aabb.Extents.X * scale.X), gxtMath.Abs(aabb.Extents.Y * scale.Y));
            return gxtAABB.Update(position, rotation, aabb);
        }

        public gxtAABB GetLocalAABB()
        {
            return gxtGeometry.ComputeAABB(vertices.GetEnumerator());
        }

        public void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            gxtDebug.Assert(gxtPrimitiveManager.SingletonIsInitialized);

            Matrix rotMat = Matrix.CreateRotationZ(rotation);
            for (int i = 0; i < vertices.Count; i += 2)
            {
                Vector2 tStart = Vector2.Transform(Vector2.Multiply(vertices[i], scale), rotMat) + position;

                int halfIndex = i / 2;

                spriteBatch.DrawSprite(gxtPrimitiveManager.Singleton.PixelTexture, tStart, colorOverlay, cachedValues[halfIndex].Y + rotation, gxtLine.ORIGIN,
                    Vector2.Multiply(new Vector2(cachedValues[halfIndex].X, lineThickness), scale), spriteEffects, renderDepth);
            }
        }
    }
}
