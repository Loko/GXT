using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GXT.Input
{
    /// <summary>
    /// A class for managing an instance of the gamepad
    /// Retreive gamepads from gxtGamepadManager rather than 
    /// constructing them yourself
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtGamepad
    {
        private GamePadState cur, prev;
        private PlayerIndex playerIndex;
        private float leftMotorVib, rightMotorVib;

        /// <summary>
        /// Previous XNA gamepad state
        /// </summary>
        public GamePadState PrevState { get { return prev; } }
        /// <summary>
        /// Current XNA gamepad state
        /// </summary>
        public GamePadState CurState { get { return cur; } }

        /// <summary>
        /// Index of the controller (1, 2, 3, 4)
        /// </summary>
        public PlayerIndex Index { get { return playerIndex; } }

        // Possibly deadzone control here
        
        /// <summary>
        /// Vibration value for the left motor
        /// </summary>
        public float LeftMotorVibration { get { return leftMotorVib; } set { leftMotorVib = value; } }

        /// <summary>
        /// Vibration motor for the right motor
        /// </summary>
        public float RightMotorVibration { get { return rightMotorVib; } set { rightMotorVib = value; } }

        /// <summary>
        /// Connected?
        /// </summary>
        public bool IsConnected { get { return cur.IsConnected; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerIndex">Controller Index</param>
        public gxtGamepad(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
        }

        /// <summary>
        /// Updates state
        /// </summary>
        public void Update()
        {
            prev = cur;
            cur = GamePad.GetState(playerIndex);
            // controller may be too busy to process the request
            // no need to check if vibration == 0 here, it will be clamped in func
            GamePad.SetVibration(playerIndex, LeftMotorVibration, RightMotorVibration);
        }

        /// <summary>
        /// Gets state for given button
        /// </summary>
        /// <param name="button">Button</param>
        /// <returns>State of button</returns>
        public gxtControlState GetState(Buttons button)
        {
            if (cur.IsButtonDown(button))
            {
                if (prev.IsButtonDown(button)) return gxtControlState.DOWN;
                else return gxtControlState.FIRST_PRESSED;
            }
            else
            {
                if (prev.IsButtonDown(button)) return gxtControlState.FIRST_RELEASED;
                else return gxtControlState.UP;
            }
        }

        /// <summary>
        /// Position of Left Thumbstick relative to deadzone
        /// </summary>
        /// <returns></returns>
        public Vector2 LStick()
        {
            return cur.ThumbSticks.Left;
        }

        /// <summary>
        /// Adjusted assuming using the default xna coordinate system
        /// Y is flipped
        /// </summary>
        /// <returns></returns>
        public Vector2 AdjLStick()
        {
            return new Vector2(cur.ThumbSticks.Left.X, -cur.ThumbSticks.Left.Y);
        }

        /// <summary>
        /// Adjusted assuming using the default xna coordinate system
        /// Y is flipped
        /// </summary>
        /// <returns></returns>
        public Vector2 AdjRStick()
        {
            return new Vector2(cur.ThumbSticks.Right.X, -cur.ThumbSticks.Right.Y);
        }

        /// <summary>
        /// Position of Right Thumbstick relative to deadzone
        /// </summary>
        /// <returns></returns>
        public Vector2 RStick()
        {
            return cur.ThumbSticks.Right;
        }

        /// <summary>
        /// 0 - 1 right trigger
        /// </summary>
        /// <returns></returns>
        public float RTrigger()
        {
            return cur.Triggers.Right;
        }

        /// <summary>
        /// 0 - 1 left trigger
        /// </summary>
        /// <returns></returns>
        public float LTrigger()
        {
            return cur.Triggers.Left;
        }

        /// <summary>
        /// Button down?
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsDown(Buttons button)
        {
            return cur.IsButtonDown(button);
        }

        /// <summary>
        /// Button up?
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsUp(Buttons button)
        {
            return cur.IsButtonUp(button);
        }


        public bool AnyButtonIsDown()
        {
            for (Buttons i = Buttons.A; i < Buttons.Y; ++i)
            {
                if (cur.IsButtonDown(i))
                    return true;
            }
            /*
            if (cur.IsButtonDown(Buttons.A))
                return true;
            if (cur.IsButtonDown(Buttons.B))
                return true;
            if (cur.IsButtonDown(Buttons.Back))
                return true;
            if (cur.IsButtonDown(Buttons.BigButton))
                return true;
            if (cur.IsButtonDown(Buttons.DPadDown))
                return true;
            if (cur.IsButtonDown(Buttons.DPadLeft))
                return true;
            */
            return false;
        }

        public bool AnyButtonIsDown(Buttons[] buttons)
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                if (cur.IsButtonDown(buttons[i]))
                    return true;
            }
            return false;
        }

        public bool AnyButtonIsFirstPressed()
        {
            for (Buttons i = Buttons.A; i < Buttons.Y; ++i)
            {
                if (GetState(i) == gxtControlState.FIRST_PRESSED)
                    return true;
            }
            return false;
        }

        public bool AnyButtonIsFirstPressed(Buttons[] buttons)
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                if (GetState(buttons[i]) == gxtControlState.FIRST_PRESSED)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Every button in the macro down?
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public bool IsMacroDown(params Buttons[] buttons)
        {
            foreach (Buttons button in buttons)
            {
                if (IsUp(button))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Every button in the macro up?
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public bool IsMacroUp(params Buttons[] buttons)
        {
            foreach (Buttons button in buttons)
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
        public bool TestMacroState(gxtControlState checkState, bool allowFirst, params Buttons[] buttons)
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

        /// <summary>
        /// Determines if the macro is in the "first pressed" state
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns></returns>
        private bool CheckMacroFirstPressed(params Buttons[] buttons)
        {
            //could just be a boolean
            int numFPressed = 0;
            foreach (Buttons button in buttons)
            {
                if (IsUp(button))
                    return false;
                else if (GetState(button) == gxtControlState.FIRST_PRESSED)
                    numFPressed++;
            }
            return numFPressed > 0;
        }

        /// <summary>
        /// Checks if the macro is in the "down" state
        /// </summary>
        /// <param name="allowFirst"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        private bool CheckMacroDown(bool allowFirst, params Buttons[] buttons)
        {
            foreach (Buttons button in buttons)
            {
                if (IsUp(button))
                    return false;
                if (!allowFirst && GetState(button) == gxtControlState.FIRST_PRESSED)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the macro is in the "up" state
        /// </summary>
        /// <param name="allowFirst"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        private bool CheckMacroUp(bool allowFirst, params Buttons[] buttons)
        {
            foreach (Buttons button in buttons)
            {
                if (IsDown(button))
                    return false;
                if (!allowFirst && GetState(button) == gxtControlState.FIRST_RELEASED)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the macro is in the "first released" state
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns></returns>
        private bool CheckMacroFirstReleased(params Buttons[] buttons)
        {
            int numFReleased = 0;
            foreach (Buttons button in buttons)
            {
                if (GetState(button) == gxtControlState.UP)
                    return false;
                else if (GetState(button) == gxtControlState.FIRST_RELEASED)
                    numFReleased++;
            }
            return numFReleased > 0;
        }

        /// <summary>
        /// Sets vibration on the frame
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void SetVibration(float left, float right)
        {
            LeftMotorVibration = left;
            RightMotorVibration = right;
        }

        /// <summary>
        /// Gets vibration of both motors in one call
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void GetVibration(out float left, out float right)
        {
            left = LeftMotorVibration;
            right = RightMotorVibration;
        }
    }
}
