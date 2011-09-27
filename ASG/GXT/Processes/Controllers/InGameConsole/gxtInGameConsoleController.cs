using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GXT.Input;


namespace GXT.Processes
{
    public class gxtInGameConsoleController : gxtIController
    {
        public static readonly Keys[] DEFAULT_CLEARLINE_MACRO = new Keys[]{ Keys.LeftControl, Keys.L };

        private bool enabled;
        private gxtInGameConsole inGameConsole;
        private Keys openCloseKey, enterCommandKey, backspaceKey, altBackSpaceKey;
        private Keys[] clearLineMacro;

        public virtual bool Enabled { get { return enabled; } set { enabled = value; } }

        public Keys ToggleOpenClosedKey { get { return openCloseKey; } set { openCloseKey = value; } }
        public Keys EnterCommandKey { get { return enterCommandKey; } set { enterCommandKey = value; } }
        public Keys BackSpaceKey { get { return backspaceKey; } set { backspaceKey = value; } }
        public Keys AltBackSpaceKey { get { return altBackSpaceKey; } set { altBackSpaceKey = value; } }
        public Keys[] ClearLineMacro { get { return clearLineMacro; } set { clearLineMacro = value; } }

        public virtual bool QueriesInput { get { return true; } }
        public gxtInGameConsole InGameConsole { get { return inGameConsole; } set { inGameConsole = value; } }

        public gxtInGameConsoleController()
        {

        }

        public void Initialize(bool initEnabled, gxtInGameConsole console, Keys toggleOpenClosedKey = Keys.OemTilde,
            Keys enterCommandKey = Keys.Enter, Keys backSpaceKey = Keys.Back, Keys altBackSpaceKey = Keys.Delete)
        {
            enabled = initEnabled;
            inGameConsole = console;
            this.openCloseKey = toggleOpenClosedKey;
            this.enterCommandKey = enterCommandKey;
            this.backspaceKey = backSpaceKey;
            this.altBackSpaceKey = altBackSpaceKey;
            this.clearLineMacro = DEFAULT_CLEARLINE_MACRO;
        }

        public virtual void Update(GameTime gameTime)
        {
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();

            if (kb.GetState(openCloseKey) == gxtControlState.FIRST_PRESSED)
                inGameConsole.ToggleOpen();

            if (!inGameConsole.IsOpen)
                return;

            if (kb.GetState(enterCommandKey) == gxtControlState.FIRST_PRESSED)
                inGameConsole.ExecuteCurrentCommand();

            if (kb.GetState(backspaceKey) == gxtControlState.FIRST_PRESSED || kb.GetState(altBackSpaceKey) == gxtControlState.FIRST_PRESSED)
                inGameConsole.DeleteLastCharacter();

            if (kb.TestMacroState(gxtControlState.FIRST_PRESSED, true, clearLineMacro))
                inGameConsole.ClearCurrentCommand();
        }

        public virtual void LateUpdate(GameTime gameTime)
        {

        }

        public Type GetTargetType()
        {
            return inGameConsole.GetType();
        }
    }
}
