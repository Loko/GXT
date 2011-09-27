using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtDrawable : gxtIDrawable
    {
        protected gxtIEntity entity;
        protected Color colorOverlay;
        protected float renderDepth;
        protected bool visible;

        public gxtIEntity Entity { get { return entity; } set { entity = value; } }
        public Color ColorOverlay { get { return colorOverlay; } set { colorOverlay = value; } }
        public float RenderDepth { get { return renderDepth; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f, "Render Depths Must Be Between 0 and 1"); renderDepth = value; } }
        public bool Visible { get { return visible; } set { visible = value; } }

        public gxtDrawable(bool visible = true, float renderDepth = 0.5f)
        {
            this.visible = visible;
            this.renderDepth = renderDepth;
            colorOverlay = Color.White;
        }

        public gxtDrawable(Color colorOverlay, bool visible = true, float renderDepth = 0.5f)
        {
            this.visible = visible;
            this.renderDepth = renderDepth;
            this.colorOverlay = colorOverlay;
        }

        public gxtDrawable(gxtIEntity entity, Color colorOverlay, bool visible = true, float renderDepth = 0.5f)
        {
            this.visible = visible;
            this.renderDepth = renderDepth;
            this.colorOverlay = colorOverlay;
            this.entity = entity;
        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            if (entity != null)
                return entity.GetAABB(position, rotation, scale);
            else
                return gxtAABB.MIN_EXTENTS_AABB;
        }

        public void Dispose()
        {
            if (entity != null)
                entity.Dispose();
        }

        public void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects)
        {
            if (!visible || entity == null)
                return;

            entity.Draw(spriteBatch, position, rotation, scale, spriteEffects, colorOverlay, renderDepth);
        }
    }
}
