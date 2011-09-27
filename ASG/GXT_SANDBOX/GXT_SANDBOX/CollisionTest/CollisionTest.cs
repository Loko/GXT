using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class CollisionTest
    {
        public static void RunTest()
        {
            using (CollisionTestGame game = new CollisionTestGame())
            {
                game.Run();
            }
        }
    }
}
