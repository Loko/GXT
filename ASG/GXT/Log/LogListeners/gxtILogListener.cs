using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// An interface for concrete types capable of logging 
    /// runtime information (e.g. System Console, File Logger, In Game Console, etc.)
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public interface gxtILogListener
    {
        /// <summary>
        /// Enabled?
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Tagged For Removal From gxtLog
        /// </summary>
        bool RemovalRequested { get; }

        /// <summary>
        /// If this logger will use the global 
        /// verbosity set in gxtLog
        /// </summary>
        bool UseGlobalVerbosity { get; set; }

        /// <summary>
        /// The verbosity of this logger
        /// </summary>
        gxtVerbosityLevel Verbosity { get; set; }

        /// <summary>
        /// Actual verbosity level, either from the global verbosity 
        /// or defined here if UseGlobalVerbosity = false
        /// </summary>
        gxtVerbosityLevel ActiveVerbosityLevel { get; }

        /// <summary>
        /// If the logger should write time stamped messages
        /// </summary>
        bool UseTimeStamps { get; set; }

        /// <summary>
        /// Shuts down the logger and tags it for removal
        /// </summary>
        void RemoveListener();

        /// <summary>
        /// Writes a pre formatted string to the logger 
        /// </summary>
        /// <param name="verbosity">Message Verbosity</param>
        /// <param name="format">Formatted message</param>
        void WriteLineV(gxtVerbosityLevel verbosity, string format);

        /// <summary>
        /// Updates internals
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(GameTime gameTime);
    }
}

