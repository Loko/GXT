using System;
using Microsoft.Xna.Framework;

namespace GXT.Physics
{
    /// <summary>
    /// A shared material representing friction and restitution properties of the object
    /// Currently just an optional attachment to instances of gxtRigidBody
    /// </summary>
    public class gxtPhysicsMaterial
    {
        // Defaults, used if no material is attached
        public static readonly float DEFAULT_FRICTION = 0.01f;
        public static readonly float DEFAULT_RESTITUTION = 0.05f;

        private float friction;
        /// <summary>
        /// Friction of surface, between 0 and 1
        /// </summary>
        public float Friction { get { return friction; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f); friction = value; } }

        private float restitution;
        /// <summary>
        /// Restitution (bounciness) of surface, between 0 and 1
        /// </summary>
        public float Restitution { get { return restitution; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f); restitution = value; } }
        
        // rolling friction?

        public gxtPhysicsMaterial() 
        {
            friction = DEFAULT_FRICTION;
            restitution = DEFAULT_RESTITUTION;
        }

        public gxtPhysicsMaterial(float friction, float resitution)
        {
            Friction = friction;
            Restitution = resitution;
        }

        public static float GetCombinedFriction(gxtPhysicsMaterial mat0, gxtPhysicsMaterial mat1)
        {
            return gxtMath.Sqrt(mat0.friction * mat1.friction);
        }

        public static float GetCombinedRestitution(gxtPhysicsMaterial mat0, gxtPhysicsMaterial mat1)
        {
            return gxtMath.Sqrt(mat0.restitution * mat1.restitution);
        }

    }
}
