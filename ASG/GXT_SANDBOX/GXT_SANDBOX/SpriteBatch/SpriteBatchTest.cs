using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class SpriteBatchTest
    {
        public static void RunTest()
        {
            using (SpriteBatchTestGame game = new SpriteBatchTestGame())
            {
                game.Run();
            }
        }
    }
}
