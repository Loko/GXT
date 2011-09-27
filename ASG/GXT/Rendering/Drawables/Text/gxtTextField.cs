using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GXT.IO;

namespace GXT.Rendering
{
    /// <summary>
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtTextField : gxtIDrawable
    {
        private string text;
        private SpriteFont spriteFont;
        private Vector2 origin;

        private gxtIMaterial material;

        /// <summary>
        /// Readable text string
        /// </summary>
        public string Text { get { return text; } set { text = value; UpdateOrigin(); } }

        /// <summary>
        /// Font used to render the text string
        /// </summary>
        public SpriteFont SpriteFont { get { return spriteFont; } set { spriteFont = value; UpdateOrigin(); } }
        
        /// <summary>
        /// Material for the textfield
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

        public gxtTextField()
        {

        }

        public gxtTextField(SpriteFont spriteFont)
        {
            gxtDebug.Assert(spriteFont != null);
            this.text = string.Empty;
            this.spriteFont = spriteFont;
        }

        public gxtTextField(SpriteFont spriteFont, string text)
        {
            gxtDebug.Assert(spriteFont != null && text != null);
            this.text = text;
            this.spriteFont = spriteFont;
            UpdateOrigin();
        }

        public gxtTextField(SpriteFont spriteFont, string text, gxtIMaterial material)
        {
            gxtDebug.Assert(spriteFont != null && text != null);
            this.text = text;
            this.spriteFont = spriteFont;
            this.material = material;
            UpdateOrigin();
        }

        private void UpdateOrigin()
        {
            if (spriteFont != null && text != null)
                origin = spriteFont.MeasureString(text) * 0.5f;
        }

        public bool LoadSpriteFont(string name)
        {
            return gxtResourceManager.Singleton.Load<SpriteFont>(name, out spriteFont);
        }

        public Vector2 GetStringMeasure()
        {
            if (spriteFont != null)
                if (text != null)
                    return spriteFont.MeasureString(text);
            return Vector2.Zero;
        }

        public void UpdateFromMaterial(gxtIMaterial material)
        {
            // nothing to update
        }

        public virtual void Dispose()
        {
            // nothing that can be disposed
        }

        public gxtAABB GetLocalAABB()
        {
            return new gxtAABB(Vector2.Zero, origin);
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Vector2 position, float rotation, ref Vector2 scale, SpriteEffects spriteEffects)
        {
            if (spriteFont != null)
            {
                if (material != null)
                {
                    if (material.Visible)
                        graphicsBatch.DrawString(spriteFont, text, material.ColorOverlay, ref position, ref scale, rotation, ref origin, spriteEffects, material.RenderDepth);
                }
                else
                {
                    if (gxtMaterial.DEFAULT_VISIBILITY)
                        graphicsBatch.DrawString(spriteFont, text, gxtMaterial.DEFAULT_COLOR_OVERLAY, ref position, ref scale, rotation, ref origin, spriteEffects, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Drawable Text Field Does Not Have A Sprite Font (Position {0}, Text: {1})", position.ToString(), text);
                if (gxtDebugDrawer.SingletonIsInitialized)
                {
                    SpriteFont debugFont = gxtDebugDrawer.Singleton.DebugFont;
                    if (debugFont != null)
                    {
                        string errorMsg = "NO SPRITEFONT";
                        Vector2 errorMsgOrigin = debugFont.MeasureString(errorMsg) * 0.5f;
                        graphicsBatch.DrawString(debugFont, errorMsg, gxtMaterial.ERROR_TEXT_COLOR_OVERLAY, ref position, ref scale, rotation, ref errorMsgOrigin, SpriteEffects.None, 0.0f);
                    }
                }
            }
        }

        public void Draw(gxtGraphicsBatch graphicsBatch, ref Matrix transform)
        {
            if (spriteFont != null)
            {
                if (material != null)
                {
                    if (material.Visible)
                        graphicsBatch.DrawString(spriteFont, text, material.ColorOverlay, ref origin, ref transform, material.RenderDepth);
                }
                else
                {
                    if (gxtMaterial.DEFAULT_VISIBILITY)
                        graphicsBatch.DrawString(spriteFont, text, gxtMaterial.ERROR_TEXT_COLOR_OVERLAY, ref origin, ref transform, gxtMaterial.DEFAULT_RENDER_DEPTH);
                }
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Drawable Text Field Does Not Have A Sprite Font (Position {0}, Text: {1})", transform.Translation.ToString(), text);
                if (gxtDebugDrawer.SingletonIsInitialized)
                {
                    SpriteFont debugFont = gxtDebugDrawer.Singleton.DebugFont;
                    if (debugFont != null)
                    {
                        string errorMsg = "NO SPRITEFONT";
                        Vector2 errorMsgOrigin = debugFont.MeasureString(errorMsg) * 0.5f;
                        graphicsBatch.DrawString(debugFont, errorMsg, gxtMaterial.ERROR_COLOR_OVERLAY, ref errorMsgOrigin, ref transform, 0.0f);
                    }
                }
            }
        }
    }
}
