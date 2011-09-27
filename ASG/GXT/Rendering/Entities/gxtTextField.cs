using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GXT.IO;

namespace GXT.Rendering
{
    public class gxtTextField : gxtIEntity
    {
        private string text;
        private SpriteFont spriteFont;
        private Vector2 origin;

        public string Text { get { return text; } set { text = value; UpdateOrigin(); } }
        public SpriteFont SpriteFont { get { return spriteFont; } set { spriteFont = value; UpdateOrigin(); } }

        public gxtTextField(SpriteFont spriteFont)
        {
            this.text = string.Empty;
            this.spriteFont = spriteFont;
        }

        public gxtTextField(SpriteFont spriteFont, string text)
        {
            this.text = text;
            this.spriteFont = spriteFont;
            UpdateOrigin();
        }

        private void UpdateOrigin()
        {
            if (spriteFont != null)
                origin = spriteFont.MeasureString(text) * 0.5f;
        }

        public bool LoadSpriteFont(string path)
        {
            return gxtResourceManager.Singleton.Load<SpriteFont>(path, out spriteFont);
        }

        public void Dispose()
        {

        }

        public gxtAABB GetAABB(Vector2 position, float rotation, Vector2 scale)
        {
            float rx = gxtMath.Abs(origin.X * scale.X);
            float ry = gxtMath.Abs(origin.Y * scale.Y);
            return gxtAABB.Update(position, rotation, new gxtAABB(Vector2.Zero, new Vector2(rx, ry)));
        }

        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, new Vector2(origin.X, origin.Y));
        }

        public void Draw(gxtSpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, SpriteEffects spriteEffects, Color colorOverlay, float renderDepth)
        {
            if (spriteFont != null)
                spriteBatch.DrawString(spriteFont, text, position, colorOverlay, rotation, origin, scale, spriteEffects, renderDepth);
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Drawable Text Field Does Not Have A Sprite Font (Position {0}, Text: {1})", position.ToString(), text);
                if (gxtDebugDrawer.SingletonIsInitialized)
                {
                    // check if debug drawer has a spritefont??
                    // why not just get the debug drawer spritefont and feed it in here?
                    gxtDebugDrawer.Singleton.AddString("NO SPRITEFONT", position, Color.Magenta, 0.0f);
                }
            }
        }
    }
}
