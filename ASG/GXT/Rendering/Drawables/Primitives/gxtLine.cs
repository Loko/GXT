using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtLine : gxtIDrawable
    {
        protected VertexPositionColorTexture[] vertices;
        protected int[] indices;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;

        protected gxtIMaterial material;

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

        public Vector2 Start
        {
            get
            {
                Vector3 start = vertices[0].Position;
                return new Vector2(start.X, start.Y);
            }
            set
            {
                // only certain times this call can be safely made
                vertices[0].Position = new Vector3(value.X, value.Y, 0.0f);
                vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            }
        }

        public Vector2 End
        {
            get
            {
                Vector3 end = vertices[1].Position;
                return new Vector2(end.X, end.Y);
            }
            set
            {
                // only certain times this call can be safely made
                vertices[1].Position = new Vector3(value.X, value.Y, 0.0f);
                vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
            }
        }

        public gxtLine()
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);

            vertices = new VertexPositionColorTexture[2];
            vertices[0] = new VertexPositionColorTexture(Vector3.Zero, gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(Vector3.Zero, gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.One);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 2, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            indices = new int[] { 0, 1 };
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 2, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        public gxtLine(gxtIMaterial material)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            gxtDebug.Assert(material != null);

            this.material = material;
            vertices = new VertexPositionColorTexture[2];
            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            vertices[0] = new VertexPositionColorTexture(Vector3.Zero, overlay, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(Vector3.Zero, overlay, Vector2.One);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 2, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            indices = new int[] { 0, 1 };
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 2, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        public gxtLine(Vector2 start, Vector2 end)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);

            vertices = new VertexPositionColorTexture[2];
            vertices[0] = new VertexPositionColorTexture(new Vector3(start.X, start.Y, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(new Vector3(end.X, end.Y, 0.0f), gxtMaterial.DEFAULT_COLOR_OVERLAY, Vector2.UnitY);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 2, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            indices = new int[] { 0, 1 };
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 2, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        public gxtLine(Vector2 start, Vector2 end, gxtIMaterial material)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            gxtDebug.Assert(material != null);

            this.material = material;
            vertices = new VertexPositionColorTexture[2];
            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            vertices[0] = new VertexPositionColorTexture(new Vector3(start.X, start.Y, 0.0f), overlay, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(new Vector3(end.X, end.Y, 0.0f), overlay, Vector2.One);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 2, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            indices = new int[] { 0, 1 };
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 2, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        public void SetVertices(Vector2 start, Vector2 end)
        {
            vertices[0].Position = new Vector3(start.X, start.Y, 0.0f);
            vertices[1].Position = new Vector3(end.X, end.Y, 0.0f);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
        }

        public gxtAABB GetLocalAABB()
        {
            Vector2 c = (Start + End) * 0.5f;
            float rX = gxtMath.Abs((End.X - c.X));
            float rY = gxtMath.Abs((End.Y - c.Y));
            return new gxtAABB(c, new Vector2(rX, rY));
        }

        public void UpdateFromMaterial(gxtIMaterial material)
        {
            gxtDebug.Assert(this.material == material);
            if (material != null)
            {
                vertices[0].Color = material.ColorOverlay;
                vertices[1].Color = material.ColorOverlay;
            }
            else
            {
                vertices[0].Color = gxtMaterial.DEFAULT_COLOR_OVERLAY;
                vertices[1].Color = gxtMaterial.DEFAULT_COLOR_OVERLAY;
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
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineStrip, null, vertexBuffer, indexBuffer, 1, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
                }
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                {
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineStrip, null, vertexBuffer, indexBuffer, 1, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtLine!  Pos: {0, 1}", position.X, position.Y);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (material != null)
            {
                if (material.Visible)
                {
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineStrip, null, vertexBuffer, indexBuffer, 1, ref transform, material.RenderDepth);
                }
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                {
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.LineStrip, null, vertexBuffer, indexBuffer, 1, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtLine!  Pos: {0, 1}", transform.M14, transform.M24);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }
    }
}
