using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GXT.Input;

namespace GXT.Processes
{
    /// <summary>
    /// 
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtKeyboardCharacterProcessor : gxtProcess
    {
        // prevents more cleanup
        private gxtKeyboard keyboard;
        private bool shift;
        private Keys[] downKeys;
        public event gxtKeyboardCharacterEnteredHandler OnCharacterEntered;

        public gxtKeyboard Keyboard { get { return keyboard; } set { keyboard = value; } }

        public gxtKeyboardCharacterProcessor(bool initEnabled) 
            : base(initEnabled, true, gxtProcess.INPUT_TYPE)
        {

        }

        public gxtKeyboardCharacterProcessor(bool initEnabled, gxtKeyboard keyboard)
            : base(initEnabled, true, gxtProcess.INPUT_TYPE)
        {
            this.keyboard = keyboard;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!Enabled || OnCharacterEntered == null)
                return;
            
            if (keyboard == null)
            {
                keyboard = gxtKeyboardManager.Singleton.GetKeyboard();
                if (keyboard == null)
                    return;
            }

            // alot faster to process the down keys then all of them
            downKeys = keyboard.GetPressedKeys();

            if (downKeys.Length == 0)
                return;

            shift = keyboard.IsDown(Keys.LeftShift) || keyboard.IsDown(Keys.RightShift);

            // space
            if (keyboard.GetState(Keys.Space) == gxtControlState.FIRST_PRESSED)
                OnCharacterEntered(' ');

            ProcessKey(Keys.OemPeriod, '.', '>');        // period, gt
            ProcessKey(Keys.OemComma, ',', '<');         // comma, lt
            ProcessKey(Keys.OemQuotes, '\'', '\"');       // quotes
            ProcessKey(Keys.OemMinus, '-', '_');         // minus, underscore
            ProcessKey(Keys.OemPlus, '=', '+');          // equals, plus
            ProcessKey(Keys.OemSemicolon, ';', ':');     // semicolon, colon
            ProcessKey(Keys.OemQuestion, '/', '?');      // slash, question
            ProcessKey(Keys.OemPipe, '\\', '|');         // backslash, pipe
            ProcessKey(Keys.OemOpenBrackets, '[', '{');  // open brackets
            ProcessKey(Keys.OemCloseBrackets, ']', '}'); // close brackets

            // all alphabetical keys
            // all the digits
            // and shift digit special characters (e.g. !, @, and #)
            for (int i = 0; i < downKeys.Length; ++i)
            {
                Keys testKey = downKeys[i];
                if (keyboard.GetState(testKey) == gxtControlState.FIRST_PRESSED)
                {
                    string keyString = testKey.ToString();
                    if (keyString.Length == 1)
                    {
                        char c = keyString[0];
                        if (char.IsLetter(c))
                        {
                            if (!shift)
                                OnCharacterEntered(char.ToLower(c));
                            else
                                OnCharacterEntered(c);
                        }
                    }
                    else
                    {
                        char digit;
                        if (IsDigit(testKey, shift, out digit))
                        {
                            OnCharacterEntered(digit);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="normal"></param>
        /// <param name="ifShift"></param>
        private void ProcessKey(Keys key, char normal, char ifShift)
        {
            if (keyboard.GetState(key) == gxtControlState.FIRST_PRESSED)
            {
                if (!shift)
                    OnCharacterEntered(normal);
                else
                    OnCharacterEntered(ifShift);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="shift"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsDigit(Keys key, bool shift, out char c)
        {
            c = char.MinValue;
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                if (!shift)
                {
                    string digitString = key.ToString();
                    digitString = digitString.Remove(0, 1);
                    c = digitString[0];
                }
                else
                {
                    if (key == Keys.D0)
                        c = ')';
                    else if (key == Keys.D1)
                        c = '!';
                    else if (key == Keys.D2)
                        c = '@';
                    else if (key == Keys.D3)
                        c = '#';
                    else if (key == Keys.D4)
                        c = '$';
                    else if (key == Keys.D5)
                        c = '%';
                    else if (key == Keys.D6)
                        c = '^';
                    else if (key == Keys.D7)
                        c = '&';
                    else if (key == Keys.D8)
                        c = '*';
                    else
                        c = '(';
                }
                return true;
            }
            return false;
        }
    }
}
