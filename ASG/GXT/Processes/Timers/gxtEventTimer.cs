using System;
using Microsoft.Xna.Framework;

namespace GXT.Processes
{
    /// <summary>
    /// Delegate for elapsed time cycles event
    /// </summary>
    /// <param name="timer">Timer</param>
    public delegate void gxtEventTimerElapsedHandler(gxtEventTimer timer);

    /// <summary>
    /// An event timer for XNA, meaning it uses GameTime.  Supports multiple cycles, 
    /// infinite looping, start/stop/reset routines, and events upon an elapsed cycle.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtEventTimer : gxtProcess
    {
        private TimeSpan elapsedTime;
        /// <summary>
        /// Currently elapsed time
        /// </summary>
        public TimeSpan ElapsedTime { get { return elapsedTime; } }

        private TimeSpan totalTime;
        /// <summary>
        /// Total duration
        /// </summary>
        public TimeSpan TotalTime { get { return totalTime; } }

        private int elapsedCycles;
        /// <summary>
        /// Elapsed cycles
        /// </summary>
        public int ElapsedCycles { get { return elapsedCycles; } }
        
        private int totalCycles;
        /// <summary>
        /// Total cycles
        /// </summary>
        public int TotalCycles { get { return totalCycles; } }

        /// <summary>
        /// Event invoked on a finished cycle
        /// </summary>
        public event gxtEventTimerElapsedHandler elapsedEvent;

        /// <summary>
        /// Makes a timer that elapse after a given duration
        /// If total cycles is set to zero, the timer will run 
        /// infinitely, firing the elapsed event appropriately, until it
        /// is manually killed
        /// </summary>
        public gxtEventTimer(TimeSpan duration, int totalCycles = 1, bool initEnabled = true) : base(initEnabled, true, gxtProcess.TIMER_TYPE)
        {
            totalTime = duration;
            this.totalCycles = totalCycles;
            elapsedCycles = 0;
            elapsedTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Update logic.  Must be called in update cycle for the timer
        /// to run properly.
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public override void Update(GameTime gameTime)
        {
            // If paused and/or finished, just return
            if (!Enabled || elapsedCycles >= totalCycles) return;

            // Increment elapsed time
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime >= totalTime)
            {
                // 0 means it runs infinitely until it is manually stopped/disposed
                if (totalCycles != 0)
                    elapsedCycles++;

                // wrap time for the next cycle
                if (elapsedCycles < totalCycles)
                    elapsedTime -= totalTime;
                else
                    KillProcess();

                // If the event handler isn't null, invoke it
                if (elapsedEvent != null)
                    elapsedEvent(this);
            }
        }

        /// <summary>
        /// Starts/Resumes the timer
        /// </summary>
        public void Resume()
        {
            Enabled = true;
        }

        /// <summary>
        /// Pauses/Stops the timer
        /// </summary>
        public void Pause()
        {
            Enabled = false;
        }

        /// <summary>
        /// Resets the timer.  Stops it and resets timer and cycles.
        /// </summary>
        public void Reset()
        {
            elapsedTime = TimeSpan.Zero;
            elapsedCycles = 0;
            Enabled = false;
        }

        /// <summary>
        /// Gives basic readout of the timer properties
        /// </summary>
        /// <returns></returns>
        public string DebugTrace()
        {
            return "Elapsed Time: " + elapsedTime + " / Total Time: " + totalTime +
                   "\nElapsed Cycles: " + elapsedCycles + " / Total Cycles: " + totalCycles +
                   "\nEnabled?: " + Enabled;
        }
    }
}
