using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtSprite : gxtIDrawable
    {
        protected Texture2D texture;
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

        public Texture2D Texture 
        { 
            get { return texture; }
            set
            {
                if (texture != value)
                { 
                    texture = value; 
                    SetVertices();
                    SetIndices();
                }
            }
         }

        public gxtSprite()
        {

        }

        public gxtSprite(Texture2D texture)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            gxtDebug.Assert(texture != null);

            this.texture = texture;
            SetVertices();
            SetIndices();
        }

        public gxtSprite(Texture2D texture, gxtIMaterial material)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            gxtDebug.Assert(texture != null);
            gxtDebug.Assert(material != null);

            this.texture = texture;
            this.material = material;
            SetVertices();
            SetIndices();
        }

        public gxtAABB GetLocalAABB()
        {
            float rX = 0.0f, rY = 0.0f;
            if (texture != null)
            {
                rX = texture.Width * 0.5f;
                rY = texture.Height * 0.5f;
            }
            return new gxtAABB(Vector2.Zero, new Vector2(rX, rY));
        }

        public void UpdateFromMaterial(gxtIMaterial material)
        {
            gxtDebug.Assert(this.material == material);
            if (material != null)
            {
                if (vertices != null && !vertices[0].Color.Equals(material.ColorOverlay))
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
                if (vertices != null && !vertices[0].Color.Equals(gxtMaterial.DEFAULT_COLOR_OVERLAY))
                {
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        vertices[i].Color = material.ColorOverlay;
                    }
                    vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
                }
            }
        }

        private void SetVertices()
        {
            if (vertices == null)
                vertices = new VertexPositionColorTexture[4];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);
            
            float rX = texture.Width * 0.5f, rY = texture.Height * 0.5f;
            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            vertices[0] = new VertexPositionColorTexture(new Vector3(-rX, -rY, 0.0f), overlay, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(new Vector3(-rX, rY, 0.0f), overlay, Vector2.UnitY);
            vertices[2] = new VertexPositionColorTexture(new Vector3(rX, rY, 0.0f), overlay, Vector2.One);
            vertices[3] = new VertexPositionColorTexture(new Vector3(rX, -rY, 0.0f), overlay, Vector2.UnitX);

            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
        }

        private void SetIndices()
        {
            if (indices == null)
            {
                indices = new int[] { 0, 1, 2, 0, 2, 3 };
                indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 6, BufferUsage.WriteOnly);
                indexBuffer.SetData<int>(indices);
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
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, 2, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, 2, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to the gxtSprite!  Pos: {0, 1}", position.X, position.Y);
                // draw NO MATERIAL string?
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (material != null)
            {
                if (material.Visible)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, 2, ref transform, material.RenderDepth);
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                    graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, texture, vertexBuffer, indexBuffer, 2, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to the gxtSprite!  Pos: {0, 1}", transform.M14, transform.M24);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }
    }
}
