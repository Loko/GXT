using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtSharedDrawable : gxtIDrawable
    {
        protected gxtIEntity entity;
        protected gxtSharedRenderProperties renderProperties;

        public gxtIEntity Entity { get { return entity; } set { entity = value; } }
        public gxtSharedRenderProperties SharedRenderProperties { get { return renderProperties; } set { renderProperties = value; } }

        public Color ColorOverlay { get { gxtDebug.Assert(renderProperties != null); return renderProperties.ColorOverlay; } set { gxtDebug.Assert(renderProperties != null); renderProperties.ColorOverlay = value; } }
        public float RenderDepth { get { gxtDebug.Assert(renderProperties != null); return renderProperties.RenderDepth; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f, "Render Depths Must Be Between 0 and 1"); renderProperties.RenderDepth = value; } }
        public bool Visible { get { gxtDebug.Assert(renderProperties != null); return renderProperties.Visible; } set { gxtDebug.Assert(renderProperties != null); renderProperties.Visible = value; } }

        public gxtSharedDrawable(gxtIEntity entity)
        {
            this.entity = entity;
        }

        public gxtSharedDrawable(gxtIEntity entity, gxtSharedRenderProperties renderProperties)
        {
            this.entity = entity;
            this.renderProperties = renderProperties;
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
            if (!Visible || entity == null)
                return;

            entity.Draw(spriteBatch, position, rotation, scale, spriteEffects, ColorOverlay, RenderDepth);
        }
    }
}
