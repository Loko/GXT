using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using GXT.IO;
using GXT.Input;
using GXT.Rendering;
using GXT.Audio;

namespace GXT
{
    /// <summary>
    /// The general state of the engine
    /// </summary>
    public enum gxtEngineState
    {
        INIT = 0,
        UPDATE = 1,
        DRAW = 2
    };

    /// <summary>
    /// Root manager for all "low level" GXT subsystems
    /// Inspired by OgreRoot.  Manages the initialization and updating 
    /// of required components for the rest of GXT.  Includes logging, 
    /// input, screen managers, and much, much more.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtRoot : gxtSingleton<gxtRoot>
    {
        // version/game
        public const float VERSION_MAJOR = 0.0f;
        public const float VERSION_MINOR = 0.25f;
        public const string VERSION_NAME = "Enigma";
        private string version;
        private gxtGame game;
        private gxtEngineState engineState;

        // graphics
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch xnaSpriteBatch;
        private gxtSpriteBatch spriteBatch;
        private gxtGraphicsBatch graphicsBatch;
        private gxtDisplayManager displayManager;
        private gxtPrimitiveManager primitiveManager;
        private gxtDebugDrawer debugDrawer;
        private gxtScreenManager screenManager;
        private Color clearColor;

        // audio
        private gxtAudioManager audioManager;

        // log/resources
        private gxtLog log;
        private gxtResourceManager resourceManager;
        
        // input
        private gxtGamepadManager gamepadManager;
        private gxtKeyboardManager keyboardManager;
        private gxtMouseManager mouseManager;

        /// <summary>
        /// If a custom file isn't specified, root will try to load 
        /// the default ini file path
        /// </summary>
        public const string DEFAULT_INI_FILE_PATH = "gxt_default.ini";

        /// <summary>
        /// Concatenated string version information
        /// </summary>
        public string Version { get { return version; } }

        /// <summary>
        /// Game
        /// </summary>
        public gxtGame Game { get { return game; } }

        /// <summary>
        /// Engine State
        /// </summary>
        public gxtEngineState EngineState { get { return engineState; } }

        /// <summary>
        /// Graphics Device Manager
        /// </summary>
        public GraphicsDeviceManager GraphicsManager { get { return graphicsDeviceManager; } }
        
        /// <summary>
        /// Graphics Device
        /// </summary>
        public GraphicsDevice Graphics { get { return graphicsDeviceManager.GraphicsDevice; } }

        /// <summary>
        /// XNA's spritebatch
        /// </summary>
        public SpriteBatch XNASpriteBatch { get { return xnaSpriteBatch; } }

        /// <summary>
        /// GXT Specific SpriteBatch
        /// </summary>
        public gxtSpriteBatch SpriteBatch { get { return spriteBatch; } }

        /// <summary>
        /// GXT Specific GraphicsBatch
        /// </summary>
        public gxtGraphicsBatch GraphicsBatch { get { return graphicsBatch; } }

        /// <summary>
        /// Back buffer clear color
        /// </summary>
        public Color ClearColor { get { return clearColor; } set { clearColor = value; } }

        /// <summary>
        /// Display Manager
        /// </summary>
        public gxtDisplayManager DisplayManager { get { return displayManager; } }

        /// <summary>
        /// Primitive Manager
        /// </summary>
        public gxtPrimitiveManager PrimitiveManager { get { return primitiveManager; } }

        /// <summary>
        /// Resource Manager
        /// </summary>
        public gxtResourceManager ResourceManager { get { return resourceManager; } }

        /// <summary>
        /// Log Manager
        /// </summary>
        public gxtLog Log { get { return log; } }

        /// <summary>
        /// Gamepad Manager
        /// </summary>
        public gxtGamepadManager GamepadManager { get { return gamepadManager; } }

        /// <summary>
        /// Keyboard Manager
        /// </summary>
        public gxtKeyboardManager KeyboardManager { get { return keyboardManager; } }
        
        /// <summary>
        /// Mouse Manager
        /// </summary>
        public gxtMouseManager MouseManager { get { return mouseManager; } }

        /// <summary>
        /// Debug Drawer
        /// </summary>
        public gxtDebugDrawer DebugDrawer { get { return debugDrawer; } }

        /// <summary>
        /// Screen Manager
        /// </summary>
        public gxtScreenManager ScreenManager { get { return screenManager; } }

        /// <summary>
        /// Audio Manager
        /// </summary>
        public gxtAudioManager AudioManager { get { return audioManager; } }

        /// <summary>
        /// Initializes main logging system
        /// </summary>
        private void InitializeLog()
        {
            if (!gxtLog.SingletonIsInitialized)
            {
                log = new gxtLog();
                log.Initialize();
            }
        }

        /// <summary>
        /// Initializes startup log listeners
        /// </summary>
        private void InitializeLogListeners()
        {
            if (gxtLog.SingletonIsInitialized)
            {
                gxtConsoleLogListener consoleListener = new gxtConsoleLogListener();
                consoleListener.Initialize(true, gxtVerbosityLevel.INFORMATIONAL, true, false);
                log.AddListener(consoleListener);

                gxtHTMLLogListener htmlListener = new gxtHTMLLogListener();
                htmlListener.Initialize(true, gxtVerbosityLevel.INFORMATIONAL, true, false);
                log.AddListener(htmlListener);

                // would preferably do it this way...
                // take the previous useTimeStamps value for each log listener
                // set it to false temporarily
                // write out the header below
                // then set them back

                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "---------------------");
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, version);
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "---------------------");

