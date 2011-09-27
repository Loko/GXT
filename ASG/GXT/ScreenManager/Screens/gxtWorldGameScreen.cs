using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GXT.Rendering;
using GXT.Input;

namespace GXT
{
    public class gxtWorldGameScreen : gxtGameScreen
    {
        protected gxtWorld world;
        protected int debugDrawId;

        public bool IsSetupForDebugDrawing { get { return debugDrawId != -1; } }
        public gxtWorld World { get { return world; } }

        public gxtWorldGameScreen()
            : base()
        {
            IsPopup = true;
            debugDrawId = -1;
        }

        public virtual void Initialize(bool setupDebugDrawing = true)
        {
            base.Initialize();
            world = new gxtWorld();
            world.Initialize();

            if (setupDebugDrawing)
            {
                if (gxtDebugDrawer.SingletonIsInitialized)
                {
                    debugDrawId = gxtDebugDrawer.Singleton.GetNewId();
                    gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawId, world.SceneGraph);
                }
                else
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot Set Up Debug Drawing For The Gameplay Screen Because the Debug Drawer Has Not Been Initialized");
                }
            }
            // setup in game console
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (IsActive)
                world.HandleInput(gameTime);
        }

        protected override void UpdateScreen(GameTime gameTime)
        {
            if (IsSetupForDebugDrawing)
                gxtDebugDrawer.Singleton.CurrentSceneId = debugDrawId;
            // if (IsActive)??
            world.Update(gameTime);
            world.LateUpdate(gameTime);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            world.UnloadWorld();
        }

        public override void Draw(gxtGraphicsBatch graphicsBatch)
        {
            // shouldn't draw if covered
            world.Draw(graphicsBatch);
        }
    }
}
