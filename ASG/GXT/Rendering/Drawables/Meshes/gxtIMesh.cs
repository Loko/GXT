using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public interface gxtIMesh : gxtIDrawable
    {
        int NumVertices { get; }
        //void ApplyTexture(Texture2D texture, gxtTextureCoordinateType uvType);
        //void ApplyTexture(Texture2D texture, Vector2 textureScale, float textureRotation, gxtTextureCoordinateType uvType);
        void SetVertices(Vector2[] verts, bool setDefaultIndices = true);
        void SetTextureCoordinates(Vector2[] textureCoords);
        void SetupMesh(Vector2[] verts, int[] indices);
        void SetupMesh(Vector2[] verts, int[] indices, Vector2[] textureCoordinates);
        Vector2[] GetTextureCoordinates();
        // might as well have GetVertices, GetIndices, etc. etc.
        // make sure they are deep copies tho
    }
}
