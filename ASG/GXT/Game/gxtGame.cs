using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GXT
{
    /// <summary>
    /// While root handles most GXT subsystem management, a lot of functionality 
    /// in XNA is dependent on the Game class, so this is basically just a compliant 
    /// inheritent that includes gxtRoot
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtGame : Microsoft.Xna.Framework.Game
    {
        protected GraphicsDeviceManager graphics;
        private gxtRoot root;
        protected string rootConfigFile;

        /// <summary>
        /// Root subsystem manager
        /// </summary>
        public gxtRoot Root { get { return root; } }

        /// <summary>
        /// Startup configuration file for gxtRoot
        /// </summary>
        public string ConfigFile { get { return rootConfigFile; } }

        // may also want to store dictionary of command line arguments

        /// <summary>
        /// Constructor, takes the path to the config file used to initiate gxtRoot
        /// </summary>
        /// <param name="configFile">Root Config File</param>
        public gxtGame(string configFile = gxtRoot.DEFAULT_INI_FILE_PATH)
        {
            graphics = new GraphicsDeviceManager(this);
            rootConfigFile = configFile;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            if (root == null)
            {
                base.Initialize();
                root = new gxtRoot();
                root.Initialize(this, this.graphics, ConfigFile);
                // on game init
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            if (root == null)
                base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (root != null)
            {
                root.Unload();
                base.UnloadContent();
                root = null;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        protected override void Update(GameTime gameTime)
        {
            gxtDebug.Assert(gxtRoot.SingletonIsInitialized);
            root.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            root.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
