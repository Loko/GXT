using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GXT.Input
{
    /// <summary>
    /// Enum representation of the buttons on 
    /// a typical mouse
    /// </summary>
    public enum gxtMouseButton
    {
        LEFT = 0,
        MIDDLE = 1,
        RIGHT = 2,
        SIDE1 = 3,
        SIDE2 = 4
    };

    /// <summary>
    /// A class for managing an instance of a mouse
    /// Functions for assesing botton, scroll, and position state
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtMouse
    {
        /// <summary>
        /// I've found 120 to equate to a single scroll click of the mouse wheel
        /// It may vary by hardware
        /// </summary>
        public const int SCROLL_MODIFIER = 120;

        private MouseState cur, prev;
        private int prevScroll;
        private int deltaScroll;

        /// <summary>
        /// Previous XNA Mouse State
        /// </summary>
        public MouseState PrevState { get { return prev; } }

        /// <summary>
        /// Current XNA Mouse State
        /// </summary>
        public MouseState CurState { get { return cur; } }

        /// <summary>
        /// Constructs instance of the mouse
        /// </summary>
        public gxtMouse()
        {
            prevScroll = Mouse.GetState().ScrollWheelValue;
            deltaScroll = 0;
        }

        /// <summary>
        /// Updates state
        /// </summary>
        public void Update()
        {
            prev = cur;
            cur = Mouse.GetState();
            deltaScroll = cur.ScrollWheelValue - prevScroll;
            prevScroll = cur.ScrollWheelValue;
        }

        /// <summary>
        /// Gets the state of mouse button
        /// </summary>
        /// <param name="button">Mouse Button</param>
        /// <returns>State</returns>
        public gxtControlState GetState(gxtMouseButton button)
        {
            // LEFT
            if (button == gxtMouseButton.LEFT)
            {
                if (cur.LeftButton == ButtonState.Pressed)
                {
                    if (prev.LeftButton == ButtonState.Pressed) return gxtControlState.DOWN;
                    else return gxtControlState.FIRST_PRESSED;
                }
                else
                {
                    if (prev.LeftButton == ButtonState.Pressed) return gxtControlState.FIRST_RELEASED;
                    else return gxtControlState.UP;
                }
            }
            // RIGHT
            else if (button == gxtMouseButton.RIGHT)
            {
                if (cur.RightButton == ButtonState.Pressed)
                {
                    if (prev.RightButton == ButtonState.Pressed) return gxtControlState.DOWN;
                    else return gxtControlState.FIRST_PRESSED;
                }
                else
                {
                    if (prev.RightButton == ButtonState.Pressed) return gxtControlState.FIRST_RELEASED;
                    else return gxtControlState.UP;
                }
            }
            // MIDDLE
            else if (button == gxtMouseButton.MIDDLE)
            {
                if (cur.MiddleButton == ButtonState.Pressed)
                {
                    if (prev.MiddleButton == ButtonState.Pressed) return gxtControlState.DOWN;
                    else return gxtControlState.FIRST_PRESSED;
                }
                else
                {
                    if (prev.MiddleButton == ButtonState.Pressed) return gxtControlState.FIRST_RELEASED;
                    else return gxtControlState.UP;
                }
            }
            // SIDE1
            else if (button == gxtMouseButton.SIDE1)
            {
                if (cur.XButton1 == ButtonState.Pressed)
                {
                    if (prev.XButton1 == ButtonState.Pressed) return gxtControlState.DOWN;
                    else return gxtControlState.FIRST_PRESSED;
                }
                else
                {
                    if (prev.XButton1 == ButtonState.Pressed) return gxtControlState.FIRST_RELEASED;
                    else return gxtControlState.UP;
                }
            }
            // SIDE2
            else if (button == gxtMouseButton.SIDE2)
            {
                if (cur.XButton2 == ButtonState.Pressed)
                {
                    if (prev.XButton2 == ButtonState.Pressed) return gxtControlState.DOWN;
                    else return gxtControlState.FIRST_PRESSED;
                }
                else
                {
                    if (prev.XButton2 == ButtonState.Pressed) return gxtControlState.FIRST_RELEASED;
                    else return gxtControlState.UP;
                }
            }
            // NOT IMPLEMENTED -- ASSERT
            else
            {
                gxtDebug.Assert(false, "The Mouse Button: " + button.ToString() + " has not been implemented in GetState()");
                return gxtControlState.UP; // will not be reached in debug mode
            }
        }

        /// <summary>
        /// Button down on this frame?
        /// </summary>
        /// <param name="button">Mouse Button</param>
        /// <returns>If down</returns>
        public bool IsDown(gxtMouseButton button)
        {
            if (button == gxtMouseButton.LEFT)
                return cur.LeftButton == ButtonState.Pressed;
            else if (button == gxtMouseButton.RIGHT)
                return cur.RightButton == ButtonState.Pressed;
            else if (button == gxtMouseButton.MIDDLE)
                return cur.MiddleButton == ButtonState.Pressed;
            else if (button == gxtMouseButton.SIDE1)
                return cur.XButton1 == ButtonState.Pressed;
            else
                return cur.XButton2 == ButtonState.Pressed;
        }

        /// <summary>
        /// Button up on this frame?
        /// </summary>
        /// <param name="button">Mouse Button</param>
        /// <returns>If up</returns>
        public bool IsUp(gxtMouseButton button)
        {
            return !IsDown(button);
        }

        /// <summary>
        /// X pos of mouse
        /// </summary>
        /// <returns></returns>
        public int GetXPos()
        {
            return cur.X;
        }

        /// <summary>
        /// Y pos of mouse
        /// </summary>
        /// <returns></returns>
        public int GetYPos()
        {
            return cur.Y;
        }

        /// <summary>
        /// Get mouse pos, relative to top left of window
        /// </summary>
        /// <returns></returns>
        public Vector2 GetPosition()
        {
            return new Vector2(cur.X, cur.Y);
        }

        /// <summary>
        /// Forces mouse position
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector2 pos)
        {
            Mouse.SetPosition((int)pos.X, (int)pos.Y);  // may want to round
        }

        /// <summary>
        /// +1 Mouse click forward
        /// -1 Mouse click back
        /// </summary>
        /// <returns></returns>
        public int GetDeltaScroll()
        {
            return deltaScroll / SCROLL_MODIFIER;
        }

        public bool AnyButtonIsDown()
        {
            if (cur.LeftButton == ButtonState.Pressed)
                return true;
            if (cur.MiddleButton == ButtonState.Pressed)
                return true;
            if (cur.RightButton == ButtonState.Pressed)
                return true;
            if (cur.XButton1 == ButtonState.Pressed)
                return true;
            if (cur.XButton2 == ButtonState.Pressed)
                return true;
            return false;
        }

        public bool AnyButtonIsDown(gxtMouseButton[] buttons)
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                if (IsDown(buttons[i]))
                    return true;
            }
            return false;
        }

        public bool AnyButtonIsFirstPressed()
        {
            if (GetState(gxtMouseButton.LEFT) == gxtControlState.FIRST_PRESSED)
                return true;
            if (GetState(gxtMouseButton.MIDDLE) == gxtControlState.FIRST_PRESSED)
                return true;
            if (GetState(gxtMouseButton.RIGHT) == gxtControlState.FIRST_PRESSED)
                return true;
            if (GetState(gxtMouseButton.SIDE1) == gxtControlState.FIRST_PRESSED)
                return true;
            if (GetState(gxtMouseButton.SIDE2) == gxtControlState.FIRST_PRESSED)
                return true;
            return false;
        }

        public bool AnyButtonIsFirstPressed(gxtMouseButton[] buttons)
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                if (GetState(buttons[i]) == gxtControlState.FIRST_PRESSED)
                    return true;
            }
            return false;
        }

        public bool IsMacroDown(params gxtMouseButton[] buttons)
        {
            foreach (gxtMouseButton button in buttons)
            {
                if (IsUp(button))
                    return false;
            }
            return true;
        }

        public bool IsMacroUp(params gxtMouseButton[] buttons)
        {
            foreach (gxtMouseButton button in buttons)
            {
                if (IsUp(button))
                    return true;
            }
            return false;
        }

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
        public bool TestMacroState(gxtControlState checkState, bool allowFirst, params gxtMouseButton[] buttons)
        {
            if (checkState == gxtControlState.UP)
                return CheckMacroUp(allowFirst, buttons);
            else if (checkState == gxtControlState.DOWN)
                return CheckMacroDown(allowFirst, buttons);
            else if (checkState == gxtControlState.FIRST_PRESSED)
                return CheckMacroFirstPressed(buttons);
            else    // first_released
                return CheckMacroFirstReleased(buttons);
        }

        private bool CheckMacroFirstPressed(params gxtMouseButton[] buttons)
        {
            //could just be a boolean
            int numFPressed = 0;
            foreach (gxtMouseButton button in buttons)
            {
                if (IsUp(button))
                    return false;
                else if (GetState(button) == gxtControlState.FIRST_PRESSED)
                    numFPressed++;
            }
            return numFPressed > 0;
        }

        private bool CheckMacroDown(bool allowFirst, params gxtMouseButton[] buttons)
        {
            foreach (gxtMouseButton button in buttons)
            {
                if (IsUp(button))
                    return false;
                if (!allowFirst && GetState(button) == gxtControlState.FIRST_PRESSED)
                    return false;
            }
            return true;
        }

        private bool CheckMacroUp(bool allowFirst, params gxtMouseButton[] buttons)
        {
            foreach (gxtMouseButton button in buttons)
            {
                if (IsDown(button))
                    return false;
                if (!allowFirst && GetState(button) == gxtControlState.FIRST_RELEASED)
                    return false;
            }
            return true;
        }

        private bool CheckMacroFirstReleased(params gxtMouseButton[] buttons)
        {
            int numFReleased = 0;
            foreach (gxtMouseButton button in buttons)
            {
                if (GetState(button) == gxtControlState.UP)
                    return false;
                else if (GetState(button) == gxtControlState.FIRST_RELEASED)
                    numFReleased++;
            }
            return numFReleased > 0;
        }
    }
}
