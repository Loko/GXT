using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using GXT.Animation;

namespace GXT.Processes
{
    /// <summary>
    /// A simple controller interface that eases interaction/operations on a 
    /// collection of string aliased animation clips.  Generally, you add all 
    /// the animations for a given object to the given controller.  And call 
    /// animController.Play("walk") and animController.Stop("jump"), etc. etc.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: IMPLEMENT CROSS FADE SUPPORT, ASSERTS FOR EVERY METHOD
    public class gxtAnimationController : gxtIController
    {
        public static readonly string ANIMATION_NOT_FOUND = "animation_not_found";

        private bool enabled;
        private Dictionary<string, gxtAnimation> animations;

        /// <summary>
        /// Enabled flag, currently has no effect on controller functionality
        /// That may change if and when animation blending is implemented
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Does this controller query input?
        /// </summary>
        public bool QueriesInput { get { return false; } }


        /*
        private bool inTransition;

        public bool InTransition { get { return inTransition; } }
        private TimeSpan elapsedTransitionTime;
        private TimeSpan transitionDuration;
        private gxtNodeTransform transitionTransform;
        private gxtNodeTransform startTransform;
        */

        /// <summary>
        /// Total number of animations registered with the controller
        /// </summary>
        public int NumAnimations { get { return animations.Count; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public gxtAnimationController(bool initEnabled = true)
        {
            animations = new Dictionary<string, gxtAnimation>();
            enabled = initEnabled;
        }

        /// <summary>
        /// Adds a new animation to the controller with the given name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="clip">Animation Clip</param>
        public void AddAnimation(string name, gxtAnimation animation)
        {
            gxtDebug.Assert(!animations.ContainsKey(name), "Controller Already Has An Animation with the Name: {0}", name);
            gxtDebug.Assert(!animations.ContainsValue(animation), "Controller Already Has This Animation with a Different Name: {0}", name);
            animations.Add(name, animation);
        }

        /// <summary>
        /// Removes an existing animation clip from the controller with the given name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>If removed</returns>
        public bool RemoveAnimation(string name)
        {
            bool wasRemoved = animations.Remove(name);
            if (!wasRemoved)
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Animation Clip with the Name: {0} Was Not Registered with the Controller", name);
            return wasRemoved;
        }

        /// <summary>
        /// Determines if the named animation is in the controller
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>If in controller</returns>
        public bool ContainsAnimationName(string name)
        {
            return animations.ContainsKey(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        public bool ContainsAnimation(gxtAnimation animation)
        {
            return animations.Values.Contains(animation);
        }

        /// <summary>
        /// Determines if the clip is inside the controller
        /// </summary>
        /// <param name="clip">Clip</param>
        /// <param name="animationName">Name of the animation, if found</param>
        /// <returns>If in controller</returns>
        public bool GetAnimationName(gxtAnimation animation, out string animationName)
        {
            bool wasFound = animations.ContainsValue(animation);
            animationName = ANIMATION_NOT_FOUND;
            if (wasFound)
            {
                foreach (KeyValuePair<string, gxtAnimation> pair in animations)
                {
                    if (pair.Value.Equals(animation))
                    {
                        animationName = pair.Key;
                        break;
                    }
                }
            }
            return wasFound;
        }

        /// <summary>
        /// Retrieves the animation clip with the given name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>The Animation, or null if it isn't found</returns>
        public gxtAnimation GetAnimation(string name)
        {
            if (animations.ContainsKey(name))
                return animations[name];
            else
                return null;
        }

        /// <summary>
        /// Determines if the named animation is currently playing
        /// If the animation is not found, it will return false
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>If playing</returns>
        public bool IsPlaying(string name)
        {
            if (animations.ContainsKey(name))
                return animations[name].Enabled && !animations[name].IsDone;
            else
                return false;
        }

        /// <summary>
        /// Determines if the named animation is done playing
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>If done playing</returns>
        public bool IsDone(string name)
        {
            if (animations.ContainsKey(name))
                return animations[name].IsDone;
            else
                return false;
        }

        /// <summary>
        /// Starts/Continues an animation of the given name
        /// If the animation is already playing nothing changes
        /// If the animation is paused it will be enabled
        /// If an animation is at the end, it will be restarted
        /// </summary>
        /// <param name="name">Animation Clip Name</param>
        /// <param name="stopAllOtherClips">If other animations are forcibly stopped</param>
        public bool Play(string name, bool stopAllOtherAnimations = false)
        {
            bool found = animations.ContainsKey(name);
            if (!found)
                return false;

            if (!stopAllOtherAnimations)
            {
                if (animations[name].IsDone)
                    animations[name].Reset(true);
                else
                    animations[name].Enabled = true;
            }
            else
            {
                foreach (KeyValuePair<string, gxtAnimation> anim in animations)
                {
                    if (anim.Key != name)
                        anim.Value.Reset(false);
                    else
                    {
                        if (anim.Value.IsDone)
                            anim.Value.Reset(true);
                        else
                            anim.Value.Enabled = true;
                        found = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Simultaneously plays every animation in the controller
        /// </summary>
        public void PlayAll()
        {
            foreach (gxtAnimation anim in animations.Values)
            {
                if (anim.IsDone)
                    anim.Reset(true);
                else
                    anim.Enabled = true;
            }
        }

        /// <summary>
        /// Stops every animation in the controller
        /// </summary>
        public void StopAll()
        {
            foreach (gxtAnimation clip in animations.Values)
            {
                clip.Reset(false);
            }
        }

        /// <summary>
        /// Stops the named animation in the controller
        /// </summary>
        /// <param name="name">Animation Name</param>
        public bool Stop(string name)
        {
            if (animations.ContainsKey(name))
            {
                animations[name].Reset(false);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pauses the named animation in the controller
        /// </summary>
        /// <param name="name">Animation Name</param>
        public bool Pause(string name)
        {
            if (animations.ContainsKey(name))
            {
                animations[name].Enabled = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pauses every animation in the controller
        /// </summary>
        public void PauseAll()
        {
            foreach (gxtAnimation anim in animations.Values)
            {
                anim.Enabled = false;
            }
        }

        /// <summary>
        /// Toggles the play/pause state of a named animation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stopAllOtherClips"></param>
        public bool Toggle(string name, bool stopAllOtherAnimations = false)
        {
            bool found = animations.ContainsKey(name);
            if (!found)
                return false;

            if (!stopAllOtherAnimations)
            {
                if (animations[name].Enabled)
                {
                    animations[name].Enabled = false;
                }
                else
                {
                    if (animations[name].IsDone)
                        animations[name].Reset(true);
                    else
                        animations[name].Enabled = true;
                }
            }
            else
            {
                foreach (KeyValuePair<string, gxtAnimation> anim in animations)
                {
                    if (anim.Key != name)
                    {
                        anim.Value.Reset(false);
                    }
                    else
                    {
                        if (!anim.Value.Enabled)
                        {
                            if (anim.Value.IsDone)
                                anim.Value.Reset(true);
                            else
                                anim.Value.Enabled = true;
                        }
                        else
                        {
                            anim.Value.Enabled = false;
                        }
                    }
                }
            }
            return true;
        }

        /*
        public void CrossFade(string name, TimeSpan fadeTime)
        {
            gxtDebug.Assert(animationClips.ContainsKey(name), "Cannot Fade To An Animation Not Added To The Controller: {0}", name);
            inTransition = true;
            elapsedTransitionTime = TimeSpan.Zero;
            transitionDuration = fadeTime;

            gxtAnimation transitionClip = animationClips[name];
            transitionTransform = transitionClip.Tweens[0].Keyframes[0].Transform;  // ugly

            startTransform = gxtNodeTransform.Identity;
            startTransform.Translation = transitionClip.Tweens[0].Node.Position;


        }
        */

        public void Update(GameTime gameTime)
        {
            if (!enabled)
                return;
            /*
            if (inTransition)
            {
                elapsedTransitionTime += gameTime.ElapsedGameTime;
            }
            */
        }

        public void LateUpdate(GameTime gameTime)
        {

        }

        public Type GetTargetType()
        {
            return typeof(gxtAnimation);
            //return animationClips.GetType();
        }
    }
}
