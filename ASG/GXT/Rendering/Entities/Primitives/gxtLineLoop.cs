using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtLineLoop : gxtIEntity
    {
        private float lineThickness;
        private List<Vector2> vertices;

        // X = distance between the two lines
        // Y = angle between the two points
        private List<Vector2> cachedValues;
        
        public float LineThickness { get { return lineThickness; } set { gxtDebug.Assert(value > 0.0f); lineThickness = value; } }

        public List<Vector2> Vertices { get { return vertices; } set { vertices = value; RecomputeCachedValues(); } }

        public gxtLineLoop(float lineThickness = 2.0f)
        {
            LineThickness = lineThickness;
            vertices = new List<Vector2>();
            cachedValues = new List<Vector2>();
        }

        public gxtLineLoop(Vector2[] verts, float lineThickness = 2.0f)
        {
            LineThickness = lineThickness;
            SetVertices(verts);
        }

        public void SetVertices(IEnumerable<Vector2> verts)
        {
            Vertices = new List<Vector2>(verts);
        }

        public void SetVertices(List<Vector2> verts)
        {
            Vertices = verts;
        }

        public void Add(Vector2 pt)
        {
            vertices.Add(pt);
            RecomputeCachedValues(vertices.Count - 2);
            RecomputeCachedValues(vertices.Count - 1);
        }

        public bool Remove(Vector2 pt)
        {
            int index = vertices.FindIndex(item => item == pt);
            if (index == -1)
                return false;
            vertices.RemoveAt(index);
            cachedValues.RemoveAt(index);
            int prevIndex = index == 0 ? vertices.Count - 1 : index - 1;
            RecomputeCachedValues(prevIndex);
            return true;
        }

        private void RecomputeCachedValues()
        {
            cachedValues = new List<Vector2>(vertices.Count + 1);
            for (int i = 0; i < vertices.Count; i++)
            {
                int nextIndex = i == vertices.Count - 1 ? 0 : i + 1;
                Vector2 d = vertices[nextIndex] - vertices[i];
                float angle = gxtMath.Atan2(d.Y, d.X);
                float dist = d.Length() + 1.0f;
                cachedValues.Add(new Vector2(dist, angle));
                //cachedValues[j] = new Vector2(dist, angle);
            }
        }

        private void RecomputeCachedValues(int index)
        {
            int nextIndex = (index + 1) % vertices.Count;
            Vector2 d = vertices[nextIndex] - vertices[index];
            float angle = gxtMath.Atan2(d.Y, d.X);
            float dist = d.Length() + 1.0f;
            cachedValues[index] = new Vector2(dist, angle);
        }

        public void Dispose()
        {
            vertices.Clear();
            cachedValues.Clear();
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
            for (int j = vertices.Count - 1, i = 0; i < vertices.Count; j = i, i++)
            {
                Vector2 tStart = Vector2.Transform(Vector2.Multiply(vertices[j], scale), rotMat) + position;
                //Vector2 tEnd = Vector2.Transform(Vector2.Multiply(vertices[i], scale), rotMat) + position;
                //Vector2 tD = tEnd - tStart;
                //float ang = gxtMath.Atan2(tD.Y, tD.X);
                //float dist = tD.Length() + 1.0f;

                //gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "New Distance: {0}", dist);
                //gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Precached Distance: {0}", cachedValues[j].X);
                //gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Scale: {0}", scale.ToString());

                //Vector2 xAxis, yAxis;
                //gxtMath.GetAxesVectors(rotation, out xAxis, out yAxis);

                //float absScaleX = 1.0f - gxtMath.Abs(scale.X - 1.0f);
                //if (scale.X == 1.0f)
                //    absScaleX = 1.0f;
                spriteBatch.DrawSprite(gxtPrimitiveManager.Singleton.PixelTexture, tStart, colorOverlay, cachedValues[j].Y + rotation, gxtLine.ORIGIN,
                    new Vector2((cachedValues[j].X * gxtMath.Abs(scale.X)), lineThickness), spriteEffects, renderDepth);
                /*
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, scale.ToString());

                float finalRot = cachedValues[j].Y + rotation;
                Vector2 xAxis, yAxis;
                gxtMath.GetAxesVectors(cachedValues[j].Y, out xAxis, out yAxis);
                xAxis *= (cachedValues[j].X * scale.X);
                yAxis *= (cachedValues[j].X * scale.Y);
                float dist = (xAxis + yAxis).Length();
                spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, tStart, null, colorOverlay, finalRot, origin,
                    new Vector2(dist, lineThickness), spriteEffects, renderDepth);
                */
                //spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, tStart, null, colorOverlay, cachedValues[j].Y + rotation, origin,
                //    new Vector2(cachedValues[j].X * scale.X * scale.Y, lineThickness), spriteEffects, renderDepth);
                
                /* Expensive, but works */
                //spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, tStart, null, colorOverlay, ang, origin,
                //    new Vector2(dist, lineThickness), spriteEffects, renderDepth);
            }
        }

    }
}
