using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class TextureMappingTest2
    {
        public static void RunTest()
        {
            using (TextureMappingTestGame2 game = new TextureMappingTestGame2())
            {
                game.Run();
            }
        }
    }
}
