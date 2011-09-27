using System;
using Microsoft.Xna.Framework;
using GXT.Physics;

namespace GXT
{
    /// <summary>
    /// An informational struct which packages pertinent info 
    /// from a ray cast.  In most cases, if Intersection = false 
    /// the rest of the information will not be calculated
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtRayHit
    {
        private bool intersection;
        private float distance;
        private Vector2 normal;
        private Vector2 pt;
        // is geom necessary?  could subclass a PhysicsRayHit
        private gxtGeom geom;

        /// <summary>
        /// Intersecting?
        /// </summary>
        public bool Intersection { get { return intersection; } set { intersection = value; } }
        
        /// <summary>
        /// Distance along the ray of the intersection point
        /// </summary>
        public float Distance { get { return distance; } set { distance = value; } }
        
        /// <summary>
        /// Edge normal of the intersected polygon
        /// </summary>
        public Vector2 Normal { get { return normal; } set { normal = value; } }

        /// <summary>
        /// Contact point
        /// </summary>
        public Vector2 Point { get { return pt; } set { pt = value; } }

        /// <summary>
        /// Hit geom (if any)
        /// </summary>
        public gxtGeom Geom { get { return geom; } set { geom = value; } }
    }
}
