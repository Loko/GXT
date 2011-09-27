using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GXT.Input;
using GXT.Rendering;

namespace GXT
{
    /// <summary>
    /// Delegate designed to be invoked when a command is entered
    /// </summary>
    /// <param name="str">Command String</param>
    public delegate void gxtCommandEntered(string str);

    /// <summary>
    /// An in game console capable of broadcasting commands to other sources and showing log 
    /// messages in game.  Extremely useful for testing/development and changing variables while 
    /// the game is running.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: needs a queue of written log messages rather than an array
    // TODO: be able to scroll thru old log messages
    // TODO: vertical, horizontal padding variables
    // TODO: this implementation currently assumes a camera with a centered identity view matrix
    // TODO: currently cannot handle log messages that include line breaks
    public class gxtInGameConsole : gxtILogListener
    {
        // log listener vars
        protected bool enabled;
        protected bool removalRequested;
        protected bool useGlobalVerbosity;
        protected gxtVerbosityLevel verbosityLevel;
        protected bool useTimeStamps;

        // size
        // currently consoleWidth always equals resolutionWidth
        float consoleWidth, consoleHeight, resolutionWidth, resolutionHeight;
        float horizontalPadding, verticalPadding, verticalTextSpacing;

        // materials for each log color and the background
        protected gxtIMaterial informationalMaterial, successMaterial, warningMaterial, criticalMaterial, backgroundMaterial, inputMaterial;

        // scene nodes and text
        protected SpriteFont consoleSpriteFont;
        protected gxtISceneNode consoleNode;
        protected gxtISceneNode consoleTextNode;
        protected gxtRectangle backgroundRectangle;
        protected gxtTextField consoleTextField;

        // current text string
        protected string consolePrefix;
        protected string text;
        protected bool isOpen;

        /// <summary>
        /// Event handler invoked when a console command is entered
        /// </summary>
        public gxtCommandEntered OnCommandEntered;

        protected TimeSpan elapsedTime;
        protected gxtISceneNode[] logBufferNodes;
        protected gxtTextField[] logBufferEntries;
        protected int logBufferSize;
        protected int logWriteIndex;

        /// <summary>
        /// Enabled?
        /// </summary>
        public bool Enabled 
        { 
            get { return enabled; } 
            set 
            { 
                enabled = value; 
                if (!enabled)
                    Close(); 
            }
        }

        /// <summary>
        /// If the console is open and visible
        /// </summary>
        public bool IsOpen { get { return isOpen; } }

        /// <summary>
        /// Tagged For Removal From gxtLog
        /// </summary>
        public bool RemovalRequested { get { return removalRequested; } }

        /// <summary>
        /// If this logger will use the global 
        /// verbosity set in gxtLog
        /// </summary>
        public bool UseGlobalVerbosity { get { return useGlobalVerbosity; } set { useGlobalVerbosity = value; } }

        /// <summary>
        /// The verbosity of this logger
        /// </summary>
        public gxtVerbosityLevel Verbosity { get { return verbosityLevel; } set { verbosityLevel = value; } }

        /// <summary>
        /// Actual verbosity level, either from the global verbosity 
        /// or defined here if UseGlobalVerbosity = false
        /// </summary>
        public gxtVerbosityLevel ActiveVerbosityLevel
        {
            get
            {
                if (UseGlobalVerbosity)
                    return gxtLog.Singleton.Verbosity;
                else
                    return Verbosity;
            }
        }

        /// <summary>
        /// Timestamps before each write?
        /// </summary>
        public bool UseTimeStamps { get { return useTimeStamps; } set { useTimeStamps = value; } }

        /// <summary>
        /// Informational Log Color
        /// </summary>
        public Color InformationalColor { get { return informationalMaterial.ColorOverlay; } set { informationalMaterial.ColorOverlay = value; } }

        /// <summary>
        /// Success Log Color
        /// </summary>
        public Color SuccessColor { get { return successMaterial.ColorOverlay; } set { successMaterial.ColorOverlay = value; } }

        /// <summary>
        /// Warning Log Color
        /// </summary>
        public Color WarningColor { get { return warningMaterial.ColorOverlay; } set { warningMaterial.ColorOverlay = value; } }

        /// <summary>
        /// Critical Log Color
        /// </summary>
        public Color CriticalColor { get { return criticalMaterial.ColorOverlay; } set { criticalMaterial.ColorOverlay = value; } }

        /// <summary>
        /// Background Log Color
        /// </summary>
        public Color BackgroundColor { get { return backgroundMaterial.ColorOverlay; } set { backgroundMaterial.ColorOverlay = value; } }

