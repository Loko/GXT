using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /*
    public interface gxtIDrawable : IDisposable
    {
        Color ColorOverlay { get; set; }
        float RenderDepth { get; set; }
        bool Visible { get; set; }

        gxtIEntity Entity { get; set; }

        gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale);

        void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects);
    }
    */

    public interface gxtIDrawable : IDisposable, gxtIMaterialListener
    {
        gxtIMaterial Material { get; set; }
        gxtAABB GetLocalAABB();

        void Draw(gxtGraphicsBatch graphicsBatch, ref Vector2 position, float rotation, ref Vector2 scale, SpriteEffects spriteEffects);
        void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform);
    }
}
