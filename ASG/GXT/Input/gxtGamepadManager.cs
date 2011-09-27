using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GXT.Input
{
    /// <summary>
    /// Delegate for controller connection change states
    /// </summary>
    /// <param name="playerIndex">Index of controller</param>
    /// <param name="connected">Bool indicated if new state is connected/disconnected</param>
    public delegate void gxtControllerConnectionChangedHandler(PlayerIndex playerIndex, bool connected);

    /// <summary>
    /// Singleton manager for all xbox 360 gamepads
    /// Updates all gamepads and fires events when a 
    /// controller's connection state chnages
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtGamepadManager : gxtSingleton<gxtGamepadManager>
    {
        private Dictionary<PlayerIndex, gxtGamepad> gamepads;
        private List<gxtGamepad> connectedGamepads;

        /// <summary>
        /// Invoked when a controller's connection state changes
        /// Will be fired on the first update if a controller is connected
        /// </summary>
        public event gxtControllerConnectionChangedHandler OnControllerConnectionChanged;

        /// <summary>
        /// Singleton comparer
        /// </summary>
        private gxtPlayerIndexEqualityComparer playerIndexComparer;

        /// <summary>
        /// Has the gamepad manager been initialized already?
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return gamepads != null && gamepads.Count != 0 && connectedGamepads != null;
        }

        /// <summary>
        /// Initializes the gamepad manager
        /// </summary>
        public void Initialize()
        {
            gxtDebug.Assert(!IsInitialized());

            // instantiates gamepads
            gxtGamepad xg1 = new gxtGamepad(PlayerIndex.One);
            gxtGamepad xg2 = new gxtGamepad(PlayerIndex.Two);
            gxtGamepad xg3 = new gxtGamepad(PlayerIndex.Three);
            gxtGamepad xg4 = new gxtGamepad(PlayerIndex.Four);

            // initializes singleton if it hasn't been already
            if (gxtPlayerIndexEqualityComparer.SingletonIsInitialized)
            {
                playerIndexComparer = new gxtPlayerIndexEqualityComparer();
            }

            // instantiates dictionary and adds gpad instances
            gamepads = new Dictionary<PlayerIndex, gxtGamepad>(playerIndexComparer);
            gamepads.Add(PlayerIndex.One, xg1);
            gamepads.Add(PlayerIndex.Two, xg2);
            gamepads.Add(PlayerIndex.Three, xg3);
            gamepads.Add(PlayerIndex.Four, xg4);

            connectedGamepads = new List<gxtGamepad>(4);
        }

        /// <summary>
        /// Updates all of the controllers
        /// Checks for a change in connection state
        /// </summary>
        public void Update()
        {
            gxtDebug.Assert(IsInitialized());
            connectedGamepads.Clear();

            // update all
            UpdateGamepad(PlayerIndex.One);
            UpdateGamepad(PlayerIndex.Two);
            UpdateGamepad(PlayerIndex.Three);
            UpdateGamepad(PlayerIndex.Four);
        }

        /// <summary>
        /// Updates individual gamepad
        /// Handles checking of change in connection state
        /// </summary>
        /// <param name="index">Index Of Gamepad To Update</param>
        private void UpdateGamepad(PlayerIndex index)
        {
            // update and track change in connection state
            bool prevConnected = gamepads[index].IsConnected;
            gamepads[index].Update();
            
            bool connected = gamepads[index].IsConnected;
            
            if (connected)
                connectedGamepads.Add(gamepads[index]);

            if (connected != prevConnected)
            {
                if (OnControllerConnectionChanged != null)
                {
                    OnControllerConnectionChanged(index, connected);
                }
            }
        }

        public void Unload()
        {
            //gamepads.Clear();
            //connectedGamepads.Clear();
        }

        /// <summary>
        /// Accessor for gamepads via player index
        /// </summary>
        /// <param name="playerIndex">Index</param>
        /// <returns>The associated Xbox Gamepad</returns>
        public gxtGamepad GetGamepad(PlayerIndex playerIndex)
        {
            gxtDebug.Assert(IsInitialized());
            gxtGamepad pad = gamepads[playerIndex];
            if (!pad.IsConnected)
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Warning: Requested Gamepad At {0} Is Disconnected!", playerIndex.ToString());
            return pad;
        }

        /// <summary>
        /// Gets number of active and connected xbox gamepads
        /// </summary>
        /// <returns>Integer number of connected gamepads</returns>
        public int GetNumConnectedGamepads()
        {
            return connectedGamepads.Count;
        }

        public List<gxtGamepad> GetConnectedGamepads()
        {
            return connectedGamepads;
        }

        /// <summary>
        /// Might want to consider caching this value and updating it in the update function
        /// </summary>
        /// <returns>Array of connected indices</returns>
        public PlayerIndex[] GetArrayOfConnectedIndices()
        {
            List<PlayerIndex> connectedIndices = new List<PlayerIndex>(4);
            if (gamepads[PlayerIndex.One].IsConnected) connectedIndices.Add(PlayerIndex.One);
            if (gamepads[PlayerIndex.Two].IsConnected) connectedIndices.Add(PlayerIndex.Two);
            if (gamepads[PlayerIndex.Three].IsConnected) connectedIndices.Add(PlayerIndex.Three);
            if (gamepads[PlayerIndex.Four].IsConnected) connectedIndices.Add(PlayerIndex.Four);
            return connectedIndices.ToArray();
        }
    }
}

