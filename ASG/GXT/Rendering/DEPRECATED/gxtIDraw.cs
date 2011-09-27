using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public interface gxtIDraw
    {
        bool Visible { get; set; }
        float Depth { get; set; }
        gxtAABB GetAABB();
        gxtOBB GetOBB();
        void Draw(ref SpriteBatch spriteBatch);
    }
}
