using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT_SANDBOX
{
    public class SAPTest
    {
        public static void RunTest()
        {
            using (SAPTestGame game = new SAPTestGame())
            {
                game.Run();
            }
        }
    }
}
