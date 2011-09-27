using GXT;
using GXT.Input;

namespace GXT_SANDBOX
{
    public class Assert_Test
    {
        public static void RunTest()
        {
            gxtLog log = new gxtLog();
            //gxtLog log = new gxtLog();
            //gxtLog.Singleton.Enabled;
            gxtConsoleLogListener consolelogger = new gxtConsoleLogListener();
            log.Initialize();
            consolelogger.Initialize();
            log.AddListener(consolelogger);
            gxtHTMLLogListener htmllogger = new gxtHTMLLogListener();
            htmllogger.Initialize();
            log.AddListener(htmllogger);
            //log.Initialize();
            //log.AddListener(consolelogger);

            gxtGamepadManager manager;
            //gxtGamepadManager.Singleton.GetGamepad(Microsoft.Xna.Framework.PlayerIndex.One);
            manager = new gxtGamepadManager();
            gxtGamepadManager manager2;
            manager2 = new gxtGamepadManager();
        }
    }
}
