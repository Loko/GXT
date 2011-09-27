#if DEBUG
#define GXT_ASSERTIONS_ENABLED
#if GXT_ASSERTIONS_ENABLED    
#define GXT_SLOW_ASSERTIONS_ENABLED
#endif
#endif

using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A general purpose class with utility functions for assertions, screenshots, and 
    /// frame-rate calculations.  Screenshot methods are currently unimplemented.  Please 
    /// note that debug drawing is provided in the seperate gxtDebugDrawer class
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: IMPLEMENT SCREENSHOTS WITH XNA 4.0 BUFFER SYSTEM
    public class gxtDebug
    {
        // variables for fps calculations
        private static float elapsedTime;
        private static float totalFrames;
        private static float fps;

        /// <summary>
        /// Determines if fps calculations are even processed
        /// </summary>
        public static bool EnableFPSCalulations { get; set; }

        /// <summary>
        /// Initializes the Debug System.  Needed for FPS calculations and screenshots, 
        /// but not for Assertions
        /// </summary>
        /// <param name="calcFPS">Enable FPS Calculations</param>
        public static void Initialize(bool calcFPS = true)
        {
            EnableFPSCalulations = calcFPS;
        }

        /// <summary>
        /// Updates debug internals
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public static void Update(GameTime gameTime)
        {
            if (!EnableFPSCalulations) return;

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            ++totalFrames;

            if (elapsedTime >= 1.0f)
            {
                fps = totalFrames;
                totalFrames = 0;
                elapsedTime = 0;
            }
        }

        /// <summary>
        /// Gets the current FPS calculation
        /// </summary>
        /// <returns>Current FPS</returns>
        public static float GetFPS()
        {
            return fps;
        }

        /// <summary>
        /// Gets the current working set of memory associated with the program
        /// </summary>
        /// <returns>Memory Usage (in bytes)</returns>
        public static long GetMemoryUsage()
        {
            return Process.GetCurrentProcess().WorkingSet64;
        }

        /// <summary>
        /// If an assertion is false, the program will terminate
        /// Assertions are conditional on the DEBUG preprocessor directive
        /// This custom assertion will display a more meaningful output to 
        /// GXT logs in addition to the usual functionality provided by C#'s
        /// Debug.Assert() function
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="message">Failure Message</param>
        /// <param name="detailMessage">Detailed Failure Message</param>
        [Conditional("DEBUG"), Conditional("GXT_ASSERTIONS_ENABLED")]
        public static void Assert(bool condition, string message = null, string detailMessage = null)
        {
            // only terminate if the condition is false
            if (!condition)
            {
                // get and format the stack trace
                StackTrace stackTrace = new StackTrace();
                StackFrame[] frames = stackTrace.GetFrames();
                string traceStr = "";
                MethodBase tmpMethod;
                string tmpMethodStr = "";
                string tmpTypeName = "";
                for (int i = 1; i < frames.Length; ++i)
                {
                    tmpMethod = frames[i].GetMethod();
                    if (tmpMethod.IsSpecialName)
                    {
                        if (tmpMethod.IsConstructor)
                        {
                            tmpMethodStr = "()";
                        }
                        else
                        {
                            tmpMethodStr = " {" + tmpMethod.Name + "}";
                        }
                        tmpTypeName = tmpMethod.DeclaringType.Name;
                        int rIdx = tmpTypeName.LastIndexOf("`");
                        if (rIdx != -1)
                        {
                            tmpTypeName = tmpTypeName.Remove(rIdx);
                        }
                    }
                    else
                    {
                        tmpTypeName = tmpMethod.DeclaringType.Name + ".";
                        tmpMethodStr = tmpMethod.Name + "()";
                    }
                    traceStr += tmpTypeName + tmpMethodStr + "\n";
                }

                // log assertion, stack trace, and messages (if message was provided)
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Assertion Failed!\n");

                if (message != null)
                    gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Message: {0}\n", message);
                if (detailMessage != null)
                    gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Detailed Message: {0}", detailMessage);
                
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "\n\n{0}", traceStr);

                // forcefully terminate the program
                if (message == null)
                    Debug.Fail("Assertion Failed!");
                if (detailMessage == null)
                    Debug.Fail(message);
                else
                    Debug.Fail(message, detailMessage);
            }
        }

        [Conditional("DEBUG"), Conditional("GXT_ASSERTIONS_ENABLED"), Conditional("GXT_SLOW_ASSERTIONS_ENABLED")]
        public static void SlowAssert(bool condition, string message = null, string detailMessage = null)
        {
            Assert(condition, message, detailMessage);
        }
    }
}
