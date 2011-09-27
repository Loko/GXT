using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class KinematicTest
    {
        public static void RunTest()
        {
            using (KinematicTestGame game = new KinematicTestGame())
            {
                game.Run();
            }
        }
    }
}
