using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    public class gxtDrawablePolygon : gxtIEntity
    {
        private Texture2D texture;
        public Texture2D Texture { get { return texture; } set { texture = value; } }

        private IndexBuffer indexBuffer;
        private VertexBuffer vertexBuffer;
        
        // needed?
        int[] indicesArray;
        VertexPositionColorTexture[] verts;

        public gxtDrawablePolygon(Vector2[] vertices)
        {
            gxtDebug.Assert(vertices != null && vertices.Length >= 3);
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 3 + ((vertices.Length - 3) * 3), BufferUsage.WriteOnly);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);

            Triangulate(vertices);
        }

        public gxtDrawablePolygon(Vector2[] vertices, Texture2D texture, gxtTextureCoordinateType uvType = gxtTextureCoordinateType.CLAMP)
        {
            gxtDebug.Assert(vertices != null && vertices.Length >= 3);
            indexBuffer = new IndexBuffer(gxtRoot.Singleton.Graphics, typeof(int), 3 + ((vertices.Length - 3) * 3), BufferUsage.WriteOnly);
            vertexBuffer = new VertexBuffer(gxtRoot.Singleton.Graphics, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);

            Triangulate(vertices);
            SetupTexture(texture, uvType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            gxtAABB localAABB = GetLocalAABB();
            localAABB.Extents = new Vector2(localAABB.Extents.X * scale.X, localAABB.Extents.Y * scale.Y);
            return gxtAABB.Update(position, rotation, localAABB);
        }

        /// <summary>
        /// Gets the local space AABB of the polygon
        /// </summary>
        /// <returns>Local AABB</returns>
        public gxtAABB GetLocalAABB()
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue; 
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].Position.X < minX) minX = verts[i].Position.X;
                if (verts[i].Position.X > maxX) maxX = verts[i].Position.X;
                if (verts[i].Position.Y < minY) minY = verts[i].Position.Y;
                if (verts[i].Position.Y > maxY) maxY = verts[i].Position.Y;
            }
            Vector2 c = new Vector2((maxX + minX) * 0.5f, (maxY + minY) * 0.5f);
            Vector2 r = new Vector2(maxX - c.X, maxY - c.Y);
            return new gxtAABB(c, r);
        }

        /// <summary>
        /// Triangulates the polygon, producing a sequence of triangles in the 
        /// index buffer.  Texture coords will all be set to Vector2.Zero and should 
        /// be calculated later with SetupTexture or some other associated call if you 
        /// intend to support texture mapping
        /// </summary>
        /// <param name="vertices">CCW Vertices</param>
        private void Triangulate(Vector2[] vertices)
        {
            // setup vertex buffer
            verts = new VertexPositionColorTexture[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                verts[i] = new VertexPositionColorTexture(new Vector3(vertices[i].X, vertices[i].Y, 0.0f), Color.White, Vector2.Zero);  // coords will be figured out later
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(verts);

            // setup index buffer, uses proper triangulation
            int indicesArraySize = 3 + ((vertices.Length - 3) * 3);
            indicesArray = new int[indicesArraySize];
            for (int i = 0, j = 1; i < indicesArraySize; i += 3)
            {
                indicesArray[i] = 0;
                indicesArray[i + 1] = j;
                ++j;
                indicesArray[i + 2] = j;
            }
            //indicesArray = indices.ToArray();
            indexBuffer.SetData<int>(indicesArray);
        }

        public void Setup(Vector2[] vertices, Vector2[] textureCoordinates, Texture2D texture)
        {

        }

        public void SetupTexture(Texture2D texture, gxtTextureCoordinateType uvType)
        {
            Texture = texture;
            CalcUVCoords(uvType);
        }

        public void CalcUVCoords(gxtTextureCoordinateType uvType)
        {
            gxtDebug.Assert(texture != null);

            Vector2 topLeft = new Vector2(-texture.Width * 0.5f, -texture.Height * 0.5f);
            Vector2 oneOverSizeVector = new Vector2(1.0f / texture.Width, 1.0f / texture.Height);

            if (uvType == gxtTextureCoordinateType.CLAMP)
            {
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i].TextureCoordinate = new Vector2(gxtMath.Clamp((verts[i].Position.X - topLeft.X) * oneOverSizeVector.X, 0.0f, 1.0f), gxtMath.Clamp((verts[i].Position.Y - topLeft.Y) * oneOverSizeVector.Y, 0.0f, 1.0f));
                }
            }
            else if (uvType == gxtTextureCoordinateType.WRAP)
            {
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i].TextureCoordinate = new Vector2(((verts[i].Position.X - topLeft.X) * oneOverSizeVector.X) % 1.0f, ((verts[i].Position.Y - topLeft.Y) * oneOverSizeVector.Y) % 1.0f);
                }
            }
            else
            {
                gxtDebug.Assert(false, "Texture Coordinate Type Not Yet Implemented!");
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(verts);
        }

        public void CalcUVCoords(Vector2 textureScale, float textureRotation, gxtTextureCoordinateType uvType)
        {
            gxtDebug.Assert(texture != null);
            gxtDebug.Assert(uvType == gxtTextureCoordinateType.CLAMP || uvType == gxtTextureCoordinateType.WRAP, "Texture Coordinate Type Not Yet Implemented");

            float sw = texture.Width * textureScale.X;
            float sh = texture.Height * textureScale.Y;
            Vector2 topLeft = new Vector2(-sw * 0.5f, -sh * 0.5f);  // may not be accurate when rotated, trying projection
            Vector2 oneOverSizeVector = new Vector2(1.0f / sw, 1.0f / sh);
            Vector2 xAxis, yAxis;
            gxtMath.GetAxesVectors(textureRotation, out xAxis, out yAxis);
            topLeft = new Vector2(Vector2.Dot(topLeft, xAxis), Vector2.Dot(topLeft, yAxis));

            if (uvType == gxtTextureCoordinateType.CLAMP)
            {
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector2 pos = new Vector2(verts[i].Position.X, verts[i].Position.Y);
                    Vector2 projPos = new Vector2(Vector2.Dot(pos - topLeft, xAxis), Vector2.Dot(pos - topLeft, yAxis));
                    verts[i].TextureCoordinate = new Vector2(gxtMath.Clamp(projPos.X * oneOverSizeVector.X, 0.0f, 1.0f), gxtMath.Clamp(projPos.Y * oneOverSizeVector.Y, 0.0f, 1.0f));
                }
            }
            else if (uvType == gxtTextureCoordinateType.WRAP)
            {
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector2 pos = new Vector2(verts[i].Position.X, verts[i].Position.Y);
                    Vector2 projPos = new Vector2(Vector2.Dot(pos - topLeft, xAxis), Vector2.Dot(pos - topLeft, yAxis));
                    verts[i].TextureCoordinate = new Vector2((projPos.X * oneOverSizeVector.X) % 1.0f, (projPos.Y * oneOverSizeVector.Y) % 1.0f);
                }
            }
        }

        /// <summary>
        /// Helper function sets a color overlay for every vertex in the buffer
        /// </summary>
        /// <param name="color">Color</param>
        public void SetColor(Color color)
        {
            // inefficient
            if (color.Equals(verts[0].Color))
                return;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i].Color = color;
            }
            vertexBuffer.SetData<VertexPositionColorTexture>(verts);
        }

        public void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            SetColor(colorOverlay);
            spriteBatch.DrawPolygon(texture, vertexBuffer, indexBuffer, position, rotation, scale, spriteEffects, renderDepth); 
        }

        public void Dispose()
        {
            texture.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }

        /*
        public bool IsValid()
        {
            return true;
        }
        */
    }
}
