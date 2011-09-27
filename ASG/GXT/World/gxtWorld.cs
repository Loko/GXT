using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GXT.Input;
using GXT.Physics;
using GXT.Processes;
using GXT.Rendering;
using GXT.Audio;
using GXT.AI;
using GXT.Animation;

namespace GXT
{
    // use world update delegates sparingly -- expensive to be called every frame
    public delegate void WorldUpdatedHandler(gxtWorld world, GameTime gameTime);
    public delegate void WorldInitializedHandler(gxtWorld world);
    public delegate void WorldEnabledHandler(gxtWorld world);
    public delegate void WorldDisabledHandler(gxtWorld world);
    public delegate void WorldUnloadedHandler(gxtWorld world);

    /// <summary>
    /// A collection of components usable in a real time game world
    /// Includes major components and interlinked management for scene rendering, 
    /// process management, actors, controllers, animations, audio, physics, and pathfinding
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: options for graphics batch rendering should be stored here
    public class gxtWorld : gxtIUpdate
    {
        #region WorldProperties
        private bool enabled;
        private string worldName;
        private float gameSpeed;

        private List<gxtIActor> actors;
        private List<gxtIController> controllers;
        private List<gxtAnimation> animations;

        private gxtCamera camera;
        private gxtSceneGraph sceneGraph;
        private gxtProcessManager processManager;
        private gxtPhysicsWorld physicsWorld;
        private gxtPathGraph pathGraph;
        private gxtAudioScene audioScene;

        /// <summary>
        /// Enabled flag
        /// </summary>
        public bool Enabled { get { return enabled; } 
            set 
            {
                if (value != enabled)
                {
                    if (value)
                    {
                        if (OnWorldEnabled != null)
                            OnWorldEnabled(this);
                    }
                    else
                    {
                        if (OnWorldDisabled != null)
                            OnWorldDisabled(this);
                    }
                    enabled = value;
                }
            } 
        }

        /// <summary>
        /// Name of the world
        /// </summary>
        public string Name { get { return worldName; } set { worldName = value; } }

        /// <summary>
        /// Scene graph of renderable game objects
        /// </summary>
        public gxtSceneGraph SceneGraph { get { return sceneGraph; } set { sceneGraph = value; } }

        /// <summary>
        /// PhysicsWorld for every geom/rigid body in this world
        /// </summary>
        public gxtPhysicsWorld PhysicsWorld { get { return physicsWorld; } }

        /// <summary>
        /// Process manager for processes in the world
        /// </summary>
        public gxtProcessManager ProcessManager { get { return processManager; } }

        /// <summary>
        /// Camera for the world (may eventually become a collection for multiplayer/splitscreen)
        /// </summary>
        public gxtCamera Camera { get { return camera; } set { camera = value; } }

        /// <summary>
        /// Path graph for every path node in the world
        /// </summary>
        public gxtPathGraph PathGraph { get { return pathGraph; } }

        /// <summary>
        /// Audio scene for the world
        /// </summary>
        public gxtAudioScene AudioScene { get { return audioScene; } set { audioScene = value; } }

        /// <summary>
        /// Game Speed (time scale) multiplier
        /// 1.0f is the default speed, 2.0f will be twice as fast,
        /// values between zero and one can achieve 'slo mo' effects
        /// </summary>
        public float GameSpeed
        {
            get { return gameSpeed; }
            set
            {
                gxtDebug.Assert(value > 0.0f, "Game Speed rate must be greater than zero");
                gameSpeed = value;
            }
        }
        #endregion WorldProperties

        #region WorldEvents
        /// <summary>
        /// Event fired after the world has been initialized
        /// </summary>
        public event WorldInitializedHandler OnWorldInit;

        /// <summary>
        /// Event fired at the end of every world update
        /// </summary>
        public event WorldUpdatedHandler OnWorldUpdate;

        /// <summary>
        /// Event fired when the world goes from disabled to enabled
        /// </summary>
        public event WorldEnabledHandler OnWorldEnabled;

        /// <summary>
        /// Event fired when the world goes from enabled to disabled
        /// </summary>
        public event WorldDisabledHandler OnWorldDisabled;

