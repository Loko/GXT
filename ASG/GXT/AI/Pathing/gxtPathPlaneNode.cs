using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.AI
{
    public class gxtPathPlanNode
    {
        private gxtPathPlanNode prev;
        private gxtPathNode node;
        private gxtPathNode goalNode;

        private bool closed;
        private float goal;
        private float heuristic;
        private float fitness;

        public gxtPathPlanNode Prev { get { return prev; } set { prev = value; UpdateHeuristics(); } }
        public gxtPathNode Node { get { return node; } }
        public bool IsClosed { get { return closed; } set { closed = value; } }
        public float Goal { get { return goal; } }
        public float Heuristic { get { return heuristic; } }
        public float Fitness { get { return fitness; } }

        public gxtPathPlanNode(gxtPathNode node, gxtPathPlanNode prev, gxtPathNode goal)
        {
            this.node = node;
            this.prev = prev;
            this.goalNode = goal;
            closed = false;
            UpdateHeuristics();
        }

        private void UpdateHeuristics()
        {
            if (prev != null)
                goal = prev.goal + node.GetCostFromNode(prev.node);
            else
                goal = 0;

            // h
            heuristic = Vector2.Distance(node.position, goalNode.position);

            // cost to goal (f)
            fitness = goal + heuristic;
        }

        public static bool operator <(gxtPathPlanNode nodeLeft, gxtPathPlanNode nodeRight)
        {
            return nodeLeft.fitness < nodeRight.fitness;
        }

        public static bool operator >(gxtPathPlanNode nodeLeft, gxtPathPlanNode nodeRight)
        {
            return nodeLeft.fitness > nodeRight.fitness;
        }
    }
}
