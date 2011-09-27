using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class SceneGraphTest
    {
        public static void RunTest()
        {
            using (SceneGraphTestGame game = new SceneGraphTestGame())
            {
                game.Run();
            }
        }
    }
}
