using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GXT.Input
{
    /// <summary>
    /// Delegate fit for notifying other objects when a character has been 
    /// pressed on the keyboard
    /// </summary>
    /// <param name="c">character entered</param>
    public delegate void gxtKeyboardCharacterEnteredHandler(char c);

    /// <summary>
    /// A class for managing an instance of the keyboard
    /// Retreive keyboards from gxtKeyboardManager rather than 
    /// constructing them yourself
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtKeyboard
    {
        private KeyboardState prev, cur;

        /// <summary>
        /// Previous XNA Keyboard State
        /// </summary>
        public KeyboardState PrevState { get { return prev; } }

        /// <summary>
        /// Current XNA Keyboard State
        /// </summary>
        public KeyboardState CurState { get { return cur; } }
        
        /// <summary>
        /// Updates current kb state
        /// pushes back the old current state to prev
        /// </summary>
        public void Update()
        {
            prev = cur;
            cur = Keyboard.GetState();
        }

        /// <summary>
        /// Gets the state of a particular control.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public gxtControlState GetState(Keys key)
        {
            if (cur.IsKeyDown(key))
            {
                if (prev.IsKeyDown(key)) return gxtControlState.DOWN;
                else return gxtControlState.FIRST_PRESSED;
            }
            else
            {
                if (prev.IsKeyDown(key)) return gxtControlState.FIRST_RELEASED;
                else return gxtControlState.UP;
            }
        }

        /// <summary>
        /// key down?
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsDown(Keys key)
        {
            return cur.IsKeyDown(key);
        }

        /// <summary>
        /// key up?
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsUp(Keys key)
        {
            return cur.IsKeyUp(key);
        }

        /// <summary>
        /// Gets all currently pressed keys
        /// </summary>
        /// <returns></returns>
        public Keys[] GetPressedKeys()
        {
            return cur.GetPressedKeys();
        }

        /// <summary>
        /// Gets all previously pressed keys
        /// </summary>
        /// <returns></returns>
        public Keys[] GetPrevPressedKeys()
        {
            return prev.GetPressedKeys();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool AnyKeyIsDown()
        {
            return cur.GetPressedKeys().Length > 0;
        }

        public bool AnyKeyIsDown(Keys[] keys)
        {
            for (int i = 0; i < keys.Length; ++i)
            {
                if (cur.IsKeyDown(keys[i]))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool AnyKeyIsFirstPressed()
        {
            Keys[] downKeys = GetPressedKeys();
            for (int i = 0; i < downKeys.Length; ++i)
            {
                if (GetState(downKeys[i]) == gxtControlState.FIRST_PRESSED)
                    return true;
            }
            return false;
        }

        public bool AnyKeyIsFirstPressed(Keys[] keys)
        {
            for (int i = 0; i < keys.Length; ++i)
            {
                if (GetState(keys[i]) == gxtControlState.FIRST_PRESSED)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if ALL passed in keys are down on this frame
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool IsMacroDown(params Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (IsUp(key))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if ONE OR MORE passed in keys are up on this frame
        /// If any of the keys in the macro are up, the macro is considered up
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool IsMacroUp(params Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (IsUp(key))
                    return true;
            }
            return true;
        }

        /*
        public gxtControlState GetMacroState(bool allowFirst, params Keys[] keys)
        {
            if (allowFirst)
            {
                int numFirstUp = 0, numFirstDown = 0, numUp = 0, numDown = 0;
                for (int i = 0; i < keys.Length; ++i)
                {
                    if ()
                }
            }
            return gxtControlState.UP;
        }
        */

        /// <summary>
        /// Checks the control state of a macro
        /// Allow first boolean only affects checks for the UP/DOWN state
        /// If false, first_pressed/first_released states won't count
        /// If true, they will
        /// </summary>
        /// <param name="checkState"></param>
        /// <param name="allowFirst"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool TestMacroState(gxtControlState checkState, bool allowFirst, params Keys[] keys)
        {
            if (checkState == gxtControlState.UP)
                return CheckMacroUp(allowFirst, keys);
            else if (checkState == gxtControlState.DOWN)
                return CheckMacroDown(allowFirst, keys);
            else if (checkState == gxtControlState.FIRST_PRESSED)
                return CheckMacroFirstPressed(keys);
            else    // first_released
                return CheckMacroFirstReleased(keys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        private bool CheckMacroFirstPressed(params Keys[] keys)
        {
            //could just be a boolean
            int numFPressed = 0;
            foreach (Keys key in keys)
            {
                if (IsUp(key))
                    return false;
                else if (GetState(key) == gxtControlState.FIRST_PRESSED)
                    numFPressed++;
            }
            return numFPressed > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowFirst"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private bool CheckMacroDown(bool allowFirst, params Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (IsUp(key))
                    return false;
                if (!allowFirst && GetState(key) == gxtControlState.FIRST_PRESSED)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowFirst"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private bool CheckMacroUp(bool allowFirst, params Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (IsDown(key))
                    return false;
                if (!allowFirst && GetState(key) == gxtControlState.FIRST_RELEASED)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        private bool CheckMacroFirstReleased(params Keys[] keys)
        {
            int numFReleased = 0;
            foreach (Keys key in keys)
            {
                if (GetState(key) == gxtControlState.UP)
                    return false;
                else if (GetState(key) == gxtControlState.FIRST_RELEASED)
                    numFReleased++;
            }
            return numFReleased > 0;
        }
    }
}
