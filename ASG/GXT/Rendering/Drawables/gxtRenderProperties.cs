using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// A shared collection of render properties.  Allows user to 
    /// adjust the visibility, color, and render depth of a group of entities 
    /// with different transforms in just one place.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSharedRenderProperties
    {
        private Color colorOverlay;
        private float renderDepth;
        private bool visible;

        public Color ColorOverlay { get { return colorOverlay; } set { colorOverlay = value; } }
        public float RenderDepth { get { return renderDepth; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f, "Render Depths Must Be Between 0 and 1"); renderDepth = value; } }
        public bool Visible { get { return visible; } set { visible = value; } }

        public gxtSharedRenderProperties(bool visible = true, float renderDepth = 0.5f)
        {
            this.visible = visible;
            this.renderDepth = renderDepth;
            colorOverlay = Color.White;
        }

        public gxtSharedRenderProperties(Color colorOverlay, bool visible = true, float renderDepth = 0.5f)
        {
            this.colorOverlay = colorOverlay;
            this.visible = visible;
            this.renderDepth = renderDepth;
        }
    }
}
