using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// Transform Space
    /// </summary>
    public enum gxtTransformSpace
    {
        LOCAL = 0,
        PARENT = 1,
        WORLD = 2
    };

    /// <summary>
    /// Basic interface for scene nodes (aka transforms) used to compose a 
    /// scene graph of renderable objects
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public interface gxtISceneNode
    {
        bool IsRoot { get; }
        gxtISceneNode Parent { get; set; }

        Vector2 Position { get; set; }
        float Rotation { get; set; }
        Vector2 Scale { get; set; }

        int NumChildrenNodes { get; }
        int NumDescendantNodes { get; }

        int NumDrawables { get; }
        int NumDescendantDrawables { get; }

        IEnumerator<gxtISceneNode> GetChildEnumerator();
        IEnumerator<gxtIDrawable> GetDrawableEnumerator();

        void AddChild(gxtISceneNode node);
        bool ContainsChild(gxtISceneNode node, bool deepSearch = false);
        bool RemoveChild(gxtISceneNode node);
        bool RemoveAndDisposeChild(gxtISceneNode node);
        void RemoveAllChildren(bool cascade = false);
        void RemoveAndDisposeAllChildren(bool cascade = false);

        void AttachDrawable(gxtIDrawable drawable);
        bool ContainsDrawable(gxtIDrawable drawable, bool deepSearch = false);
        bool DetachDrawable(gxtIDrawable drawable);
        void DetachAllDrawables();

        void Update();
        void QueueForUpdate();
        void MarkAsDirty();
        void Draw(gxtGraphicsBatch graphicsBatch);

        void Translate(Vector2 t, gxtTransformSpace space = gxtTransformSpace.LOCAL);
        void Translate(float tx, float ty, gxtTransformSpace space = gxtTransformSpace.LOCAL);
        void Rotate(float rot, gxtTransformSpace space = gxtTransformSpace.LOCAL);        
        void ScaleAxes(float s);
        void ScaleAxes(float sx, float sy);
        void ScaleAxes(Vector2 s);

        // have other set methods which can take values by reference?
        void SetAll(ref Vector2 position, float rotation, ref Vector2 scale);

        void SetColor(Color color, bool cascade = true);
        void BlendColor(Color color, float t, bool cascade = true);
        void SetVisibility(bool visible, bool cascade = true);
        void FlipVisibility(bool cascade = true);
        void FlipHorizontal();
        void FlipVertical();

        Vector2 GetDerivedPosition();
        float GetDerivedRotation();
        Vector2 GetDerivedScale();
        Matrix GetDerivedTransform();

        void SetDerivedPosition(Vector2 worldPosition);
        void SetDerivedRotation(float worldRotation);
        void SetDerivedScale(Vector2 worldScale);

        gxtAABB GetAABB();
        string DebugTrace(bool cascade = false, int parentDepth = 0);
    }
}
