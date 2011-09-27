using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASG;

namespace GXT_SANDBOX
{
    public class ASGPrototype
    {
        public static void RunTest()
        {
            using (asgGame game = new asgGame())
            {
                game.Run();
            }
        }
    }
}
