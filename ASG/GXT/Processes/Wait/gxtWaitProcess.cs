using System;
using Microsoft.Xna.Framework;

namespace GXT.Processes
{
    /// <summary>
    /// A frequently needed, simple, wait process
    /// Chain this to another process to add a timed delay
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtWaitProcess : gxtProcess
    {
        private TimeSpan elapsedTime, endTime;

        /// <summary>
        /// Elapsed Time
        /// </summary>
        public TimeSpan ElapsedTime { get { return elapsedTime; } }
        
        /// <summary>
        /// Full duration of the process
        /// </summary>
        public TimeSpan EndTime { get { return endTime; } set { endTime = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="duration">Duration of the wait</param>
        public gxtWaitProcess(TimeSpan duration)
            : base(true, true, gxtProcess.WAIT_TYPE)
        {
            endTime = duration;
        }

        /// <summary>
        /// Updates wait process
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public override void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                elapsedTime += gameTime.ElapsedGameTime;
                if (elapsedTime >= endTime)
                    KillProcess();
            }
        }
    }
}
