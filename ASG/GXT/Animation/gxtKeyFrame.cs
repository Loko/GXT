using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GXT.Rendering;

namespace GXT.Animation
{
    public enum gxtAnimationInterpolationType
    {
        LERP = 0,
        SMOOTH_STEP = 1,
        SMOOTHER_STEP = 2
    };

    /// <summary>
    /// A keyframe represented by a animation pose and the localized time in the clip 
    /// as well as a type for the interpolation method used between keyframes
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtKeyframe
    {
        /// <summary>
        /// Default interpolation, used in the keyframe constructor and elsewhere if one isn't specified
        /// </summary>
        public const gxtAnimationInterpolationType DEFAULT_INTERPOLATION_TYPE = gxtAnimationInterpolationType.SMOOTH_STEP;

        private float localTime;
        private gxtAnimationPose animationPose;
        private gxtAnimationInterpolationType interpolationType;

        /// <summary>
        /// Local Time 0 - 1 on the animation timeline
        /// </summary>
        public float LocalTime { get { return localTime; } set { localTime = gxtMath.Saturate(value); } }

        /// <summary>
        /// Pose at this keyframe
        /// </summary>
        public gxtAnimationPose AnimationPose { get { return animationPose; } set { animationPose = value; } }

        /// <summary>
        /// Interpolation type to use for this keyframe
        /// </summary>
        public gxtAnimationInterpolationType InterpolationType { get { return interpolationType; } set { interpolationType = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transform">Transform</param>
        /// <param name="t">Localized Time</param>
        /// <param name="interpolationType">Interpolation type to use from this keyframe to the next</param>
        public gxtKeyframe(gxtAnimationPose pose, float localTime, gxtAnimationInterpolationType interpolationType = gxtAnimationInterpolationType.SMOOTH_STEP)
        {
            LocalTime = gxtMath.Saturate(localTime);
            AnimationPose = pose;
            InterpolationType = interpolationType;
        }
    }
}
