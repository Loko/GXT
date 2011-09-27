using System;
using Microsoft.Xna.Framework;
using GXT;
using GXT.Processes;

namespace GXT_SANDBOX
{
    public class DummyProcess : gxtProcess
    {
        public TimeSpan ElapsedTime { get; private set; }
        public TimeSpan EndTime { get; set; }
        public gxtVerbosityLevel Verbosity { get; private set; }
        private string message;

        public DummyProcess(TimeSpan duration, gxtVerbosityLevel verbosity, string msg)
            : base(true)
        {
            EndTime = duration;
            Verbosity = verbosity;
            message = msg;
        }

        public override void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                ElapsedTime += gameTime.ElapsedGameTime;
                if (ElapsedTime >= EndTime)
                    KillProcess();
                else
                {
                    gxtLog.WriteLineV(Verbosity, message);
                }
            }
        }
    }
}
