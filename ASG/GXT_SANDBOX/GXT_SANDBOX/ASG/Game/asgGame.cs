using System;
using System.Collections.Generic;
using GXT;

namespace ASG
{
    public class asgGame : gxtGame
    {
        asgWorldGameScreen worldGameScreen;

        public asgGame(string configFile = gxtRoot.DEFAULT_INI_FILE_PATH) : base(configFile)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            worldGameScreen = new asgWorldGameScreen();
            worldGameScreen.Initialize(true);
            gxtScreenManager.Singleton.AddScreen(worldGameScreen);
        }
    }
}
