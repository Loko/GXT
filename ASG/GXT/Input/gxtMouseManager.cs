using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GXT.Input
{
    /// <summary>
    /// A class to manage updates and access to the mouse
    /// </summary>
    public class gxtMouseManager : gxtSingleton<gxtMouseManager>
    {
        private gxtMouse mouse;

        /// <summary>
        /// Initializes mouse manager
        /// </summary>
        public void Initialize()
        {
            mouse = new gxtMouse();
        }

        /// <summary>
        /// Updates manager
        /// </summary>
        public void Update()
        {
            mouse.Update();
        }

        /// <summary>
        /// Gets instance of the mouse
        /// </summary>
        /// <returns></returns>
        public gxtMouse GetMouse()
        {
            return mouse;
        }

        public void Unload()
        {

        }
    }
}
