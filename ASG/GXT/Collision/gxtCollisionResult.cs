using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GXT
{
    /// <summary>
    /// Struct that packages collision information between two polygons
    /// that is determined elsewhere.  Always check to insure Intersection = true 
    /// before performing any collision response.  In most cases, if Intersection = false 
    /// the depth, normal, and contact points won't be calculated
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public struct gxtCollisionResult
    {
        private Vector2 collisionNormal;
        private float penetrationDepth;
        private bool intersection;
        private Vector2 contactptA;
        private Vector2 contactptB;

        /// <summary>
        /// Collision normal
        /// </summary>
        public Vector2 Normal { get { return collisionNormal; } set { collisionNormal = value; } }
        
        /// <summary>
        /// Intersection Depth
        /// </summary>
        public float Depth { get { return penetrationDepth; } set { penetrationDepth = value; } }
        
        /// <summary>
        /// Boolean indicating a positive/negative collision
        /// </summary>
        public bool Intersection { get { return intersection; } set { intersection = value; } }

        /// <summary>
        /// World space contact point on shape A
        /// </summary>
        public Vector2 ContactPointA { get { return contactptA; } set { contactptA = value; } }

        /// <summary>
        /// World space contact point on shape B
        /// </summary>
        public Vector2 ContactPointB { get { return contactptB; } set { contactptB = value; } }
    }
}
