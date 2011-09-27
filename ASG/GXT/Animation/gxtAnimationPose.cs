using Microsoft.Xna.Framework;

namespace GXT.Animation
{
    /// <summary>
    /// A keyframeable animation pose that stores the data needed for interpolated transforms.
    /// Allows for the transform interpolation of position, rotation, and scale.
    /// Texture animation is also possible, with support for flipbook animation at each keyframe, 
    /// or a smooth interpolation along the texture coordinates.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtAnimationPose
    {
        /// <summary>
        /// UV coordinates at the current state
        /// </summary>
        public Vector2[] UVCoordinates;

        /// <summary>
        /// Translation relative to the parent
        /// </summary>
        public Vector2 Translation;

        /// <summary>
        /// Scale relative to the parent
        /// </summary>
        public Vector2 Scale;

        /// <summary>
        /// Rotation relative to the parent
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Overlay Tint at this animation state
        /// </summary>
        public Color ColorOverlay;

        /// <summary>
        /// Boolean that optionally lets you interpolate the UV coordinates
        /// If false they are directly copied at each individual keyframe
        /// Interpolation is dependant on the first state in a given interpolation
        /// So when going from keyframe a to b, the value of a.InterpolateUVCoords 
        /// determines if they are interpolated, even if keyframe b has a different value
        /// </summary>
        public bool InterpolateUVCoords;

        /// <summary>
        /// Flag for optional interpolation of the color overlay
        /// Interpolation rules work the same as they do for the InterpolateUVCoords flag
        /// </summary>
        public bool InterpolateColorOverlay;

        /// <summary>
        /// 
        /// </summary>
        public gxtAnimationPose()
        {
            Translation = Vector2.Zero;
            Rotation = 0.0f;
            Scale = Vector2.One;
            UVCoordinates = null;
            ColorOverlay = Color.White;
            InterpolateUVCoords = false;
            InterpolateColorOverlay = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uvCoordinates"></param>
        /// <param name="interpolateUVCoords"></param>
        public gxtAnimationPose(Vector2[] uvCoordinates, bool interpolateUVCoords)
        {
            gxtDebug.Assert(uvCoordinates != null && !interpolateUVCoords);

            Translation = Vector2.Zero;
            Rotation = 0.0f;
            Scale = Vector2.One;
            UVCoordinates = uvCoordinates;
            ColorOverlay = Color.White;
            InterpolateUVCoords = interpolateUVCoords;
            InterpolateColorOverlay = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animState"></param>
        /// <returns></returns>
        public static bool IsValid(gxtAnimationPose animPose)
        {
            if (animPose == null)
                return false;
            if (animPose.InterpolateUVCoords)
                return animPose.UVCoordinates != null;
            else
                return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animState"></param>
        /// <param name="numVertices"></param>
        /// <returns></returns>
        public static bool IsValid(gxtAnimationPose animPose, int numVertices)
        {
            gxtDebug.Assert(numVertices < 3, "A mesh must have at least 3 vertices!");
            if (animPose == null)
                return false;
            if (animPose.InterpolateUVCoords)
                if (animPose.UVCoordinates != null)
                    return animPose.UVCoordinates.Length == numVertices;

            return true;
        }

        /// <summary>
        /// Sets the identity node transform
        /// </summary>
        public void SetIdentityTransform()
        {
            Translation = Vector2.Zero;
            Rotation = 0.0f;
            Scale = Vector2.One;
        }

        /// <summary>
        /// Shallow copies keep the same collection of UV coords
        /// This can save a lot of space, especially for big meshes
        /// But the editor has to handle this optimization carefully
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static gxtAnimationPose ShallowCopy(gxtAnimationPose animPose)
        {
            gxtDebug.Assert(animPose != null);
            gxtAnimationPose poseCopy = new gxtAnimationPose();
            poseCopy.Translation = animPose.Translation;
            poseCopy.Rotation = animPose.Rotation;
            poseCopy.Scale = animPose.Scale;
            poseCopy.ColorOverlay = animPose.ColorOverlay;
            poseCopy.InterpolateUVCoords = animPose.InterpolateUVCoords;
            poseCopy.UVCoordinates = animPose.UVCoordinates;
            poseCopy.InterpolateColorOverlay = animPose.InterpolateColorOverlay;
            return poseCopy;
        }

        /// <summary>
        /// Same as shallow copy, but operates on a newly allocated copy of the UV Coordinates
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static gxtAnimationPose DeepCopy(gxtAnimationPose animPose)
        {
            gxtDebug.Assert(animPose != null);
            gxtAnimationPose poseCopy = new gxtAnimationPose();
            poseCopy.Translation = animPose.Translation;
            poseCopy.Rotation = animPose.Rotation;
            poseCopy.Scale = animPose.Scale;
            poseCopy.ColorOverlay = animPose.ColorOverlay;
            poseCopy.InterpolateUVCoords = animPose.InterpolateUVCoords;
            poseCopy.InterpolateColorOverlay = animPose.InterpolateColorOverlay;
            if (animPose.UVCoordinates != null)
            {
                Vector2[] uvCopy = new Vector2[animPose.UVCoordinates.Length];
                for (int i = 0; i < uvCopy.Length; ++i)
                    uvCopy[i] = animPose.UVCoordinates[i];
            }
            return poseCopy;
        }

        // we really ought to reuse variables passed in here
        // Could just have one interpolator, pass them each in by reference
        // Resizes the UV coordinate array as necessary
        /*
        public static void Interpolate(gxtAnimationPose a, gxtAnimationPose b, float t, ref Vector2 translation, ref float rotation,
            ref Vector2 scale, ref Color colorOverlay, Vector2[] uvCoordinates)
        {
            translation = gxtMath.SmoothStep(a.Translation, b.Translation, t);
            rotation = gxtMath.SmoothStep(a.Rotation, b.Rotation, t);
            scale = gxtMath.SmoothStep(a.Scale, b.Scale, t);
            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, translation.ToString());
            // optionally interpolate UV coords
            if (a.InterpolateColorOverlay)
                colorOverlay = gxtMath.SmoothStep(a.ColorOverlay, b.ColorOverlay, t);
            else
                colorOverlay = a.ColorOverlay;

            if (a.InterpolateUVCoords)
            {
                gxtDebug.SlowAssert(a.UVCoordinates != null && b.UVCoordinates != null, "Cannot interpolate between null UV Coordinates!");
                gxtDebug.SlowAssert(a.UVCoordinates.Length == b.UVCoordinates.Length, "Cannot interpolate between UV Coordinate arrays of different sizes!");
                for (int i = 0; i < a.UVCoordinates.Length; ++i)
                {
                    uvCoordinates[i] = gxtMath.SmoothStep(a.UVCoordinates[i], b.UVCoordinates[i], t);
                }
            }
            else
            {
                for (int i = 0; i < a.UVCoordinates.Length; ++i)
                {
                    uvCoordinates[i] = a.UVCoordinates[i];
                }
            }
        }
        */
    }
}
