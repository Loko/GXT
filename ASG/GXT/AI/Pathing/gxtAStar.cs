using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.AI
{
    // TODO: CONSIDERING TURNING INTO A STATIC CLASS WITH RESET METHOD FOR NEW QUERIES
    public class gxtAstar
    {
        private Dictionary<gxtPathNode, gxtPathPlanNode> nodes;
        private gxtPathNode start;
        private gxtPathNode goal;
        private List<gxtPathPlanNode> openSet;

        public gxtAstar()
        {
            start = null;
            goal = null;
            nodes = new Dictionary<gxtPathNode, gxtPathPlanNode>();
            openSet = new List<gxtPathPlanNode>();
        }

        public void Destroy()
        {
            start = null;
            goal = null;
            nodes.Clear();
            openSet.Clear();
        }

        public gxtPathPlan GetPath(gxtPathNode startNode, gxtPathNode goalNode)
        {
            // if the start and end nodes are the same, we're close enough to b-line to the goal
            if (startNode == goalNode)
                return null;

            // set our members
            start = startNode;
            goal = goalNode;

            // The open set is a priority queue of the nodes to be evaluated.  If it's ever empty, it means 
            // we couldn't find a path to the goal.  The start node is the only node that is initially in 
            // the open set.
            AddToOpenSet(start, null);

            while (openSet.Count != 0)
            {
                // grab the most likely candidate
                gxtPathPlanNode pNode = openSet[0];

                // If this node is our goal node, we've successfully found a path.
                if (pNode.Node == goal)
                    return RebuildPath(pNode);

                // we're processing this node so remove it from the open set and add it to the closed set
                openSet.RemoveAt(0);
                AddToClosedSet(pNode);

                // get the neighboring nodes
                List<gxtPathNode> neighbors = new List<gxtPathNode>();
                pNode.Node.GetNeighbors(ref neighbors);

                // loop though all the neighboring nodes and evaluate each one
                for (int i = 0; i < neighbors.Count; i++)
                {
                    gxtPathNode pNodeToEvaluate = neighbors[i];

                    // Try and find a PathPlanNode object for this node.
                    gxtPathPlanNode searchNode;
                    bool found = nodes.TryGetValue(pNodeToEvaluate, out searchNode);

                    if (found && searchNode.IsClosed)
                        continue;

                    // figure out the cost for this route through the node
                    float costForThisPath = pNode.Goal + pNodeToEvaluate.GetCostFromNode(pNode.Node); //pNode->GetGoal() + pNodeToEvaluate->GetCostFromNode(pNode->GetPathingNode());
                    bool isPathBetter = false;

                    // Grab the PathPlanNode if there is one.
                    gxtPathPlanNode pPathPlanNodeToEvaluate = null;
                    if (!found)
                        pPathPlanNodeToEvaluate = searchNode; //findIt->second;

                    // No PathPlanNode means we've never evaluated this pathing node so we need to add it to 
                    // the open set, which has the side effect of setting all the heuristic data.  It also 
                    // means that this is the best path through this node that we've found so the nodes are 
                    // linked together (which is why we don't bother setting isPathBetter to true; it's done
                    // for us in AddToOpenSet()).
                    if (pPathPlanNodeToEvaluate == null)
                        pPathPlanNodeToEvaluate = AddToOpenSet(pNodeToEvaluate, pNode);

                    // If this node is already in the open set, check to see if this route to it is better than
                    // the last.
                    else if (costForThisPath < pPathPlanNodeToEvaluate.Goal)
                        isPathBetter = true;

                    // If this path is better, relink the nodes appropriately, update the heuristics data, and
                    // reinsert the node into the open list priority queue.
                    if (isPathBetter)
                    {
                        pPathPlanNodeToEvaluate.Prev = pNode;
                        ReinsertNode(pPathPlanNodeToEvaluate);
                    }
                }
            }

            // If we get here, there's no path to the goal.
            return null;
        }

        public gxtPathPlanNode AddToOpenSet(gxtPathNode node, gxtPathPlanNode prevNode)
        {
            //int index = nodes.Keys.GetEnumerator().
            gxtPathNode searchNode = node;
            gxtPathPlanNode thisNode = null;

            if (nodes.ContainsKey(node))
            {
                thisNode = new gxtPathPlanNode(node, prevNode, goal);
                nodes.Add(node, thisNode);
            }
            else
            {
                thisNode = nodes[searchNode];
                thisNode.IsClosed = false;
            }

            InsertNode(thisNode);

            return thisNode;
        }

        public void AddToClosedSet(gxtPathPlanNode node)
        {
            node.IsClosed = true;
        }

        public void InsertNode(gxtPathPlanNode planNode)
        {
            // just add the node if the open set is empty
            if (openSet.Count == 0)
            {
                openSet.Add(planNode);
                return;
            }

            // otherwise, perform an insertion sort	
            int index = 0;
            gxtPathPlanNode compareNode = openSet[index];
            while (compareNode < planNode)
            {
                ++index;
                if (index != openSet.Count)
                    compareNode = openSet[index];
                else
                    break;
            }
            openSet.Insert(index, planNode);
        }

        public void ReinsertNode(gxtPathPlanNode planNode)
        {
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i] == planNode)
                {
                    openSet.RemoveAt(i);
                    InsertNode(planNode);
                    return;
                }
            }
            InsertNode(planNode);
        }

        public gxtPathPlan RebuildPath(gxtPathPlanNode pathGoalNode)
        {
            gxtPathPlan plan = new gxtPathPlan();

            gxtPathPlanNode tmpGoal = pathGoalNode;

            while (tmpGoal != null)
            {
                plan.AddNode(tmpGoal.Node);
                tmpGoal = tmpGoal.Prev;
            }

            return plan;
        }

    }
}
