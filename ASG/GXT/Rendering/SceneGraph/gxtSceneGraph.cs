using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// A tree of scene nodes for parent-child hierarchies
    /// See gxtSceneNode for a better understanding 
    /// of how the graph works
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtSceneGraph
    {
        private gxtISceneNode root;
        private bool sceneVisible;

        private static Queue<gxtISceneNode> nodeUpdateQueue;

        /// <summary>
        /// Adds the node to the collection if it isn't in the update queue already
        /// Returns boolean indicating it has been added and the subtree should be 
        /// recursively marked as dirty.  If a node is already in the queue it will 
        /// not be added twice
        /// </summary>
        /// <param name="node">Scene Node</param>
        /// <returns>If added to the queue</returns>
        public static bool AddNodeToUpdateQueue(gxtISceneNode node)
        {
            gxtDebug.Assert(nodeUpdateQueue != null, "Node Update Queue Hasn't Been Initialized Yet!");
            if (!nodeUpdateQueue.Contains(node))
            {
                nodeUpdateQueue.Enqueue(node);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Root of the tree
        /// </summary>
        public gxtISceneNode Root { get { return root; } }

        /// <summary>
        /// Forcibly disables all scene drawing if set to false
        /// </summary>
        public bool Visible { get { return sceneVisible; } set { sceneVisible = value; } }

        /// <summary>
        /// Number of nodes in the tree
        /// </summary>
        public int NodeCount { get { return root.NumDescendantNodes; } }
        
        /// <summary>
        /// Number of attached drawables in the tree 
        /// </summary>
        public int DrawableCount { get { return root.NumDescendantDrawables; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtSceneGraph() { }

        /// <summary>
        /// If the scene graph has been initialized
        /// </summary>
        /// <returns>If initialized</returns>
        public bool IsInitialized()
        {
            return root != null && nodeUpdateQueue != null;
        }

        /// <summary>
        /// Initializes the scene graph
        /// </summary>
        /// <param name="sceneVisible">Flag enabling/disabling rendering of the scene</param>
        /// <param name="updateQueueCapacity">Initial capacity of the update queue, it will grow if needed</param>
        public virtual void Initialize(bool sceneVisible = true, int updateQueueCapacity = 8)
        {
            gxtDebug.Assert(!IsInitialized(), "Instance of scene graph has already been initialized");
            gxtDebug.Assert(updateQueueCapacity >= 0, "Can't have a negative capacity update queue!");
            nodeUpdateQueue = new Queue<gxtISceneNode>(updateQueueCapacity);
            Visible = sceneVisible;
            root = new gxtSceneNode();
        }

        /// <summary>
        /// Adds a node as a child of the root scene node
        /// </summary>
        /// <param name="node">Node</param>
        public virtual void AddNode(gxtISceneNode node)
        {
            gxtDebug.Assert(IsInitialized(), "Scene graph hasn't been initialized!");
            root.AddChild(node);
        }

        /// <summary>
        /// Removes the node from the scene, regardless of its parent
        /// </summary>
        /// <param name="node">Node</param>
        public virtual bool RemoveNode(gxtISceneNode node)
        {
            if (node.Parent != null)
                return node.Parent.RemoveChild(node);
            return false;
        }

        /// <summary>
        /// Removes all the nodes from the scene graph
        /// </summary>
        public virtual void RemoveAllNodes()
        {
            root.RemoveAllChildren();
        }

        /// <summary>
        /// Determines if the node exists as a child of the root, or, if deepSearch is set to true,
        /// is in the tree at all
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="deepSearch">Recursive Search</param>
        /// <returns>If found</returns>
        public virtual bool ContainsNode(gxtISceneNode node, bool deepSearch = true)
        {
            return root.ContainsChild(node, deepSearch);
        }

        /// <summary>
        /// Updates the scene graph, it's important to do this after the rest of your scene 
        /// has been updated and only once per frame
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            gxtDebug.Assert(IsInitialized(), "Scene Graph has not been initialized");
            gxtISceneNode updateNode;
            while (nodeUpdateQueue.Count != 0)
            {
                updateNode = nodeUpdateQueue.Dequeue();
                updateNode.Update();
            }
        }

        /// <summary>
        /// Draws the scene using the graphics batch if the scene is visible
        /// </summary>
        /// <param name="graphicsBatch"></param>
        public void Draw(gxtGraphicsBatch graphicsBatch)
        {
            gxtDebug.Assert(gxtRoot.Singleton.EngineState == gxtEngineState.DRAW, "You can only draw the scene graph during the draw engine state!");
            if (sceneVisible)
                root.Draw(graphicsBatch);
        }

        /// <summary>
        /// Returns a debug trace string describing nodes/drawables in the graph
        /// </summary>
        /// <returns>Debug Trace String</returns>
        public string DebugTrace()
        {
            return root.DebugTrace(true, 0);
        }
    }
}
