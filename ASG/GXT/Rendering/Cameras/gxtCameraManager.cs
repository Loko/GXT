using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public enum gxtSplitScreenMode
    {
        NO_SPLIT = 0,
        TWO_HORIZONTAL = 1,
        THREE_HORIZONTAL = 2,
        FOUR_HORIZONTAL = 3
    };

    public class gxtScreenManager
    {
        private Viewport viewports;
        private Matrix viewMatrices;

        public int NumCameras { get { return 1; } }

        public void OnResolutionChanged(gxtDisplayManager displayManager)
        {

        }

        public void GetCameraViewportPairs(Matrix[] viewMatrices, Viewport[] viewports)
        {

        }
    }
}
