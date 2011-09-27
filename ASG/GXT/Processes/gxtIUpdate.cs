using Microsoft.Xna.Framework;

namespace GXT
{
    public interface gxtIUpdate
    {
        bool Enabled { get; set; }
        void Update(GameTime gameTime);
    }
}