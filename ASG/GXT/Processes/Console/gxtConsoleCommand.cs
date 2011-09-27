using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT
{
    /// <summary>
    /// Returns feedback string from the command
    /// Actual logic should be performed in the handler
    /// Arguments for the command should be passed in the handler
    /// Out Verbosity Level for the feedback string should be determined in the handler
    /// </summary>
    /// <param name="arguments">Command Arguments</param>
    /// <param name="verbosityLevel">Verbosity Level of the FeedBack string</param>
    /// <returns>Feedback String</returns>
    public delegate string gxtConsoleCommandExecutionHandler(string[] arguments, out gxtVerbosityLevel verbosityLevel);

    public class gxtConsoleCommand
    {
        private string name, description;
        public gxtConsoleCommandExecutionHandler OnCommandExecution;
        
        public string Name { get { return name; } set { name = value; } }
        public string Description { get { return description; } set { description = value; } }
    }
}
