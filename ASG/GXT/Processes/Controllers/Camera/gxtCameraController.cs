using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using GXT.Rendering;

namespace GXT.Processes
{
    public abstract class gxtCameraController : gxtIController
    {
        private bool enabled;
        private gxtCamera camera;
        
        public bool Enabled { get { return enabled; } set { enabled = value; } }
        
        public gxtCamera Camera { get { return camera; } set { camera = value; } }

        public virtual bool QueriesInput { get { return true; } }

        public gxtCameraController(gxtCamera targetCamera)
        {
            enabled = true;
            camera = targetCamera;
        }

        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Since camera controllers are usually dependent on other variables, 
        /// you will likely want to do most, if not all, of the behavoir in LateUpdate() 
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void LateUpdate(GameTime gameTime)
        {

        }

        public Type GetTargetType()
        {
            return camera.GetType();
        }
    }
}
