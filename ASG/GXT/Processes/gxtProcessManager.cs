using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT.Processes
{
    /// <summary>
    /// A data structure for managing processes
    /// Based on the model seen in Game Coding Complete
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtProcessManager : gxtIUpdate
    {
        private bool enabled;
        private List<gxtProcess> processList;

        /// <summary>
        /// Enabled?
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Count of processes which will be updated on this frame
        /// </summary>
        public int Count { get { return processList.Count; } }

        /// <summary>
        /// Total count of processes in the manager, includes chained processes 
        /// which currently are not active
        /// </summary>
        public int DeepCount
        {
            get
            {
                int total = processList.Count;
                gxtProcess proc;
                for (int i = 0; i < processList.Count; ++i)
                {
                    proc = processList[i];
                    while (proc.NextProcess != null)
                    {
                        ++total;
                        proc = proc.NextProcess;
                    }
                }
                return total;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtProcessManager() { }

        /// <summary>
        /// Determines if the process manager has been initialized
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return processList != null;
        }

        /// <summary>
        /// Initializes process manager
        /// </summary>
        public void Initialize(bool initEnabled = true, int initProcessListSize = 32)
        {
            gxtDebug.Assert(!IsInitialized(), "Process Manager has already been initialized!");
            gxtDebug.Assert(initProcessListSize > 0, "Process list must have a non-zero positive size!");
            Enabled = initEnabled;
            processList = new List<gxtProcess>(initProcessListSize);
        }

        /// <summary>
        /// Adds a process to the collection
        /// </summary>
        /// <param name="process"></param>
        public void Add(gxtProcess process)
        {
            gxtDebug.Assert(!processList.Contains(process), "Cannot add the same process twice!");
            processList.Add(process);
        }

        /// <summary>
        /// Removes a process from the collection
        /// </summary>
        /// <param name="process"></param>
        /// <returns>If removed</returns>
        public bool Remove(gxtProcess process)
        {
            return processList.Remove(process);
        }

        /// <summary>
        /// Determines if the process exists in the collection
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public bool Contains(gxtProcess process)
        {
            return processList.Contains(process);
        }

        /// <summary>
        /// Clears all of the processes in the world
        /// </summary>
        public void Clear()
        {
            processList.Clear();
        }

        /// <summary>
        /// Updates the collection of processes, removing dead ones, and 
        /// activating chained processes when necessary
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Update(GameTime gameTime)
        {
            if (!Enabled) return;

            // if a process is dead, replace it with the next one (if any)
            gxtProcess procNext;
            for (int i = 0; i < processList.Count; ++i)
            {
                if (processList[i].ProcessIsDead)
                {
                    procNext = processList[i].NextProcess;
                    if (procNext != null)
                        processList[i] = procNext;
                    else
                    {
                        processList.RemoveAt(i);
                        --i;
                    }
                }
                else
                {
                    // if enabled could be checked here...
                    // for now it is done in the function...
                    processList[i].Update(gameTime);
                }
            }
        }
    }
}
