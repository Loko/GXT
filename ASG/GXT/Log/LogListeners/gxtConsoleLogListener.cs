using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A LogListener that formats color coded output to the windows system console
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtConsoleLogListener : gxtILogListener
    {
        // log listener variables
        protected bool enabled;
        protected bool removalRequested;
        protected bool useGlobalVerbosity;
        protected bool useTimeStamps;
        protected gxtVerbosityLevel loggerVerbosity;

        // console colors for each verbosity level
        protected ConsoleColor informationalConsoleColor;
        protected ConsoleColor successConsoleColor;
        protected ConsoleColor warningConsoleColor;
        protected ConsoleColor criticalConsoleColor;

        /// <summary>
        /// Enabled?
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Tagged For Removal From gxtLog
        /// </summary>
        public bool RemovalRequested { get { return removalRequested; } }

        /// <summary>
        /// If this logger will use the global 
        /// verbosity set in gxtLog
        /// </summary>
        public bool UseGlobalVerbosity { get { return useGlobalVerbosity; } set { useGlobalVerbosity = value; } }

        /// <summary>
        /// The verbosity of this logger
        /// </summary>
        public gxtVerbosityLevel Verbosity { get { return loggerVerbosity; } set { loggerVerbosity = value; } }

        /// <summary>
        /// Actual verbosity level, either from the global verbosity 
        /// or defined here if UseGlobalVerbosity = false
        /// </summary>
        public gxtVerbosityLevel ActiveVerbosityLevel
        {
            get
            {
                if (UseGlobalVerbosity)
                {
                    if (gxtLog.SingletonIsInitialized)
                        return gxtLog.Singleton.Verbosity;
                }
                return Verbosity;
            }
        }

        /// <summary>
        /// Timestamps before each write?
        /// </summary>
        public bool UseTimeStamps { get { return useTimeStamps; } set { useTimeStamps = value; } }
        
        /// <summary>
        /// Informational Console Color
        /// </summary>
        public ConsoleColor InformationalColor { get { return informationalConsoleColor; } set { informationalConsoleColor = value; } }

        /// <summary>
        /// Success Console Color
        /// </summary>
        public ConsoleColor SuccessColor { get { return successConsoleColor; } set { successConsoleColor = value; } }

        /// <summary>
        /// Warning Console Color
        /// </summary>
        public ConsoleColor WarningColor { get { return warningConsoleColor; } set { warningConsoleColor = value; } }

        /// <summary>
        /// Critical Console Color
        /// </summary>
        public ConsoleColor CriticalColor { get { return criticalConsoleColor; } set { criticalConsoleColor = value; } }

        /// <summary>
        /// Title of the console window
        /// </summary>
        public string WindowTitle { get { return Console.Title; } set { Console.Title = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtConsoleLogListener() { }

        /// <summary>
        /// Initializes the ConsoleLogger
        /// Every parameter has a default
        /// </summary>
        /// <param name="initEnabled">Enabled in init</param>
        /// <param name="verbosity">Logger Verbosity</param>
        /// <param name="useGlobalVerbosity">Use Global Verbosity?</param>
        /// <param name="useTimeStamps">Timestamps?</param>
        /// <param name="windowTitle">Window Title</param>
        /// <param name="informationalColor">Informational Console Color</param>
        /// <param name="successColor">Success Console Color</param>
        /// <param name="warningColor">Warning Console Color</param>
        /// <param name="criticalColor">Critical Console Color</param>
        public void Initialize(bool initEnabled = true, gxtVerbosityLevel verbosity = gxtVerbosityLevel.INFORMATIONAL, bool useGlobalVerbosity = true, bool useTimeStamps = false, string windowTitle = "GXT Console Log", 
            ConsoleColor informationalColor = ConsoleColor.White, ConsoleColor successColor = ConsoleColor.Green, ConsoleColor warningColor = ConsoleColor.Yellow, ConsoleColor criticalColor = ConsoleColor.Red)
        {
            Enabled = initEnabled;
            UseGlobalVerbosity = useGlobalVerbosity;
            Verbosity = verbosity;
            UseTimeStamps = useTimeStamps;

            InformationalColor = informationalColor;
            SuccessColor = successColor;
            WarningColor = warningColor;
            CriticalColor = criticalColor;
            WindowTitle = windowTitle;
        }

        /// <summary>
        /// Writeline, with verbosiy arguments
        /// </summary>
        /// <param name="verbosity">Verbosity</param>
        /// <param name="format">Formatted string</param>
        public void WriteLineV(gxtVerbosityLevel verbosity, string format)
        {
            if (!enabled) return;
            gxtVerbosityLevel activeVerbosity = ActiveVerbosityLevel;
            if (verbosity > activeVerbosity) return;
            ConsoleColor color = GetLogColor(verbosity);
            Console.ForegroundColor = color;
            if (UseTimeStamps)
                Console.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff tt : ") + format);
            else
                Console.WriteLine(format);
        }

        /// <summary>
        /// Tells gamelog class it should remove this listener
        /// </summary>
        public void RemoveListener()
        {
            removalRequested = true;
        }

        /// <summary>
        /// Updates the logger
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Update(GameTime gameTime)
        {
            // nothing to update
        }

        /// <summary>
        /// Clears the console
        /// </summary>
        public void Clear()
        {
            Console.Clear();
        }

        /// <summary>
        /// Simple function that gets the console color based on the verbosity level
        /// Keep updated if number of verbosity levels grow
        /// </summary>
        /// <param name="v">Verbosity</param>
        /// <returns>Matching Console Color</returns>
        private ConsoleColor GetLogColor(gxtVerbosityLevel v)
        {
            if (v == gxtVerbosityLevel.CRITICAL)
                return criticalConsoleColor;
            else if (v == gxtVerbosityLevel.WARNING)
                return warningConsoleColor;
            else if (v == gxtVerbosityLevel.SUCCESS)
                return successConsoleColor;
            else
                return informationalConsoleColor;
        }
    }
}
