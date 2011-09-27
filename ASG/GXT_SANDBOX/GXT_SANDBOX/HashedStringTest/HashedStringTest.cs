using System;
using GXT;
namespace GXT_SANDBOX
{
    public class HashedStringTest
    {
        public static void Print(gxtHashedString hstr)
        {
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "String: {0} Value: {1}", hstr.String, hstr.Id);
        }

        public static void RunTest()
        {
            gxtLog log = new gxtLog();
            log.Initialize(true, gxtVerbosityLevel.INFORMATIONAL);

            gxtConsoleLogListener console = new gxtConsoleLogListener();
            console.Initialize();

            log.AddListener(console);

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Starting hashed string test...\n");

            string a = "ccl";
            string b = "gxt";
            string c = "rit";
            string d = "hashedString";
            string e = "123abc";
            string f = "zebra";
            string g = "gazongas";
            string h = "PLAYER_TYPE";
            string i = "ENEMY_TYPE";
            string j = "0";

            gxtHashedString ha = new gxtHashedString(a);
            gxtHashedString hb = new gxtHashedString(b);
            gxtHashedString hc = new gxtHashedString(c);
            gxtHashedString hd = new gxtHashedString(d);
            gxtHashedString he = new gxtHashedString(e);
            gxtHashedString hf = new gxtHashedString(f);
            gxtHashedString hg = new gxtHashedString(g);
            gxtHashedString hh = new gxtHashedString(h);
            gxtHashedString hi = new gxtHashedString(i);
            gxtHashedString hj = new gxtHashedString(j);

            Print(ha);
            Print(hb);
            Print(hc);
            Print(hd);
            Print(he);
            Print(hf);
            Print(hg);
            Print(hh);
            Print(hi);
            Print(hj);

            bool lessThan = (ha < hb);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, lessThan);
            bool eq = he.Id == gxtHashedString.Hash("123abc");
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, eq);

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "\nFinished hashed string test...");
        }
    }
}
