namespace GXT
{
    /// <summary>
    /// A class which restricts an object to only one instance and 
    /// provides a global point of access.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    public class gxtSingleton<T> where T : class, new()
    {
        private static T singleton;
        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static T Singleton { get { gxtDebug.Assert(singleton != null, "gxtSingleton not yet instantiated"); return singleton; } }

        /// <summary>
        /// Determines if the Singleton Has Been Initialized
        /// </summary>
        public static bool SingletonIsInitialized { get { return singleton != null; } }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        protected gxtSingleton()
        {
            gxtDebug.Assert(singleton == null, string.Format("gxtSingleton of type {0} already instantiated", typeof(T)));
            singleton = this as T;
            gxtDebug.Assert(singleton != null, string.Format("gxtSingleton of type {0} failed to be instantiated", typeof(T)), "Casting to the singleton type failed");
        }
    }
}
