using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// Interface for cameras usable with GXT
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public interface gxtICamera
    {
        float ScreenScale { get; set; }
        Vector2 Position { get; set; }
        float Rotation { get; set; }
        float Zoom { get; set; }

        gxtAABB GetViewAABB();
        gxtOBB GetViewOBB();
        Vector2 GetVirtualMousePosition(Vector2 screenPos);
        void ResolutionChangedHandler(gxtDisplayManager manager);
        Matrix GetTransform();
    }
}
