using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GXT;
using GXT.IO;

namespace GXT_SANDBOX
{
    public class ConfigTest
    {
        public static void RuntTest()
        {
            gxtLog log = new gxtLog();
            log.Initialize(true, gxtVerbosityLevel.INFORMATIONAL);

            gxtConsoleLogListener console = new gxtConsoleLogListener();
            console.Initialize();

            log.AddListener(console);

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "starting config test...");

            gxtINIFile file = new gxtINIFile();
            file.Initialize();
            file.Read("gxt_default.ini");
            gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, file.DebugTrace());
        }
    }
}
