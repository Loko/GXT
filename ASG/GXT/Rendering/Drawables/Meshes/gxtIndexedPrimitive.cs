﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public enum gxtPrimitiveType
    {
        TRIANGLE_LIST = 0,
        LINE_STRIP = 1,
        LINE_LIST = 2
        // line loop, then maybe tri strip
    };

    public class gxtIndexedPrimitive : gxtIDrawable
    {
        protected Texture2D texture;
        protected gxtIMaterial material;
        protected VertexPositionColorTexture[] vertices;
        protected int[] indices;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;
        protected gxtAABB localAABB;
        protected PrimitiveType primitiveType;
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

        public gxtAABB GetLocalAABB()
        {
            // does not work right for scaled AABBs and definetly not negative scales
            return localAABB;
        }

        public gxtIndexedPrimitive()
        {
            //vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
        }

        public gxtIndexedPrimitive(Vector2[] verts, PrimitiveType primitiveType = PrimitiveType.TriangleList)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.primitiveType = primitiveType;
            int indicesArraySize = CalcIndicesArraySize(verts.Length, primitiveType);
            this.primitiveCount = CalcPrimitiveCount(indicesArraySize, primitiveType);
            SetupVertices(verts);
            SetupIndices(indicesArraySize, primitiveType);
            localAABB = gxtGeometry.ComputeAABB(verts);
        }

        public gxtIndexedPrimitive(Vector2[] verts, PrimitiveType primitiveType, gxtIMaterial material)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.primitiveType = primitiveType;
            int indicesArraySize = CalcIndicesArraySize(verts.Length, primitiveType);
            this.primitiveCount = CalcPrimitiveCount(indicesArraySize, primitiveType);
            this.material = material;
            // set up both buffers
            SetupVertices(verts);
            SetupIndices(indicesArraySize, primitiveType);
            localAABB = gxtGeometry.ComputeAABB(verts);
        }

        public gxtIndexedPrimitive(Vector2[] verts, PrimitiveType primitiveType, gxtIMaterial material, Texture2D texture, gxtTextureCoordinateType uvType = gxtTextureCoordinateType.CLAMP)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.primitiveType = primitiveType;
            int indicesArraySize = CalcIndicesArraySize(verts.Length, primitiveType);

            this.material = material;
            this.texture = texture;
            Vector2[] texCoords = gxtGeometry.CalculateTextureCoordinates(verts, texture.Width, texture.Height, uvType);
            // set up vertices
            SetupVertices(verts, texCoords);
            SetupIndices(indicesArraySize, primitiveType);
            localAABB = gxtGeometry.ComputeAABB(verts);
        }

        public gxtIndexedPrimitive(Vector2[] verts, PrimitiveType primitiveType, gxtIMaterial material, Texture2D texture, Vector2[] textureCoordinates)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.primitiveType = primitiveType;
            int indicesArraySize = CalcIndicesArraySize(verts.Length, primitiveType);

            this.material = material;
            this.texture = texture;
            // set up vertices
            SetupVertices(verts, textureCoordinates);
            SetupIndices(indicesArraySize, primitiveType);
            localAABB = gxtGeometry.ComputeAABB(verts);
        }

        // shallow copy with a different material

        public void ApplyTexture(Texture2D texture, gxtTextureCoordinateType uvType)
        {
            this.texture = texture;
            Vector2[] verts = new Vector2[vertices.Length];

            int i;
            for (i = 0; i < verts.Length; ++i)
            {
                verts[i] = new Vector2(vertices[i].Position.X, vertices[i].Position.Y);
            }
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

        private int CalcIndicesArraySize(int vertexCount, PrimitiveType primitiveType)
        {
            int indicesArraySize;
            if (primitiveType == PrimitiveType.TriangleList)
            {
                indicesArraySize = 3 + ((vertexCount - 3) * 3);
                primitiveCount = indicesArraySize / 3;
            }
            else if (primitiveType == PrimitiveType.LineList)
            {
                indicesArraySize = vertexCount;
                primitiveCount = vertexCount / 2;
            }
            else
            {
                indicesArraySize = vertexCount;
                primitiveCount = vertexCount - 1;
            }
            return indicesArraySize;
        }

        private int CalcPrimitiveCount(int indicesArraySize, PrimitiveType primitiveType)
        {
            if (primitiveType == PrimitiveType.TriangleList)
                return indicesArraySize / 3;
            else if (primitiveType == PrimitiveType.LineList)
                return indicesArraySize / 2;
            else
                return indicesArraySize - 1;
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

        public void SetupVertices(Vector2[] verts)
        {
            if (vertices == null)
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
        }

        public void SetupVertices(Vector2[] verts, Vector2[] textureCoordinates)
        {
            if (vertices == null)
                vertices = new VertexPositionColorTexture[verts.Length];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, primitiveType.GetType(), verts.Length, BufferUsage.WriteOnly);
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
        }

        private void SetupIndices(int indicesArraySize, PrimitiveType primitiveType)
        {
            indices = new int[indicesArraySize];
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), indicesArraySize, BufferUsage.WriteOnly);
            if (primitiveType == PrimitiveType.TriangleList)
            {
                for (int i = 0, j = 1; i < indicesArraySize; i += 3)
                {
                    indices[i] = 0;
                    indices[i + 1] = j;
                    ++j;
                    indices[i + 2] = j;
                }
            }
            else if (primitiveType == PrimitiveType.LineList)
            {
                for (int i = 0; i < indicesArraySize; i += 2)
                {
                    indices[i] = i;
                }
            }
            else
            {
                for (int i = 0; i < indicesArraySize; ++i)
                {
                    indices[i] = i;
                }
            }
            indexBuffer.SetData<int>(indices);
        }

        public void Dispose()
        {
            if (texture != null)
                texture.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Vector2 position, float rotation, ref Vector2 scale, SpriteEffects spriteEffects)
        {
            if (material != null)
            {
                if (material.Visible)
                    graphicsBatch.DrawIndexedPrimitives(primitiveType, texture, vertexBuffer, indexBuffer, primitiveCount, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(primitiveType, texture, vertexBuffer, indexBuffer, primitiveCount, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to the indexed primitive!  Pos: {0, 1}", position.X, position.Y);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (material != null)
            {
                if (material.Visible)
                    graphicsBatch.DrawIndexedPrimitives(primitiveType, texture, vertexBuffer, indexBuffer, primitiveCount, ref transform, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(primitiveType, texture, vertexBuffer, indexBuffer, primitiveCount, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to the indexed primitive!  Pos: {0, 1}", transform.M14, transform.M24);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }
    }
}
