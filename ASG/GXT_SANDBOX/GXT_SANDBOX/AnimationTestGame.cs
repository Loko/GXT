using System;
using GXT;
using GXT.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT_SANDBOX
{
    /*
    public class AnimationTestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        AnimationPlayer animationPlayer = new AnimationPlayer();

        public AnimationTestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            animationPlayer.AddAnimation("walk", Content.Load<Animation>("Animations\\guy_walk"));
            animationPlayer.StartAnimation("walk");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            animationPlayer.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            animationPlayer.Draw(spriteBatch, new Vector2(400, 200));
            base.Draw(gameTime);
        }
    }
    */
    /*
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class AnimationTestGame : gxtGame
    {
        AnimationPlayer animPlayer = new AnimationPlayer();
        Vector2 dir;

        public AnimationTestGame()
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
            animPlayer.AddAnimation("walk", Content.Load<Animation>("Animations\\guy_walk"));
            animPlayer.AddAnimation("idle", Content.Load<Animation>("Animations\\guy_default"));
            animPlayer.StartAnimation("walk");
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
            gxtGamepad pad = gxtGamepadManager.Singleton.GetGamepad(PlayerIndex.One);
            Vector2 lstick = pad.LStick();
            float t = lstick.Length();
            float updateTime;
            if (t <= 0.01f)
            {
                updateTime = 1.0f;
                if (animPlayer.CurrentAnimation == "walk")
                    animPlayer.TransitionToAnimation("idle", 0.5f);
            }
            else
            {
                animPlayer.ForceAnimationSwitch("walk");
                dir = lstick;
                updateTime = t;
            }

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Animation State: " + animPlayer.CurrentAnimation);
            animPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds * updateTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //GXT_ROOT.SpriteBatch.End();
            animPlayer.Draw(Root.SpriteBatch, new Vector2(400, 300), dir.X < 0.0f, false, 0, Color.White, new Vector2(1, 1), Matrix.Identity);
        }
    }
    */
    
}
