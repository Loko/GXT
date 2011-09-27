using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtLineLoop : gxtIDrawable
    {
        protected VertexPositionColorTexture[] vertices;
        protected int[] indices;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;

        protected gxtIMaterial material;
        protected int primitiveCount;
        protected gxtAABB localAABB;

        public gxtIMaterial Material
        {
            get { return material; }
            set
            {
                if (material != value)
                {
                    if (material != null)
                        material.RemoveListener(this);
                    material = value;
                    UpdateFromMaterial(material);
                    material.AddListener(this);
                }
            }
        }
        /*
        public void Add(Vector2 ptA, Vector2 ptB)
        {
            verticesList.Add(ptA);
            verticesList.Add(ptB);
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
            verticesList.RemoveAt(i);
            verticesList.RemoveAt(i);
        }

        public bool Contains(Vector2 ptA, Vector2 ptB)
        {
            return FindIndex(ptA, ptB) != -1;
        }

        private int FindIndex(Vector2 ptA, Vector2 ptB)
        {
            for (int i = 0; i < verticesList.Count; i += 2)
            {
                if (verticesList[i] == ptA)
                {
                    if (verticesList[i + 1] == ptB)
                        return i;
                }
                else if (verticesList[i] == ptB)
                {
                    if (verticesList[i + 1] == ptA)
                        return i;
                }
            }
            return -1;
        }
        */

        public gxtLineLoop()
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
        }

        public gxtLineLoop(gxtIMaterial material)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.material = material;
        }

        public gxtLineLoop(Vector2[] verts, gxtIMaterial material, bool setDefaultIndices = true)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            gxtDebug.Assert(verts != null && verts.Length >= 2);
            this.material = material;
            SetVertices(verts, setDefaultIndices);
        }

        public gxtLineLoop(Vector2[] verts, int[] indices)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            gxtDebug.Assert(verts != null && verts.Length >= 2);
            gxtDebug.Assert(indices != null && indices.Length >= 2);
            Set(verts, indices);
        }

        public gxtLineLoop(Vector2[] verts, int[] indices, gxtIMaterial material)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            gxtDebug.Assert(verts != null && verts.Length >= 2);
            gxtDebug.Assert(indices != null && indices.Length >= 2);
            gxtDebug.Assert(material != null);

            this.material = material;
            Set(verts, indices);
        }

        public void SetVertices(List<Vector2> verts, bool setDefaultIndices = true)
        {
            // overwrite the existing vertices/indices
            vertices = new VertexPositionColorTexture[verts.Count];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), verts.Count, BufferUsage.WriteOnly);

            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            for (int i = 0; i < verts.Count; ++i)
            {
                vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), overlay, Vector2.Zero);
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            this.localAABB = gxtGeometry.ComputeAABB(verts.GetEnumerator());

            if (setDefaultIndices)
            {
                indices = new int[verts.Count * 2];
                for (int i = 0, offset = 0; i < indices.Length; ++i, ++offset)
                {
                    indices[i] = i - offset;
                    ++i;
                    indices[i] = i - offset;
                }

                if (indexBuffer == null)
                    indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData<int>(indices);
                this.primitiveCount = indices.Length / 2;
            }
        }

        public void SetVertices(Vector2[] verts, bool setDefaultIndices = true)
        {
            // overwrite the existing vertices
            vertices = new VertexPositionColorTexture[verts.Length];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), verts.Length, BufferUsage.WriteOnly);

            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            for (int i = 0; i < verts.Length; ++i)
            {
                vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), overlay, Vector2.Zero);
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            this.localAABB = gxtGeometry.ComputeAABB(verts);

            if (setDefaultIndices)
            {
                indices = new int[verts.Length * 2];
                for (int i = 0, offset = 0; i < indices.Length; ++i, ++offset)
                {
                    indices[i] = i - offset;
                    ++i;
                    if (i != indices.Length - 1)
                        indices[i] = i - offset;
                    else
                        indices[i] = 0;
                }

                if (indexBuffer == null)
                    indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData<int>(indices);
                this.primitiveCount = indices.Length / 2;
            }
        }

        public void SetIndices(int[] indices)
        {
            /*
            if (vertices != null)
            {
                gxtDebug.SlowAssert(indices.Min() >= 0 && indices.Max() < vertices.Length);
            }
            */

            this.indices = indices;
            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        public void Set(Vector2[] vertices, int[] indices)
        {
            SetVertices(vertices, false);
            SetIndices(indices);
        }

        public bool ValidIndices(int[] indices)
        {
            return true;
        }

        /*
        public void Add(params Vector2[] verts)
        {

        }

        public void Add(Vector2 ptA, Vector2 ptB)
        {
            // assumes material is attached
            int growth = 0;
            int idxA = GetIndex(ptA);
            int idxB = GetIndex(ptB);
            if (idxA != vertices.Length)
                ++growth;
            if (idxB != vertices.Length)
                ++growth;
            if (growth != 0)
            {
                VertexPositionColorTexture[] verticesCopy = new VertexPositionColorTexture[vertices.Length + growth];
                int i;
                for (i = 0; i < verticesCopy.Length; ++i)
                {
                    verticesCopy[i] = vertices[i];
                }
                for (i = 0; i < growth; ++i)
                {
                    if (i == 0)
                        verticesCopy[vertices.Length] = new VertexPositionColorTexture(new Vector3(ptA.X, ptA.Y, 0.0f), material.ColorOverlay, Vector2.Zero);
                    else
                        verticesCopy[vertices.Length + 1] = new VertexPositionColorTexture(new Vector3(ptB.X, ptB.Y, 0.0f), material.ColorOverlay, Vector2.Zero);
                }
                vertices = verticesCopy;
                vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            }

            int[] indicesCopy = new int[indices.Length + 2];
            for (int i = 0; i < indices.Length; ++i)
            {
                indicesCopy[i] = indices[i];
            }
            indicesCopy[indices.Length] = idxA;
            indicesCopy[indices.Length + 1] = idxB;
            indices = indicesCopy;
            indexBuffer.SetData<int>(indices);
            this.primitiveCount = indices.Length / 2;
            // also need to adjust the aabb approptiately
        }

        private int GetIndex(Vector2 pt)
        {
            int i;
            for (i = 0; i < vertices.Length; ++i)
            {
                if (vertices[i].Position.X == pt.X && vertices[i].Position.Y == pt.Y)
                    return i;
            }
            return i;
        }
        */

        public gxtAABB GetLocalAABB()
        {
            return localAABB;
        }

        public void UpdateFromMaterial(gxtIMaterial material)
        {
            gxtDebug.Assert(this.material == material);
            if (material != null)
            {
                if (vertices.Length > 0 && !vertices[0].Color.Equals(material.ColorOverlay))
                {
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        vertices[i].Color = material.ColorOverlay;
                    }
                    vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
                }
            }
            else
            {
                if (vertices.Length > 0 && !vertices[0].Color.Equals(gxtMaterial.DEFAULT_COLOR_OVERLAY))
                {
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        vertices[i].Color = material.ColorOverlay;
                    }
                    vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
                }
            }
        }

        public virtual void Dispose()
        {
            if (vertexBuffer != null)
                vertexBuffer.Dispose();
            if (indexBuffer != null)
                indexBuffer.Dispose();
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Vector2 position, float rotation, ref Vector2 scale, SpriteEffects spriteEffects)
        {
            if (material != null)
            {
                if (material.Visible)
                {
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineList, null, vertexBuffer, indexBuffer, primitiveCount, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
                }
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                {
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineList, null, vertexBuffer, indexBuffer, primitiveCount, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to the indexed primitive!  Pos: {0, 1}", position.X, position.Y);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (material != null)
            {
                if (material.Visible)
                {
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineList, null, vertexBuffer, indexBuffer, primitiveCount, ref transform, material.RenderDepth);
                }
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                {
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineList, null, vertexBuffer, indexBuffer, primitiveCount, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to the indexed primitive!  Pos: {0, 1}", transform.M14, transform.M24);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }
    }
}
