using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class AudioTest
    {
        public static void RunTest()
        {
            using (AudioTestGame game = new AudioTestGame())
            {
                game.Run();
            }
        }
    }
}
