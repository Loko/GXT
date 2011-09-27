using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// Delegate to handle a change in resolution
    /// </summary>
    /// <param name="manager">Display Manager</param>
    public delegate void ResolutionChangedHandler(gxtDisplayManager manager);

    /// <summary>
    /// Handles display settings changes and needs including 
    /// resolution, Vsync, WindowTitle and so on and so forth
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtDisplayManager : gxtSingleton<gxtDisplayManager>
    {
        private GraphicsDeviceManager graphics; // GDM
        private GameWindow window;  // Window

        /// <summary>
        /// Event to be invoked on a resolution change
        /// </summary>
        public event ResolutionChangedHandler resolutionChanged;

        // constants for default widths and height
        // should vary for different platforms
        #if (WINDOWS)
        public const int DEFAULT_RESOLUTION_WIDTH = 800;
        public const int DEFAULT_RESOLUTION_HEIGHT = 600;
        #elif (XBOX)
        public const int DEFAULT_RESOLUTION_WIDTH = 1280;
        public const int DEFAULT_RESOLUTION_HEIGHT = 720;
        #endif

        /// <summary>
        /// Resolution Width
        /// </summary>
        public int ResolutionWidth { get { return graphics.PreferredBackBufferWidth; } }

        /// <summary>
        /// Resolution Height
        /// </summary>
        public int ResolutionHeight { get { return graphics.PreferredBackBufferHeight; } }

        /// <summary>
        /// Floating point screen ratio (width / height)
        /// </summary>
        public float AspectRatio { get { return (float)ResolutionWidth / ResolutionHeight; } }

        /// <summary>
        /// Target resolution width, useful for screen scaling
        /// </summary>
        public int TargetResolutionWidth { get; private set; }

        /// <summary>
        /// Target resolution height, useful for screen scaling
        /// </summary>
        public int TargetResolutionHeight { get; private set; }

        /// <summary>
        /// Fullscreen
        /// </summary>
        public bool FullScreen { get { return graphics.IsFullScreen; } }

        /// <summary>
        /// Vertical Sync
        /// </summary>
        public bool VSync { get { return graphics.SynchronizeWithVerticalRetrace; } set { graphics.SynchronizeWithVerticalRetrace = value; graphics.ApplyChanges(); } }

        /// <summary>
        /// Window Title (only supported on Windows)
        /// </summary>
        public string WindowTitle { get { return window.Title; } set { window.Title = value; } }

        /// <summary>
        /// Initializes the Display Manager with the given device and game window
        /// Width and height parameters are only the defaults which your game/art is 
        /// designed to be run at
        /// </summary>
        /// <param name="graphics">Graphics Manager</param>
        /// <param name="window">Window</param>
        /// <param name="resolutionWidth">Resolution Width</param>
        /// <param name="resolutionHeight">Resolution Height</param>
        /// <param name="targetResolutionWidth">Target Resolution Width</param>
        /// <param name="targetResolutionHeight">Target Resolution Height</param>
        /// <param name="fullscreen">Fullscreen</param>
        public void Initialize(GraphicsDeviceManager graphics, GameWindow window, int resolutionWidth = DEFAULT_RESOLUTION_WIDTH, int resolutionHeight = DEFAULT_RESOLUTION_HEIGHT, int targetResolutionWidth = DEFAULT_RESOLUTION_WIDTH, int targetResolutionHeight = DEFAULT_RESOLUTION_HEIGHT, bool fullscreen = false)
        {
            gxtDebug.Assert(resolutionWidth > 0 && resolutionHeight > 0 && targetResolutionWidth > 0 && targetResolutionHeight > 0, "Resolution values cannot be zero or negative!");

            this.graphics = graphics;
            this.window = window;
            window.AllowUserResizing = false;
            TargetResolutionWidth = targetResolutionWidth;
            TargetResolutionHeight = targetResolutionHeight;

            if (!SetResolution(resolutionWidth, resolutionHeight, fullscreen))
            {
                if (resolutionWidth != targetResolutionWidth || resolutionHeight != targetResolutionHeight)
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Attempting to set the resolution to the given desired targets...");
                    if (!SetResolution(targetResolutionWidth, targetResolutionHeight, fullscreen))
                    {
                        gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Resolution defaults also failed to make a successful request.  Attempting safe values...");
                        if (!SetResolution(DEFAULT_RESOLUTION_WIDTH, DEFAULT_RESOLUTION_HEIGHT, fullscreen))
                        {
                            gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Safe default values also failed!");
                            
                            return;
                        }
                    }
                }
                else
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Given defaults are not valid on this display!  Attempting safe values...");
                    if (!SetResolution(DEFAULT_RESOLUTION_WIDTH, DEFAULT_RESOLUTION_HEIGHT, fullscreen))
                    {
                        gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Safe default values also failed!");
                        return;
                        // assert?
                    }
                }
            }

            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Resolution Successfully Set ({0}x{1}, Fullscreen?: {2})", ResolutionWidth, ResolutionHeight, FullScreen);
        }

        /// <summary>
        /// Sets resolution, flag for fullscreen
        /// Returns true if successful, false otherwise
        /// </summary>
        /// <param name="width">Desired width</param>
        /// <param name="height">Desired height</param>
        /// <param name="fullScreen">Fullscreen</param>
        /// <param name="logErrors">If you want to log issues that resulted in the failed request</param>
        /// <returns>If successful</returns>
        public bool SetResolution(int width, int height, bool fullScreen, bool logErrors = true)
        {
            if (!fullScreen)
            {
                if (width <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                    height <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                {
                    graphics.PreferredBackBufferWidth = width;
                    graphics.PreferredBackBufferHeight = height;
                    graphics.IsFullScreen = false;
                    graphics.ApplyChanges();
                    if (resolutionChanged != null)
                    {
                        resolutionChanged(this);
                    }
                    return true;
                }
            }
            else
            {
                if (SupportsFullScreenResolution(width, height))
                {
                    graphics.PreferredBackBufferWidth = width;
                    graphics.PreferredBackBufferHeight = height;
                    graphics.IsFullScreen = true;
                    graphics.ApplyChanges();
                    if (resolutionChanged != null)
                    {
                        resolutionChanged(this);
                    }
                    return true;
                }
                else if (logErrors)
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "FullScreen Resolution is Not Supported on this Display! ({0}x{1})", width, height);
                }
            }
            if (logErrors)
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Resolution Change Request Failed ({0}x{1}, Fullscreen?: {2})", width, height, fullScreen);
            return false;
        }

        /// <summary>
        /// Determines if current display supports a given resolution at fullscreen
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>If supported</returns>
        public bool SupportsFullScreenResolution(int width, int height)
        {
            foreach (DisplayMode dispMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (dispMode.Width == width && dispMode.Height == height)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] TraceSupportedFullScreenResolutions()
        {
            int size = 0;
            IEnumerator<DisplayMode> modes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.GetEnumerator();
            while (modes.MoveNext())
            {
                ++size;
            }
            modes.Reset();

            string[] displayModes = new string[size];
            for (int i = 0; i < size; ++i)
            {
                modes.MoveNext();
                displayModes[i] = modes.Current.Width + " x " + modes.Current.Height;
            }
            return displayModes;
        }

        
        /// <summary>
        /// Registers a camera, for screen scaling on a resolution change
        /// </summary>
        /// <param name="camera">Camera</param>
        public void RegisterCamera(gxtICamera camera)
        {
            resolutionChanged += new ResolutionChangedHandler(camera.ResolutionChangedHandler);
        }

        /*
        public bool IsRegistered(ResolutionChangedHandler resolutionChangedHandler)
        {
            Delegate[] methods = resolutionChanged.GetInvocationList();
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i] == resolutionChangedHandler)
                    return true;
            }
            return false;
        }
        */
        
        /// <summary>
        /// Removes camera as a listener
        /// </summary>
        /// <param name="camera"></param>
        public void UnRegisterCamera(gxtICamera camera)
        {
            resolutionChanged -= camera.ResolutionChangedHandler;
        }

        /// <summary>
        /// Registers all the cameras in an array
        /// </summary>
        /// <param name="cameras">Cameras</param>
        public void RegisterCamera(gxtICamera[] cameras)
        {
            foreach (gxtICamera camera in cameras)
                resolutionChanged += new ResolutionChangedHandler(camera.ResolutionChangedHandler);
        }

        /// <summary>
        /// Info readout
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Resolution: {0} x {1}" +
                                 "\nDefault Resolution: {2} x {3}" +
                                 "\nFullScreen Enabled: {4}" +
                                 "\nVSync Enabled: {5}" +
                                 "\nWindow Title: {6}", ResolutionWidth, ResolutionHeight, TargetResolutionWidth, TargetResolutionHeight,
                                                        FullScreen, VSync, WindowTitle);
        }
    }
}
