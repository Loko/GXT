using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Physics;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class AudioTestGame : gxtGame
    {
        int debugDrawerId;
        gxtSceneGraph sceneGraph;
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        public AudioTestGame()
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
            Root.DisplayManager.WindowTitle = "Audio Test";
            sceneGraph = new gxtSceneGraph();
            Content.RootDirectory = "Content";
            audioEngine = new AudioEngine("Content\\Audio\\gxt_test_sounds.xgs");
            soundBank = new SoundBank(audioEngine, "Content\\Audio\\gxt_test_sound_bank.xsb");
            waveBank = new WaveBank(audioEngine, "Content\\Audio\\gxt_test_wave_bank.xwb");

            audioEngine.Update();

            Cue cue = soundBank.GetCue("park_1");
            cue.Play();
            if (gxtDebugDrawer.SingletonIsInitialized)
            {
                debugDrawerId = gxtDebugDrawer.Singleton.GetNewId();
                gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawerId, sceneGraph);
                gxtDebugDrawer.Singleton.CurrentSceneId = debugDrawerId;
                //gxtDebugDrawer.Singleton.SetTargetDrawManager(world.DrawManager);
                gxtDebugDrawer.Singleton.DebugFont = Content.Load<SpriteFont>("Fonts\\debug_font");
                //gxtDebugDrawer.Singleton.SetDebugFont(Root);
            }
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

            audioEngine.Update();
            //audioEngine.SetGlobalVariable(
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "{0}", float.Epsilon);

            string fpsString = "FPS: " + gxtDebug.GetFPS().ToString();
            Vector2 strSize = gxtDebugDrawer.Singleton.DebugFont.MeasureString(fpsString);
            strSize *= 0.5f;
            Vector2 topLeftCorner = new Vector2(-gxtDisplayManager.Singleton.ResolutionWidth * 0.5f, -gxtDisplayManager.Singleton.ResolutionHeight * 0.5f);
            gxtDebugDrawer.Singleton.AddString("FPS: " + gxtDebug.GetFPS(), topLeftCorner + strSize, Color.White, 0.0f);


            //gxtDebugDrawer.Singleton.AddString("");
            //xna reach profile has no alpha transparency!
            //world.Physics.DebugDrawGeoms(Color.Yellow, 0.5f, new Color(0.0f, 0.0f, 1.0f, 0.25f), 0.51f, true);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            gxtRoot.Singleton.SpriteBatch.Begin((new gxtCamera()).GetTransform());
            sceneGraph.Draw(gxtRoot.Singleton.GraphicsBatch);
            //drawManager.Draw(ref spriteBatch, camera.GetViewAABB());
            gxtRoot.Singleton.SpriteBatch.End();
        }
    }
}
