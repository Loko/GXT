using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public enum gxtCircleDrawMode
    {
        CIRCLE = 0,
        SHELL = 1
    };

    /// <summary>
    /// 
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtCircle : gxtIDrawable
    {
        protected float radius;
        protected gxtCircleDrawMode circleDrawMode;
        
        protected gxtIMaterial material;

        protected VertexPositionColorTexture[] vertices;
        protected int[] indices;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;

        /// <summary>
        /// Circle Material
        /// </summary>
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

        /// <summary>
        /// Circle Radius
        /// </summary>
        public float Radius 
        { 
            get { return radius; } 
            set 
            { 
                gxtDebug.Assert(value >= 0.0f);
                if (radius != value)
                {
                    radius = value;
                    SetVertices();
                    SetIndices();
                }
            } 
        }

        /// <summary>
        /// Draw Mode.  The default is CIRCLE, but you can optionally draw it as a shell.
        /// </summary>
        public gxtCircleDrawMode CircleDrawMode { get { return circleDrawMode; } set { circleDrawMode = value; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="circleDrawMode"></param>
        public gxtCircle(float radius, gxtCircleDrawMode circleDrawMode = gxtCircleDrawMode.CIRCLE)
        {
            gxtDebug.Assert(radius > 0.0f);
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            
            this.radius = radius;
            this.circleDrawMode = circleDrawMode;

            SetVertices();
            SetIndices();
        }

        public gxtCircle(float radius, gxtIMaterial material, gxtCircleDrawMode circleDrawMode = gxtCircleDrawMode.CIRCLE)
        {
            gxtDebug.Assert(radius > 0.0f);
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            
            this.radius = radius;
            this.circleDrawMode = circleDrawMode;
            this.material = material;

            SetVertices();
            SetIndices();
        }

        /// <summary>
        /// The local AABB of the drawable
        /// </summary>
        /// <returns></returns>
        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, new Vector2(radius, radius));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
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

        /// <summary>
        /// Internal vertex setup routine
        /// </summary>
        private void SetVertices()
        {
            if (vertices == null)
                vertices = new VertexPositionColorTexture[4];
            if (vertexBuffer == null)
                vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);

            Color overlay = (material != null) ? material.ColorOverlay : gxtMaterial.DEFAULT_COLOR_OVERLAY;
            vertices[0] = new VertexPositionColorTexture(new Vector3(-radius, -radius, 0.0f), overlay, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(new Vector3(-radius, radius, 0.0f), overlay, Vector2.UnitY);
            vertices[2] = new VertexPositionColorTexture(new Vector3(radius, radius, 0.0f), overlay, Vector2.One);
            vertices[3] = new VertexPositionColorTexture(new Vector3(radius, -radius, 0.0f), overlay, Vector2.UnitX);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
        }

        private void ResetVertexPositions()
        {
            // have a function that simply adjusts the positions of the vertices, rather than changing everything again...
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
                {
                    if (circleDrawMode == gxtCircleDrawMode.CIRCLE)
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleTexture, vertexBuffer, gxtPrimitiveManager.Singleton.QuadIndexBuffer, 2, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
                    else
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleShellTexture, vertexBuffer, gxtPrimitiveManager.Singleton.QuadIndexBuffer, 2, ref position, ref scale, rotation, spriteEffects, material.RenderDepth);
                }
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                {
                    if (circleDrawMode == gxtCircleDrawMode.CIRCLE)
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleTexture, vertexBuffer, indexBuffer, 2, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                    else
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleShellTexture, vertexBuffer, indexBuffer, 2, ref position, ref scale, rotation, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtCircle!  Pos: {0, 1}", position.X, position.Y);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (material != null)
            {
                if (material.Visible)
                {
                    if (circleDrawMode == gxtCircleDrawMode.CIRCLE)
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleTexture, vertexBuffer, indexBuffer, 2, ref transform, material.RenderDepth);
                    else
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleShellTexture, vertexBuffer, indexBuffer, 2, ref transform, material.RenderDepth);
                }
            }
            else
            {
                if (gxtMaterial.DEFAULT_VISIBILITY)
                {
                    if (circleDrawMode == gxtCircleDrawMode.CIRCLE)
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleTexture, vertexBuffer, indexBuffer, 2, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                    else
                        graphicsBatch.DrawIndexedPrimitives(PrimitiveType.TriangleList, gxtPrimitiveManager.Singleton.CircleShellTexture, vertexBuffer, indexBuffer, 2, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "No material attached to an instance of gxtCircle!  Pos: {0, 1}", transform.M14, transform.M24);
                // draw NO MATERIAL with the debug drawer's debug spritefont?
            }
        }
    }
}