        /// <summary>
        /// Event fired when the world is unloaded
        /// </summary>
        public event WorldUnloadedHandler OnWorldUnload;
        #endregion WorldEvents

        #region Add/Remove
        public void AddSceneNode(gxtISceneNode sceneNode)
        {
            sceneGraph.AddNode(sceneNode);
        }

        public void RemoveSceneNode(gxtISceneNode sceneNode)
        {
            sceneGraph.RemoveNode(sceneNode);
        }

        public void AddProcess(gxtProcess process)
        {
            processManager.Add(process);
        }

        public void RemoveProcess(gxtProcess process)
        {
            processManager.Remove(process);
        }

        public void AddActor(gxtIActor actor)
        {
            actors.Add(actor);
        }

        public bool RemoveActor(gxtIActor actor)
        {
            return actors.Remove(actor);
        }

        public void AddGeom(gxtGeom geom)
        {
            physicsWorld.AddGeom(geom);
        }

        public void RemoveGeom(gxtGeom geom)
        {
            physicsWorld.RemoveGeom(geom);
        }

        public void AddRigidBody(gxtRigidBody body)
        {
            physicsWorld.AddBody(body);
        }

        public void RemoveRigidBody(gxtRigidBody body)
        {
            physicsWorld.RemoveBody(body);
        }

        public void AddController(gxtIController controller)
        {
            controllers.Add(controller);
        }

        public bool RemoveController(gxtIController controller)
        {
            return controllers.Remove(controller);
        }

        public void AddAnimation(gxtAnimation animation)
        {
            animations.Add(animation);
        }

        public bool RemoveAnimation(gxtAnimation animation)
        {
            return animations.Remove(animation);
        }
        #endregion Add/Remove

        /// <summary>
        /// Constructor for gxtWorld
        /// </summary>
        public gxtWorld()
        {

        }

        /// <summary>
        /// Determines if the world has been initialized
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInitialized()
        {
            return (actors != null && animations != null && controllers != null && sceneGraph != null &&
                processManager != null && physicsWorld != null && pathGraph != null && audioScene != null);
        }

        // some log method here that checks the state of everything
        // see if major components are null and initialized
        // maybe some other things can be printed

        /// <summary>
        /// Initializes the world and all of its components
        /// Should be called first (before load)
        /// </summary>
        public virtual void Initialize(bool initEnabled = true, string name = "GXT World")
        {
            gxtDebug.Assert(!IsInitialized(), "World has already been initialized!");
            enabled = initEnabled;
            worldName = name;
            gameSpeed = 1.0f;

            // init lists
            actors = new List<gxtIActor>();
            animations = new List<gxtAnimation>();
            controllers = new List<gxtIController>();

            // construct components
            sceneGraph = new gxtSceneGraph();
            processManager = new gxtProcessManager();
            physicsWorld = new gxtPhysicsWorld();
            pathGraph = new gxtPathGraph();
            audioScene = new gxtAudioScene();

            // init components
            sceneGraph.Initialize();
            camera = new gxtCamera(Vector2.Zero, 0, 0, true);
            //gxtDisplayManager.Singleton.RegisterCamera(camera);
            processManager.Initialize();
            physicsWorld.Initialize();
            //audioScene.Initialize("soundBankFile", "waveBankFile");

            // fire on init event
            if (OnWorldInit != null)
                OnWorldInit(this);
        }

        /// <summary>
        /// Loads game data (file string will be taken eventually)
        /// </summary>
        //public virtual void Load()
        //{
        //    gxtDebug.Assert(IsInitialized());
        //}

        /// <summary>
        /// Will not reload static geometry, renderables, only whats needed for save files
        /// </summary>
        /// <param name="filename"></param>
        /*
        public virtual void LoadSaveFile(string filename)
        {

        }

        /// <summary>
        /// Will return save file...eventually
        /// </summary>
        /// <param name="filename"></param>
        public virtual void Save(string filename)
        {

        }
        */

