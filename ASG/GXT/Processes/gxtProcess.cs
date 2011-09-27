using Microsoft.Xna.Framework;

namespace GXT.Processes
{
    /// <summary>
    /// A base class to handle robust processes, which can be enabled,
    /// initialized, killed, and chained.  Instances of gxtProcess are 
    /// meant to be added to a gxtProcessManager.
    /// 
    /// Heavily based on the model presented in the book Game Coding Complete
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtProcess : gxtIUpdate
    {
        // given process type constants
        public const int UNKNOWN_TYPE = 0;
        public const int WAIT_TYPE = 1;
        public const int TIMER_TYPE = 2;
        public const int ANIMATION_TYPE = 3;
        public const int INPUT_TYPE = 4;

        /// <summary>
        /// Enabled?
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Integer process type id
        /// </summary>
        public int ProcessType { get; set; }
        
        /// <summary>
        /// Processes marked as dead are removed from the process manager
        /// </summary>
        public bool ProcessIsDead { get; protected set; }

        /// <summary>
        /// Has been initialized?
        /// </summary>
        public bool ProcessIsInitialized { get; protected set; }
        
        /// <summary>
        /// Next process to run when this process is killed
        /// </summary>
        public gxtProcess NextProcess { get; protected set; }

        /// <summary>
        /// Process constructor.  Optional defintions for type, enable on construction, and intialization 
        /// on construction.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="initEnabled"></param>
        /// <param name="initializeNow"></param>
        public gxtProcess(bool initEnabled = true, bool isInitialized = false, int processType = UNKNOWN_TYPE)
        {
            ProcessType = processType;
            Enabled = initEnabled;
            ProcessIsDead = false;
            ProcessIsInitialized = isInitialized;
        }

        /// <summary>
        /// Shuts down the process and marks it for removal 
        /// by the overarching process manager
        /// </summary>
        public virtual void KillProcess()
        {
            ProcessIsDead = true;
        }

        /// <summary>
        /// Sets the chained process to run when this one is done
        /// A reference to the new process is returned to make long chains 
        /// easier to create semantically.  Example shown below:
        /// 
        /// a.SetNextProcess(b).SetNextProcess(c).SetNextProcess(d)
        /// </summary>
        /// <param name="process">Dependent process</param>
        /// <returns>Passed in process</returns>
        public virtual gxtProcess SetNextProcess(gxtProcess process)
        {
            NextProcess = process;
            return process; 
        }

        /// <summary>
        /// Runs startup procedures for the process
        /// </summary>
        public virtual void InitializeProcess()
        {
            ProcessIsInitialized = true;
        }

        /// <summary>
        /// Updates the process
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public virtual void Update(GameTime gameTime)
        {
            if (!Enabled)
                return;
            if (!ProcessIsInitialized)
            {
                InitializeProcess();
            }
        }
    }
}