                htmlListener.UseTimeStamps = true;
            }
        }

        /// <summary>
        /// Initializes trig tables and other cached values in gxtMath
        /// </summary>
        private void InitializeMath()
        {
            gxtMath.Initialize();
            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Math System Successfully Initialized...");
        }

        /// <summary>
        /// Sets up the string hashing function used for string ids
        /// </summary>
        private void InitializeHashedStrings()
        {
            gxtHashedString.HashFunction = gxtHashedString.JenkinsStringHashFunction;
        }

        /// <summary>
        /// Sets up custom resource manager
        /// </summary>
        private void InitializeResourceManager()
        {
            if (!gxtResourceManager.SingletonIsInitialized)
            {
                resourceManager = new gxtResourceManager();
                resourceManager.Initialize(game.Content);
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Resource Manager Sucessfully Initialized...");
            }
        }

        /// <summary>
        /// Initializes all three major input managers
        /// </summary>
        private void InitializeInput()
        {
            if (!gxtGamepadManager.SingletonIsInitialized)
            {
                gamepadManager = new gxtGamepadManager();
                gamepadManager.Initialize();
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Gamepad Manager Successfully Initialized...");
            }
            if (!gxtKeyboardManager.SingletonIsInitialized)
            {
                keyboardManager = new gxtKeyboardManager();
                keyboardManager.Initialize();
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Keyboard Manager Successfully Initialized...");
            }
            if (!gxtMouseManager.SingletonIsInitialized)
            {
                mouseManager = new gxtMouseManager();
                mouseManager.Initialize();
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Mouse Manager Successfully Initialized...");
            }
        }

        /// <summary>
        /// Initializes graphics and display properties
        /// </summary>
        private void InitializeDisplay()
        {
            if (!gxtDisplayManager.SingletonIsInitialized)
            {
                displayManager = new gxtDisplayManager();
                displayManager.Initialize(GraphicsManager, game.Window);
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Display Manager Successfully Initialized...");
            }
            if (spriteBatch == null)
            {
                xnaSpriteBatch = new SpriteBatch(Graphics);
                spriteBatch = new gxtSpriteBatch();
                spriteBatch.Initialize(xnaSpriteBatch, Graphics);
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "SpriteBatch Successfully Initialized...");
            }
            if (graphicsBatch == null)
            {
                graphicsBatch = new gxtGraphicsBatch();
                graphicsBatch.Initialize(Graphics, xnaSpriteBatch, true);
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Graphics Batch Successfully Initialized...");
            }
            clearColor = Color.Black; // make it chooseable
        }

        /// <summary>
        /// Initializes centralized primitve manager
        /// </summary>
        private void InitializePrimitives()
        {
            if (!gxtPrimitiveManager.SingletonIsInitialized)
            {
                primitiveManager = new gxtPrimitiveManager();
                primitiveManager.Initialize(Graphics);
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Primitive Manager Successfully Initialized...");
            }
        }

        /// <summary>
        /// Initializes debug components
        /// </summary>
        private void InitializeDebug()
        {
            gxtDebug.Initialize(true);
        }

        /// <summary>
        /// Initializes screen manager stack
        /// </summary>
        private void InitializeScreenManager()
        {
            if (!gxtScreenManager.SingletonIsInitialized)
            {
                screenManager = new gxtScreenManager();
                screenManager.Initialize(game);
            }
        }

        /// <summary>
        /// Initializes debug drawing queue
        /// </summary>
        private void InitializeDebugDrawer()
        {
            if (!gxtDebugDrawer.SingletonIsInitialized)
            {
                debugDrawer = new gxtDebugDrawer();
                debugDrawer.Initialize(true, true);
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Debug Drawer Successfully Initialized...");
            }
        }

        /// <summary>
        /// Initializes audio subsystems
        /// </summary>
        private void InitializeAudioManager()
        {
            if (!gxtAudioManager.SingletonIsInitialized)
            {
                audioManager = new gxtAudioManager();
                // TEMPORARY, A PROPER SOLUTION READS THIS FROM THE INI FILE
                //audioManager.Initialize("Content\\Audio\\gxt_test_sounds.xgs");
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Audio Manager Successfully Initialized...");
            }
        }
        
        /// <summary>
        /// Initializes all of GXT's root subsystems
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="graphics">Graphics Device Manager</param>
        /// <param name="iniFile">Configuration File</param>
        public void Initialize(gxtGame game, GraphicsDeviceManager graphics, string iniFile = DEFAULT_INI_FILE_PATH)
        {
            this.engineState = gxtEngineState.INIT;
            this.game = game;
            this.graphicsDeviceManager = graphics;

            version = "GXT: " + string.Format("{0:0.#}", VERSION_MAJOR) + string.Format("{0:#.00}", VERSION_MINOR) + " (" + VERSION_NAME + ")";
            
            // intialize log system first so we can view entries for failed startups
            InitializeLog();
            InitializeLogListeners();
            
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Initializing {0}", version);
            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Log Subsystems Successfully Initialized...");

            InitializeDebug();          // assertions, fps calculations
            InitializeMath();           // trig tables
            InitializeHashedStrings();  // hash function for special strings
            InitializeResourceManager();// content manager wrapper
            InitializeDisplay();        // display properties
            InitializePrimitives();     // primitive textures
            InitializeInput();          // keyboard, gamepad, mouse input
            InitializeDebugDrawer();    // procedural debug drawer
            //InitializeAudioManager(); // audio subsystems
            InitializeScreenManager();  // screen stack manager
        }

        /// <summary>
        /// Updates all major subsystem components
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Update(GameTime gameTime)
        {
            this.engineState = gxtEngineState.UPDATE;
            log.Update(gameTime);
            gamepadManager.Update();
            keyboardManager.Update();
            mouseManager.Update();
            debugDrawer.Update(gameTime);
            //audioManager.Update(gameTime);
            screenManager.Update(gameTime);
            gxtDebug.Update(gameTime);
        }

        /// <summary>
        /// Disposes of every susbsystem inside of root
        /// </summary>
        public void Unload()
        {
            // these methods should probably return booleans indicating the success/failure of their disposal
            gamepadManager.Unload();
            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Successfully Unloaded GamePad Manager");
            
            mouseManager.Unload();
            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Successfully Unloaded Mouse Manager");

            keyboardManager.Unload();
            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Successfully Unloaded Keyboard Manager");

            screenManager.Unload();
            screenManager.Dispose();
            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Successfully Unloaded Screen Manager");

            primitiveManager.Unload();
            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Successfully Unloaded Primitive Manager");

            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Successfully Unloaded gxtRoot");
            log.Unload();
            game.Exit();
        }

        /// <summary>
        /// Draws all screens added to the screen manager
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Draw(GameTime gameTime)
        {
            this.engineState = gxtEngineState.DRAW;
            Graphics.Clear(clearColor);
            screenManager.Draw(graphicsBatch);
        }
    }
}