        /// <summary>
        /// Call Update() on every controller that queries input
        /// </summary>
        public virtual void HandleInput(GameTime gameTime)
        {
            // do we use adjusted here?
            GameTime adjGt = GetAdjustedGameTime(gameTime, gameSpeed);
            for (int i = 0; i < controllers.Count; ++i)
            {
                if (controllers[i].QueriesInput)
                    controllers[i].Update(adjGt);
            }
        }

        /// <summary>
        /// Handles all state based and logic computations for the game world.
        /// Manages updates for all enabled actors, animations, and processes inside 
        /// the world.  Also updates the attached physics world audio scene.
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public virtual void Update(GameTime gameTime)
        {
            gxtDebug.SlowAssert(IsInitialized());
            if (!enabled)
                return;

            GameTime adjGT = GetAdjustedGameTime(gameTime, gameSpeed);
            int i;
            // every controller that doesn't query input
            // we already processed those in HandleInput()
            for (i = 0; i < controllers.Count; ++i)
            {
                if (!controllers[i].QueriesInput)
                    controllers[i].Update(adjGT);
            }
            for (i = 0; i < animations.Count; ++i)
            {
                animations[i].Update(adjGT);
            }
            physicsWorld.Update(adjGT);
            processManager.Update(adjGT);
            //audioScene.Update();
            for (i = 0; i < actors.Count; ++i)
            {
                actors[i].Update(adjGT);
            }
        }

        /// <summary>
        /// An additional update method that gives extra control over the 
        /// order of execution.  Objects dependant on being updated after 
        /// other objects (e.g. camera controllers) should be updated here. 
        /// Controllers have their LateUpdate method called here, the scene 
        /// graph is updated here and the OnWorldUpdate handler is invoked.  
        /// This allows overrides of gxtWorld.Update to call the base 
        /// implementation and add their own logic without breaking crucial 
        /// order execution.
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public virtual void LateUpdate(GameTime gameTime)
        {
            GameTime adjGT = GetAdjustedGameTime(gameTime, gameSpeed);
            sceneGraph.Update(gameTime);

            for (int i = 0; i < controllers.Count; ++i)
            {
                controllers[i].LateUpdate(adjGT);
            }
            
            if (OnWorldUpdate != null)
                OnWorldUpdate(this, adjGT);
        }

        /// <summary>
        /// Gets gametime modified by the game speed multiplier
        /// </summary>
        /// <param name="gameTime">Vanilla gametime</param>
        /// <param name="speed">Speed modifier</param>
        /// <returns>Adjusted GameTime</returns>
        public virtual GameTime GetAdjustedGameTime(GameTime gameTime, float speed)
        {
            gxtDebug.Assert(speed > 0.0f, "Game Speed rate must be greater than zero");
            // may want to clamp this to a min of 1
            return new GameTime(new TimeSpan(gxtMath.Max<long>((long)(gameTime.TotalGameTime.Ticks * speed), 1)), new TimeSpan(gxtMath.Max<long>((long)(gameTime.ElapsedGameTime.Ticks * speed), 1)), gameTime.IsRunningSlowly);
        }

        /// <summary>
        /// Disposes of everything in the world
        /// </summary>
        public virtual void UnloadWorld()
        {
            // do intended unloading first for actors
            if (IsInitialized())
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].UnloadActor();
                }
                actors.Clear();
                controllers.Clear();
                animations.Clear();

                //audioScene.Clear();
                pathGraph.Destroy();
                processManager.Clear();
                physicsWorld.Dispose(true); // flag resets geom ids
                sceneGraph.RemoveAllNodes();
                gxtDisplayManager.Singleton.UnRegisterCamera(camera);

                if (OnWorldUnload != null)
                    OnWorldUnload(this);
            }
        }

        /// <summary>
        /// Draws everything in the world scene graph relative to the camera
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public virtual void Draw(gxtGraphicsBatch graphicsBatch)
        {
            gxtDebug.SlowAssert(IsInitialized() && sceneGraph.IsInitialized());
            graphicsBatch.Begin(gxtBatchDrawOrder.PRIMITIVES_FIRST, gxtBatchSortMode.TEXTURE, gxtBatchDepthMode.FRONT_TO_BACK, camera.GetTransform());
            sceneGraph.Draw(graphicsBatch);
            graphicsBatch.End();
        }
    }
}
