using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.AI
{
    /// <summary>
    /// Currently everything here works but it could be much better
    /// </summary>
    public class gxtPathGraph
    {
        private List<gxtPathNode> nodes;
        private List<gxtPathArc> arcs;
        private gxtAstar astar;

        public gxtPathGraph()
        {
            nodes = new List<gxtPathNode>();
            arcs = new List<gxtPathArc>();
            astar = new gxtAstar();
        }

        public void Destroy()
        {
            nodes.Clear();
            arcs.Clear();
        }

        public int NumNodes { get { return nodes.Count; } }

        public void AddNode(gxtPathNode node)
        {
            nodes.Add(node);
        }

        public void RemoveNode(gxtPathNode node)
        {
            nodes.Remove(node);
        }

        public bool ContainsNode(gxtPathNode node)
        {
            return nodes.Contains(node);
        }

        public gxtPathNode FindClosestNode(Vector2 position)
        {
            gxtDebug.Assert(nodes.Count > 0, "Cannot find the closest node in an empty graph!");

            // inefficient brute force search for now
            gxtPathNode closest = nodes[0];
            float minDist = Vector2.DistanceSquared(closest.position, position);
            for (int i = 1; i < nodes.Count; i++)
            {
                float dist = Vector2.DistanceSquared(nodes[i].position, position);
                if (dist < minDist)
                {
                    closest = nodes[i];
                    minDist = dist;
                }
            }
            return closest;
        }

        public gxtPathNode FindFurthestNode(Vector2 position)
        {
            gxtDebug.Assert(nodes.Count > 0, "Cannot find the furthest node in an empty graph!");

            // inefficient brute force search for now
            gxtPathNode furthest = nodes[0];
            float maxDist = Vector2.DistanceSquared(furthest.position, position);
            for (int i = 1; i < nodes.Count; i++)
            {
                float dist = Vector2.DistanceSquared(nodes[i].position, position);
                if (dist > maxDist)
                {
                    furthest = nodes[i];
                    maxDist = dist;
                }
            }
            return furthest;
        }

        public gxtPathNode FindRandomNode(Vector2 position)
        {
            gxtDebug.Assert(false, "Function Not Yet Implemented");
            return new gxtPathNode();
        }

        public gxtPathPlan FindPath(Vector2 startPt, Vector2 endPt)
        {
            gxtPathNode startNode = FindClosestNode(startPt);
            gxtPathNode endNode = FindClosestNode(endPt);
            return FindPath(startNode, endNode);
        }

        public gxtPathPlan FindPath(Vector2 startPt, gxtPathNode endNode)
        {
            gxtPathNode startNode = FindClosestNode(startPt);
            return FindPath(startNode, endNode);
        }

        public gxtPathPlan FindPath(gxtPathNode startNode, Vector2 endPt)
        {
            gxtPathNode endNode = FindClosestNode(endPt);
            return FindPath(startNode, endNode);
        }

        public gxtPathPlan FindPath(gxtPathNode startNode, gxtPathNode endNode)
        {
            astar.Destroy();
            return astar.GetPath(startNode, endNode);
        }

        private void LinkNodes(gxtPathNode nodeA, gxtPathNode nodeB)
        {
            gxtPathArc arc = new gxtPathArc();
            arc.LinkNodes(nodeA, nodeB);
            nodeA.AddArc(arc);
            nodeB.AddArc(arc);
            arcs.Add(arc);
        }
    }
}
