#define GXT_MATERIAL_AUTO_NOTIFY
//#undef GXT_MATERIAL_AUTO_NOTIFY

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    /// <summary>
    /// A concrete material usable with gxtIDrawable's
    /// If GXT_MATERIAL_AUTO_NOTIFY is defined, every listener 
    /// will be updated after every change, otherwise NotifyListeners()
    /// must be called manually
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtMaterial : gxtIMaterial
    {
        #if DEBUG
        // defaults used if no material is attached and in SetDefaults()
        // todo: rename to error_color_overlay
        public static readonly Color DEFAULT_COLOR_OVERLAY = Color.White;
        public const bool DEFAULT_VISIBILITY = true;
        public const float DEFAULT_RENDER_DEPTH = 0.5f;
        public static readonly Color ERROR_COLOR_OVERLAY = Color.Red;
        public static readonly Color ERROR_TEXT_COLOR_OVERLAY = Color.Red;
        #else
        public static readonly Color DEFAULT_COLOR_OVERLAY = Color.White;
        public const bool DEFAULT_VISIBILITY = false;
        public const float DEFAULT_RENDER_DEPTH = 0.5f;
        public static readonly Color ERROR_COLOR_OVERLAY = Color.Transparent;
        public static readonly Color ERROR_TEXT_COLOR_OVERLAY = Color.Transparent;
        #endif

        /// <summary>
        /// A material composed of all of the default values
        /// </summary>
        public static readonly gxtIMaterial DEFAULT_MATERIAL = new gxtMaterial(DEFAULT_VISIBILITY, DEFAULT_COLOR_OVERLAY, DEFAULT_RENDER_DEPTH);

        protected Color colorOverlay;
        protected bool visible;
        protected float renderDepth;
        protected List<gxtIMaterialListener> materialListeners;
        
        /// <summary>
        /// Color Tint Overlay
        /// </summary>
        public virtual Color ColorOverlay 
        { 
            get { return colorOverlay; }
            set
            {

                #if GXT_MATERIAL_AUTO_NOTIFY
                if (!colorOverlay.Equals(value))
                {
                    colorOverlay = value;
                    NotifyListeners();
                }
                #else
                colorOverlay = value;
                #endif
            } 
        }

        /// <summary>
        /// Visibility Flag
        /// </summary>
        public virtual bool Visible 
        { 
            get { return visible; } 
            set
            {
                #if GXT_MATERIAL_AUTO_NOTIFY
                if (visible != value)
                {
                    visible = value;
                    NotifyListeners();
                }
                #else
                visible = value;
                #endif
            } 
        }

        /// <summary>
        /// Render Depth
        /// </summary>
        public virtual float RenderDepth 
        { 
            get { return renderDepth; } 
            set 
            { 
                gxtDebug.Assert(value >= 0.0f && value <= 1.0f);
                #if GXT_MATERIAL_AUTO_NOTIFY
                if (renderDepth != value)
                {
                    renderDepth = value;
                    NotifyListeners();
                }
                #else
                renderDepth = value;
                #endif
            } 
        }

        /// <summary>
        /// Constructs a material with the defaults
        /// </summary>
        /// <param name="initListenerListSize"></param>
        public gxtMaterial(int initListenerListSize = 4)
        {
            gxtDebug.Assert(initListenerListSize > 0);
            materialListeners = new List<gxtIMaterialListener>(initListenerListSize);
            SetDefaults();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="colorOverlay"></param>
        /// <param name="renderDepth"></param>
        /// <param name="initListenerListSize"></param>
        public gxtMaterial(bool visible, Color colorOverlay, float renderDepth, int initListenerListSize = 4)
        {
            gxtDebug.Assert(initListenerListSize > 0);
            gxtDebug.Assert(renderDepth >= 0.0f && renderDepth <= 1.0f);
            materialListeners = new List<gxtIMaterialListener>(initListenerListSize);
            this.visible = visible;
            this.colorOverlay = colorOverlay;
            this.renderDepth = renderDepth;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void SetDefaults()
        {
            #if GXT_MATERIAL_AUTO_NOTIFY
            bool changed = false;
            if (this.visible != DEFAULT_VISIBILITY || this.renderDepth != DEFAULT_RENDER_DEPTH || !this.colorOverlay.Equals(DEFAULT_COLOR_OVERLAY))
                changed = true;
            #endif

            visible = DEFAULT_VISIBILITY;
            colorOverlay = DEFAULT_COLOR_OVERLAY;
            renderDepth = DEFAULT_RENDER_DEPTH;

            #if GXT_MATERIAL_AUTO_NOTIFY
            if (changed)
                NotifyListeners();
            #endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="renderDepth"></param>
        /// <param name="colorOverlay"></param>
        public virtual void Set(bool visible, float renderDepth, ref Color colorOverlay)
        {
            gxtDebug.Assert(renderDepth >= 0.0f && renderDepth <= 1.0f);
            
            #if GXT_MATERIAL_AUTO_NOTIFY
            bool changed = false;
            if (this.visible != visible || this.renderDepth != renderDepth || !this.colorOverlay.Equals(colorOverlay))
                changed = true;
            #endif
            
            this.visible = visible;
            this.renderDepth = renderDepth;
            this.colorOverlay = colorOverlay;

            #if GXT_MATERIAL_AUTO_NOTIFY
            if (changed)
                NotifyListeners();
            #endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener"></param>
        public virtual void AddListener(gxtIMaterialListener listener)
        {
            if (!materialListeners.Contains(listener))
                materialListeners.Add(listener);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        public virtual bool RemoveListener(gxtIMaterialListener listener)
        {
            return materialListeners.Remove(listener);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void NotifyListeners()
        {
            for (int i = 0; i < materialListeners.Count; ++i)
            {
                materialListeners[i].UpdateFromMaterial(this);
            }
        }
    }
}
