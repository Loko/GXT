using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class ProcessTest
    {
        //ProcessTestGame game;
        public static void RunTest()
        {
            using (ProcessTestGame game = new ProcessTestGame())
            {
                game.Run();
            }
        }
    }
}
