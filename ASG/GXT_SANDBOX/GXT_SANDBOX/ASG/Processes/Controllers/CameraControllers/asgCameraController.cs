using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Rendering;
using GXT.Physics;

namespace ASG
{
    public enum asgCameraMode 
    {
        AUTONOMOUS = 0,
        PLAYER_CONTROLLED = 1
    };

    public class asgCameraController : gxtCameraController
    {
        private asgCameraMode cameraMode;
        private float cameraSpeed;
        private gxtOBB cameraSafeZone;
        private gxtRigidBody cameraBody;
        private float cameraForce;
        private asgPlayerActor player;
        private Vector2 offset;

        public asgCameraMode Mode { get { return cameraMode; } set { cameraMode = value; } }
        public float CameraSpeed { get { return cameraSpeed; } set { gxtDebug.Assert(value >= float.Epsilon, "Must have a positive camera speed!"); cameraSpeed = value; } }

        public asgCameraController(gxtCamera camera)
            : base(camera)
        {

        }

        public void Initialize(asgCameraMode mode, asgPlayerActor player, float cameraForce, Vector2 targetOffset)
        {
            cameraMode = mode;
            this.cameraForce = cameraForce;
            if (gxtDisplayManager.SingletonIsInitialized)
            {
                gxtDisplayManager.Singleton.resolutionChanged += OnResolutionChanged;
            }

            gxtOBB tmp = Camera.GetViewOBB();
            cameraSafeZone = new gxtOBB(tmp.Position, tmp.Extents * 0.8f, tmp.Rotation);
            if (gxtDisplayManager.SingletonIsInitialized)
            {
                gxtDisplayManager.Singleton.resolutionChanged += OnResolutionChanged;
            }

            // cameraSpeed is in screen space
            cameraSpeed = 3.0f;
            cameraBody = new gxtRigidBody();
            cameraBody.MotionType = gxtRigidyBodyMotion.DYNAMIC;
            cameraBody.AngularDamping = 1.0f;
            cameraBody.Damping = 1.0f;
            cameraBody.IgnoreGravity = true;
            cameraBody.Mass = 2.0f;
            cameraBody.CanSleep = false;
            cameraBody.Awake = true;
            player.World.AddRigidBody(cameraBody);

            this.player = player;
            this.offset = targetOffset;
        }

        private void OnResolutionChanged(gxtDisplayManager manager)
        {
            // change camera safe zone
        }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled)
                return;
            
            gxtGamepad gp = gxtGamepadManager.Singleton.GetGamepad(PlayerIndex.One);
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();

            if (kb.GetState(Keys.F1) == gxtControlState.FIRST_PRESSED)
            {
                if (cameraMode == asgCameraMode.AUTONOMOUS)
                    cameraMode = asgCameraMode.PLAYER_CONTROLLED;
                else
                    cameraMode = asgCameraMode.AUTONOMOUS;
            }

            if (cameraMode == asgCameraMode.AUTONOMOUS)
            {
                // we'll be naive for the time being
                // just set the position to the player + offset
                Camera.Position = (player.Position + offset) * gxtPhysicsWorld.PHYSICS_SCALE;
            }
            else
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                // IMPORTANT -- REVERSED
                if (!gp.IsConnected)
                {
                    ProcessGamepad(gp, dt);
                }
                else
                {
                    ProcessKeyboard(kb, dt);
                }
                //Camera.TranslateLocal(gp.AdjLStick() * cameraSpeed * dt);
                //gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "The camera mode: {0} is not yet supported by this controller", cameraMode.ToString());
            }
        }

        private void ProcessGamepad(gxtGamepad gp, float dt)
        {
            // implement
        }

        private void ProcessKeyboard(gxtKeyboard kb, float dt)
        {
            float dX = 0.0f, dY = 0.0f;
            if (kb.IsDown(Keys.Left))
                dX = -1.0f;
            if (kb.IsDown(Keys.Right))
                dX = 1.0f;
            if (kb.IsDown(Keys.Up))
                dY = -1.0f;
            if (kb.IsDown(Keys.Down))
                dY = 1.0f;
            Camera.TranslateLocal(dX * cameraSpeed, dY * cameraSpeed);
            // use + and - for zoom
            // use { and } for rotation

        }

        public void TrackPoint(float dt, Vector2 pt)
        {
            float sphereTol = 0.5f;
            float dist = cameraSafeZone.DistanceToPointSquared(pt);
            if (dist <= sphereTol)
                return;

            Vector2 cp = cameraSafeZone.ClosestPtOnOBB(pt);

        }
    }
}
