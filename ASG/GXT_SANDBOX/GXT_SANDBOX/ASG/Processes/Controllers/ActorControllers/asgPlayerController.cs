using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GXT;
using GXT.Input;
using GXT.Processes;

namespace ASG
{
    public class asgPlayerController : gxtIController
    {
        private bool enabled;
        private asgPlayerActor playerActor;

        public bool Enabled { get { return enabled; } set { enabled = value; } }
        public asgPlayerActor Player { get { return playerActor; } set { playerActor = value; } }
        public bool QueriesInput { get { return true; } }

        public asgPlayerController()
        {

        }

        public void Initialize(asgPlayerActor player, bool initEnabled = true)
        {
            this.playerActor = player;
            this.enabled = initEnabled;
        }

        public void Update(GameTime gameTime)
        {
            if (!enabled)
                return;

            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            gxtGamepad gp = gxtGamepadManager.Singleton.GetGamepad(PlayerIndex.One);
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // logic here
            // reverse because I'm remoting in and don't have a gamepad on me
            /* IMPORTANT */
            if (!gp.IsConnected)
            {
                ProcessGamepad(gp, dt);
            }
            else
            {
                ProcessKeyboard(kb, dt);
            }

            /*
            float speedSq = playerActor.PlayerBody.Velocity.LengthSquared();
            float maxSpeedSq = playerActor.MaxSpeed * playerActor.MaxSpeed;
            if (speedSq > maxSpeedSq)
            {
                float speed = gxtMath.Sqrt(speedSq);
                float modifier = playerActor.MaxSpeed / speed;
                playerActor.PlayerBody.Velocity *= modifier;
            }
            */
        }

        public void LateUpdate(GameTime gameTime)
        {

        }

        private void ProcessGamepad(gxtGamepad gp, float dt)
        {
            Vector2 lstick = gp.AdjLStick();
            playerActor.Body.Position += lstick * 2.0f * dt;
            //playerActor.Move(lstick);
        }

        private void ProcessKeyboard(gxtKeyboard kb, float dt)
        {
            if (kb.GetState(Keys.F2) == gxtControlState.FIRST_PRESSED)
            {
                if (playerActor.ClipMode == asgClipMode.NORMAL)
                    playerActor.ClipMode = asgClipMode.NOCLIP;
                else
                    playerActor.ClipMode = asgClipMode.NORMAL;
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "F2 First Pressed!");
            }

            float dX = 0.0f, dY = 0.0f;
            if (kb.IsDown(Keys.D))
                dX = 1.0f;
            if (kb.IsDown(Keys.A))
                dX -= 1.0f;
            if (kb.IsDown(Keys.W))
                dY = -1.0f;
            if (kb.IsDown(Keys.S))
                dY = 1.0f;

            // process jump
            if (playerActor.OnGround)
            {
                bool jumpRequested = kb.GetState(Keys.Space) == gxtControlState.FIRST_PRESSED;
                if (jumpRequested)
                {
                    playerActor.Body.ApplyImpulseAtLocalPoint(new Vector2(0.0f, -20.0f), Vector2.Zero);
                }
            }

            if (playerActor.ClipMode == asgClipMode.NORMAL)
            {
                if ((gxtMath.Abs(dX) < float.Epsilon))
                {
                    
                }
                else
                {
                    Vector2 surfaceRight = gxtMath.RightPerp(playerActor.SurfaceNormal);
                    Vector2 inputVec = new Vector2(dX, 0.0f);
                    Vector2 proj = surfaceRight * Vector2.Dot(surfaceRight, inputVec);
                    // apply friction here

                    // todo: make it slower to walk up certain things then down them
                    // also, consider using velocity and zeroing it after every frame
                    // the straight translation technique has a lot of flaws
                    playerActor.Translate(proj * playerActor.MoveSpeed * dt);
                    //playerActor
                }
            }
            else
            {
                playerActor.Translate(new Vector2(dX, dY) * playerActor.MoveSpeed * dt);
            }
        }

        public Type GetTargetType()
        {
            return typeof(asgPlayerActor);
        }


    }
}
