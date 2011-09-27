using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// A node class for scene graph transform hierarchies
    /// Attach drawables to scene nodes to render them in your scene
    /// Supports operations for adding/removing children and attaching multiple drawables
    /// Can transform position, rotation, and scale with reasonably efficient updates
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSceneNode : gxtISceneNode
    {
        #region Fields
        // parent/children/drawables
        private gxtISceneNode parent;
        private List<gxtISceneNode> children;
        private List<gxtIDrawable> drawables;

        // transform properties relative to the parent
        private Vector2 relativePosition;
        private float relativeRotation;
        private Vector2 relativeScale;
        
        // fully derived values and dirty flag for efficient updating
        private bool dirty;
        private Vector2 derivedPosition;
        private float derivedRotation;
        private Vector2 derivedScale;
        private Matrix derivedTransform;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Reference to the parent node
        /// </summary>
        public virtual gxtISceneNode Parent 
        { 
            get { return parent; }
            set { 
                gxtDebug.Assert(!this.Equals(value), "You cannot parent a node to itself!"); 
                parent = value;
            }
        }

        /// <summary>
        /// Is the root of the tree?
        /// </summary>
        public virtual bool IsRoot { get { return parent == null; } }

        /// <summary>
        /// Position relative to the parent node
        /// </summary>
        public virtual Vector2 Position { get { return relativePosition; } set { relativePosition = value; QueueForUpdate(); } }

        /// <summary>
        /// Rotation relative to the parent node
        /// </summary>
        public virtual float Rotation { get { return relativeRotation; } set { relativeRotation = value; QueueForUpdate(); } }

        /// <summary>
        /// Scale relative to the parent node
        /// </summary>
        public virtual Vector2 Scale { get { return relativeScale; } set { relativeScale = value; QueueForUpdate(); } }

        /// <summary>
        /// Number of immediate descendant children nodes
        /// </summary>
        public virtual int NumChildrenNodes { get { return children.Count; } }

        /// <summary>
        /// Total number of descendant children nodes
        /// </summary>
        public virtual int NumDescendantNodes
        {
            get
            {
                int count = NumChildrenNodes;
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    count += children[i].NumDescendantNodes;
                }
                return count;
            }
        }

        /// <summary>
        /// Number of attached drawables
        /// </summary>
        public virtual int NumDrawables { get { return drawables.Count; } }

        /// <summary>
        /// Total number of descendant drawables
        /// </summary>
        public virtual int NumDescendantDrawables
        {
            get
            {
                int count = NumDrawables;
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    count += children[i].NumDescendantDrawables;
                }
                return count;
            }
        }
        #endregion Properties

        /// <summary>
        /// Constructor, sets up a node with the defaults
        /// </summary>
        public gxtSceneNode()
        {
            children = new List<gxtISceneNode>();
            drawables = new List<gxtIDrawable>();
            relativePosition = Vector2.Zero;
            relativeRotation = 0.0f;
            relativeScale = Vector2.One;
            derivedPosition = Vector2.Zero;
            derivedRotation = 0.0f;
            derivedScale = Vector2.One;
            derivedTransform = Matrix.Identity;
            dirty = false;
        }

        #region Add/Contains/Remove
        /// <summary>
        /// Adds a child node
        /// </summary>
        /// <param name="node">Node</param>
        public virtual void AddChild(gxtISceneNode node)
        {
            gxtDebug.Assert(node != null, "Can't add a null reference as a child node!");
            gxtDebug.Assert(!this.Equals(node), "Can't add a node as a child of itself");
            
            // changing parents without detaching is allowed, but not reccomended
            if (node.Parent != null)
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Scene Node With Non Null Parent Is Being Changed");
            children.Add(node);
            node.Parent = this;
            node.QueueForUpdate();
        }

        /// <summary>
        /// Contains search, which can optionally be run on all descendent 
        /// nodes.  
        /// </summary>
        /// <param name="node">Node to search for</param>
        /// <param name="deepSearch">If you want to run the search on all descendants</param>
        /// <returns>If found</returns>
        public virtual bool ContainsChild(gxtISceneNode node, bool deepSearch = false)
        {
            if (deepSearch)
            {
                for (int i = 0; i < NumChildrenNodes; ++i)
                {
                    if (children[i].Equals(node))
                        return true;
                    if (children[i].ContainsChild(node, true))
                        return true;
                }
            }
            else if (node != null && node.Parent != null && node.Parent.Equals(this))
            {
                gxtDebug.Assert(children.Contains(node), "Node is not a child to it's parent reference!");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the child node and unlinks parent reference
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>True if removed, false if not</returns>
        public virtual bool RemoveChild(gxtISceneNode node)
        {
            for (int i = NumChildrenNodes - 1; i >= 0; --i)
            {
                if (children[i].Equals(node))
                {
                    children.RemoveAt(i);
                    node.Parent = null;
                    // mark as clean?  should we have an in graph variable?
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the child and explicitly disposes of all 
        /// of its attached drawable content.  Does not dispose of the 
        /// node itself!
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>True if removed, false if not</returns>
        public virtual bool RemoveAndDisposeChild(gxtISceneNode node)
        {
            for (int i = NumChildrenNodes - 1; i >= 0; --i)
            {
                if (children[i].Equals(node))
                {
                    children.RemoveAt(i);
                    node.Parent = null;
                    gxtIDrawable drawable;
                    for (int j = node.NumDrawables - 1; j >= 0; --j)
                    {
                        drawable = drawables[j];
                        drawables.RemoveAt(j);
                        drawable.Dispose();
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes all children nodes.  Cascade option can unlink the entire subtree if desired
        /// </summary>
        /// <param name="cascade">If cascade, the subtree is also unlinked</param>
        public virtual void RemoveAllChildren(bool cascade = false)
        {
            if (cascade)
            {
                for (int i = NumChildrenNodes - 1; i >= 0; --i)
                {
                    gxtISceneNode node = children[i];
                    node.Parent = null;
                    children.RemoveAt(i);
                    node.RemoveAllChildren();
                }
            }
            else
            {
                for (int i = NumChildrenNodes - 1; i >= 0; --i)
                {
                    gxtISceneNode node = children[i];
                    node.Parent = null;
                    children.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes and explicitly disposes of all attached 
        /// drawable content
        /// </summary>
        /// <param name="cascade">If cascade, the subtree is also unlinked and disposed</param>
        public virtual void RemoveAndDisposeAllChildren(bool cascade = false)
        {
            if (cascade)
            {
                for (int i = NumChildrenNodes - 1; i >= 0; --i)
                {
                    gxtISceneNode node = children[i];
                    node.Parent = null;
                    children.RemoveAt(i);
                    node.RemoveAndDisposeAllChildren(true);

                    gxtIDrawable drawable;
                    for (int j = node.NumDrawables - 1; j >= 0; --j)
                    {
                        drawable = drawables[j];
                        drawables.RemoveAt(j);
                        drawable.Dispose();
                    }
                }
            }
            else
            {
                for (int i = NumChildrenNodes - 1; i >= 0; --i)
                {
                    gxtISceneNode node = children[i];
                    node.Parent = null;
                    children.RemoveAt(i);

                    gxtIDrawable drawable;
                    for (int j = node.NumDrawables - 1; j >= 0; --j)
                    {
                        drawable = drawables[j];
                        drawables.RemoveAt(j);
                        drawable.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the enumerator for the child nodes
        /// </summary>
        /// <returns>Child Enumerator</returns>
        public virtual IEnumerator<gxtISceneNode> GetChildEnumerator()
        {
            return children.GetEnumerator();
        }

        /// <summary>
        /// Attaches a drawable object to this node
        /// </summary>
        /// <param name="drawable">Drawable</param>
        public virtual void AttachDrawable(gxtIDrawable drawable)
        {
            gxtDebug.Assert(!drawables.Contains(drawable), "You cannot attach the same drawable to the same node more than once!");
            if (drawable != null)
                drawables.Add(drawable);
        }

        /// <summary>
        /// Returns a boolean indicating if the drawable is attached to this node
        /// If deep search is used, it only guarantees it is attached to a node in the subtree
        /// </summary>
        /// <param name="drawable">Drawable</param>
        /// <param name="deepSearch">Optionally search all children</param>
        /// <returns>If attached</returns>
        public virtual bool ContainsDrawable(gxtIDrawable drawable, bool deepSearch = false)
        {
            for (int i = 0; i < NumDrawables; i++)
            {
                if (drawables[i].Equals(drawable))
                    return true;
            }

            if (deepSearch)
            {
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    if (children[i].ContainsDrawable(drawable, true))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Detaches a drawable object from this node
        /// </summary>
        /// <param name="drawable">Drawable</param>
        /// <returns>If found and detached</returns>
        public virtual bool DetachDrawable(gxtIDrawable drawable)
        {
            for (int i = NumDrawables - 1; i >= 0; --i)
            {
                if (drawables[i].Equals(drawable))
                {
                    drawables.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Detaches all drawable objects from this node
        /// </summary>
        public virtual void DetachAllDrawables()
        {
            for (int i = NumDrawables - 1; i >= 0; --i)
            {
                drawables.RemoveAt(i);
            }
        }

        /// <summary>
        /// Gets the enumerator for the child nodes
        /// </summary>
        /// <returns>Drawable Enumerator</returns>
        public virtual IEnumerator<gxtIDrawable> GetDrawableEnumerator()
        {
            return drawables.GetEnumerator();
        }
        #endregion Add/Contains/Remove

        #region Update/Draw
        /// <summary>
        /// Updates the derived values of the node and all children nodes
        /// Also resets the dirty flag to false after updating
        /// </summary>
        public virtual void Update()
        {
            if (parent != null)
            {
                // get derived parent values
                Vector2 parentPos = parent.GetDerivedPosition();
                float parentRot = parent.GetDerivedRotation();
                Vector2 parentScale = parent.GetDerivedScale();

                float cos = gxtMath.Cos(parentRot);
                float sin = gxtMath.Sin(parentRot);

                Vector2 xAxis = new Vector2(cos, sin);
                Vector2 yAxis = new Vector2(-sin, cos);
                Vector2 offset = xAxis * (relativePosition.X * parentScale.X) + yAxis * (relativePosition.Y * parentScale.Y);

                // update the derived values
                derivedPosition = parentPos + offset;
                derivedRotation = parentRot + relativeRotation;
                derivedScale.X = relativeScale.X * parentScale.X;
                derivedScale.Y = relativeScale.Y * parentScale.Y;
            }
            else
            {
                // if it's the root, the relative values are the derived values
                derivedPosition = relativePosition;
                derivedRotation = relativeRotation;
                derivedScale = relativeScale;
            }

            // update the derived transform matrix
            derivedTransform = Matrix.CreateScale(derivedScale.X, derivedScale.Y, 1.0f) *
                               Matrix.CreateRotationZ(derivedRotation) *
                               Matrix.CreateTranslation(derivedPosition.X, derivedPosition.Y, 0.0f);

            // if we ever implemement culling the merged aabb would need to be calculated here

            // set as clean
            dirty = false;

            for (int i = 0; i < NumChildrenNodes; ++i)
            {
                children[i].Update();
            }
        }

        /// <summary>
        /// Queues the node for an update and sets its dirty flag to true
        /// </summary>
        public virtual void QueueForUpdate()
        {
            if (!dirty)
                if (gxtSceneGraph.AddNodeToUpdateQueue(this))
                    MarkAsDirty();
        }

        /// <summary>
        /// Sets flag on node inidcating it's derived values are out of date for this 
        /// node and all of its desecendant nodes
        /// </summary>
        public virtual void MarkAsDirty()
        {
            dirty = true;
            for (int i = 0; i < NumChildrenNodes; ++i)
            {
                children[i].MarkAsDirty();
            }
        }

        /// <summary>
        /// New draw method, designed to be used in conjunction with more efficient updates
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public virtual void Draw(gxtGraphicsBatch graphicsBatch)
        {
            // world matrices are faster for everything except text, which has to 
            // decompose it to play nicely with spritebatch
            for (int i = 0; i < NumDrawables; ++i)
            {
                drawables[i].Draw(graphicsBatch, ref derivedTransform);
            }

            // do the same for all the children
            for (int i = 0; i < NumChildrenNodes; ++i)
            {
                children[i].Draw(graphicsBatch);
            }
        }

        /*
        /// <summary>
        /// Draws the node given the passed in properties of the parent
        /// This function will make draw calls and pass inherited values down to 
        /// its children
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="parentVisibility">Derived Visibility of the Parent</param>
        /// <param name="parentPos">Derived Position of the Parent</param>
        /// <param name="parentRot">Derived Rotation of the Parent</param>
        /// <param name="parentScale">Derived Scale of the Parent</param>
        public void DrawNode(gxtSpriteBatch spriteBatch, Vector2 parentPos, float parentRot, Vector2 parentScale)
        {
            if (parent != null)
            {
                Vector2 pos = parentPos;
                float rot = parentRot;
                Vector2 scale = Vector2.Multiply(Scale, parentScale);

                float cos = gxtMath.Cos(rot);
                float sin = gxtMath.Sin(rot);

                Vector2 xAxis = new Vector2(cos, sin);
                Vector2 yAxis = new Vector2(-sin, cos);
                Vector2 offset = xAxis * (Position.X * parentScale.X) + yAxis * (Position.Y * parentScale.Y);
                
                pos += offset;
                rot += Rotation;

                if (NumDrawables != 0)
                {
                    float drawRot = rot;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Vector2 drawScale = scale;
                    if (drawScale.X < 0.0f)
                    {
                        if (drawScale.Y < 0.0f)
                        {
                            drawRot += gxtMath.PI;
                            drawScale.Y = -drawScale.Y;
                        }
                        else
                        {
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        drawScale.X = -drawScale.X;
                    }
                    else if (drawScale.Y < 0.0f)
                    {
                        spriteEffects = SpriteEffects.FlipVertically;
                        drawScale.Y = -drawScale.Y;
                    }

                    for (int i = 0; i < NumDrawables; i++)
                    {
                        drawables[i].Draw(spriteBatch, pos, drawRot, drawScale, spriteEffects);
                    }
                }
                
                // draw every drawable with spritebatch friendly values
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    children[i].DrawNode(spriteBatch, pos, rot, scale);
                }
            }
            else
            {
                // otherwise it is the root node and we draw all 
                // the descendants relative to the passed in values
                Position = parentPos;
                Rotation = parentRot;
                Scale = parentScale;

                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    children[i].DrawNode(spriteBatch, Position, Rotation, Scale);
                }
                
            }
        }
        */
        #endregion Update/Draw

        /// <summary>
        /// Translates the node by the given vector in the desired space
        /// </summary>
        /// <param name="t">Translation Vector</param>
        /// <param name="space">Transform Space</param>
        public virtual void Translate(Vector2 t, gxtTransformSpace space = gxtTransformSpace.LOCAL)
        {
            if (space == gxtTransformSpace.LOCAL)
            {
                // project onto the local axes
                float cos = gxtMath.Cos(relativeRotation);
                float sin = gxtMath.Sin(relativeRotation);
                Vector2 xAxis = new Vector2(cos, sin);
                Vector2 yAxis = new Vector2(-sin, cos);
                relativePosition += (xAxis * t.X) + (yAxis * t.Y);
                QueueForUpdate();
            }
            else if (space == gxtTransformSpace.PARENT)
            {
                relativePosition += t;
                QueueForUpdate();
            }
            else  // world space
            {
                if (parent != null)
                {
                    // negative rotation == inverse in this case
                    Matrix pInvRotation = Matrix.CreateRotationZ(-parent.GetDerivedRotation());
                    Vector2 parentScale = parent.GetDerivedScale();
                    Vector2 wt = Vector2.Transform(t, pInvRotation);
                    relativePosition += new Vector2(wt.X / parentScale.X, wt.Y / parentScale.Y);
                    QueueForUpdate();
                }
                else
                {
                    relativePosition += t;
                    QueueForUpdate();
                }
            }
        }

        /// <summary>
        /// Translates the node by the given values in the desired space
        /// </summary>
        /// <param name="tx">tx</param>
        /// <param name="ty">ty</param>
        public virtual void Translate(float tx, float ty, gxtTransformSpace space = gxtTransformSpace.LOCAL)
        {
            Translate(new Vector2(tx, ty), space);
        }

        /// <summary>
        /// Rotates the node by the given value in the desired space
        /// </summary>
        /// <param name="rot">Additional rotation in radians</param>
        /// <param name="space">Transform Space</param>
        public virtual void Rotate(float rot, gxtTransformSpace space = gxtTransformSpace.LOCAL)
        {
            if (space == gxtTransformSpace.LOCAL)
            {
                relativeRotation += rot;
                QueueForUpdate();
            }
            else if (space == gxtTransformSpace.PARENT)
            {
                relativeRotation += rot;
                float cos = gxtMath.Cos(rot);
                float sin = gxtMath.Sin(rot);
                float pX = relativePosition.X * cos - relativePosition.Y * sin;
                float pY = relativePosition.X * sin + relativePosition.Y * cos;
                relativePosition.X = pX;
                relativePosition.Y = pY;
                QueueForUpdate();
            }
            else if (space == gxtTransformSpace.WORLD)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Scene Node Rotate Method Not Implemented For World Space Yet!");
                /*
                if (rot != 0.0f)
                {
                    //relativeRotation += rot;
                    float prevDerivedRotation = GetDerivedRotation();
                    float derivedRot = prevDerivedRotation + rot;
                    float cos = gxtMath.Cos(rot);
                    float sin = gxtMath.Sin(rot);

                    Vector2 derivedPos = GetDerivedPosition();
                    float pX = derivedPos.X * cos - derivedPos.Y * sin;
                    float pY = derivedPos.X * sin + derivedPos.Y * cos;
                    SetDerivedPosition(new Vector2(pX, pY));
                }
                */
                //gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Scene Node Rotate Method Not Implemented For World Space Yet!");
                /*
                float prevDerivedRot = GetDerivedRotation();
                Rotation += rot;
                float derivedRot = GetDerivedRotation();
                float dr = derivedRot - prevDerivedRot;
                float cos = gxtMath.Cos(dr);
                float sin = gxtMath.Sin(dr);
                Vector2 derivedPos = GetDerivedPosition();
                float pX = derivedPos.X * cos - derivedPos.Y * sin;
                float pY = derivedPos.X * sin + derivedPos.Y * cos;

                SetDerivedPosition(new Vector2(pX, pY));
                */
                //Matrix derivedRotMat = Matrix.CreateRotationZ(GetDerivedRotation());
                //Matrix derivedInverse = Matrix.Invert(derivedRotMat);
                
            }
        }

        /// <summary>
        /// Scales both axes by the given factor
        /// </summary>
        /// <param name="s">Uniform Scale Factor</param>
        public virtual void ScaleAxes(float s)
        {
            relativeScale.X *= s;
            relativeScale.Y *= s;
            QueueForUpdate();
        }

        /// <summary>
        /// Scales each axis independently with the given factors
        /// </summary>
        /// <param name="sx">X Axis Scale Factor</param>
        /// <param name="sy">Y Axis Scale Factor</param>
        public virtual void ScaleAxes(float sx, float sy)
        {
            relativeScale.X *= sx;
            relativeScale.Y *= sy;
            QueueForUpdate();
        }

        /// <summary>
        /// Scales each axis independently with the given factors
        /// </summary>
        /// <param name="s">Scale Vector</param>
        public virtual void ScaleAxes(Vector2 s)
        {
            relativeScale.X *= s.X;
            relativeScale.Y *= s.Y;
            QueueForUpdate();
        }

        /// <summary>
        /// Sets all of the transform fields relative to the parent at once
        /// It's a bit more optimal since it passes values by reference and 
        /// only queues the node for an update once
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        public virtual void SetAll(ref Vector2 position, float rotation, ref Vector2 scale)
        {
            relativePosition.X = position.X;
            relativePosition.Y = position.Y;
            relativeRotation = rotation;
            relativeScale.X = scale.X;
            relativeScale.Y = scale.Y;
            QueueForUpdate();
        }

        /// <summary>
        /// Sets the visibility of all attached drawables
        /// Cascade option will explicitly set this value for all 
        /// children drawable true if set to true.  Be careful setting visibility from 
        /// the node, especially with cascade and shared render properties.
        /// </summary>
        /// <param name="visible">Visibility</param>
        /// <param name="cascade">Cascade</param>
        public virtual void SetVisibility(bool visible, bool cascade = true)
        {
            for (int i = 0; i < NumDrawables; ++i)
            {
                if (drawables[i].Material != null)
                    drawables[i].Material.Visible = visible;
            }
            if (cascade)
            {
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    children[i].SetVisibility(visible, true);
                }
            }
        }

        /// <summary>
        /// Toggles the visibility of all the drawables in the node
        /// Will do the same for all children nodes if cascade is set to true
        /// </summary>
        /// <param name="cascade">Cascade</param>
        public virtual void FlipVisibility(bool cascade = true)
        {
            // flip visibility on all drawables attached here
            for (int i = 0; i < NumDrawables; ++i)
            {
                if (drawables[i].Material != null)
                    drawables[i].Material.Visible = !drawables[i].Material.Visible;
            }
            // if cascade, do the same for all the descendant drawables
            if (cascade)
            {
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    children[i].FlipVisibility(true);
                }
            }
        }

        /// <summary>
        /// Sets the color overlay of all drawables attached to this node 
        /// Cascade will apply this color to every child node drawable too
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="cascade">Cascade</param>
        public virtual void SetColor(Color color, bool cascade = true)
        {
            for (int i = 0; i < NumDrawables; i++)
            {
                if (drawables[i].Material != null)
                    drawables[i].Material.ColorOverlay = color;
            }
            if (cascade)
            {
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    children[i].SetColor(color, true);
                }
            }
        }

        /// <summary>
        /// Blends the color of existing drawable objects with the passed 
        /// in value.  Cascade will apply this blend operation for every 
        /// child node if set to true.
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="t">Blend Factor</param>
        /// <param name="cascade">Cascade</param>
        public virtual void BlendColor(Color color, float t, bool cascade = true)
        {
            for (int i = 0; i < NumDrawables; ++i)
            {
                // range checking is done inside Color.Lerp
                if (drawables[i].Material != null)
                    drawables[i].Material.ColorOverlay = Color.Lerp(drawables[i].Material.ColorOverlay, color, t);
            }
            if (cascade)
            {
                for (int i = 0; i < NumChildrenNodes; i++)
                {
                    children[i].BlendColor(color, t, true);
                }
            }
        }

        /// <summary>
        /// Flips the node around its x-axis
        /// </summary>
        public virtual void FlipHorizontal()
        {
            relativeScale.X = -relativeScale.X;
            QueueForUpdate();
        }

        /// <summary>
        /// Flips the node around its y-axis
        /// </summary>
        public virtual void FlipVertical()
        {
            relativeScale.Y = -relativeScale.Y;
            QueueForUpdate();
        }

        /// <summary>
        /// Adjusts the relative position to achieve the given derived position
        /// </summary>
        /// <param name="dPosition">Derived Position</param>
        public virtual void SetDerivedPosition(Vector2 dPosition)
        {
            if (parent != null)
            {
                Matrix pInvRotation = Matrix.CreateRotationZ(-parent.GetDerivedRotation());
                Vector2 d = Vector2.Transform(dPosition - parent.GetDerivedPosition(), pInvRotation);
                Vector2 parentScale = parent.GetDerivedScale();
                relativePosition = d / parentScale;
                QueueForUpdate();
            }
            else
            {
                relativePosition = dPosition;
                QueueForUpdate();
            }
        }

        /// <summary>
        /// Adjusts the relative rotation to achieve the given derived rotation
        /// </summary>
        /// <param name="dRotation">Derived Rotation</param>
        public virtual void SetDerivedRotation(float dRotation)
        {
            if (parent != null)
            {
                float dr = dRotation - parent.GetDerivedRotation();
                relativeRotation = dr;
                QueueForUpdate();
            }
            else
            {
                relativeRotation = dRotation;
                QueueForUpdate();
            }
        }

        /// <summary>
        /// Adjusts the relative scale to achieve the given derived scale
        /// In cases where this operation requires division by zero, the operation 
        /// will not be performed and a warning message will be logged
        /// </summary>
        /// <param name="dScale">Derived Scale</param>
        public virtual void SetDerivedScale(Vector2 dScale)
        {
            if (parent != null)
            {
                Vector2 parentDerivedScale = parent.GetDerivedScale();
                if (parentDerivedScale.X != 0.0f && parentDerivedScale.Y != 0.0f)
                {
                    Scale = new Vector2(dScale.X / parentDerivedScale.X, dScale.Y / parentDerivedScale.Y);
                }
                else
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Couldn't set derived scale as it would have resulted in division by zero!", "SetDerivedScale()");
                }
            }
            else
            {
                Scale = dScale;
            }
        }

        /// <summary>
        /// Gets the fully derived position of the node
        /// </summary>
        /// <returns>Derived Position</returns>
        public virtual Vector2 GetDerivedPosition()
        {
            if (dirty)
            {
                Vector2 dPos = relativePosition;
                gxtISceneNode parentNode = parent;
                while (parentNode != null)
                {
                    dPos += parentNode.Position;
                    parentNode = parentNode.Parent;
                }
                return dPos;
            }
            return derivedPosition;
        }

        /// <summary>
        /// Gets the fully derived rotation of the node
        /// </summary>
        /// <returns>Derived Rotation</returns>
        public virtual float GetDerivedRotation()
        {
            if (dirty)
            {
                float dRot = relativeRotation;
                gxtISceneNode parentNode = parent;
                while (parentNode != null)
                {
                    dRot += parentNode.Rotation;
                    parentNode = parentNode.Parent;
                }
                return dRot;
            }
            return derivedRotation;
        }

        /// <summary>
        /// Gets the fully derived scale of the node
        /// </summary>
        /// <returns>Derived Scale</returns>
        public virtual Vector2 GetDerivedScale()
        {
            if (dirty)
            {
                Vector2 dScale = relativeScale;
                gxtISceneNode parentNode = parent;
                while (parentNode != null)
                {
                    dScale.X *= parent.Scale.X;
                    dScale.Y *= parent.Scale.Y;
                    parentNode = parentNode.Parent;
                }
                return dScale;
            }
            return derivedScale;
        }

        /// <summary>
        /// Gets the fully derived matrix transform
        /// This operation can be rather expensive if the 
        /// node hasn't been updated
        /// </summary>
        /// <returns>Derived Transform</returns>
        public virtual Matrix GetDerivedTransform()
        {
            if (dirty)
            {
                Vector2 dPos = GetDerivedPosition();
                float dRot = GetDerivedRotation();
                Vector2 dScale = GetDerivedScale();
                return Matrix.CreateScale(dScale.X, dScale.Y, 1.0f) *
                       Matrix.CreateRotationZ(dRot) *
                       Matrix.CreateTranslation(dPos.X, dPos.Y, 0.0f);
            }
            return derivedTransform;
        }

        /// <summary>
        /// Gets the merged world space AABB of the node.
        /// Takes into account the AABB's of all drawable 
        /// objects for all children nodes and merges them into 
        /// one AABB.  Potentially useful for culling algorithms.
        /// </summary>
        /// <returns>Merged AABB</returns>
        public virtual gxtAABB GetAABB()
        {
            Vector2 pos = GetDerivedPosition();
            Vector2 s = GetDerivedScale();
            float r = GetDerivedRotation();

            gxtAABB aabb = new gxtAABB(pos, Vector2.Zero);
            gxtAABB curDrawableAABB;
            int i;
            for (i = 0; i < NumDrawables; ++i)
            {
                curDrawableAABB = drawables[i].GetLocalAABB();
                curDrawableAABB = curDrawableAABB.GetRotatedAABB(r);
                curDrawableAABB = gxtAABB.Update(relativePosition, relativeRotation, curDrawableAABB);
                curDrawableAABB = new gxtAABB(pos, new Vector2(gxtMath.Abs(curDrawableAABB.Extents.X * s.X), gxtMath.Abs(curDrawableAABB.Extents.Y * s.Y)));
                aabb = gxtAABB.Merge(aabb, curDrawableAABB);
            }

            for (i = 0; i < NumChildrenNodes; ++i)
            {
                aabb = gxtAABB.Merge(children[i].GetAABB(), aabb);
            }

            return aabb;
        }

        /// <summary>
        /// Unfinished, trace message function
        /// A lot could potentially go here, trying to sort out what will be the most useful
        /// </summary>
        /// <returns>Trace String</returns>
        public virtual string DebugTrace(bool cascade = false, int parentDepth = 0)
        {
            gxtDebug.Assert(parentDepth >= 0, "Can't have a negative depth");
            string message = string.Empty;
            string prefix = string.Empty;
            if (cascade)
            {
                for (int i = 0; i < parentDepth; ++i)
                {
                    prefix += "\t";
                }
            }

            string parentStr = (parent != null) ? parent.GetHashCode().ToString() : "NULL";
            message += prefix + "-----------------\n";
            message += prefix + "Node: " + GetHashCode().ToString() + " Parent: " + parentStr + "\n";
            message += prefix + "Num Children: " + NumChildrenNodes.ToString() + " Num Drawables: " + NumDrawables.ToString() + "\n";
            message += prefix + "Descendant Num Children: " + NumDescendantNodes.ToString() + " Descendant Num Drawables: " + NumDescendantDrawables.ToString() + "\n";
            message += prefix + "-----------------\n";

            if (cascade)
            {
                for (int i = 0; i < NumChildrenNodes; ++i)
                {
                    message += children[i].DebugTrace(true, parentDepth + 1) + "\n";
                }
            }

            return message;
        }
    }
}
