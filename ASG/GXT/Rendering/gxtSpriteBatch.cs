using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public class gxtSpriteBatch
    {
        /*
        private enum gxtDrawInfoType
        {
            TEXTURE = 0,
            STRING = 1,
            POLYGON = 2
        };
        */

        private abstract class gxtDrawInfo
        {
            public float depth;
            public abstract void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect effect);
        }

        private class gxtBaseDrawInfo : gxtDrawInfo
        {
            //public abstract gxtDrawInfoType Type { get; }
            public Color color;
            public Vector2 position;
            public Vector2 scale;
            public Vector2 origin;
            public float rotation;
            public SpriteEffects effects;

            public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect effect)
            {
                // override
            }
        }

        private class gxtTextureDrawInfo : gxtBaseDrawInfo
        {
            //public override gxtDrawInfoType Type { get { return gxtDrawInfoType.TEXTURE; } }

            public Texture2D texture;

            public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect effect)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, cameraMatrix);
                spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, effects, depth);
                spriteBatch.End();
            }
        }

        private class gxtStringDrawInfo : gxtBaseDrawInfo
        {
            //public override gxtDrawInfoType Type { get { return gxtDrawInfoType.STRING; } }

            public SpriteFont font;
            public string text;

            public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect effect)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, cameraMatrix);
                spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, effects, depth);
                spriteBatch.End();
            }
        }

        private class TextureFrameDrawInfo : gxtTextureDrawInfo
        {
            public Rectangle rectangle;

            public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect effect)
            {
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, cameraMatrix);
                spriteBatch.Draw(texture, position, rectangle, color, rotation, origin, scale, effects, depth);
                spriteBatch.End();
            }
        }

        private class gxtPolygonDrawInfo : gxtDrawInfo
        {
            public VertexBuffer vertexBuffer;
            public IndexBuffer indexBuffer;
            public Matrix worldMatrix;
            public Texture2D texture;


            public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect effect)
            {
                graphics.RasterizerState = RasterizerState.CullNone;    // needed??
                effect.World = worldMatrix;
                effect.Texture = texture;
                effect.TextureEnabled = (texture != null);
                graphics.SetVertexBuffer(vertexBuffer);
                graphics.Indices = indexBuffer;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                }
                //graphics.SetVertexBuffer(null);
                //graphics.Indices = null;
            }
        }

        private SpriteBatch spriteBatch;
        private GraphicsDevice graphics;
        public static Matrix cameraMatrix;
        private BasicEffect effect;
        private List<gxtDrawInfo> drawables;
        private bool begun;
        private float screenScale;

        public void Initialize(SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;
            drawables = new List<gxtDrawInfo>();
            effect = new BasicEffect(graphics);
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
            if (gxtDisplayManager.SingletonIsInitialized)
            {
                gxtDisplayManager.Singleton.resolutionChanged += ResolutionChangedHandler;
                //effect.Projection = Matrix.CreateOrthographic(gxtDisplayManager.Singleton.ResolutionWidth, gxtDisplayManager.Singleton.ResolutionHeight, 0.0f, 1.0f);
                effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateOrthographic(gxtDisplayManager.Singleton.ResolutionWidth, gxtDisplayManager.Singleton.ResolutionHeight, 0.0f, 1.0f);//Matrix.CreateOrthographicOffCenter(0.0f, gxtDisplayManager.Singleton.ResolutionWidth, gxtDisplayManager.Singleton.ResolutionHeight, 0.0f, 0.0f, 1.0f);
                screenScale = gxtDisplayManager.Singleton.ResolutionWidth / (float)gxtDisplayManager.Singleton.TargetResolutionWidth;
            }
            else
            {
                effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateOrthographic(gxtDisplayManager.DEFAULT_RESOLUTION_WIDTH, gxtDisplayManager.DEFAULT_RESOLUTION_HEIGHT, 0.0f, 1.0f);
                screenScale = 1.0f;
            }
            begun = false;
        }

        /// <summary>
        /// Begins a batch with the given view matrix
        /// </summary>
        /// <param name="viewMatrix"></param>
        /*
        public void Begin(gxtCamera camera)
        {
            gxtDebug.Assert(!begun);
            cameraMatrix = camera.GetTransformation();
            effect.View = GetEffectMatrix(camera);
            begun = true;
            // draw in immediate mode
            // update matrices appropriately
        }
        */
        
        public void Begin(Matrix viewMatrix)
        {
            gxtDebug.Assert(!begun);
            cameraMatrix = viewMatrix;
            effect.View = GetEffectViewMatrix();
            begun = true;
        }

        /// <summary>
        /// A simple comparison function that sorts the drawables by depth
        /// This function should probably include more comparisons for objects at the 
        /// same depth in an attempt to minimize the amount of batches and state changes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int DrawInfoCompare(gxtDrawInfo x, gxtDrawInfo y)
        {
            if (x.depth < y.depth)
                return 1;
            if (x.depth > y.depth)
                return -1;
            return 0;
        }

        /// <summary>
        /// Commits all the commands and draws everything to the screen
        /// </summary>
        public void End()
        {
            gxtDebug.Assert(begun);

            // setup the effect and sort by depth
            //effect.View = GetEffectMatrix(cameraMatrix);
            drawables.Sort(DrawInfoCompare);
            // draw everything
            for (int i = 0; i < drawables.Count; i++)
            {
                drawables[i].Draw(spriteBatch, graphics, effect);
            }
            
            drawables.Clear();
            begun = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <param name="effects"></param>
        /// <param name="layerDepth"></param>
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            gxtDebug.Assert(begun);
            gxtStringDrawInfo str = new gxtStringDrawInfo();

            str.depth = layerDepth;
            str.font = spriteFont;
            str.text = text;
            str.position = position;
            str.scale = scale;
            str.rotation = rotation;
            str.origin = origin;
            str.effects = effects;
            str.color = color;
            
            drawables.Add(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <param name="effects"></param>
        /// <param name="layerDepth"></param>
        public void DrawSprite(Texture2D texture, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            gxtDebug.Assert(begun);
            gxtTextureDrawInfo t = new gxtTextureDrawInfo();

            t.depth = layerDepth;
            t.texture = texture;
            t.position = position;
            t.scale = scale;
            t.rotation = rotation;
            t.origin = origin;
            t.effects = effects;
            t.color = color;
            
            drawables.Add(t);
        }

        /// <summary>
        /// Has the nullable rectangle
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="rectangle"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <param name="effects"></param>
        /// <param name="layerDepth"></param>
        public void DrawSpriteFrame(Texture2D texture, Vector2 position, Nullable<Rectangle> rectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            TextureFrameDrawInfo tf = new TextureFrameDrawInfo();
            tf.depth = layerDepth;
            tf.texture = texture;
            tf.position = position;
            tf.scale = scale;
            tf.rotation = rotation;
            tf.origin = origin;
            tf.effects = effects;
            tf.color = color;
            tf.rectangle = rectangle.Value;

            drawables.Add(tf);

        }

        /// <summary>
        /// Queues a polygon with the given texture, buffers, and orientation
        /// Recall that color information is stored in the vertces themselves
        /// It is reccomended you use VertexPositionColorTexture Vertices for your 
        /// Vertex Buffer
        /// </summary>
        /// <param name="texture">Texture, if null then the color will be drawn for the entire polygon</param>
        /// <param name="vertexBuffer">Vertex Buffer (local)</param>
        /// <param name="indexBuffer">Triangulated Index Buffer For the Polygon</param>
        /// <param name="position">World Position</param>
        /// <param name="rotation">World Rotation</param>
        /// <param name="scale">World Scale</param>
        /// <param name="effects">World sprite Effects</param>
        /// <param name="layerDepth">Render Depth</param>
        public void DrawPolygon(Texture2D texture, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, Vector2 position, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            gxtDebug.Assert(begun);
            // assert on vertex type?

            gxtPolygonDrawInfo p = new gxtPolygonDrawInfo();
            p.depth = layerDepth;
            p.texture = texture;
            p.vertexBuffer = vertexBuffer;
            p.indexBuffer = indexBuffer;
            graphics.SetVertexBuffer(vertexBuffer);
            graphics.Indices = indexBuffer;

            // create world matrix, make sure ops are in the right order
            Matrix worldMat;
            if (effects == SpriteEffects.FlipHorizontally)
                worldMat = Matrix.CreateScale(-1.0f, 1.0f, 1.0f);
            else if (effects == SpriteEffects.FlipVertically)
                worldMat = Matrix.CreateScale(1.0f, -1.0f, 1.0f);
            else
                worldMat = Matrix.Identity;
            worldMat *= Matrix.CreateScale(scale.X, scale.Y, 1.0f);
            worldMat *= Matrix.CreateRotationZ(rotation);
            worldMat *= Matrix.CreateTranslation(position.X, position.Y, 0.0f);

            Vector2 tmpT, tmpS;
            float tmpR;
            gxtMath.Decompose2D(ref worldMat, out tmpT, out tmpS, out tmpR);
            if (tmpT == position)
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Translation decomposition was correct");
            else
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Translation decomposition was wrong!");
            if (gxtMath.Equals(tmpS.X, scale.X, float.Epsilon) && gxtMath.Equals(tmpS.Y, scale.Y, float.Epsilon))
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Scale decomposition was correct");
            else
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Scale decomposition was wrong!");
            if (gxtMath.Equals(gxtMath.WrapAngle(tmpR), gxtMath.WrapAngle(rotation), float.Epsilon))
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Rotation decomposition was correct");
            else
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Rotation decomposition was wrong!");

            p.worldMatrix = worldMat;
            drawables.Add(p);
        }

        /// <summary>
        /// Gets the equivalent effect view matrix
        /// </summary>
        /// <param name="camMatrix">SpriteBatch Camera Matrix</param>
        /// <returns>Effect View Matrix</returns>
        private Matrix GetEffectMatrix(gxtCamera camera)
        {
            // this is the "identity" transform
            Matrix mat = Matrix.CreateScale(1.0f, -1.0f, 1.0f);//Matrix.CreateLookAt(new Vector3(0, 0, -1.0f), new Vector3(0.0f, 0.0f, 0.0f), -Vector3.Up);
            Matrix cameraCopy = cameraMatrix;
            cameraCopy.Translation = new Vector3(cameraMatrix.Translation.X - (gxtDisplayManager.Singleton.ResolutionWidth * 0.5f), cameraMatrix.Translation.Y - (gxtDisplayManager.Singleton.ResolutionHeight * 0.5f), 0.0f);
            Vector3 ct = cameraMatrix.Translation;
            Matrix tmat = Matrix.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0.0f);
            Matrix desiredMatrix = cameraCopy * mat;
            //return tmat * mat;
            
            //return mat * Matrix.Invert(camera.GetTransformation()) * Matrix.CreateTranslation(translation.X, translation.Y, 0.0f);
            // TEMPORARY, HARD CODED VALUES
            //Vector2 toOrigin = -translation;
            Vector2 translation = new Vector2(cameraMatrix.Translation.X - (gxtDisplayManager.Singleton.ResolutionWidth * 0.5f), -cameraMatrix.Translation.Y + (gxtDisplayManager.Singleton.ResolutionHeight * 0.5f));
            Matrix rotMat = Matrix.CreateRotationZ(-camera.Rotation);
            Matrix scaleMat = Matrix.CreateScale(screenScale + camera.Zoom, screenScale + camera.Zoom, 1.0f);
            Matrix returnMat = mat * scaleMat * rotMat * Matrix.CreateTranslation(translation.X, translation.Y, 0.0f);

            if (desiredMatrix == returnMat)
                gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "same");
            else
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "not the same");

            return returnMat;
            //return mat * camMatrix;
        }

        private Matrix GetEffectViewMatrix()
        {
            // effect view matrix equivalent in spritebatch's coordinate system
            Matrix xnaIdentity = Matrix.CreateScale(1.0f, -1.0f, 1.0f);
            // readjust translation
            Matrix cameraCopy = cameraMatrix;
            cameraCopy.Translation = new Vector3(cameraMatrix.Translation.X - (gxtDisplayManager.Singleton.ResolutionWidth * 0.5f), cameraMatrix.Translation.Y - (gxtDisplayManager.Singleton.ResolutionHeight * 0.5f), 0.0f);
            return cameraCopy * xnaIdentity;
        }

        /// <summary>
        /// Handles scaling when resolution is changed
        /// </summary>
        /// <param name="manager">Display Manager</param>
        public void ResolutionChangedHandler(gxtDisplayManager manager)
        {
            effect.Projection = Matrix.CreateOrthographic(manager.ResolutionWidth, manager.ResolutionHeight, 0.1f, 1.5f);   // 0.1 and 1.5 could change
            screenScale = manager.ResolutionWidth / (float)manager.TargetResolutionWidth;
        }
    }
}
