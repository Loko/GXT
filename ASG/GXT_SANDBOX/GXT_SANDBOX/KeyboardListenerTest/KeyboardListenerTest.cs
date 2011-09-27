using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class KeyboardListenerTest
    {
        public static void RunTest()
        {
            using (KeyboardListenerTestGame game = new KeyboardListenerTestGame())
            {
                game.Run();
            }
        }
    }
}