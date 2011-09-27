using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    public enum gxtBatchDepthMode
    {
        FRONT_TO_BACK = 0,
        BACK_TO_FRONT = 1
    };

    public enum gxtBatchSortMode
    {
        DEFAULT = 0,
        TEXTURE = 1,
        IMMEDIATE = 2
    };

    public enum gxtBatchDrawOrder
    {
        PRIMITIVES_FIRST = 0,
        SPRITEFONTS_FIRST = 1,
        INLINE = 2
    };

    /// <summary>
    /// a replacement for gxtSpriteBatch
    /// must support sorting, shared effect
    /// will hopefully support full indexed primitives
    /// the idea is to have a system that draws the indexed primitives first, and the spritefonts on top of that
    /// would like to support split screen / multiple viewports
    /// </summary>
    public class gxtGraphicsBatch
    {

        #region VertexInfo
        private abstract class gxtVertexInfo
        {
            public float depth;
            public PrimitiveType primitiveType;
            public int primitiveCount;
            public Matrix worldMatrix;
            public Texture2D texture;

            public abstract void Draw(GraphicsDevice graphics, BasicEffect effect);
        }

        private class gxtVertexBufferInfo : gxtVertexInfo
        {
            public VertexBuffer vertexBuffer;
            public IndexBuffer indexBuffer;

            public override void Draw(GraphicsDevice graphics, BasicEffect effect)
            {
                graphics.SetVertexBuffer(vertexBuffer);
                graphics.Indices = indexBuffer;
                // would rather not set everytime here...
                effect.Texture = texture;
                effect.TextureEnabled = (texture != null);
                effect.World = worldMatrix;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawIndexedPrimitives(primitiveType, 0, 0, vertexBuffer.VertexCount, 0, primitiveCount);
                }
            }
        }

        private class gxtUserVertexInfo : gxtVertexInfo
        {
            public VertexPositionColorTexture[] vertices;
            public int[] indices;

            public override void Draw(GraphicsDevice graphics, BasicEffect effect)
            {
                effect.Texture = texture;
                effect.TextureEnabled = (texture != null);
                effect.World = worldMatrix;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives<VertexPositionColorTexture>(primitiveType, vertices, 0, vertices.Length, indices, 0, primitiveCount);
                }
            }
        }
        #endregion VertexInfo

        #region SpriteFontInfo
        /// <summary>
        /// SpriteFont batch object
        /// </summary>
        private class gxtSpriteFontInfo
        {
            public SpriteFont spriteFont;
            public string text;
            public Color color;
            public Vector2 position;
            public Vector2 scale;
            public Vector2 origin;
            public float rotation;
            public SpriteEffects spriteEffects;
            public float depth;

            public void Draw(SpriteBatch spriteBatch, BasicEffect effect)
            {
                spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, spriteEffects, depth);
            }
        }
        #endregion SpriteFontInfo

        #region GeneralInfo
        private abstract class gxtGeneralBatchObjInfo
        {
            public float depth;
            // maybe a resource getter
            public abstract void Draw(GraphicsDevice graphics, SpriteBatch spriteBatch, BasicEffect effect);
        }

        private class gxtGeneralVertexBufferInfo : gxtGeneralBatchObjInfo
        {
            public VertexBuffer vertexBuffer;
            public IndexBuffer indexBuffer;
            public PrimitiveType primitiveType;
            public int primitiveCount;
            public Matrix worldMatrix;
            public Texture2D texture;

            public override void Draw(GraphicsDevice graphics, SpriteBatch spriteBatch, BasicEffect effect)
            {
                graphics.SetVertexBuffer(vertexBuffer);
                graphics.Indices = indexBuffer;
                // would rather not set everytime here...
                effect.Texture = texture;
                effect.TextureEnabled = (texture != null);
                effect.World = worldMatrix;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawIndexedPrimitives(primitiveType, 0, 0, vertexBuffer.VertexCount, 0, primitiveCount);
                }
            }
        }

        private class gxtGeneralUserVertexInfo : gxtGeneralBatchObjInfo
        {
            public PrimitiveType primitiveType;
            public int primitiveCount;
            public Matrix worldMatrix;
            public Texture2D texture;
            public VertexPositionColorTexture[] vertices;
            public int[] indices;

            public override void Draw(GraphicsDevice graphics, SpriteBatch spriteBatch, BasicEffect effect)
            {
                effect.Texture = texture;
                effect.TextureEnabled = (texture != null);
                effect.World = worldMatrix;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives<VertexPositionColorTexture>(primitiveType, vertices, 0, vertices.Length, indices, 0, primitiveCount);                    //graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                }
            }
        }

        private class gxtGeneralSpriteFontInfo : gxtGeneralBatchObjInfo
        {
            public SpriteFont spriteFont;
            public string text;
            public Color color;
            public Vector2 position;
            public Vector2 scale;
            public Vector2 origin;
            public float rotation;
            public SpriteEffects spriteEffects;

            public override void Draw(GraphicsDevice graphics, SpriteBatch spriteBatch, BasicEffect effect)
            {
                spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, spriteEffects, depth);
            }
        }
        #endregion GeneralInfo

        private SpriteBatch spriteBatch;
        private GraphicsDevice graphics;
        private BasicEffect effect;
        private gxtBatchDepthMode depthMode;
        private gxtBatchSortMode sortMode;
        private gxtBatchDrawOrder drawOrder;
        private List<gxtVertexInfo> vertexData;
        private List<gxtSpriteFontInfo> spriteFontData;
        private List<gxtGeneralBatchObjInfo> generalData;
        private Matrix viewMatrix;
        private Matrix spriteBatchIdentity;
        private float resolutionWidth, resolutionHeight;
        private bool begun;

        /// <summary>
        /// Determines if the Graphics Batch has undergone it's one time startup 
        /// initialization.  This is not the same as Begin/End states.
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return vertexData != null && spriteFontData != null;
        }

        /// <summary>
        /// Startup initialization that should be handled once at the begininning of run time
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="registerWithDisplayManager"></param>
        public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, bool registerWithDisplayManager = true)
        {
            gxtDebug.Assert(!IsInitialized());

            this.graphics = graphicsDevice;
            this.spriteBatch = spriteBatch;
            // common states we only want to set once
            graphics.RasterizerState = RasterizerState.CullNone;
            graphics.BlendState = BlendState.NonPremultiplied;
            effect = new BasicEffect(graphicsDevice);
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
            spriteBatchIdentity = Matrix.CreateScale(1.0f, -1.0f, 1.0f);
            resolutionWidth = gxtDisplayManager.Singleton.ResolutionWidth;
            resolutionHeight = gxtDisplayManager.Singleton.ResolutionHeight;
            // notice the texel offsets
            if (registerWithDisplayManager)
            {
                if (gxtDisplayManager.SingletonIsInitialized)
                {
                    effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateOrthographic(resolutionWidth, resolutionHeight, 0.0f, 1.0f);
                    gxtDisplayManager.Singleton.resolutionChanged += OnResolutionChange;
                }
                else
                {
                    effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateOrthographic(resolutionWidth, resolutionHeight, 0.0f, 1.0f);
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot register Graphics Batch with the Display Manager!");
                }
            }
            else
            {
                effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateOrthographic(resolutionWidth, resolutionHeight, 0.0f, 1.0f);
            }

            // pick high initial capacities, resizing an array of these sizes will be very expensive
            vertexData = new List<gxtVertexInfo>(256);
            spriteFontData = new List<gxtSpriteFontInfo>(128);
            generalData = new List<gxtGeneralBatchObjInfo>(256);
            begun = false;
        }

        public void OnResolutionChange(gxtDisplayManager display)
        {
            this.resolutionWidth = display.ResolutionWidth;
            this.resolutionHeight = display.ResolutionHeight;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatchViewMatrix"></param>
        /// <param name="effectViewMatrix"></param>
        private void GetEffectMatrix(ref Matrix spriteBatchViewMatrix, out Matrix effectViewMatrix)
        {
            effectViewMatrix = spriteBatchViewMatrix;
            effectViewMatrix.Translation = new Vector3(viewMatrix.Translation.X - (resolutionWidth * 0.5f), viewMatrix.Translation.Y - (resolutionHeight * 0.5f), 0.0f);
            effectViewMatrix *= spriteBatchIdentity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawOrder"></param>
        /// <param name="sortMode"></param>
        /// <param name="depthMode"></param>
        /// <returns></returns>
        public bool Begin(gxtBatchDrawOrder drawOrder, gxtBatchSortMode sortMode, gxtBatchDepthMode depthMode)
        {
            gxtDebug.Assert(IsInitialized() || begun);
            Matrix centeredIdentity = Matrix.Identity;
            centeredIdentity.M41 = resolutionWidth * 0.5f;
            centeredIdentity.M42 = resolutionHeight * 0.5f;
            return Begin(drawOrder, sortMode, depthMode, centeredIdentity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawOrder"></param>
        /// <param name="sortMode"></param>
        /// <param name="depthMode"></param>
        /// <param name="viewMatrix"></param>
        /// <returns></returns>
        public bool Begin(gxtBatchDrawOrder drawOrder, gxtBatchSortMode sortMode, gxtBatchDepthMode depthMode, Matrix viewMatrix)
        {
            gxtDebug.Assert(IsInitialized() || begun);
            if (!begun)
            {
                this.viewMatrix = viewMatrix;
                Matrix effectView;
                GetEffectMatrix(ref viewMatrix, out effectView);
                effect.View = effectView;
                this.drawOrder = drawOrder;
                this.sortMode = sortMode;
                this.depthMode = depthMode;
                // it appears the spritebatch should always draw in immediate mode
                SpriteSortMode spriteFontSortMode;
                if (sortMode == gxtBatchSortMode.IMMEDIATE)
                    spriteFontSortMode = SpriteSortMode.Immediate;
                else if (depthMode == gxtBatchDepthMode.FRONT_TO_BACK)
                    spriteFontSortMode = SpriteSortMode.FrontToBack;
                else
                    spriteFontSortMode = SpriteSortMode.BackToFront;
                spriteBatch.Begin(spriteFontSortMode, BlendState.NonPremultiplied, SamplerState.AnisotropicWrap, DepthStencilState.None, RasterizerState.CullNone, null, viewMatrix);
                vertexData.Clear();
                spriteFontData.Clear();
                generalData.Clear();
                begun = true;
                return true;
            }
            return false;
        }

        public void Begin(gxtBatchDrawOrder drawOrder, gxtBatchSortMode sortMode, gxtBatchDepthMode depthMode, Matrix[] viewMatrices, Viewport[] viewports)
        {
            gxtDebug.Assert(false, "This begin method has not been implemented yet!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="indexBuffer"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawIndexedTriangleList(Texture2D texture, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, ref Matrix worldMatrix, float layerDepth)
        {
            if (begun)
            {
                if (sortMode == gxtBatchSortMode.IMMEDIATE)
                {

                    if (effect.Texture == null)
                    {
                        if (effect.TextureEnabled)
                            effect.TextureEnabled = false;
                    }
                    else if (effect.Texture != texture)
                    {
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                    }
                    effect.World = worldMatrix;
                    graphics.SetVertexBuffer(vertexBuffer);
                    graphics.Indices = indexBuffer;
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                    }
                }
                else
                {
                    if (drawOrder != gxtBatchDrawOrder.INLINE)
                    {
                        gxtVertexBufferInfo vbi = new gxtVertexBufferInfo();
                        vbi.primitiveType = PrimitiveType.TriangleList;
                        vbi.vertexBuffer = vertexBuffer;
                        vbi.indexBuffer = indexBuffer;
                        vbi.texture = texture;
                        vbi.primitiveCount = indexBuffer.IndexCount / 3;
                        vbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        vbi.worldMatrix = worldMatrix;
                        vertexData.Add(vbi);
                    }
                    else
                    {
                        gxtGeneralVertexBufferInfo gvbi = new gxtGeneralVertexBufferInfo();
                        gvbi.primitiveType = PrimitiveType.TriangleList;
                        gvbi.vertexBuffer = vertexBuffer;
                        gvbi.indexBuffer = indexBuffer;
                        gvbi.texture = texture;
                        gvbi.primitiveCount = indexBuffer.IndexCount / 3;
                        gvbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        gvbi.worldMatrix = worldMatrix;
                        generalData.Add(gvbi);
                    }
                }
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="indexBuffer"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawIndexedLineList(Texture2D texture, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, ref Matrix worldMatrix, float layerDepth)
        {
            if (begun)
            {
                if (sortMode == gxtBatchSortMode.IMMEDIATE)
                {
                    if (effect.Texture == null)
                    {
                        if (effect.TextureEnabled)
                            effect.TextureEnabled = false;
                    }
                    else if (effect.Texture != texture)
                    {
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                    }
                    effect.World = worldMatrix;
                    graphics.SetVertexBuffer(vertexBuffer);
                    graphics.Indices = indexBuffer;
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 2);
                    }
                }
                else
                {
                    if (drawOrder != gxtBatchDrawOrder.INLINE)
                    {
                        gxtVertexBufferInfo vbi = new gxtVertexBufferInfo();
                        vbi.primitiveType = PrimitiveType.LineList;
                        vbi.vertexBuffer = vertexBuffer;
                        vbi.indexBuffer = indexBuffer;
                        vbi.texture = texture;
                        vbi.primitiveCount = indexBuffer.IndexCount / 2;
                        vbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        vbi.worldMatrix = worldMatrix;
                        vertexData.Add(vbi);
                    }
                    else
                    {
                        gxtGeneralVertexBufferInfo gvbi = new gxtGeneralVertexBufferInfo();
                        gvbi.primitiveType = PrimitiveType.LineList;
                        gvbi.vertexBuffer = vertexBuffer;
                        gvbi.indexBuffer = indexBuffer;
                        gvbi.texture = texture;
                        gvbi.primitiveCount = indexBuffer.IndexCount / 2;
                        gvbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        gvbi.worldMatrix = worldMatrix;
                        generalData.Add(gvbi);
                    }
                }
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="indexBuffer"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawIndexedLineStrip(Texture2D texture, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, ref Matrix worldMatrix, float layerDepth)
        {
            if (begun)
            {
                if (sortMode == gxtBatchSortMode.IMMEDIATE)
                {
                    effect.World = worldMatrix;
                    if (effect.Texture == null)
                    {
                        if (effect.TextureEnabled)
                            effect.TextureEnabled = false;
                    }
                    else if (effect.Texture != texture)
                    {
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                    }

                    graphics.SetVertexBuffer(vertexBuffer);
                    graphics.Indices = indexBuffer;
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount - 1);
                    }
                }
                else
                {
                    if (drawOrder != gxtBatchDrawOrder.INLINE)
                    {
                        gxtVertexBufferInfo vbi = new gxtVertexBufferInfo();
                        vbi.primitiveType = PrimitiveType.LineStrip;
                        vbi.vertexBuffer = vertexBuffer;
                        vbi.indexBuffer = indexBuffer;
                        vbi.texture = texture;
                        vbi.primitiveCount = indexBuffer.IndexCount - 1;
                        vbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        vbi.worldMatrix = worldMatrix;
                        vertexData.Add(vbi);
                    }
                    else
                    {
                        gxtGeneralVertexBufferInfo gvbi = new gxtGeneralVertexBufferInfo();
                        gvbi.primitiveType = PrimitiveType.LineStrip;
                        gvbi.vertexBuffer = vertexBuffer;
                        gvbi.indexBuffer = indexBuffer;
                        gvbi.texture = texture;
                        gvbi.primitiveCount = indexBuffer.IndexCount - 1;
                        gvbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        gvbi.worldMatrix = worldMatrix;
                        generalData.Add(gvbi);
                    }
                }
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="texture"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="indexBuffer"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawIndexedPrimitives(PrimitiveType type, Texture2D texture, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int primitiveCount, ref Matrix worldMatrix, float layerDepth)
        {
            if (begun)
            {
                if (sortMode == gxtBatchSortMode.IMMEDIATE)
                {
                    if (effect.Texture == null)
                    {
                        if (effect.TextureEnabled)
                            effect.TextureEnabled = false;
                    }
                    else if (effect.Texture != texture)
                    {
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                    }
                    effect.World = worldMatrix;
                    graphics.SetVertexBuffer(vertexBuffer);
                    graphics.Indices = indexBuffer;
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawIndexedPrimitives(type, 0, 0, vertexBuffer.VertexCount, 0, primitiveCount);
                    }
                }
                else
                {
                    if (drawOrder != gxtBatchDrawOrder.INLINE)
                    {
                        gxtVertexBufferInfo vbi = new gxtVertexBufferInfo();
                        vbi.primitiveType = type;
                        vbi.vertexBuffer = vertexBuffer;
                        vbi.indexBuffer = indexBuffer;
                        vbi.texture = texture;
                        vbi.primitiveCount = primitiveCount;
                        vbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        vbi.worldMatrix = worldMatrix;
                        vertexData.Add(vbi);
                    }
                    else
                    {
                        gxtGeneralVertexBufferInfo gvbi = new gxtGeneralVertexBufferInfo();
                        gvbi.primitiveType = type;
                        gvbi.vertexBuffer = vertexBuffer;
                        gvbi.indexBuffer = indexBuffer;
                        gvbi.texture = texture;
                        gvbi.primitiveCount = primitiveCount;
                        gvbi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        gvbi.worldMatrix = worldMatrix;
                        generalData.Add(gvbi);
                    }
                }
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="texture"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="indexBuffer"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="spriteEffects"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawIndexedPrimitives(PrimitiveType type, Texture2D texture, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int primitiveCount, ref Vector2 position, ref Vector2 scale, float rotation, SpriteEffects spriteEffects, float layerDepth)
        {
            if (begun)
            {
                Matrix worldMat;
                if (spriteEffects == SpriteEffects.FlipHorizontally)
                    worldMat = Matrix.CreateScale(-1.0f, 1.0f, 1.0f);
                else if (spriteEffects == SpriteEffects.FlipVertically)
                    worldMat = Matrix.CreateScale(1.0f, -1.0f, 1.0f);
                else
                    worldMat = Matrix.Identity;
                worldMat *= Matrix.CreateScale(scale.X, scale.Y, 1.0f);
                worldMat *= Matrix.CreateRotationZ(rotation);
                worldMat *= Matrix.CreateTranslation(position.X, position.Y, 0.0f);
                return DrawIndexedPrimitives(type, texture, vertexBuffer, indexBuffer, primitiveCount, ref worldMat, layerDepth);
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="texture"></param>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawUserIndexedPrimitives(PrimitiveType type, Texture2D texture, VertexPositionColorTexture[] vertices, int[] indices, int primitiveCount, ref Matrix worldMatrix, float layerDepth)
        {
            if (begun)
            {
                if (sortMode == gxtBatchSortMode.IMMEDIATE)
                {
                    if (effect.Texture == null)
                    {
                        if (effect.TextureEnabled)
                            effect.TextureEnabled = false;
                    }
                    else if (effect.Texture != texture)
                    {
                        effect.Texture = texture;
                        effect.TextureEnabled = true;
                    }
                    effect.World = worldMatrix;
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawUserIndexedPrimitives<VertexPositionColorTexture>(type, vertices, 0, vertices.Length, indices, 0, primitiveCount);
                    }
                }
                else
                {
                    if (drawOrder != gxtBatchDrawOrder.INLINE)
                    {
                        gxtUserVertexInfo uvi = new gxtUserVertexInfo();
                        uvi.primitiveType = type;
                        uvi.vertices = vertices;
                        uvi.indices = indices;
                        uvi.texture = texture;
                        uvi.primitiveCount = primitiveCount;
                        uvi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        uvi.worldMatrix = worldMatrix;
                        vertexData.Add(uvi);
                    }
                    else
                    {
                        gxtGeneralUserVertexInfo guvi = new gxtGeneralUserVertexInfo();
                        guvi.primitiveType = type;
                        guvi.vertices = vertices;
                        guvi.indices = indices;
                        guvi.texture = texture;
                        guvi.primitiveCount = primitiveCount;
                        guvi.depth = gxtMath.Clamp(layerDepth, 0.0f, 1.0f);
                        guvi.worldMatrix = worldMatrix;
                        generalData.Add(guvi);
                    }
                }
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="texture"></param>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="primitiveCount"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="spriteEffects"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawUserIndexedPrimitives(PrimitiveType type, Texture2D texture, VertexPositionColorTexture[] vertices, int[] indices, int primitiveCount, ref Vector2 position, ref Vector2 scale, float rotation, SpriteEffects spriteEffects, float layerDepth)
        {
            if (begun)
            {
                Matrix worldMat;
                if (spriteEffects == SpriteEffects.FlipHorizontally)
                    worldMat = Matrix.CreateScale(-1.0f, 1.0f, 1.0f);
                else if (spriteEffects == SpriteEffects.FlipVertically)
                    worldMat = Matrix.CreateScale(1.0f, -1.0f, 1.0f);
                else
                    worldMat = Matrix.Identity;
                worldMat *= Matrix.CreateScale(scale.X, scale.Y, 1.0f);
                worldMat *= Matrix.CreateRotationZ(rotation);
                worldMat *= Matrix.CreateTranslation(position.X, position.Y, 0.0f);
                return DrawUserIndexedPrimitives(type, texture, vertices, indices, primitiveCount, ref worldMat, layerDepth);
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="spriteEffects"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawString(SpriteFont spriteFont, string text, Color color, ref Vector2 position, ref Vector2 scale, float rotation, ref Vector2 origin, SpriteEffects spriteEffects, float layerDepth)
        {
            if (begun)
            {
                if (sortMode == gxtBatchSortMode.IMMEDIATE)
                {
                    spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, spriteEffects, layerDepth);
                }
                else
                {
                    if (drawOrder != gxtBatchDrawOrder.INLINE)
                    {
                        gxtSpriteFontInfo sfi = new gxtSpriteFontInfo();
                        sfi.depth = layerDepth;
                        sfi.color = color;
                        sfi.position = position;
                        sfi.rotation = rotation;
                        sfi.scale = scale;
                        sfi.origin = origin;
                        sfi.spriteEffects = spriteEffects;
                        sfi.text = text;
                        sfi.spriteFont = spriteFont;
                        spriteFontData.Add(sfi);
                        // add buffer object
                    }
                    else
                    {
                        gxtGeneralSpriteFontInfo gsfi = new gxtGeneralSpriteFontInfo();
                        gsfi.color = color;
                        gsfi.depth = layerDepth;
                        gsfi.origin = origin;
                        gsfi.position = position;
                        gsfi.rotation = rotation;
                        gsfi.scale = scale;
                        gsfi.spriteEffects = spriteEffects;
                        gsfi.spriteFont = spriteFont;
                        gsfi.text = text;
                        generalData.Add(gsfi);
                    }
                }
            }
            return begun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="origin"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="layerDepth"></param>
        /// <returns></returns>
        public bool DrawString(SpriteFont spriteFont, string text, Color color, ref Vector2 origin, ref Matrix worldMatrix, float layerDepth)
        {
            if (begun)
            {
                Vector2 position, scale;
                float rotation;
                SpriteEffects spriteEffects = SpriteEffects.None;
                gxtMath.Decompose2D(ref worldMatrix, out position, out scale, out rotation);
                if (scale.X < 0.0f)
                {
                    if (scale.Y < 0.0f)
                    {
                        rotation += gxtMath.PI;
                        scale.Y = -scale.Y;
                    }
                    else
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                }
                else if (scale.Y < 0.0f)
                {
                    spriteEffects = SpriteEffects.FlipVertically;
                    scale.Y = -scale.Y;
                }
                return DrawString(spriteFont, text, color, ref position, ref scale, rotation, ref origin, spriteEffects, layerDepth);
            }
            return begun;
        }

        // depth functions all assume front to back for now
        private int CompareDepth(gxtVertexInfo x, gxtVertexInfo y)
        {
            if (depthMode == gxtBatchDepthMode.FRONT_TO_BACK)
            {
                if (x.depth < y.depth)
                    return 1;
                if (x.depth > y.depth)
                    return -1;
                return 0;
            }
            else
            {
                if (x.depth < y.depth)
                    return -1;
                if (x.depth > y.depth)
                    return 1;
                return 0;
            }
        }

        private int CompareDepth(gxtSpriteFontInfo x, gxtSpriteFontInfo y)
        {
            if (depthMode == gxtBatchDepthMode.FRONT_TO_BACK)
            {
                if (x.depth < y.depth)
                    return 1;
                if (x.depth > y.depth)
                    return -1;
                return 0;
            }
            else
            {
                if (x.depth < y.depth)
                    return -1;
                if (x.depth > y.depth)
                    return 1;
                return 0;
            }
        }

        private int CompareDepth(gxtGeneralBatchObjInfo x, gxtGeneralBatchObjInfo y)
        {
            if (depthMode == gxtBatchDepthMode.FRONT_TO_BACK)
            {
                if (x.depth < y.depth)
                    return 1;
                if (x.depth > y.depth)
                    return -1;
                return 0;
            }
            else
            {
                if (x.depth < y.depth)
                    return -1;
                if (x.depth > y.depth)
                    return 1;
                return 0;
            }
        }

        private int CompareDepthAndTexture(gxtVertexInfo x, gxtVertexInfo y)
        {
            if (depthMode == gxtBatchDepthMode.FRONT_TO_BACK)
            {
                if (x.depth < y.depth)
                    return 1;
                if (x.depth > y.depth)
                    return -1;
                if (x.texture == y.texture)
                    return 0;
                return -1;
            }
            else
            {
                if (x.depth < y.depth)
                    return -1;
                if (x.depth > y.depth)
                    return 1;
                if (x.texture == y.texture)
                    return 0;
                return 1;
            }
        }

        private int CompareDepthAndFont(gxtSpriteFontInfo x, gxtSpriteFontInfo y)
        {
            if (depthMode == gxtBatchDepthMode.FRONT_TO_BACK)
            {
                if (x.depth < y.depth)
                    return -1;
                if (x.depth > y.depth)
                    return 1;
                if (x.spriteFont == y.spriteFont)
                    return 0;
                return -1;
            }
            else
            {
                if (x.depth < y.depth)
                    return 1;
                if (x.depth > y.depth)
                    return -1;
                if (x.spriteFont == y.spriteFont)
                    return 0;
                return -1;
            }
        }

        public void End()
        {
            gxtDebug.Assert(begun);
            if (sortMode != gxtBatchSortMode.IMMEDIATE)
            {
                if (sortMode == gxtBatchSortMode.DEFAULT)
                {
                    vertexData.Sort(CompareDepth);
                    spriteFontData.Sort(CompareDepth);  // may not need to sort, spritebatch could do it for us
                    generalData.Sort(CompareDepth);
                    // sharedBuffer sorting
                }
                else if (sortMode == gxtBatchSortMode.TEXTURE)
                {
                    vertexData.Sort(CompareDepthAndTexture);
                    spriteFontData.Sort(CompareDepthAndFont);
                    generalData.Sort(CompareDepth);
                    // sharedBuffer sorting
                }

                // 
                if (drawOrder == gxtBatchDrawOrder.PRIMITIVES_FIRST)
                {
                    if (sortMode == gxtBatchSortMode.TEXTURE)
                    {
                        // only change the effect texture here if it must be done
                        for (int i = 0; i < vertexData.Count; ++i)
                        {
                            vertexData[i].Draw(graphics, effect);
                        }
                        for (int i = 0; i < spriteFontData.Count; ++i)
                        {
                            spriteFontData[i].Draw(spriteBatch, effect);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < vertexData.Count; ++i)
                        {
                            vertexData[i].Draw(graphics, effect);
                        }
                        for (int i = 0; i < spriteFontData.Count; ++i)
                        {
                            spriteFontData[i].Draw(spriteBatch, effect);
                        }
                    }
                }
                else if (drawOrder == gxtBatchDrawOrder.SPRITEFONTS_FIRST)
                {
                    if (sortMode == gxtBatchSortMode.TEXTURE)
                    {
                        for (int i = 0; i < spriteFontData.Count; ++i)
                        {
                            spriteFontData[i].Draw(spriteBatch, effect);
                        }
                        for (int i = 0; i < vertexData.Count; ++i)
                        {
                            vertexData[i].Draw(graphics, effect);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < spriteFontData.Count; ++i)
                        {
                            spriteFontData[i].Draw(spriteBatch, effect);
                        }
                        for (int i = 0; i < vertexData.Count; ++i)
                        {
                            vertexData[i].Draw(graphics, effect);
                        }

                    }
                }
                else
                {
                    for (int i = 0; i < generalData.Count; ++i)
                    {
                        generalData[i].Draw(graphics, spriteBatch, effect);
                    }
                }
            }

            if (begun)
                spriteBatch.End();
            begun = false;
        }
    }
}
