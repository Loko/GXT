using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.AI
{
    public class gxtPathPlan
    {
        private List<gxtPathNode> pathNodes;
        private int index;

        public gxtPathPlan()
        {
            pathNodes = new List<gxtPathNode>();
            index = pathNodes.Count;
        }

        public void Reset()
        {
            index = 0;
        }

        public Vector2 CurrentNodePosition()
        {
            return pathNodes[index].position;
        }

        public bool CheckForNextNode(Vector2 position)
        {
            float dist = Vector2.Distance(position, pathNodes[index].position);
            if (dist <= pathNodes[index].tolerance)
            {
                ++index;
                return true;
            }
            return false;
        }

        public bool AtEnd()
        {
            return index == pathNodes.Count;
        }

        public void AddNode(gxtPathNode node)
        {
            pathNodes.Insert(0, node);
        }
    }
}
