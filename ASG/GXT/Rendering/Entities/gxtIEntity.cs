using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public interface gxtIEntity : IDisposable
    {
        // neccessary??
        //gxtIDrawable Copy(gxtISceneNode node, float depth);
        gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale);
        gxtAABB GetLocalAABB();
        void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth);
    }
    /*
    public interface gxtIDrawable : IDisposable, gxtIMaterialListener
    {
        gxtIMaterial Material { get; set; }
        gxtAABB GetLocalAABB();

        void Draw(gxtGraphicsBatch graphicsBatch, ref Vector2 position, float rotation, ref Vector2 scale, SpriteEffects spriteEffects);
        void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform);
    }
    */
}
