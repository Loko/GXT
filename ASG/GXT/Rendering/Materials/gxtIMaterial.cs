using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    /// <summary>
    /// A renderable object whose vertex color data (or other data) can 
    /// be updated from a material
    /// </summary>
    public interface gxtIMaterialListener
    {
        void UpdateFromMaterial(gxtIMaterial material);
    }

    /// <summary>
    /// A shareable definition of basic render properties
    /// </summary>
    public interface gxtIMaterial
    {
        /// <summary>
        /// Visibility Flag
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Render Depth
        /// </summary>
        float RenderDepth { get; set; }

        /// <summary>
        /// Color Tint
        /// </summary>
        Color ColorOverlay { get; set; }

        /// <summary>
        /// Sets the defaults on an existing material
        /// </summary>
        void SetDefaults();

        /// <summary>
        /// Adds a listener to the material
        /// </summary>
        /// <param name="listener">Material Listener</param>
        void AddListener(gxtIMaterialListener listener);

        /// <summary>
        /// Removes a listener from the material
        /// </summary>
        /// <param name="listener">Material Listener</param>
        /// <returns>If removed</returns>
        bool RemoveListener(gxtIMaterialListener listener);

        /// <summary>
        /// Manually notifies the material listeners
        /// </summary>
        void NotifyListeners();
    }
}
