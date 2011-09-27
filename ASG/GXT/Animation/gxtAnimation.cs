using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GXT.Rendering;

namespace GXT.Animation
{
    /// <summary>
    /// Shared properties and controls for a collection of tweens
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtAnimation : gxtIUpdate
    {
        #region Fields
        private bool enabled;
        private List<gxtAnimationClip> clips;
        private TimeSpan duration;
        private TimeSpan elapsedTime;
        private float playbackRate;
        private bool loop;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Enabled Flag
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Total duration of the clip
        /// </summary>
        public TimeSpan Duration { get { return duration; } set { duration = value; } }

        /// <summary>
        /// Currently elapsed time in the animation
        /// </summary>
        public TimeSpan ElapsedTime { get { return elapsedTime; } set { elapsedTime = value; } }

        // should have a global start time -> for collections of clips??

        /// <summary>
        /// Collection of transform tweens
        /// </summary>
        public List<gxtAnimationClip> Clips { get { return clips; } set { clips = value; } }

        /// <summary>
        /// Play Rate Speed Scale
        /// Negative values will play the animation backwards
        /// </summary>
        public float PlaybackRate { get { return playbackRate; } set { playbackRate = value; } }

        /// <summary>
        /// Flag that determines if the animation should be looped
        /// </summary>
        public bool Loop { get { return loop; } set { loop = value; } }

        /// <summary>
        /// Determines if the animation is done
        /// If loop is set to true this will always return false
        /// </summary>
        public bool IsDone { get { if (loop) return false; return elapsedTime >= duration; } }
        #endregion Properties

        /// <summary>
        /// Constructs an animation clip
        /// </summary>
        /// <param name="clipDuration"></param>
        /// <param name="initEnabled"></param>
        /// <param name="loop"></param>
        /// <param name="playbackRate"></param>
        public gxtAnimation(TimeSpan clipDuration, bool initEnabled = true, bool loop = false, float playbackRate = 1.0f)
        {
            enabled = initEnabled;
            elapsedTime = TimeSpan.Zero;
            duration = clipDuration;
            clips = new List<gxtAnimationClip>();
            this.playbackRate = playbackRate;
            this.loop = loop;
        }

        /// <summary>
        /// Updates the clip by the given time step
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Update(GameTime gameTime)
        {
            if (!enabled || clips.Count == 0)
                return;

            if (!loop && elapsedTime > duration)
                return;

            elapsedTime += gameTime.ElapsedGameTime;
            // find local t in the clip, based on rules seen in Game Engine Architecture
            float localT;
            if (!loop)
                localT = (float)(playbackRate * elapsedTime.TotalMilliseconds / duration.TotalMilliseconds);
            else
                localT = (float)(((playbackRate * elapsedTime.TotalMilliseconds) % duration.TotalMilliseconds) / duration.TotalMilliseconds);

            // update the tweens
            bool backwards = playbackRate < 0.0f;
            for (int i = 0; i < clips.Count; ++i)
            {
                clips[i].Update(localT, backwards);
            }
        }

        /// <summary>
        /// Resets the animation clip
        /// </summary>
        /// <param name="enabled">If enabled on reset</param>
        public void Reset(bool enabled = true)
        {
            this.enabled = enabled;
            elapsedTime = TimeSpan.Zero;
            bool backwards = playbackRate < 0.0f;
            for (int i = 0; i < clips.Count; i++)
            {
                clips[i].Reset(backwards);
            }
        }

        /// <summary>
        /// Adds a tween to the animation clip
        /// </summary>
        /// <param name="tween"></param>
        public void AddClip(gxtAnimationClip clip)
        {
            clips.Add(clip);
        }

        /// <summary>
        /// Removes a tween from the animation clip
        /// </summary>
        /// <param name="tween"></param>
        /// <returns></returns>
        public bool RemoveClip(gxtAnimationClip clip)
        {
            return clips.Remove(clip);
        }
    }
}
