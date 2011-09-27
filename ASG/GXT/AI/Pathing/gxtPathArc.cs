using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.AI
{
    public class gxtPathArc
    {
        public float weight;
        public gxtPathNode[] nodes;

        public gxtPathArc()
        {
            this.weight = 5.0f;
            nodes = new gxtPathNode[2];
        }

        public gxtPathArc(float weight)
        {
            this.weight = weight;
            nodes = new gxtPathNode[2];
        }

        public void LinkNodes(gxtPathNode a, gxtPathNode b)
        {
            nodes[0] = a;
            nodes[1] = b;
        }

        public gxtPathNode GetNeighbor(gxtPathNode pNode)
        {
            if (nodes[0] == pNode)
                return nodes[1];
            else
                return nodes[0];
        }
    }
}
