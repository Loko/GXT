#define RAY_CAST_TEST
//#undef RAY_CAST_TEST
using GXT;
using GXT.Processes;
using GXT.Input;
using GXT.Physics;
using GXT.Rendering;
using GXT.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace GXT_SANDBOX
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MyAnimationTestGame : gxtGame
    {

        gxtCamera camera;
        gxtSceneGraph sceneGraph;
        /*
        gxtAnimationClip animClip;
        gxtKeyframe initKeyframe;
        gxtKeyframe finalKeyframe;
        */
        gxtISceneNode parent;
        gxtISceneNode child;

        gxtAnimation animClip;
        gxtAnimationController animController;

        public MyAnimationTestGame()
            : base()
        {
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;
            camera = new gxtCamera(Vector2.Zero, 0.0f, 0.0f, false);
            gxtDisplayManager.Singleton.WindowTitle = "Animation Scene Graph Test";
            sceneGraph = new gxtSceneGraph();
            sceneGraph.Initialize();

            Texture2D grassTexture, metalTexture;
            bool textureLoaded = gxtResourceManager.Singleton.LoadTexture("Textures\\grass", out grassTexture);
            gxtDebug.Assert(textureLoaded, "Texture load failed!");
            textureLoaded = gxtResourceManager.Singleton.LoadTexture("Textures\\scratched_metal", out metalTexture);
            gxtDebug.Assert(textureLoaded, "Texture load failed!");

            parent = new gxtSceneNode();
            Vector2[] rectangleVertices = gxtGeometry.CreateRectangleVertices(150, 100);
            gxtDynamicMesh rectangle = new gxtDynamicMesh(rectangleVertices);
            gxtIMaterial rectangleMaterial = new gxtMaterial(true, Color.Yellow, 0.5f);
            rectangle.Material = rectangleMaterial;
            rectangle.ApplyTexture(grassTexture, gxtTextureCoordinateType.WRAP);
            parent.AttachDrawable(rectangle);

            child = new gxtSceneNode();
            child.Position = new Vector2(37.5f, 55.5f);
            Vector2[] rectangleVertices2 = gxtGeometry.CreateRectangleVertices(75, 175);
            gxtIMaterial rectangleMaterial2 = new gxtMaterial(true, Color.Blue, 1.0f);
            gxtDynamicMesh rectangle2 = new gxtDynamicMesh(rectangleVertices2);
            rectangle2.Material = rectangleMaterial2;
            rectangle2.ApplyTexture(metalTexture, gxtTextureCoordinateType.WRAP);
            child.AttachDrawable(rectangle2);
            parent.AddChild(child);

            sceneGraph.AddNode(parent);

            gxtAnimationPose a0 = new gxtAnimationPose();
            a0.InterpolateUVCoords = true;
            a0.InterpolateColorOverlay = true;
            a0.UVCoordinates = rectangle.GetTextureCoordinates();

            gxtAnimationPose a1 = new gxtAnimationPose();
            a1.InterpolateUVCoords = false;
            a1.InterpolateColorOverlay = true;
            a1.ColorOverlay = Color.Red;
            a1.Translation = new Vector2(-150, -200);

            Vector2[] uvCoordsCopy = rectangle.GetTextureCoordinates();
            for (int i = 0; i < uvCoordsCopy.Length; ++i)
            {
                uvCoordsCopy[i] += new Vector2(-0.75f);
            }
            a1.UVCoordinates = uvCoordsCopy;

            gxtAnimationPose a2 = new gxtAnimationPose();
            a2.Translation = new Vector2(200, -225);
            a2.Rotation = gxtMath.PI_OVER_FOUR;
            a2.InterpolateUVCoords = false;
            Vector2[] uvCoordsCopy2 = rectangle.GetTextureCoordinates();
            for (int i = 0; i < uvCoordsCopy2.Length; ++i)
            {
                uvCoordsCopy2[i] *= (1.0f / 1.5f);
                //uvCoordsCopy2[i] += new Vector2(-3.75f, 0.0f);
            }
            a2.UVCoordinates = uvCoordsCopy2;


            gxtAnimationPose a3 = new gxtAnimationPose();
            a3.Translation = new Vector2(50, 200);
            a3.Rotation = gxtMath.DegreesToRadians(-235);
            a3.Scale = new Vector2(1.85f, 1.75f);

            gxtKeyframe k0 = new gxtKeyframe(a0, 0.0f);
            gxtKeyframe k1 = new gxtKeyframe(a1, 0.4f);
            gxtKeyframe k2 = new gxtKeyframe(a2, 0.65f);
            gxtKeyframe k3 = new gxtKeyframe(a3, 1.0f);


            gxtAnimationClip clip = new gxtAnimationClip(parent, rectangle);
            clip.AddKeyframe(k0);
            clip.AddKeyframe(k1);
            clip.AddKeyframe(k2);
            clip.AddKeyframe(k3);

            animClip = new gxtAnimation(TimeSpan.FromSeconds(5.0), true, true, 1.0f);
            animClip.AddClip(clip);

            animController = new gxtAnimationController();
            animController.AddAnimation("default", animClip);
            //animClip.AddTween();
            /*
            gxtAnimationKeyFrame k3 = new gxtAnimationKeyFrame();
            */

            /*
            parent = new gxtSceneNode();
            //gxtIDrawable rectangleDrawable = new gxtDrawable(Color.Yellow, true, 0.5f);
            gxtRectangle rectangle = new gxtRectangle(150, 100);
            gxtIMaterial rectangleMaterial = new gxtMaterial(true, Color.Yellow, 0.2f);
            rectangle.Material = rectangleMaterial;
            parent.AttachDrawable(rectangle);

            child = new gxtSceneNode();
            //gxtIDrawable childRectDrawable = new gxtDrawable(Color.Blue, true, 0.0f);
            gxtRectangle childRect = new gxtRectangle(75, 175);
            gxtIMaterial childRectangleMaterial = new gxtMaterial(true, new Color(0, 0, 255, 100), 0.0f);
            childRect.Material = childRectangleMaterial;
            child.AttachDrawable(childRect);
            child.Position = new Vector2(37.5f, 55.5f);

            parent.AddChild(child);

            sceneGraph.AddNode(parent);

            initKeyframe = new gxtKeyframe(gxtNodeTransform.Identity, 0.0f);

            gxtNodeTransform midTransform = new gxtNodeTransform();
            midTransform.Translation = new Vector2(-185, -100);
            midTransform.Scale = new Vector2(-1.5f, 1.5f);
            midTransform.Rotation = gxtMath.DegreesToRadians(-25.0f);
            gxtKeyframe midKeyFrame = new gxtKeyframe(midTransform, 0.235f);

            gxtNodeTransform finalPose = new gxtNodeTransform();
            finalPose.Translation = new Vector2(100.0f, -200.0f);
            finalPose.Rotation = gxtMath.DegreesToRadians(90.0f);
            finalKeyframe = new gxtKeyframe(finalPose, 0.6f);

            gxtNodeTransform lastXform = new gxtNodeTransform();
            lastXform.Translation = new Vector2(100.0f, -200.0f);
            lastXform.Rotation = -6.0f * gxtMath.PI;
            lastXform.Scale = new Vector2(0.25f, 1.45f);
            gxtKeyframe lastKeyframe = new gxtKeyframe(lastXform, 0.85f);


            gxtTween tween = new gxtTween(parent);
            tween.AddKeyframe(initKeyframe);
            tween.AddKeyframe(finalKeyframe);
            tween.AddKeyframe(midKeyFrame);
            tween.AddKeyframe(lastKeyframe);


            animClip = new gxtAnimationClip(TimeSpan.FromSeconds(10.0));
            animClip.AddTween(tween);
            animClip.Loop = false;
            animClip.PlaybackRate = 1.0f;

            animController = new gxtAnimationController();
            animController.AddClip("default", animClip);
            animController.Stop("default");
            */

            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Window Width: {0}", gxtDisplayManager.Singleton.ResolutionWidth);
            gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Window Height: {0}", gxtDisplayManager.Singleton.ResolutionHeight);
            //gxtSprite sprite = new gxtSprite(texture);
            //sprite.Depth = 0.0f;
            //sprite.Alpha = 100;
            //manager.Add(sprite);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //root.Initialize(this);
            //spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            animController.Update(gameTime);
            animClip.Update(gameTime);

            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            if (kb.GetState(Keys.Space) == gxtControlState.FIRST_PRESSED)
                animController.Toggle("default");
            if (kb.GetState(Keys.S) == gxtControlState.FIRST_PRESSED)
                animController.Stop("default");
            if (kb.GetState(Keys.P) == gxtControlState.FIRST_PRESSED)
                animController.Play("default");
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Is Playing?: {0}", animController.IsPlaying("default"));
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Is Done?: {0}", animController.IsDone("default"));
            /*
            gxtKeyboard kb = gxtKeyboardManager.Singleton.GetKeyboard();
            if (kb.GetState(Keys.T) == gxtControlState.FIRST_PRESSED)
                animController.Toggle("default");
            if (kb.GetState(Keys.S) == gxtControlState.FIRST_PRESSED)
                animController.StopAll();
            if (kb.GetState(Keys.P) == gxtControlState.FIRST_PRESSED)
                animController.Play("default");

            animClip.Update(gameTime);
            animController.Update(gameTime);
            */
            //manager.Update();
            sceneGraph.Update(gameTime);
        
         }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "cam pos: {0}", camera.Position);
            base.Draw(gameTime);
            gxtGraphicsBatch gb = gxtRoot.Singleton.GraphicsBatch;
            gb.Begin(gxtBatchDrawOrder.PRIMITIVES_FIRST, gxtBatchSortMode.DEFAULT, gxtBatchDepthMode.FRONT_TO_BACK, camera.GetTransform());
            sceneGraph.Draw(gb);
            gb.End();
        }
    }
}
