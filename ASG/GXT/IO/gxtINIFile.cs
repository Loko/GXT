#define GXT_INI_FILE_SEMICOLON_COMMENTS
//#define GXT_INI_FILE_HASH_COMMENTS
//#undef GXT_INI_FILE_SEMICOLON_COMMENTS
//#undef GXT_INI_FILE_HASH_COMMENTS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// ; Example ini file below
// [section]
// name=value
// name2=value2
//
// [section2]
// someid=somevalue
// 
// [Physics]
// Enabled=true
// FrictionType=average
// Gravity=100
//
// ; semicolons delimit comments (unless INI_FILE_HASH_COMMENTS is defined instead)
// please note that repeat identifiers in the same section will cause an abort
// please avoid whitespace, especially between equals signs
// it is reccomended you place everything into a section, although the default is allowed
namespace GXT.IO
{
    /// <summary>
    /// A class designed for reading and writing ini config files
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: STATIC METHODS FOR READING/WRITING DIRECTLY TO A DOUBLE DICTIONARY
    // METHODS FOR PARSING THINGS LIKE VECTORS/COLORS/FLOATS/INTS/ETC??
    public class gxtINIFile
    {
        public const string DEFAULT_SECTION_NAME = "";
        
        #if (GXT_INI_FILE_SEMICOLON_COMMENTS)
        public const char COMMENT_DELIMETER = ';';
        #elif (GXT_INI_FILE_HASH_COMMENTS)
        public const char COMMENT_DELIMETER = '#';
        #else
        #error No Comment Delimeter Is Defined for gxtINIFile
        #endif

        /// <summary>
        /// Internal, represents component of 
        /// the ini file being read
        /// </summary>
        private enum gxtINIReadState
        {
            UNKNOWN = 0,
            BEGIN_SECTION = 1,
            END_SECTION = 2,
            BEGIN_NAME = 3,
            BEGIN_VALUE = 4
        };
        
        /// <summary>
        /// Double dictionary of entries
        /// First key is the section name
        /// Second key is the identifier name
        /// Example: entries[section][name] = value
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> entries;
        // streamWriter
        // streamReader
        // stringBuilder?


        /// <summary>
        /// Initializes instance of ini file reader/writer
        /// </summary>
        public void Initialize()
        {
            gxtDebug.Assert(!IsInitialized());
            entries = new Dictionary<string, Dictionary<string, string>>();
        }

        /// <summary>
        /// Determines if this instance has already been initialized
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return entries != null;
        }

        /// <summary>
        /// Clears all entries
        /// </summary>
        public void Clear()
        {
            gxtDebug.Assert(IsInitialized());
            entries.Clear();
        }

