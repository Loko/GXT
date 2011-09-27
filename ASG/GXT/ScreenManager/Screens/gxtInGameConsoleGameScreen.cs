using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GXT.Rendering;
using GXT.Input;
using GXT.Processes;

namespace GXT
{
    public class gxtInGameConsoleGameScreen : gxtGameScreen
    {
        private gxtCamera camera;
        private gxtKeyboardCharacterProcessor characterProcessor;
        private gxtCommandProcessor commandProcessor;
        private gxtInGameConsole inGameConsole;
        private gxtInGameConsoleController consoleController;
        private gxtSceneGraph sceneGraph;

        public gxtInGameConsoleGameScreen()
            : base()
        {
            IsPopup = true;
        }

        /// <summary>
        /// should take delegate for OnCommandEntered
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            camera = new gxtCamera(Vector2.Zero, 0.0f, 0.0f, true);
            sceneGraph = new gxtSceneGraph();
            sceneGraph.Initialize();
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            characterProcessor = new gxtKeyboardCharacterProcessor(true, kb);
            SpriteFont consoleFont;
            if (gxtResourceManager.Singleton.Load<SpriteFont>("Fonts\\calibri", out consoleFont))
            {
                inGameConsole = new gxtInGameConsole();
                inGameConsole.Initialize(sceneGraph, 150.0f, true, gxtVerbosityLevel.INFORMATIONAL, true, false, consoleFont, true, "console: ", 0.0f, 1.0f, 6); 
                gxtLog.Singleton.AddListener(inGameConsole);
                characterProcessor.OnCharacterEntered += inGameConsole.AppendCharacter;
                consoleController = new gxtInGameConsoleController();
                consoleController.Initialize(true, inGameConsole);

                commandProcessor = new gxtCommandProcessor(true);
                commandProcessor.AddConsoleCommand("fps", "Gets the current frames per second", FPSCommand);
                commandProcessor.AddConsoleCommand("exit", "Shuts down the entire application", ExitCommand);
                commandProcessor.AddConsoleCommand("memory", "Gets the memory usage \nin bytes or mb", MemoryUsageCommand);
                commandProcessor.AddConsoleCommand("resolution", "Sets the display resolution, width height", ResolutionCommand);
                inGameConsole.OnCommandEntered += commandProcessor.Process;
            }
        }

        public string FPSCommand(string[] args, out gxtVerbosityLevel v)
        {
            string result = gxtDebug.GetFPS().ToString();
            v = gxtVerbosityLevel.INFORMATIONAL;
            return result;
        }

        public string ExitCommand(string[] args, out gxtVerbosityLevel v)
        {
            string result = "Exiting the application...";
            v = gxtVerbosityLevel.INFORMATIONAL;
            gxtRoot.Singleton.Unload();
            return result;
        }

        public string MemoryUsageCommand(string[] args, out gxtVerbosityLevel v)
        {
            string result = gxtDebug.GetMemoryUsage().ToString();
            v = gxtVerbosityLevel.INFORMATIONAL;
            return result;
        }

        public string ResolutionCommand(string[] args, out gxtVerbosityLevel v)
        {
            if (args != null && args.Length == 2)
            {
                int width, height;
                bool parseSucceeded = int.TryParse(args[0], out width);
                if (parseSucceeded)
                {
                    parseSucceeded = int.TryParse(args[1], out height);
                    if (parseSucceeded)
                    {
                        bool requestSucceeded = gxtDisplayManager.Singleton.SetResolution(width, height, gxtDisplayManager.Singleton.FullScreen);
                        // possibly have a different request if it fails
                        v = gxtVerbosityLevel.SUCCESS;
                        return "Resolution request sent!";
                    }
                    else
                    {
                        v = gxtVerbosityLevel.WARNING;
                        return "Couldn't parse height parameter";
                    }
                }
                else
                {
                    v = gxtVerbosityLevel.WARNING;
                    return "Couldn't parse width parameter";
                }
            }
            else if (args.Length == 3)
            {
                int width, height;
                bool parseSucceeded = int.TryParse(args[0], out width);
                if (parseSucceeded)
                {
                    parseSucceeded = int.TryParse(args[1], out height);
                    if (parseSucceeded)
                    {
                        bool full;
                        bool fullValid = bool.TryParse(args[2], out full);
                        if (fullValid)
                        {
                            bool requestSucceeded = gxtDisplayManager.Singleton.SetResolution(width, height, full);

                            // possibly have a different request if it fails
                            v = gxtVerbosityLevel.SUCCESS;
                            return "Resolution request sent!";
                        }
                        else
                        {
                            v = gxtVerbosityLevel.WARNING;
                            return "FullScreen parameter invalid!";
                        }
                    }
                    else
                    {
                        v = gxtVerbosityLevel.WARNING;
                        return "Couldn't parse height parameter";
                    }
                }
                else
                {
                    v = gxtVerbosityLevel.WARNING;
                    return "Couldn't parse width parameter";
                }
            }
            else
            {
                v = gxtVerbosityLevel.WARNING;
                return "Must pass in arguments for width and height!";
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void HandleInput(GameTime gameTime)
        {
            consoleController.Update(gameTime);
        }

        protected override void UpdateScreen(GameTime gameTime)
        {
            characterProcessor.Update(gameTime);
            commandProcessor.Update(gameTime);
            sceneGraph.Update(gameTime);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            //sceneGraph.Root.RemoveAndDisposeAllChildren(true);
            if (gxtDisplayManager.SingletonIsInitialized)
                gxtDisplayManager.Singleton.resolutionChanged -= camera.ResolutionChangedHandler;
            //inGameConsole.RemoveListener();
        }

        public override void Draw(gxtGraphicsBatch graphicsBatch)
        {
            //camera.Position = Vector2.Zero;
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, camera.Position.ToString());
            graphicsBatch.Begin(gxtBatchDrawOrder.PRIMITIVES_FIRST, gxtBatchSortMode.TEXTURE, gxtBatchDepthMode.FRONT_TO_BACK, camera.GetTransform());
            sceneGraph.Draw(graphicsBatch);
            graphicsBatch.End();
        }
    }
}
