using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GXT
{
    /// <summary>
    /// A simple wrapper around XNA's ContentManager
    /// Has Specialized calls, logs activity, handles exceptions 
    /// inside the class itself
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtResourceManager : gxtSingleton<gxtResourceManager>
    {
        private ContentManager contentManager;
        private bool logResources;

        /// <summary>
        /// If true logs information about loaded resources
        /// </summary>
        public bool LogResources { get { return logResources; } }

        /// <summary>
        /// Root directory of the manager
        /// </summary>
        public string RootDirectory { get { return contentManager.RootDirectory; } }

        public void Initialize(ContentManager contentManager, string rootDirectory = "Content", bool logResources = false)
        {
            this.contentManager = contentManager;
            this.contentManager.RootDirectory = rootDirectory;
            this.logResources = logResources;
        }

        public void Unload()
        {
            contentManager.Unload();
            contentManager.Dispose();
        }

        public virtual bool LoadTexture(string name, out Texture2D texture)
        {
            try
            {
                texture = contentManager.Load<Texture2D>(name);
                if (logResources)
                    gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Loaded Texture: \"{0}\" ({1})", name, contentManager.RootDirectory);
            }
            catch
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Failed To Load Texture: \"{0}\" ({1})", name, contentManager.RootDirectory);
                texture = null;
                return false;
            }
            return true;
        }

        public virtual Texture2D LoadTexture(string name)
        {
            try
            {
                Texture2D texture = contentManager.Load<Texture2D>(name);
                if (logResources)
                    gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Loaded Texture: \"{0}\" ({1})", name, contentManager.RootDirectory);
                return texture;
            }
            catch
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Failed To Load Texture: \"{0}\" ({1})", name, contentManager.RootDirectory);
                return null;
            }
        }

        // load spritefont methods here...

        public virtual bool Load<T>(string name, out T resource)
        {
            try
            {
                resource = contentManager.Load<T>(name);
                if (logResources)
                    gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Loaded Resource {0}: \"{1}\" ({2})", typeof(T).ToString(), name, contentManager.RootDirectory);
            }
            catch
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Failed To Load Resource {0}: \"{1}\" ({2})", contentManager.RootDirectory);
                resource = default (T);
                return false;
            }
            return true;
        }

        public virtual T Load<T>(string name)
        {
            try
            {
                T resource = contentManager.Load<T>(name);
                if (logResources)
                    gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Loaded Resource {0}: \"{1}\" ({2})", typeof(T).ToString(), name, contentManager.RootDirectory);
                return resource;
            }
            catch
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Failed To Load Resource {0}: \"{1}\" ({2})", contentManager.RootDirectory);
                return default (T);
            }
        }
    }
}
