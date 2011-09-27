using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.AI
{
    public class gxtPathNode
    {
        public float tolerance;
        public Vector2 position;
        public List<gxtPathArc> arcs;

        public gxtPathNode()
        {
            position = new Vector2();
            arcs = new List<gxtPathArc>();
        }

        public gxtPathNode(Vector2 pos, float tolerance)
        {
            this.tolerance = tolerance;
            position = pos;
            arcs = new List<gxtPathArc>();
        }

        public gxtPathNode(Vector2 pos, float tolerance, List<gxtPathArc> arcs)
        {
            this.tolerance = tolerance;
            position = pos;
            this.arcs = arcs;
        }

        public void AddArc(gxtPathArc arc)
        {
            arcs.Add(arc);
        }

        public void GetNeighbors(ref List<gxtPathNode> neighbors)
        {
            foreach (gxtPathArc arc in arcs)
            {
                neighbors.Add(arc.GetNeighbor(this));
            }
        }

        public float GetCostFromNode(gxtPathNode pNode)
        {
            gxtPathArc arc = FindArc(pNode);
            Vector2 d = pNode.position - position;
            return arc.weight * d.Length();
        }

        private gxtPathArc FindArc(gxtPathNode linkedNode)
        {
            foreach (gxtPathArc arc in arcs)
            {
                if (arc.GetNeighbor(this) == linkedNode)
                    return arc;
            }
            return null;
        }
    }
}
