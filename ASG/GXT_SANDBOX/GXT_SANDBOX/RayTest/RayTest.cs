using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class RayTest
    {
        public static void RunTest()
        {
            using (RayTestGame game = new RayTestGame())
            {
                game.Run();
            }
        }
    }
}
