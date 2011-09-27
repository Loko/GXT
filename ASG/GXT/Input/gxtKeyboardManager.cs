using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GXT.Input
{
    /// <summary>
    /// A class to manage updates for all keyboards
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtKeyboardManager : gxtSingleton<gxtKeyboardManager>
    {
        private gxtKeyboard keyboard;

        public bool IsInitialized()
        {
            return keyboard != null;
        }

        /// <summary>
        /// Initializes a keyboard manager
        /// </summary>
        public void Initialize()
        {
            gxtDebug.Assert(!IsInitialized());
            keyboard = new gxtKeyboard();
        }

        /// <summary>
        /// Updates all keyboards
        /// </summary>
        public void Update()
        {
            gxtDebug.Assert(IsInitialized());
            keyboard.Update();
        }

        /// <summary>
        /// Returns PlayerIndex.One keyboard instance
        /// </summary>
        /// <returns></returns>
        public gxtKeyboard GetKeyboard()
        {
            gxtDebug.Assert(IsInitialized());
            return keyboard;
        }

        public void Unload()
        {

        }

        // CHATPAD SUPPORT?
    }
}
