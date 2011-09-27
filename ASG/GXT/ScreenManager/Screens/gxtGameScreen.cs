using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GXT.Rendering;

namespace GXT
{
    /// <summary>
    /// Enum describes the screen transition state.
    /// </summary>
    public enum gxtScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }

    /// <summary>
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class gxtGameScreen : IDisposable
    {
        private bool otherScreenHasFocus;

        protected gxtGameScreen()
        {
            ScreenState = gxtScreenState.TransitionOn;
            TransitionPosition = 1;
            TransitionOffTime = TimeSpan.Zero;
            TransitionOnTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Normally when one screen is brought up over the top of another,
        /// the first screen will transition off to make room for the new
        /// one. This property indicates whether the screen is only a small
        /// popup, in which case screens underneath it do not need to bother
        /// transitioning off.
        /// </summary>
        public bool IsPopup { get; protected set; }

        /// <summary>
        /// Indicates how long the screen takes to
        /// transition on when it is activated.
        /// </summary>
        public TimeSpan TransitionOnTime { get; protected set; }

        /// <summary>
        /// Indicates how long the screen takes to
        /// transition off when it is deactivated.
        /// </summary>
        public TimeSpan TransitionOffTime { get; protected set; }

        /// <summary>
        /// Gets the current position of the screen transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionPosition { get; protected set; }

        /// <summary>
        /// Gets the current alpha of the screen transition, ranging
        /// from 255 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public byte TransitionAlpha
        {
            get { return (byte)(255 - TransitionPosition * 255); }
        }

        /// <summary>
        /// Gets the current screen transition state.
        /// </summary>
        public gxtScreenState ScreenState { get; protected set; }

        /// <summary>
        /// There are two possible reasons why a screen might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting { get; protected set; }

        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return !otherScreenHasFocus &&
                       (ScreenState == gxtScreenState.TransitionOn ||
                        ScreenState == gxtScreenState.Active);
            }
        }

        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public gxtScreenManager ScreenManager { get; internal set; }

        #region IDisposable Members

        public virtual void Dispose() { }

        #endregion

        public virtual void Initialize() { }

        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Unload content for the screen.
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// Unlike <see cref="HandleInput"/>, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>
        public void Update(GameTime gameTime, bool otherScreenHasFocus,
                                   bool coveredByOtherScreen)
        {
            this.otherScreenHasFocus = otherScreenHasFocus;

            if (IsExiting)
            {
                // If the screen is going away to die, it should transition off.
                ScreenState = gxtScreenState.TransitionOff;

                if (!UpdateTransition(gameTime, TransitionOffTime, 1))
                {
                    // When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(this);

                    IsExiting = false;
                }
            }
            else if (coveredByOtherScreen)
            {
                // If the screen is covered by another, it should transition off.
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                {
                    // Still busy transitioning.
                    ScreenState = gxtScreenState.TransitionOff;
                }
                else
                {
                    // Transition finished!
                    ScreenState = gxtScreenState.Hidden;
                }
            }
            else
            {
                // Otherwise the screen should transition on and become active.
                if (UpdateTransition(gameTime, TransitionOnTime, -1))
                {
                    // Still busy transitioning.
                    ScreenState = gxtScreenState.TransitionOn;
                }
                else
                {
                    // Transition finished!
                    ScreenState = gxtScreenState.Active;
                }
            }

            if (!coveredByOtherScreen && !otherScreenHasFocus)
            {
                UpdateScreen(gameTime);
            }
        }

        /// <summary>
        /// Place your own update logic here
        /// This function will only be called if the screen is 
        /// in focus, not covered, and active
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        protected abstract void UpdateScreen(GameTime gameTime);

        /// <summary>
        /// Helper for updating the screen transition position.
        /// </summary>
        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            // How much should we move by?
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                          time.TotalMilliseconds);

            // Update the transition position.
            TransitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if ((TransitionPosition <= 0) || (TransitionPosition >= 1))
            {
                TransitionPosition = gxtMath.Clamp(TransitionPosition, 0, 1);
                return false;
            }
            else
            {
                ScreenManager.FadeScreen(Color.Black, TransitionAlpha);
                // Otherwise we are still busy transitioning.
                return true;
            }
        }

        /// <summary>
        /// Allows the screen to handle user input. Unlike Update, this method
        /// is only called when the screen is active, and not when some other
        /// screen has taken the focus.
        /// </summary>
        public abstract void HandleInput(GameTime gameTime);

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public abstract void Draw(gxtGraphicsBatch graphicsBatch);

        /// <summary>
        /// Tells the screen to go away. Unlike <see cref="ScreenManager"/>.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen()
        {
            if (TransitionOffTime == TimeSpan.Zero)
            {
                // If the screen has a zero transition time, remove it immediately.
                ScreenManager.RemoveScreen(this);
            }
            else
            {
                // Otherwise flag that it should transition off and then exit.
                IsExiting = true;
            }
        }
    }
}