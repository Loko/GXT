using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class TextureMappingTest
    {
        public static void RunTest()
        {
            using (TextureMappingTestGame game = new TextureMappingTestGame())
            {
                game.Run();
            }
        }
    }
}
