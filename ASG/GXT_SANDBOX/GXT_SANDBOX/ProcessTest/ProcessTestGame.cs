using GXT;
using GXT.Processes;
using GXT.Input;
using Microsoft.Xna.Framework;
using System;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ProcessTestGame : gxtGame
    {
        gxtProcessManager processManager;

        public ProcessTestGame()
            : base()
        {
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            processManager = new gxtProcessManager();
            processManager.Initialize();

            DummyProcess proc0 = new DummyProcess(TimeSpan.FromSeconds(3.0), gxtVerbosityLevel.SUCCESS, "green message for three seconds");
            DummyProcess proc1 = new DummyProcess(TimeSpan.FromSeconds(3.0), gxtVerbosityLevel.CRITICAL, "red message for three seconds");
            DummyProcess proc2 = new DummyProcess(TimeSpan.FromSeconds(3.0), gxtVerbosityLevel.INFORMATIONAL, "white message for three seconds");

            proc0.SetNextProcess(proc1).SetNextProcess(proc2);
            processManager.Add(proc0);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //root.Initialize(this);
            //spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            processManager.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }

}