        /// <summary>
        /// Gets string representation of the value for a given name in a given section
        /// May need to be casted outside this function to a different type to be usable
        /// </summary>
        /// <param name="section"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetValue(string section, string name, out string value)
        {
            gxtDebug.Assert(IsInitialized());
            if (entries.ContainsKey(section))
            {
                if (entries[section].ContainsKey(name))
                {
                    value = entries[section][name];
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="section"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetValue(Dictionary<string, Dictionary<string, string>> entries, string section, string name, out string value)
        {
            gxtDebug.Assert(entries != null);
            if (entries.ContainsKey(section))
            {
                if (entries[section].ContainsKey(name))
                {
                    value = entries[section][name];
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Sets the value of variable with the given name and section
        /// The section must already exist, but if the property name does not
        /// it will be added.  Otherwise, it will merely have its value changed.
        /// </summary>
        /// <param name="section">Section name</param>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value</param>
        public void SetValue(string section, string name, string value)
        {
            gxtDebug.Assert(IsInitialized());
            //gxtDebug.Assert(entries.Keys.Contains<string>(section), "The Section [" + section + "] Does Not Exist in the INI File");
            // make a new section if needed
            if (!entries.ContainsKey(section))
                entries.Add(section, new Dictionary<string,string>());
            if (!entries[section].Keys.Contains<string>(name))
            {
                entries[section].Add(name, value);
            }
            else
            {
                entries[section][name] = value;
                //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Changing the value of {0} in section {1} to {2}", name, value, section);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="section"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetValue(Dictionary<string, Dictionary<string, string>> entries, string section, string name, string value)
        {
            gxtDebug.Assert(entries != null);

            if (!entries.ContainsKey(section))
                entries.Add(section, new Dictionary<string, string>());
            if (!entries[section].Keys.Contains<string>(name))
                entries[section].Add(name, value);
            else
                entries[section][name] = value;
        }

        /// <summary>
        /// Adds a new section to the ini file
        /// </summary>
        /// <param name="section"></param>
        public void AddSection(string section)
        {
            gxtDebug.Assert(IsInitialized());
            if (!entries.ContainsKey(section))
                entries.Add(section, new Dictionary<string, string>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="section"></param>
        public static void AddSection(Dictionary<string, Dictionary<string, string>> entries, string section)
        {
            gxtDebug.Assert(entries != null);
            if (!entries.ContainsKey(section))
                entries.Add(section, new Dictionary<string, string>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public bool RemoveSection(string section)
        {
            gxtDebug.Assert(IsInitialized());
            //if (section != DEFAULT_SECTION_NAME)
            return entries.Remove(section);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static bool RemoveSection(Dictionary<string, Dictionary<string, string>> entries, string section)
        {
            gxtDebug.Assert(entries != null);
            return entries.Remove(section);
        }

        /// <summary>
        /// Reads the ini file with the given path
        /// Adds sections, names, and value pairs appropriately 
        /// inside this class.  If the path does not end in ".ini" it 
        /// will be added for you
        /// </summary>
        /// <param name="path">Path to ini file</param>
        public bool Read(string path, bool logErrors = true)
        {
            gxtDebug.Assert(IsInitialized());
            if (string.IsNullOrEmpty(path))
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot read from null or empty ini string path");
                return false;
            }

            // modify path if extension isn't given, INI in all caps is allowed
            string readPath = path;
            if (!readPath.EndsWith(".ini", StringComparison.CurrentCultureIgnoreCase))
                readPath += ".ini";

            StreamReader reader;
            string line;

            // handle possible errors that may occur when accessing the file
            try
            {
                reader = new StreamReader(readPath);
            }
            catch (FileNotFoundException)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The file: \"{0}\" was not found!");
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The directory wasn't found!  Full Path: \"{0}\"");
                return false;
            }
            catch (IOException ioe)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "An IO Exception occurred while reading the file: \"{0}\"\nMessage: {1}", readPath, ioe.Message);
                return false;
            }

            // init with an unknown state
            // states help the loop guess which sections come next and determine when they are invalid
            gxtINIReadState readState = gxtINIReadState.UNKNOWN;
            
            // one buffer for all file reading
            StringBuilder buffer = new StringBuilder();

            // temporary holders
            string currentSection = DEFAULT_SECTION_NAME;
            string currentValue = String.Empty;
            string currentName = String.Empty;

            AddSection(DEFAULT_SECTION_NAME);

            // read every line in the file, one at a time
            while ((line = reader.ReadLine()) != null)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    for (int i = 0; i < line.Length; ++i)
                    {
                        // stop line reading on comment
                        if (line[i] == COMMENT_DELIMETER)
                            break;
                        // continue on whitespace
                        if (line[i] == ' ')
                            continue;
                        // set to begin section state on open bracket
                        if (line[i] == '[')
                        {
                            //gxtDebug.Assert(readState == gxtINIReadState.UNKNOWN);
                            if (readState != gxtINIReadState.UNKNOWN)
                            {
                                if (logErrors)
                                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Begin Section Token (\'[\') found in an unexpected place!  Full Line: {0}", line);
                                reader.Dispose();
                                return false;
                            }
                            readState = gxtINIReadState.BEGIN_SECTION;
                            continue;
                        }
                        // set to end section state on close bracket
                        if (line[i] == ']')
                        {
                            //gxtDebug.Assert(readState == gxtINIReadState.BEGIN_SECTION);
                            if (readState != gxtINIReadState.BEGIN_SECTION)
                            {
                                if (logErrors)
                                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "End Section Token (\']\') found in an unexpected place!  Full Line: {0}", line);
                                reader.Dispose();
                                return false;
                            }
                            readState = gxtINIReadState.END_SECTION;
                            currentSection = buffer.ToString();
                            continue;
                        }
                        // set to begin value on equals sign
                        if (line[i] == '=')
                        {
                            //gxtDebug.Assert(readState == gxtINIReadState.BEGIN_NAME);
                            if (readState != gxtINIReadState.BEGIN_NAME)
                            {
                                if (logErrors)
                                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Assignment Operator (\'=\') found in an unexpected place!  Full Line: {0}", line);
                                reader.Dispose();
                                return false;
                            }
                            readState = gxtINIReadState.BEGIN_VALUE;
                            currentName = buffer.ToString();
                            buffer.Clear();
                            continue;
                        }
                        // begin name otherwise
                        if (readState == gxtINIReadState.UNKNOWN)
                        {
                            readState = gxtINIReadState.BEGIN_NAME;
                            buffer.Append(line[i]);
                        }
                        // append to buffer for current section
                        else
                        {
                            buffer.Append(line[i]);
                        }
                    }
                    if (readState == gxtINIReadState.END_SECTION)
                    {
                        currentSection = buffer.ToString();
                        AddSection(currentSection);
                    }
                    else if (readState == gxtINIReadState.BEGIN_VALUE)
                    {
                        currentValue = buffer.ToString();
                        SetValue(currentSection, currentName, currentValue);
                    }
                    // reset for a new line
                    buffer.Clear();
                    readState = gxtINIReadState.UNKNOWN;
                    currentName = String.Empty;
                    currentValue = String.Empty;
                }
            }

            reader.Dispose();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="path"></param>
        /// <param name="logErrors"></param>
        /// <returns></returns>
        public static bool Read(Dictionary<string, Dictionary<string, string>> entries, string path, bool logErrors = true)
        {
            gxtDebug.Assert(entries != null);
            if (string.IsNullOrEmpty(path))
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot read from null or empty ini string path");
                return false;
            }

            // modify path if extension isn't given, INI in all caps is allowed
            string readPath = path;
            if (!readPath.EndsWith(".ini", StringComparison.CurrentCultureIgnoreCase))
                readPath += ".ini";

            StreamReader reader;
            string line;

            // handle possible errors that may occur when accessing the file
            try
            {
                reader = new StreamReader(readPath);
            }
            catch (FileNotFoundException)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The file: \"{0}\" was not found!");
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The directory wasn't found!  Full Path: \"{0}\"");
                return false;
            }
            catch (IOException ioe)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "An IO Exception occurred while reading the file: \"{0}\"\nMessage: {1}", readPath, ioe.Message);
                return false;
            }

            // init with an unknown state
            // states help the loop guess which sections come next and determine when they are invalid
            gxtINIReadState readState = gxtINIReadState.UNKNOWN;

            // one buffer for all file reading
            StringBuilder buffer = new StringBuilder();

            // temporary holders
            string currentSection = DEFAULT_SECTION_NAME;
            string currentValue = String.Empty;
            string currentName = String.Empty;

            AddSection(entries, DEFAULT_SECTION_NAME);

            // read every line in the file, one at a time
            while ((line = reader.ReadLine()) != null)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    for (int i = 0; i < line.Length; ++i)
                    {
                        // stop line reading on comment
                        if (line[i] == COMMENT_DELIMETER)
                            break;
                        // continue on whitespace
                        if (line[i] == ' ')
                            continue;
                        // set to begin section state on open bracket
                        if (line[i] == '[')
                        {
                            //gxtDebug.Assert(readState == gxtINIReadState.UNKNOWN);
                            if (readState != gxtINIReadState.UNKNOWN)
                            {
                                if (logErrors)
                                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Begin Section Token (\'[\') found in an unexpected place!  Full Line: {0}", line);
                                reader.Dispose();
                                return false;
                            }
                            readState = gxtINIReadState.BEGIN_SECTION;
                            continue;
                        }
                        // set to end section state on close bracket
                        if (line[i] == ']')
                        {
                            //gxtDebug.Assert(readState == gxtINIReadState.BEGIN_SECTION);
                            if (readState != gxtINIReadState.BEGIN_SECTION)
                            {
                                if (logErrors)
                                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "End Section Token (\']\') found in an unexpected place!  Full Line: {0}", line);
                                reader.Dispose();
                                return false;
                            }
                            readState = gxtINIReadState.END_SECTION;
                            currentSection = buffer.ToString();
                            continue;
                        }
                        // set to begin value on equals sign
                        if (line[i] == '=')
                        {
                            //gxtDebug.Assert(readState == gxtINIReadState.BEGIN_NAME);
                            if (readState != gxtINIReadState.BEGIN_NAME)
                            {
                                if (logErrors)
                                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Assignment Operator (\'=\') found in an unexpected place!  Full Line: {0}", line);
                                reader.Dispose();
                                return false;
                            }
                            readState = gxtINIReadState.BEGIN_VALUE;
                            currentName = buffer.ToString();
                            buffer.Clear();
                            continue;
                        }
                        // begin name otherwise
                        if (readState == gxtINIReadState.UNKNOWN)
                        {
                            readState = gxtINIReadState.BEGIN_NAME;
                            buffer.Append(line[i]);
                        }
                        // append to buffer for current section
                        else
                        {
                            buffer.Append(line[i]);
                        }
                    }
                    if (readState == gxtINIReadState.END_SECTION)
                    {
                        currentSection = buffer.ToString();
                        AddSection(entries, currentSection);
                    }
                    else if (readState == gxtINIReadState.BEGIN_VALUE)
                    {
                        currentValue = buffer.ToString();
                        SetValue(entries, currentSection, currentName, currentValue);
                    }
                    // reset for a new line
                    buffer.Clear();
                    readState = gxtINIReadState.UNKNOWN;
                    currentName = String.Empty;
                    currentValue = String.Empty;
                }
            }

            reader.Dispose();
            return true;
        }

        /// <summary>
        /// A work in progress write out method that outputs existing 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="header"></param>
        /// <param name="logErrors"></param>
        public bool Write(string path, string header = "INI File Created By GXT", bool logErrors = true)
        {
            gxtDebug.Assert(IsInitialized());
            if (string.IsNullOrEmpty(path))
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot write to a null or empty ini string path");
                return false;
            }

            string writePath = path;
            if (!writePath.EndsWith(".ini", StringComparison.CurrentCultureIgnoreCase))
                writePath += ".ini";

            StreamWriter writer;

            // handle errors upon accessing the write path
            // since the stream writer throws exeptions, we gotta catch 
            // them internally for a return code interface
            try
            {
                writer = new StreamWriter(writePath);
            }
            catch (DirectoryNotFoundException)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Directory was not found when attempting to write to the path: {0}", writePath);
                return false;
            }
            catch (IOException ioe)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "An IO Exception occurred while writing the file: \"{0}\"\nMessage: {1}", writePath, ioe.Message);
                return false;
            }

            // may need to split header on \n to ensure it is all commented out
            writer.WriteLine(COMMENT_DELIMETER + header);

            // write out every section and name value pair 
            foreach (string section in entries.Keys)
            {
                if (section != DEFAULT_SECTION_NAME)
                    writer.WriteLine("[" + section + "]");
                foreach (string name in entries[section].Keys)
                {
                    writer.WriteLine(name + "=" + entries[section][name].ToString());
                }
                writer.WriteLine();
            }

            writer.Close();
            writer.Dispose();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="path"></param>
        /// <param name="header"></param>
        /// <param name="logErrors"></param>
        /// <returns></returns>
        public static bool Write(Dictionary<string, Dictionary<string, string>> entries, string path, string header = "INI File Created By GXT", bool logErrors = true)
        {
            gxtDebug.Assert(entries != null);
            if (string.IsNullOrEmpty(path))
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot write to a null or empty ini string path");
                return false;
            }

            string writePath = path;
            if (!writePath.EndsWith(".ini", StringComparison.CurrentCultureIgnoreCase))
                writePath += ".ini";

            StreamWriter writer;

            // handle errors upon accessing the write path
            // since the stream writer throws exeptions, we gotta catch 
            // them internally for a return code interface
            try
            {
                writer = new StreamWriter(writePath);
            }
            catch (DirectoryNotFoundException)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Directory was not found when attempting to write to the path: {0}", writePath);
                return false;
            }
            catch (IOException ioe)
            {
                if (logErrors)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "An IO Exception occurred while writing the file: \"{0}\"\nMessage: {1}", writePath, ioe.Message);
                return false;
            }

            // may need to split header on \n to ensure it is all commented out
            writer.WriteLine(COMMENT_DELIMETER + header);

            // write out every section and name value pair 
            foreach (string section in entries.Keys)
            {
                if (section != DEFAULT_SECTION_NAME)
                    writer.WriteLine("[" + section + "]");
                foreach (string name in entries[section].Keys)
                {
                    writer.WriteLine(name + "=" + entries[section][name].ToString());
                }
                writer.WriteLine();
            }

            writer.Close();
            writer.Dispose();
            return true;
        }

        /// <summary>
        /// Gets a string representation of the ini file contents
        /// </summary>
        /// <returns>Trace string</returns>
        public string DebugTrace()
        {
            gxtDebug.Assert(IsInitialized());
            StringBuilder builder = new StringBuilder();

            foreach (string section in entries.Keys)
            {
                builder.AppendLine("[" + section + "]");
                foreach (string name in entries[section].Keys)
                {
                    builder.AppendLine(name + "=" + entries[section][name].ToString());
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static string DebugTrace(Dictionary<string, Dictionary<string, string>> entries)
        {
            gxtDebug.Assert(entries != null);
            StringBuilder builder = new StringBuilder();

            foreach (string section in entries.Keys)
            {
                builder.AppendLine("[" + section + "]");
                foreach (string name in entries[section].Keys)
                {
                    builder.AppendLine(name + "=" + entries[section][name].ToString());
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
