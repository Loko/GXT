using System;
using Microsoft.Xna.Framework;

namespace GXT.Processes
{
    /// <summary>
    /// Simple "stopwatch" timer that uses GameTime.  Usable
    /// for the common task of seeing how much time has elapsed.
    /// With no "end" cutoff time this process must be killed manually.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtStopWatch : gxtProcess
    {
        private TimeSpan elapsedTime;

        /// <summary>
        /// Elapsed TimeSpan
        /// </summary>
        public TimeSpan ElapsedTime { get { return elapsedTime; } set { elapsedTime = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtStopWatch(bool initEnabled = true) : base(initEnabled, true, gxtProcess.TIMER_TYPE)
        {
            elapsedTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Increments elapsed time if the elapsed timer is active
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public override void Update(GameTime gameTime)
        {
            if (Enabled)
                elapsedTime += gameTime.ElapsedGameTime;
        }

        /// <summary>
        /// Resumes the timer
        /// </summary>
        public void Resume()
        {
            Enabled = true;
        }

        /// <summary>
        /// Pauses the timer
        /// </summary>
        public void Pause()
        {
            Enabled = false;
        }

        /// <summary>
        /// Resets the timer back to zero.  Active status
        /// dependant on passed in value.
        /// </summary>
        /// <param name="initActive">Active on reset?</param>
        public void Reset(bool initEnabled = false)
        {
            Enabled = initEnabled;
            elapsedTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Elapsed time in seconds
        /// </summary>
        /// <returns></returns>
        public float GetElapsedSeconds()
        {
            return (float)ElapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Elapsed time in milliseconds
        /// </summary>
        /// <returns></returns>
        public float GetElapsedMilliSeconds()
        {
            return (float)ElapsedTime.TotalMilliseconds;
        }
    }
}
