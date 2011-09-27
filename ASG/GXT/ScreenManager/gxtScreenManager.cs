using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GXT.Rendering;

namespace GXT
{
    /// <summary>
    /// A screen stack modeled closesly after Microsoft's Developer Example Code
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtScreenManager : gxtSingleton<gxtScreenManager>
    {
        private List<gxtGameScreen> screens;
        private List<gxtGameScreen> screensToUpdate;
        
        /*
        private int screenWidth, screenHeight;
        public Vector2 ScreenCenter { get { return new Vector2(gxtRoot.Singleton.Graphics.Viewport.Width / 2f, gxtRoot.Singleton.Graphics.Viewport.Height / 2f); } }
        

        public int ScreenWidth { get { return screenWidth; } }

        public int ScreenHeight { get { return screenHeight; } }
        */

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(gxtGame game)
        {
            screens = new List<gxtGameScreen>();
            screensToUpdate = new List<gxtGameScreen>();
            
            /*
            if (gxtDisplayManager.SingletonIsInitialized)
            {
                screenWidth = gxtDisplayManager.Singleton.ResolutionWidth;
                screenHeight = gxtDisplayManager.Singleton.ResolutionHeight;
            }
            else
            {
                screenWidth = gxtDisplayManager.DEFAULT_RESOLUTION_WIDTH;
                screenHeight = gxtDisplayManager.DEFAULT_RESOLUTION_HEIGHT;
            }
            */
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        public void LoadContent()
        {
            for (int i = 0; i < screens.Count; ++i)
            {
                screens[i].LoadContent();
            }
        }

        public void Dispose()
        {
            for (int i = screens.Count - 1; i >= 0; --i)
            {
                screens[i].UnloadContent();
                screens[i].Dispose();
                screens.RemoveAt(i);
            }
            screensToUpdate.Clear();
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        public void Unload()
        {
            for (int i = screens.Count - 1; i >= 0; --i)
            {
                screens[i].UnloadContent();
            }
        }

        /// <summary>
        /// Updates each screen on the stack
        /// </summary>
        public void Update(GameTime gameTime)
        {
            screensToUpdate.Clear();

            for (int i = 0; i < screens.Count; i++)
                screensToUpdate.Add(screens[i]);

            bool otherScreenHasFocus = !gxtRoot.Singleton.Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                gxtGameScreen screen = screensToUpdate[screensToUpdate.Count - 1];
                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == gxtScreenState.TransitionOn ||
                    screen.ScreenState == gxtScreenState.Active)
                {
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime);

                        if (!screen.IsPopup)
                            otherScreenHasFocus = true;
                    }
                    
                    // inform others they are covered
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }
        }

        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        public string TraceScreens()
        {
            if (screens.Count == 0)
                return "Empty";
            string traceString = "";
            for (int i = 0; i < screens.Count; ++i)
            {
                traceString += screens[i].GetType().Name + "\n";
            }
            return traceString;
        }

        /// <summary>
        /// Tells each screen to draw itself, should probably take SpriteBatch instead
        /// </summary>
        public void Draw(gxtGraphicsBatch graphicsBatch)
        {
            for (int i = 0; i < screens.Count; ++i)
            {
                if (screens[i].ScreenState != gxtScreenState.Hidden)
                    screens[i].Draw(graphicsBatch);
            }
        }

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(gxtGameScreen screen)
        {
            gxtDebug.Assert(!screens.Contains(screen));
            screen.ScreenManager = this;
            screen.Initialize();
            screen.LoadContent();
            screens.Add(screen);
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use <see cref="GameScreen"/>.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public bool RemoveScreen(gxtGameScreen screen)
        {
            screen.UnloadContent();
            bool wasRemoved = screens.Remove(screen);
            if (wasRemoved)
            {
                screensToUpdate.Remove(screen);
                screen.Dispose();
            }
            return wasRemoved;
        }

        public void FadeScreen(Color color, int alpha)
        {
            /*
            gxtMaterial fadeMaterial = new gxtMaterial(true, new Color(color.R, color.G, color.B, alpha), 0.0f);
            gxtRectangle screenRectangle = new gxtRectangle(800, 600, fadeMaterial);
            gxtGraphicsBatch gb = gxtRoot.Singleton.GraphicsBatch;
            Matrix screenMatrix = Matrix.Identity;
            screenMatrix.M41 = 400;
            screenMatrix.M42 = 300;
            gb.Begin(gxtBatchDrawOrder.PRIMITIVES_FIRST, gxtBatchSortMode.DEFAULT, gxtBatchDepthMode.FRONT_TO_BACK, screenMatrix);
            Vector2 pos = Vector2.Zero;
            float rot = 0.0f;
            Vector2 scale = Vector2.One;
            screenRectangle.Draw(gb, ref pos, rot, ref scale, SpriteEffects.None);
            gb.End();
            */
            /*
            Viewport viewport = gxtRoot.Singleton.Graphics.Viewport;
            gxtRoot.Singleton.SpriteBatch.Begin();

            gxtRoot.Singleton.SpriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             new Color(color.R, color.G, color.B, (byte)alpha));

            gxtRoot.Singleton.SpriteBatch.End();
            */
        }
    }
}