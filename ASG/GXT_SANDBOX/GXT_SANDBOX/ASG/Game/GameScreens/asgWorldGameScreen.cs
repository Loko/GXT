using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GXT;
using GXT.IO;
using GXT.Rendering;
using GXT.Physics;
using GXT.Input;

namespace ASG
{
    public class asgWorldGameScreen : gxtWorldGameScreen
    {
        public asgWorldGameScreen()
            : base()
        {

        }

        public override void Initialize(bool setupDebugDrawing = true)
        {
            base.Initialize();
            world = new gxtWorld();
            world.Initialize(true, "ASG Prototype");
            gxtDisplayManager.Singleton.WindowTitle = "ASG Prototype";
            if (setupDebugDrawing)
            {
                if (gxtDebugDrawer.SingletonIsInitialized)
                {
                    debugDrawId = gxtDebugDrawer.Singleton.GetNewId();
                    gxtDebugDrawer.Singleton.AddSceneGraph(debugDrawId, world.SceneGraph);
                    gxtDebugDrawer.Singleton.DebugFont = gxtResourceManager.Singleton.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>("Fonts\\debug_font");
                }
                else
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot Set Up Debug Drawing For The Gameplay Screen Because the Debug Drawer Has Not Been Initialized");
                }
            }

            gxtLog.WriteLineV(gxtVerbosityLevel.SUCCESS, "Initialized GXT World \"{0}\"", world.Name);

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Populating world...");
            world.PhysicsWorld.AddCollisionGroupName("player", gxtCollisionGroup.GROUP1);
            world.PhysicsWorld.AddCollisionGroupName("traversable_world_geometry", gxtCollisionGroup.GROUP2);

            // setup player
            asgPlayerActor player = new asgPlayerActor(world);
            player.Initialize(Vector2.Zero, 6.5f);
            world.AddActor(player);

            // player controller
            asgPlayerController playerController = new asgPlayerController();
            playerController.Initialize(player);
            world.AddController(playerController);

            // camera controller
            asgCameraController cameraController = new asgCameraController(world.Camera);
            cameraController.Initialize(asgCameraMode.AUTONOMOUS, player, 1.0f, new Vector2(0.0f, -3.5f));
            world.AddController(cameraController);

            // geoms (platforms and walls)
            gxtPolygon platformPolygon = gxtGeometry.CreateRectanglePolygon(15.0f, 2.0f);
            CreatePlatformGeom(platformPolygon, new Vector2(0.0f, 8.5f));

            // dynamic box
            gxtRigidBody boxBody = new gxtRigidBody();
            boxBody.Mass = 1.0f;
            boxBody.CanSleep = false;
            boxBody.Awake = true;

            gxtPolygon box = gxtGeometry.CreateRectanglePolygon(1.5f, 1.5f);
            gxtGeom boxGeom = new gxtGeom(box, true);
            boxGeom.RigidBody = boxBody;
            boxBody.Inertia = gxtRigidBody.GetInertiaForRectangle(1.5f, 1.5f, boxBody.Mass);
            boxBody.Position = new Vector2(-3.5f, -3.5f);
            world.AddGeom(boxGeom);
            world.AddRigidBody(boxBody);

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Done populating world...");
        }

        private gxtGeom CreatePlatformGeom(gxtPolygon polygon, Vector2 position)
        {
            gxtGeom platGeom = new gxtGeom(polygon, true);
            gxtRigidBody platBody = new gxtRigidBody();
            platBody.MotionType = gxtRigidyBodyMotion.FIXED;
            platBody.CanSleep = false;
            platBody.Awake = true;
            platGeom.RigidBody = platBody;
            gxtPhysicsMaterial mat = new gxtPhysicsMaterial(0.6f, 0.3f);
            platGeom.CollisionGroups = world.PhysicsWorld.GetCollisionGroup("traversable_world_geometry");
            platGeom.CollidesWithGroups = world.PhysicsWorld.GetCollisionGroup("player");
            platGeom.Material = mat;
            platGeom.SetPosition(new Vector2(0.0f, 8.5f));
            world.AddGeom(platGeom);
            return platGeom;
        }

        public override void HandleInput(GameTime gameTime)
        {
            // still haven't figured out how I want to handle multiple inputs
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            gxtGamepad gp = gxtGamepadManager.Singleton.GetGamepad(PlayerIndex.One);

            if (kb.GetState(Keys.Escape) == gxtControlState.FIRST_PRESSED)
                world.Enabled = !world.Enabled;

            if (gp.GetState(Buttons.Start) == gxtControlState.FIRST_PRESSED)
                world.Enabled = !world.Enabled;
        }

        protected override void UpdateScreen(GameTime gameTime)
        {
            if (IsSetupForDebugDrawing)
                gxtDebugDrawer.Singleton.CurrentSceneId = debugDrawId;

            world.Update(gameTime);
            world.PhysicsWorld.DebugDrawGeoms(Color.Yellow, Color.Pink, Color.Gray, Color.Blue, Color.Red, 0.5f, 0.55f, 0.51f, true, true, true);
            world.LateUpdate(gameTime);
        }

        public override void Draw(gxtGraphicsBatch graphicsBatch)
        {
            base.Draw(graphicsBatch);
            //world.Draw(spriteBatch);
        }
    }
}
