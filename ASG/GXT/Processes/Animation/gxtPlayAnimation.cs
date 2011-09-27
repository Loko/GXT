using System;
using Microsoft.Xna.Framework;
using GXT.Animation;

namespace GXT.Processes
{
    public class gxtAnimationPlayerProcess : gxtProcess
    {
        private gxtAnimation animClip;

        public gxtAnimationPlayerProcess(gxtAnimation animClip)
            : base(true, true, gxtProcess.ANIMATION_TYPE)
        {
            this.animClip = animClip;
        }

        public override void InitializeProcess()
        {
            animClip.Reset();
            ProcessIsInitialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled)
                return;
            if (!ProcessIsInitialized)
                InitializeProcess();
            if (animClip.IsDone)
                KillProcess();
        }
    }
}
