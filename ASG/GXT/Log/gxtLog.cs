using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// Enum representing the importance of a log message
    /// Since logging is expensive, we can catergorize log requests 
    /// by their importance, and prune less important messages with a 
    /// variable verbosity level
    /// </summary>
    public enum gxtVerbosityLevel
    {
        CRITICAL = 0,
        WARNING = 1,
        SUCCESS = 2,
        INFORMATIONAL = 3
    };

    /// <summary>
    /// A singleton log manager.  Log requests to this manager will 
    /// write information to all registered gxtILogListeners.  Supports multiple levels 
    /// of verbosity and an interface similar to C#'s Console.WriteLine() function.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtLog : gxtSingleton<gxtLog>
    {
        protected List<gxtILogListener> listeners;
        protected bool enabled;
        protected gxtVerbosityLevel verbosity;

        /// <summary>
        /// Enabled?
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// The global verbosity level for all log listeners
        /// </summary>
        public gxtVerbosityLevel Verbosity { get { return verbosity; } set { verbosity = value; } }

        /// <summary>
        /// Sets up the log manager
        /// </summary>
        /// <param name="initEnabled">Enabled?</param>
        /// <param name="verbosity">Global Verbosity Level</param>
        /// <param name="logListeners">Optional list of log listeners to add imeadiately to the log system, if null, the listener list will be constructed and nothing will be added</param>
        public void Initialize(bool initEnabled = true, gxtVerbosityLevel verbosity = gxtVerbosityLevel.INFORMATIONAL, IEnumerable<gxtILogListener> logListeners = null)
        {
            Enabled = initEnabled;
            Verbosity = verbosity;
            if (logListeners == null)
                listeners = new List<gxtILogListener>();
            else
                listeners = new List<gxtILogListener>(logListeners);
        }

        /// <summary>
        /// Adds a log listener to the log manager
        /// </summary>
        /// <param name="listener">Log Listener</param>
        public void AddListener(gxtILogListener listener)
        {
            if (listeners.Contains(listener))
                WriteLineV(gxtVerbosityLevel.WARNING, "The manager already contains an instance of {0}.  It will not be added twice!", listener.ToString());
            else
                listeners.Add(listener);
        }

        /// <summary>
        /// Removes a listener from the log manager
        /// </summary>
        /// <param name="listener">Log Listener</param>
        public bool RemoveListener(gxtILogListener listener)
        {
            bool wasRemoved = listeners.Remove(listener);
            if (wasRemoved)
                listener.RemoveListener();
            else
                WriteLineV(gxtVerbosityLevel.WARNING, "The manager does not contain an instance of {0}.  It cannot be removed!", listener.ToString());
            return wasRemoved;
        }

        /// <summary>
        /// Removes every log listener from the log manager
        /// </summary>
        public void RemoveAllListeners()
        {
            for (int i = listeners.Count - 1; i >= 0; --i)
            {
                // seemingly redundant call disposes logger specific resources
                listeners[i].RemoveListener();
                listeners.RemoveAt(i);
            }
        }

        /// <summary>
        /// Updates all the log listeners
        /// Necessary for those which maintain buffers and 
        /// do not flush autmatically on every log request
        /// </summary>
        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < listeners.Count; ++i)
            {
                if (listeners[i].RemovalRequested)
                {
                    listeners.RemoveAt(i);
                    --i;
                }
                else
                {
                    listeners[i].Update(gameTime);
                }
            }
        }

        /// <summary>
        /// Unloads all associated log components
        /// </summary>
        public void Unload()
        {
            WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Unloading All GXT Log Components and Listeners!");
            RemoveAllListeners();
            listeners.Clear();
            Enabled = false;
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="format"></param>
        public static void WriteLineV(string format)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, format);
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public static void WriteLineV(string format, object arg0)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, format, arg0);
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void WriteLineV(string format, object arg0, object arg1)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, format, arg0, arg1);
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void WriteLineV(string format, object arg0, object arg1, object arg2)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLineV(string format, params object[] args)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, format, args);
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="value"></param>
        public static void WriteLineV(bool value)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, value);
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="value"></param>
        public static void WriteLineV(int value)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, value);
        }

        /// <summary>
        /// Logs a message to all log listeners using the current global verbosity
        /// </summary>
        /// <param name="value"></param>
        public static void WriteLineV(float value)
        {
            if (!gxtLog.SingletonIsInitialized) return;
            WriteLineV(gxtLog.Singleton.Verbosity, value);
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="format"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, string format)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; i++)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, format);
            }
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, string format, object arg0)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            string str = string.Format(format, arg0);
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; i++)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, str);
            }
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, string format, object arg0, object arg1)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            string str = string.Format(format, arg0, arg1);
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; i++)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, str);
            }
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, string format, object arg0, object arg1, object arg2)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            string str = string.Format(format, arg0, arg1, arg2);
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; i++)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, str);
            }
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, string format, params object[] args)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            string str = string.Format(format, args);
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; ++i)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, str);
            }
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="value"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, bool value)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            string str = value.ToString();
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; ++i)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, str);
            }
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="value"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, float value)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            string str = value.ToString();
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; i++)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, str);
            }
        }

        /// <summary>
        /// Logs the message with the given verbosity to all log listeners
        /// For a message to be printed by the log listener the log manager and the 
        /// log listener must be enabled.  The verbosity of the message must be as or more critical 
        /// than the verbosity set for the listener, or in the log manager if UseGlobalVerbosity is set to 
        /// true
        /// </summary>
        /// <param name="verbosity"></param>
        /// <param name="value"></param>
        public static void WriteLineV(gxtVerbosityLevel verbosity, int value)
        {
            if (!gxtLog.SingletonIsInitialized || !gxtLog.Singleton.Enabled) return;
            string str = value.ToString();
            for (int i = 0; i < gxtLog.Singleton.listeners.Count; i++)
            {
                gxtLog.Singleton.listeners[i].WriteLineV(verbosity, str);
            }
        }
    }
}
