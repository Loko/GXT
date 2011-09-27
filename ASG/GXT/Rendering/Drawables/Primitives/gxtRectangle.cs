using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtRectangle : gxtIDrawable
    {
        protected Vector2 size;
        protected gxtIMaterial material;

        protected VertexPositionColorTexture[] vertices;
        protected int[] indices;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;

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

        public float Width { get { return size.X; } set { gxtDebug.Assert(value >= 0.0f);  size = new Vector2(value, size.Y); } }
        public float Height { get { return size.Y; } set { gxtDebug.Assert(value >= 0.0f);  size = new Vector2(size.X, value); } }

        public gxtRectangle(float width, float height)
        {
            gxtDebug.Assert(width >= 0.0f && height >= 0.0f);
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.size = new Vector2(width, height);
            float rX = width * 0.5f, rY = height * 0.5f;

            vertices = new VertexPositionColorTexture[4];
            vertices[0] = new VertexPositionColorTexture(new Vector3(-rX, -rY, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(new Vector3(-rX, rY, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.UnitY);
            vertices[2] = new VertexPositionColorTexture(new Vector3(rX, rY, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.One);
            vertices[3] = new VertexPositionColorTexture(new Vector3(rX, -rY, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.UnitX);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            indices = new int[] { 0, 1, 2, 0, 2, 3 };
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 6, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        public gxtRectangle(float width, float height, gxtIMaterial material)
        {
            gxtDebug.Assert(width >= 0.0f && height >= 0.0f);
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            this.size = new Vector2(width, height);
            this.material = material;

            float rX = width * 0.5f, rY = height * 0.5f;
            vertices = new VertexPositionColorTexture[4];
            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            vertices[0] = new VertexPositionColorTexture(new Vector3(-rX, -rY, 0.0f), overlay, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(new Vector3(-rX, rY, 0.0f), overlay, Vector2.UnitY);
            vertices[2] = new VertexPositionColorTexture(new Vector3(rX, rY, 0.0f), overlay, Vector2.One);
            vertices[3] = new VertexPositionColorTexture(new Vector3(rX, -rY, 0.0f), overlay, Vector2.UnitX);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            indices = new int[] { 0, 1, 2, 0, 2, 3 };
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 6, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, new Vector2(size.X * 0.5f, size.Y * 0.5f));
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

        public void Dispose()
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
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, null, vertexBuffer, indexBuffer, 2, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, null, vertexBuffer, indexBuffer, 2, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtRectangle!  Pos: {0, 1}", position.X, position.Y);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (material != null)
            {
                if (material.Visible)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, null, vertexBuffer, indexBuffer, 2, ref transform, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, null, vertexBuffer, indexBuffer, 2, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtRectangle!  Pos: {0, 1}", transform.M14, transform.M24);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }
    }
}
