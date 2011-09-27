using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GXT;

namespace GXT_SANDBOX
{
    public class CircularBufferTest
    {
        public static void RunTest()
        {
            gxtLog log = new gxtLog();
            log.Initialize();

            gxtConsoleLogListener consoleLogger = new gxtConsoleLogListener();
            consoleLogger.Initialize();

            log.AddListener(consoleLogger);

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Circular Buffer Test");

            gxtRandom rng = new gxtRandom();

            gxtCircularBuffer<int> cbuffer = new gxtCircularBuffer<int>(6);
            for (int i = 0; i < cbuffer.Capacity; i++)
            {
                cbuffer.Enqueue(i);
            }

            for (int i = 0; i < cbuffer.Capacity / 2; i++)
            {
                cbuffer.Enqueue(i * 2);
            }

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Finished Adding Values...Popping off the values now");

            while (!cbuffer.IsEmpty)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, cbuffer.Dequeue());
            }

        }
    }
}
