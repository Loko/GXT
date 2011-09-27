using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GXT;
using GXT.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.Input;

namespace GXTA
{
    public class gxtaEditorGameScreen : gxtGameScreen 
    {
        public gxtaEditorGameScreen() : base()
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            /*
            gxtDisplayManager.Singleton.WindowTitle = "GXT Animation Editor";
            input = new InputManager(gxtRoot.Singleton.Game.Services, gxtRoot.Singleton.Game.Window.Handle);
            gui = new GuiManager(gxtRoot.Singleton.Game.Services);

            gxtRoot.Singleton.Game.Components.Add(input);
            gxtRoot.Singleton.Game.Components.Add(gui);

            gxtRoot.Singleton.Game.IsMouseVisible = true;
            */

        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void HandleInput(GameTime gameTime)
        {

        }

        protected override void UpdateScreen(GameTime gameTime)
        {

        }

        public override void Draw(gxtGraphicsBatch graphicsBatch)
        {

        }
    }
}
