using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtMesh : gxtIMesh
    {
        protected Texture2D texture;
        protected gxtIMaterial material;

        protected VertexPositionColorTexture[] vertices;
        protected int[] indices;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;
        
        protected gxtAABB localAABB;
        protected int primitiveCount;

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

        public Texture2D Texture { get { return texture; } set { texture = value; } }

        public int NumVertices { get { return (vertices != null) ? vertices.Length : 0; } }

        public gxtAABB GetLocalAABB()
        {
            // does not work right for scaled AABBs and definetly not negative scales
            return localAABB;
        }

        public gxtMesh()
        {

        }

        public gxtMesh(Vector2[] verts, bool setDefaultIndices = true)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            SetVertices(verts, setDefaultIndices);
        }

        public gxtMesh(Vector2[] verts, gxtIMaterial material, bool setDefaultIndices = true)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.material = material;
            SetVertices(verts, setDefaultIndices);
        }

        public gxtMesh(Vector2[] verts, gxtIMaterial material, Texture2D texture, gxtTextureCoordinateType uvType = gxtTextureCoordinateType.CLAMP, bool setDefaultIndices = true)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.material = material;
            this.texture = texture;
            Vector2[] texCoords = gxtGeometry.CalculateTextureCoordinates(verts, texture.Width, texture.Height, uvType);
            SetVertices(verts, texCoords, setDefaultIndices);
        }

        public gxtMesh(Vector2[] verts, gxtIMaterial material, Texture2D texture, Vector2[] textureCoordinates, bool setDefaultIndices = true)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.material = material;
            this.texture = texture;
            SetVertices(verts, textureCoordinates, setDefaultIndices);
        }


        public gxtMesh(Vector2[] verts, int[] indices)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.primitiveCount = indices.Length / 3;
            // set up vertices
            SetupMesh(verts, indices);
        }

        // shallow copy with a different material

        public void ApplyTexture(Texture2D texture, gxtTextureCoordinateType uvType)
        {
            this.texture = texture;
            // this copy is expensive...
            Vector2[] verts = new Vector2[vertices.Length];
            int i;
            for (i = 0; i < verts.Length; ++i)
            {
                verts[i] = new Vector2(vertices[i].Position.X, vertices[i].Position.Y);
            }
            // uv coordinates
            Vector2[] uvCoords = gxtGeometry.CalculateTextureCoordinates(verts, texture.Width, texture.Height, uvType);
            for (i = 0; i < verts.Length; ++i)
            {
                vertices[i].TextureCoordinate = uvCoords[i];
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
        }

        public void ApplyTexture(Texture2D texture, Vector2 textureScale, float textureRotation, gxtTextureCoordinateType uvType)
        {
            this.texture = texture;
            Vector2[] verts = new Vector2[vertices.Length];

            int i;
            for (i = 0; i < verts.Length; ++i)
            {
                verts[i] = new Vector2(vertices[i].Position.X, vertices[i].Position.Y);
            }
            Vector2[] uvCoords = gxtGeometry.CalculateTextureCoordinates(verts, texture.Width, texture.Height, textureScale, textureRotation, uvType);
            for (i = 0; i < verts.Length; ++i)
            {
                vertices[i].TextureCoordinate = uvCoords[i];
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
        }

        public void SetTextureCoordinates(Vector2[] textureCoordinates)
        {
            gxtDebug.Assert(this.vertices != null);
            gxtDebug.Assert(textureCoordinates != null);
            gxtDebug.Assert(this.vertices.Length == textureCoordinates.Length);

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].TextureCoordinate = textureCoordinates[i];
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
        }

        public void UpdateFromMaterial(gxtIMaterial material)
        {
            gxtDebug.Assert(this.material == material);
            if (material != null)
            {
                if (vertices != null && vertices.Length > 0 && !vertices[0].Color.Equals(material.ColorOverlay))
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
                if (vertices != null && vertices.Length > 0 && !vertices[0].Color.Equals(gxtMaterial.DEFAULT_COLOR_OVERLAY))
                {
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        vertices[i].Color = material.ColorOverlay;
                    }
                    vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
                }
            }
        }

        public void SetVertices(Vector2[] verts, bool setDefaultIndices = true)
        {
            gxtDebug.Assert(verts != null && verts.Length >= 3, "Vertices in a triangle mesh cannot be null or have a size less than 3!");

            vertices = new VertexPositionColorTexture[verts.Length];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), verts.Length, BufferUsage.WriteOnly);
            
            if (material == null)
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.Zero);
                }
            }
            else
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.Zero);
                }
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            localAABB = gxtGeometry.ComputeAABB(verts);
            
            if (setDefaultIndices)
                SetDefaultIndices(3 + ((verts.Length - 3) * 3));
        }

        public void SetVertices(Vector2[] verts, Vector2[] textureCoordinates, bool setDefaultIndices = true)
        {
            gxtDebug.Assert(verts != null && verts.Length >= 3);
            gxtDebug.Assert(indices != null && indices.Length >= 3);
            gxtDebug.Assert(textureCoordinates != null && textureCoordinates.Length >= 3);
            gxtDebug.Assert(verts.Length == textureCoordinates.Length);

            vertices = new VertexPositionColorTexture[verts.Length];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), verts.Length, BufferUsage.WriteOnly);
            
            if (material == null)
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, textureCoordinates[i]);
                }
            }
            else
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), material.ColorOverlay, textureCoordinates[i]);
                }
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            localAABB = gxtGeometry.ComputeAABB(verts);

            if (setDefaultIndices)
                SetDefaultIndices(3 + ((verts.Length - 3) * 3));
        }

        public void SetIndices(int[] indices)
        {
            gxtDebug.Assert(indices != null && indices.Length >= 3);

            if (this.indices == null)
                this.indices = new int[indices.Length];
            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indices.Length, BufferUsage.WriteOnly);
            for (int i = 0; i < indices.Length; ++i)
            {
                this.indices[i] = indices[i];
            }
            indexBuffer.SetData<int>(this.indices);
            this.primitiveCount = this.indices.Length / 3;
        }

        private void SetDefaultIndices(int indicesArraySize)
        {
            indices = new int[indicesArraySize];
            for (int i = 0, j = 1; i < indicesArraySize; i += 3)
            {
                indices[i] = 0;
                indices[i + 1] = j;
                ++j;
                indices[i + 2] = j;
            }

            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indicesArraySize, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);

            this.primitiveCount = indicesArraySize / 3;
        }

        public void SetupMesh(Vector2[] verts, int[] indices)
        {
            gxtDebug.Assert(verts != null && verts.Length >= 3, "Vertices array is null or less than 3 vertices");
            gxtDebug.Assert(indices != null && indices.Length >= 3, "Indices array is null or less than 3 indices");

            vertices = new VertexPositionColorTexture[verts.Length];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), verts.Length, BufferUsage.WriteOnly);

            // set up vertices
            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            for (int i = 0; i < verts.Length; ++i)
            {
                vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), overlay, Vector2.Zero);
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            localAABB = gxtGeometry.ComputeAABB(verts);

            // set up indices
            // deep copy necessary?
            for (int i = 0; i < indices.Length; ++i)
            {
                this.indices[i] = indices[i];
            }
            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
            this.primitiveCount = indices.Length / 3;
        }

        public void SetupMesh(Vector2[] verts, int[] indices, Vector2[] textureCoordinates)
        {
            gxtDebug.Assert(verts != null && verts.Length >= 3, "Vertices array is null or less than 3 vertices");
            gxtDebug.Assert(indices != null && indices.Length >= 3, "Indices array is null or less than 3 indices");
            gxtDebug.Assert(textureCoordinates != null && textureCoordinates.Length >= 3, "Texture Coordinates array is null or less than 3 UV coordinates");
            gxtDebug.Assert(verts.Length == textureCoordinates.Length, "The vertices array and texture coordinates array have a size mismatch!");

            // set up verts
            vertices = new VertexPositionColorTexture[verts.Length];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), verts.Length, BufferUsage.WriteOnly);

            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            for (int i = 0; i < verts.Length; ++i)
            {
                vertices[i] = new VertexPositionColorTexture(new Vector3(verts[i].X, verts[i].Y, 0.0f), overlay, textureCoordinates[i]);
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            localAABB = gxtGeometry.ComputeAABB(verts);

            // set up indices
            // deep copy necessary?
            for (int i = 0; i < indices.Length; ++i)
            {
                this.indices[i] = indices[i];
            }
            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
            this.primitiveCount = indices.Length / 3;
        }

        public Vector2[] GetTextureCoordinates()
        {
            gxtDebug.Assert(vertices != null);
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < uvs.Length; ++i)
            {
                uvs[i] = vertices[i].TextureCoordinate;
            }
            return uvs;
        }

        public virtual void Dispose()
        {
            if (texture != null)
                texture.Dispose();
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
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, primitiveCount, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, primitiveCount, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtMesh!  Pos: {0, 1}", position.X, position.Y);
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (material != null)
            {
                if (material.Visible)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, primitiveCount, ref transform, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, primitiveCount, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtMesh!  Pos: {0, 1}", transform.M14, transform.M24);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }
    }
}
