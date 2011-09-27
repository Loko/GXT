using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class PhysicsStressTest
    {
        public static void RunTest()
        {
            using (PhysicsStressTestGame game = new PhysicsStressTestGame())
            {
                game.Run();
            }
        }
    }
}
