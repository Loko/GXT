#define RAY_CAST_TEST
//#undef RAY_CAST_TEST
using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Physics;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KeyboardListenerTestGame : gxtGame
    {
        Vector2 position = Vector2.Zero;
        gxtAABB aabb = new gxtAABB(Vector2.Zero, new Vector2(100, 100));
        float speed = 10.0f;
        gxtWorldGameScreen worldGameScreen;
        gxtInGameConsoleGameScreen consoleGameScreen;

        public KeyboardListenerTestGame()
            : base()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            worldGameScreen = new gxtWorldGameScreen();
            worldGameScreen.Initialize(true);
            consoleGameScreen = new gxtInGameConsoleGameScreen();
            //consoleGameScreen.Initialize();

            //Root.ScreenManager.AddScreen(consoleGameScreen);
            Root.ScreenManager.AddScreen(worldGameScreen);
            Root.ScreenManager.AddScreen(consoleGameScreen);
        }

        protected override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            float tx = 0.0f, ty = 0.0f;
            if (kb.IsDown(Keys.A))
                tx = -speed;
            if (kb.IsDown(Keys.D))
                tx += speed;
            if (kb.IsDown(Keys.W))
                ty = -speed;
            if (kb.IsDown(Keys.S))
                ty += speed;

            position += new Vector2(tx, ty);
            //aabb = new gxtAABB(aabb.Position + new Vector2(tx, ty), aabb.Extents);
            //gxtDebugDrawer.Singleton.AddPt(position, Color.Red, 0.5f, TimeSpan.FromSeconds(0.35f));
            //gxtDebugDrawer.Singleton.AddAABB(aabb.Min, aabb.Max, Color.Red);

        }

        /*
        //gxtKeyboardListener kbListener;
        gxtWorld world;
        gxtInGameConsole inGameConsole;
        gxtKeyboardListener kbListener;

        public KeyboardListenerTestGame()
            : base()
        {
        }

        public void OnCommandEnter(string str)
        {
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, str);
            if (str == "quit")
                this.Exit();
            else if (str == "fps")
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "FPS: " + gxtDebug.GetFPS().ToString());
            else if (str == "memory")
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Memory Usage: " + (gxtDebug.GetMemoryUsage() / 1024).ToString());
            else if (str == "informational")
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Some random informational message");
            else if (str == "success")
                gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Some random success message");
            else if (str == "warning")
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Some random warning message");
            else if (str == "critical")
                gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Some random critical message");
            else
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Command not recognized: " + str);
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
            IsMouseVisible = true;
            gxtDisplayManager.Singleton.WindowTitle = "In Game Console Test";

            world = new gxtWorld();
            world.Initialize();
            //world.Load();

            //kbListener = new gxtKeyboardListener();
            //kbListener.Initialize();

            inGameConsole = new gxtInGameConsole();
            SpriteFont consoleFont;
            if (!gxtResourceManager.Singleton.Load<SpriteFont>("Fonts\\calibri", out consoleFont))
                gxtDebug.Assert(false, "This demo assumes the spritefont for the in game console loads successfully");
            inGameConsole.Initialize(world.SceneGraph, 150, true, gxtVerbosityLevel.INFORMATIONAL, true, false, consoleFont, true, "console: ", 0.0f, 1.0f, 6);

            gxtKeyboard keyboard = gxtKeyboardManager.Singleton.GetKeyboard();
            //keyboard.OnCharacterEntered += inGameConsole.AppendCharacter;
            kbListener = new gxtKeyboardListener(true, keyboard);
            kbListener.OnCharacterEntered += inGameConsole.AppendCharacter;

            inGameConsole.OnCommandEntered += OnCommandEnter;

            world.AddProcess(kbListener);
            gxtLog.Singleton.AddListener(inGameConsole);
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
            world.Update(gameTime);
            world.LateUpdate(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "cam pos: {0}", camera.Position);
            base.Draw(gameTime);
            world.Draw(gxtRoot.Singleton.GraphicsBatch);
        }
        */
    }
}
