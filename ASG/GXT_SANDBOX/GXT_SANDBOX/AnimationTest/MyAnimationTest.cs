using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class MyAnimationTest
    {
        public static void RunTest()
        {
            using (MyAnimationTestGame game = new MyAnimationTestGame())
            {
                game.Run();
            }
        }
    }
}
