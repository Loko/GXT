using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A LogListener which will write color coded HTML log files to the 
    /// desired directory.  Partially inspired by this implementation: 
    /// http://www.david-amador.com/2009/11/how-to-do-a-xna-log-file/
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: VARIABLE AUTO-FLUSH?
    // TODO: VERIFY THE STRING IS A VALID HTML COLOR?
    // TODO: MAKE IO ROUTINES PLATFORM INDEPENDENT
    public class gxtHTMLLogListener : gxtILogListener
    {
        // log listener variables
        private bool enabled;
        private bool removalRequested;
        protected bool useGlobalVerbosity;
        protected bool useTimeStamps;
        protected gxtVerbosityLevel loggerVerbosity;

        // html log specific
        private StreamWriter streamWriter;
        private string filePath;
        private string informationalColor;
        private string successColor;
        private string warningColor;
        private string criticalColor;

        /// <summary>
        /// Enabled?
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// If tagged for removal
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
                    return gxtLog.Singleton.Verbosity;
                else
                    return Verbosity;
            }
        }

        /// <summary>
        /// If the logger should write time stamped messages
        /// </summary>
        public bool UseTimeStamps { get { return useTimeStamps; } set { useTimeStamps = value; } }

        /// <summary>
        /// File path to the HTML file
        /// </summary>
        public string FilePath { get { return filePath; } }

        /// <summary>
        /// HTML Color for Informational Messages
        /// </summary>
        public string InformationalColor { get { return informationalColor; } set { informationalColor = value; } }

        /// <summary>
        /// HTML Color for Success Messages
        /// </summary>
        public string SuccessColor { get { return successColor; } set { successColor = value; } }

        /// <summary>
        /// HTML Color for Warning Messages
        /// </summary>
        public string WarningColor { get { return warningColor; } set { warningColor = value; } }

        /// <summary>
        /// HTML Color for Critical Messages
        /// </summary>
        public string CriticalColor { get { return criticalColor; } set { criticalColor = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtHTMLLogListener() { }

        /// <summary>
        /// Sets up the listener with the given settings
        /// </summary>
        /// <param name="initEnabled">Enabled flag</param>
        /// <param name="verbosity">Logger verbosity level</param>
        /// <param name="useGlobalVerbosity">If using the global verbosity</param>
        /// <param name="useTimeStamps">Timestamps before each log entry</param>
        /// <param name="relativePath">Relative file path of the file</param>
        /// <param name="informationalColor">Informational HTML color</param>
        /// <param name="successColor">Success HTML color</param>
        /// <param name="warningColor">Warning HTML color</param>
        /// <param name="criticalColor">Critical HTML color</param>
        public void Initialize(bool initEnabled = true, gxtVerbosityLevel verbosity = gxtVerbosityLevel.INFORMATIONAL, bool useGlobalVerbosity = true, bool useTimeStamps = true, string relativePath = @"\log\", string informationalColor = "000000", string successColor = "#009900",
            string warningColor = "#FFCC00", string criticalColor = "#CC0000")
        {
            Enabled = initEnabled;

            UseGlobalVerbosity = useGlobalVerbosity;
            Verbosity = verbosity;

            InformationalColor = informationalColor;
            SuccessColor = successColor;
            WarningColor = warningColor;
            CriticalColor = criticalColor;

            string timeStamp = DateTime.Now.ToString("yyyy.MM.dd-hh.mm.ss");

            // warning, this is PC specific
            string directory = Directory.GetCurrentDirectory() + relativePath;
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            filePath = directory + timeStamp + "_log.html";

            streamWriter = new StreamWriter(filePath);
            streamWriter.AutoFlush = true;
        }

        /// <summary>
        /// Writes a pre formatted string to the logger
        /// </summary>
        /// <param name="verbosity">Verbosity</param>
        /// <param name="format">Formatted string</param>
        public void WriteLineV(gxtVerbosityLevel verbosity, string format)
        {
            if (!Enabled) return;
            gxtVerbosityLevel activeVerbosity = ActiveVerbosityLevel;
            if (verbosity > activeVerbosity) return;

            string color = GetLogColor(verbosity);
            // need to split on linebreaks
            // \n is not a line break in HTML
            string[] lines = format.Split('\n');
            if (UseTimeStamps)
            {
                string timeStamp = DateTime.Now.ToString("hh:mm:ss.fff tt : ");
                for (int i = 0; i < lines.Length; ++i)
                {
                    streamWriter.WriteLine("<span style=\"color:" + color + "\">" + timeStamp + lines[i] + "</span><br>");
                }
            }
            else
            {
                for (int i = 0; i < lines.Length; ++i)
                {
                    streamWriter.WriteLine("<span style=\"color:" + color + "\">" + lines[i] + "</span><br>");
                }
            }
        }

        /// <summary>
        /// Updates the log listener, mainly by flushing the buffer if auto flush is disabled
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            gxtDebug.Assert(streamWriter != null);
            if (!streamWriter.AutoFlush)
                streamWriter.Flush();
        }

        /// <summary>
        /// Tells the controlling log class it should remove this listener
        /// </summary>
        public void RemoveListener()
        {
            removalRequested = true;
            streamWriter.Dispose();
        }

        /// <summary>
        /// Simple function that gets the console color based on the verbosity level
        /// Keep updated if number of verbosity levels grow
        /// </summary>
        /// <param name="v">Verbosity</param>
        /// <returns>Matching Log Color</returns>
        private string GetLogColor(gxtVerbosityLevel v)
        {
            if (v == gxtVerbosityLevel.CRITICAL)
                return CriticalColor;
            else if (v == gxtVerbosityLevel.WARNING)
                return WarningColor;
            else if (v == gxtVerbosityLevel.SUCCESS)
                return SuccessColor;
            else
                return InformationalColor;
        }
    }
}
