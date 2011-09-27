using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT.Processes
{
    /// <summary>
    /// Unfinished.  The idea is to have objects for each command, thereby 
    /// making it easy to extend, have a consistent way that unknown commands are handled, etc.
    /// It may be more appropriate for this to be a process, or neither.
    /// </summary>
    // TODO: get rid of excessive OOP, two dictionaries is acceptable
    // todo: make add and remove handle the bad cases appropriately
    // todo: treat help and show_commands like normal commands, should be able to type help help
    public class gxtCommandProcessor : gxtProcess
    {
        private Dictionary<string, gxtConsoleCommand> consoleCommands;
        private string helpCommandString;
        private string showAllCommandString;

        public gxtCommandProcessor(bool initEnabled, string helpCommandString = "help", string showCommandsString = "show_commands") : base(initEnabled, true, gxtProcess.INPUT_TYPE)
        {
            consoleCommands = new Dictionary<string, gxtConsoleCommand>();
            this.helpCommandString = helpCommandString;
            this.showAllCommandString = showCommandsString;
            // show all string...
        }

        public bool AddConsoleCommand(string name, string description, gxtConsoleCommandExecutionHandler commandHandler, bool logRequest = true)
        {
            gxtConsoleCommand command = new gxtConsoleCommand();
            command.Name = name;
            command.Description = description;
            command.OnCommandExecution = commandHandler;
            consoleCommands.Add(name, command);
            if (logRequest)
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Added Console Command: {0}", name);
            return true;
        }

        public bool RemoveConsoleCommand(string name, bool logRequest = true)
        {
            bool wasRemoved = consoleCommands.Remove(name);
            if (wasRemoved)
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Removed Console Command: {0}", name);
            return true;
        }

        public void Process(string command)
        {
            if (!Enabled || command == null || command == string.Empty)
                return;
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, command);
            string[] tokens = command.Split(' ');
            string[] arguments = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, arguments, 0, tokens.Length - 1); 
            if (tokens[0] == helpCommandString)
            {
                if (tokens.Length > 1)
                {
                    if (consoleCommands.ContainsKey(tokens[1]))
                    {
                        gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, tokens[1] + ": " + consoleCommands[tokens[1]].Description);
                        return;
                    }
                    else
                    {
                        gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot provide help for unknown command: {0}", tokens[1]);
                    }
                }
                else
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "You must type a command name after help!");
                }
            }
            else if (tokens[0] == showAllCommandString)
            {
                foreach (string commandName in consoleCommands.Keys)
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, commandName);
                }
            }
            else
            {
                if (consoleCommands.ContainsKey(tokens[0]))
                {
                    gxtConsoleCommand c = consoleCommands[tokens[0]];
                    if (c.OnCommandExecution != null)
                    {
                        gxtVerbosityLevel feedBackVerbosity;
                        // FIX FIX FIX
                        // the first string must be stripped from the tokens array
                        string feedBackString = c.OnCommandExecution(arguments, out feedBackVerbosity);
                        gxtLog.WriteLineV(feedBackVerbosity, feedBackString);
                    }
                }
                else
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Command not found: {0}", tokens[0]);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled)
                return;
        }

        public string PrintKnownCommands()
        {
            return "";
        }
    }
}