        /// <summary>
        /// Command Input Color
        /// </summary>
        public Color InputColor { get { return inputMaterial.ColorOverlay; } set { inputMaterial.ColorOverlay = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtInGameConsole() { }

        /// <summary>
        /// Note: consider making a struct that packages all these settings, or one or two structs
        /// At this rate, the number of variables for this function is growing to the point that it 
        /// deteroriates readability
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="consoleHeight"></param>
        /// <param name="initEnabled"></param>
        /// <param name="verbosity"></param>
        /// <param name="useGlobalVerbosity"></param>
        /// <param name="useTimeStamps"></param>
        /// <param name="consoleFont"></param>
        /// <param name="initOpen"></param>
        /// <param name="consolePrefix"></param>
        /// <param name="consoleTextRenderDepth"></param>
        /// <param name="consoleBackgroundRenderDepth"></param>
        /// <param name="logBufferSize"></param>
        public void Initialize(gxtSceneGraph scene, float consoleHeight, bool initEnabled = true, gxtVerbosityLevel verbosity = gxtVerbosityLevel.INFORMATIONAL, bool useGlobalVerbosity = true, bool useTimeStamps = false, SpriteFont consoleFont = null, 
            bool initOpen = true, string consolePrefix = "console: ", float consoleTextRenderDepth = 0.0f, float consoleBackgroundRenderDepth = 1.0f, int logBufferSize = 6)
        {
            gxtDebug.Assert(logBufferSize >= 0);
            gxtDebug.Assert(gxtDisplayManager.SingletonIsInitialized);

            this.enabled = initEnabled;
            this.useGlobalVerbosity = useGlobalVerbosity;
            this.verbosityLevel = verbosity;
            this.useTimeStamps = useTimeStamps;
            this.isOpen = initOpen;
            this.text = string.Empty;

            // all these colors and depths should be taken as parameters
            this.consoleSpriteFont = consoleFont;
            this.informationalMaterial = new gxtMaterial(isOpen, Color.White, consoleTextRenderDepth);
            this.successMaterial = new gxtMaterial(isOpen, Color.Green, consoleTextRenderDepth);
            this.warningMaterial = new gxtMaterial(isOpen, Color.Yellow, consoleTextRenderDepth);
            this.criticalMaterial = new gxtMaterial(isOpen, Color.Red, consoleTextRenderDepth);
            this.inputMaterial = new gxtMaterial(isOpen, Color.Black, consoleTextRenderDepth);
            this.backgroundMaterial = new gxtMaterial(isOpen, new Color(255, 255, 255, 65), consoleBackgroundRenderDepth);

            this.resolutionWidth = gxtDisplayManager.Singleton.ResolutionWidth;
            this.resolutionHeight = gxtDisplayManager.Singleton.ResolutionHeight;
            this.consoleWidth = resolutionWidth;
            this.consoleHeight = gxtMath.Clamp(consoleHeight, 0.0f, resolutionHeight);
            gxtDisplayManager.Singleton.resolutionChanged += OnResolutionChange;
            // these should be taken as parameters
            this.horizontalPadding = 15.0f;
            this.verticalPadding = 0.0f;
            this.verticalTextSpacing = 5.0f;

            // background rectangle and container node
            backgroundRectangle = new gxtRectangle(consoleWidth, consoleHeight, backgroundMaterial);
            consoleNode = new gxtSceneNode();
            consoleNode.Position = new Vector2(0.0f, (-resolutionHeight * 0.5f) + (consoleHeight * 0.5f));
            consoleNode.AttachDrawable(backgroundRectangle);

            // textnode
            this.consolePrefix = consolePrefix;
            consoleTextField = new gxtTextField(consoleFont, consolePrefix + "_", inputMaterial);
            consoleTextNode = new gxtSceneNode();
            consoleTextNode.AttachDrawable(consoleTextField);

            consoleNode.AddChild(consoleTextNode);

            this.logBufferSize = logBufferSize;
            this.logWriteIndex = 0;

            logBufferNodes = new gxtSceneNode[logBufferSize];
            logBufferEntries = new gxtTextField[logBufferSize];

            // consider using a fixed size queue instead
            gxtISceneNode topAnchor = new gxtSceneNode();
            topAnchor.Position = new Vector2((consoleWidth * 0.5f) - horizontalPadding, (-consoleHeight * 0.5f) + verticalPadding);
            logBufferNodes[0] = topAnchor;
            logBufferEntries[0] = new gxtTextField(consoleFont);
            logBufferEntries[0].Material = new gxtMaterial();
            logBufferNodes[0].AttachDrawable(logBufferEntries[0]);

            gxtISceneNode current = topAnchor;
            for (int i = 1; i < logBufferSize; ++i)
            {
                gxtISceneNode node = new gxtSceneNode();
                logBufferNodes[i] = node;
                current.AddChild(node);
                node.Position = new Vector2(0.0f, verticalTextSpacing);
                current = node;

                gxtTextField tf = new gxtTextField(consoleFont);
                logBufferEntries[i] = tf;
                tf.Material = new gxtMaterial();
                current.AttachDrawable(tf);
            }

            consoleNode.AddChild(topAnchor);
            scene.AddNode(consoleNode);

            AdjustConsoleTextPos();
        }

        /*
        public void Initialize(gxtSceneGraph scene, bool initEnabled, gxtVerbosityLevel verbosity, bool useGlobalVerbosity, bool useTimeStamps, string consolePrefix, Color backgroundColor, 
            Color inputColor, Color informationalColor, Color successColor, Color warningColor, Color criticalColor, )
        {

        }
        */

        /// <summary>
        /// Appends a character to the current command
        /// </summary>
        /// <param name="c">Character</param>
        public void AppendCharacter(char c)
        {
            if (enabled && isOpen)
            {
                text += c;
                consoleTextField.Text = consolePrefix + text + "_";
                AdjustConsoleTextPos();
            }
        }

        /// <summary>
        /// Deletes the last character in the current command
        /// </summary>
        public void DeleteLastCharacter()
        {
            if (enabled && isOpen)
            {
                if (text.Length > 0)
                {
                    text = text.Remove(text.Length - 1, 1);
                    consoleTextField.Text = consolePrefix + text + "_";
                    AdjustConsoleTextPos();
                }
            }
        }

        /// <summary>
        /// Clears the current command
        /// </summary>
        public void ClearCurrentCommand()
        {
            if (enabled && isOpen && text != string.Empty)
            {
                text = string.Empty;
                consoleTextField.Text = consolePrefix + "_";
                AdjustConsoleTextPos();
            }
        }

        /// <summary>
        /// Send the current command thru the OnCommandEntered delegate
        /// Also clears the current command from the console buffer 
        /// </summary>
        public void ExecuteCurrentCommand()
        {
            if (enabled && isOpen && text != String.Empty)
            {
                if (OnCommandEntered != null)
                {
                    OnCommandEntered(text);
                }
                ClearCurrentCommand();
            }
        }

        /// <summary>
        /// Opens the console window (if enabled)
        /// </summary>
        /// <returns>If open after the request</returns>
        public bool Open()
        {
            if (enabled)
            {
                if (!isOpen)
                {
                    consoleNode.SetVisibility(true, true);
                    isOpen = true;
                }
            }
            return isOpen;
        }

        /// <summary>
        /// Toggles open/closed state of the console window (if enabled)
        /// </summary>
        /// <returns>If the toggle request was successful</returns>
        public bool ToggleOpen()
        {
            if (enabled)
            {
                if (isOpen)
                {
                    consoleNode.SetVisibility(false, true);
                    isOpen = false;
                }
                else
                {
                    consoleNode.SetVisibility(true, true);
                    isOpen = true;
                }
            }
            return enabled;
        }

        /// <summary>
        /// Closes the console window
        /// </summary>
        /// <returns>If closed after the request</returns>
        public bool Close()
        {
            // if not enabled should it even be open?
            if (isOpen)
            {
                consoleNode.SetVisibility(false, true);
                isOpen = false;
            }
            return !isOpen;
        }

        /// <summary>
        /// Writeline, with verbosiy arguments
        /// </summary>
        /// <param name="verbosity">Verbosity</param>
        /// <param name="format">Formatted string</param>
        public void WriteLineV(gxtVerbosityLevel verbosity, string format)
        {
            if (!enabled) return;
            gxtVerbosityLevel activeVerbosity = ActiveVerbosityLevel;
            if (verbosity > activeVerbosity) return;

            string[] entries = format.Split('\n');
            int entry = 0;

            while (entry < entries.Length)
            {
                int idx = logWriteIndex;
                if (logWriteIndex == logBufferSize)
                {
                    idx = logBufferSize - 1;
                    int i;
                    for (i = 0; i < logBufferSize - 1; ++i)
                    {
                        logBufferNodes[i].DetachDrawable(logBufferEntries[i]);
                        logBufferEntries[i].Text = logBufferEntries[i + 1].Text;
                        logBufferEntries[i].Material = logBufferEntries[i + 1].Material;
                    }
                    logBufferNodes[idx].DetachDrawable(logBufferEntries[idx]);
                    for (i = 0; i < logBufferSize - 1; ++i)
                    {
                        logBufferNodes[i].AttachDrawable(logBufferEntries[i]);
                    }
                }

                gxtIMaterial logMaterial = GetLogMaterial(verbosity);
                logBufferEntries[idx].Material = logMaterial;

                if (UseTimeStamps)
                {
                    logBufferNodes[idx].DetachAllDrawables();
                    logBufferEntries[idx].Text = DateTime.Now.ToString("hh:mm:ss.fff tt :") + format;
                    logBufferNodes[idx].AttachDrawable(logBufferEntries[idx]);
                }
                else
                {
                    logBufferNodes[idx].DetachAllDrawables();
                    logBufferEntries[idx].Text = format;
                    logBufferNodes[idx].AttachDrawable(logBufferEntries[idx]);
                }
                logWriteIndex = gxtMath.IMin(logWriteIndex + 1, logBufferSize);
                ++entry;
            }
            AdjustConsoleTextPos();
        }

        /// <summary>
        /// Tells gamelog class it should remove this listener
        /// </summary>
        public void RemoveListener()
        {
            // set enabled to false?  make invisible?
            if (consoleNode != null)
            {
                gxtDebug.Assert(consoleNode.Parent != null);
                consoleNode.Parent.RemoveAndDisposeAllChildren(true);
                removalRequested = true;
            }
        }

        /// <summary>
        /// Updates the in game console
        /// Very important method since the in game console currently lacks a controller
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Update(GameTime gameTime)
        {
            if (!enabled || !isOpen)
                return;
            // some version of adjust console position should probably be called here instead
            // that way it's called once, and not every time a log entry is made

            // update a stopwatch that blinks the insertion cursor on and off
        }

        /// <summary>
        /// Internally readjusts the positions of text scene nodes so they are properly aligned 
        /// given the size of strings and padding values
        /// </summary>
        private void AdjustConsoleTextPos()
        {
            if (enabled)
            {
                Vector2 strSize = consoleTextField.SpriteFont.MeasureString(consolePrefix + text + "_");
                // horrible practice, get rid of the hard coded values
                consoleTextNode.Position = new Vector2((strSize.X * 0.5f) - (resolutionWidth * 0.5f) + horizontalPadding, (consoleHeight * 0.5f) - (strSize.Y * 0.5f) - verticalPadding);

                
                Vector2 entrySize = logBufferEntries[0].GetStringMeasure() * 0.5f;
                logBufferNodes[0].Position = new Vector2(-entrySize.X + (resolutionWidth * 0.5f) - horizontalPadding,  (-consoleHeight * 0.5f) + (strSize.Y * 0.5f) + verticalPadding);
                for (int i = 1; i < logBufferSize; ++i)
                {
                    entrySize = logBufferEntries[i].GetStringMeasure() * 0.5f;
                    Vector2 parentPos = logBufferNodes[i - 1].GetDerivedPosition();
                    Vector2 dPos = new Vector2(-entrySize.X + (consoleWidth * 0.5f) - horizontalPadding, parentPos.Y + strSize.Y * 0.5f + verticalTextSpacing);
                    logBufferNodes[i].SetDerivedPosition(dPos);
                }
            }
        }

        /// <summary>
        /// Simple function that gets the console color based on the verbosity level
        /// Keep updated if number of verbosity levels grow
        /// </summary>
        /// <param name="v">Verbosity</param>
        /// <returns>Matching Console Color</returns>
        /*
        private Color GetLogColor(gxtVerbosityLevel v)
        {
            if (v == gxtVerbosityLevel.CRITICAL)
                return CriticalColor;
            else if (v == gxtVerbosityLevel.WARNING)
                return WarningColor;
            else if (v == gxtVerbosityLevel.SUCCESS)
                return SuccessColor;
            else
                return InformationalColor;
        }
        */

        /// <summary>
        /// Retrieves appropriate material for the given verbosity level
        /// </summary>
        /// <param name="v">Verbosity Level</param>
        /// <returns>Log Material</returns>
        private gxtIMaterial GetLogMaterial(gxtVerbosityLevel v)
        {
            if (v == gxtVerbosityLevel.CRITICAL)
                return criticalMaterial;
            else if (v == gxtVerbosityLevel.WARNING)
                return warningMaterial;
            else if (v == gxtVerbosityLevel.SUCCESS)
                return successMaterial;
            else
                return informationalMaterial;
        }

        /// <summary>
        /// Adjusts internal variables appropriately in the event of 
        /// a change in display resolution
        /// </summary>
        /// <param name="displayManager"></param>
        public void OnResolutionChange(gxtDisplayManager displayManager)
        {
            if (displayManager.ResolutionWidth != resolutionWidth)
            {
                resolutionWidth = displayManager.ResolutionWidth;
                resolutionHeight = displayManager.ResolutionHeight;
                consoleWidth = displayManager.ResolutionWidth;
                consoleHeight = gxtMath.Min(consoleHeight, resolutionHeight);
                consoleNode.Position = new Vector2(0.0f, (-resolutionHeight * 0.5f) + (consoleHeight * 0.5f));
            }
        }
    }
}
