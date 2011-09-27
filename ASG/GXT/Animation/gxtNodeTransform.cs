using Microsoft.Xna.Framework;

namespace GXT.Animation
{
    /// <summary>
    /// An animation pose/state used in keyframed animation
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtNodeTransform
    {
        /// <summary>
        /// Translation relative to the parent of the node
        /// </summary>
        public Vector2 Translation;

        /// <summary>
        /// Scale relative to parent of the node
        /// </summary>
        public Vector2 Scale;

        /// <summary>
        /// Rotation relative to the parent of the node
        /// </summary>
        public float Rotation;


        public gxtNodeTransform()
        {
            Scale = Vector2.One;
        }

        public static readonly gxtNodeTransform Identity = new gxtNodeTransform();

        /*
        /// <summary>
        /// The identity transform
        /// </summary>
        public static gxtNodeTransform Identity { get { return identity; } }
        */
    }
}
