using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GXT.Rendering;

namespace GXT.Animation
{
    /// <summary>
    /// A tween is a collection of keyframes meant to operate on one node
    /// Proper interpolation between the active keyframes is determined in this class
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtAnimationClip
    {
        private gxtISceneNode node;
        private List<gxtKeyframe> keyframes;
        private gxtIMesh mesh;
        private Vector2[] uvCoords;
        private int currentFrame;
        // cache current tween duration?

        /// <summary>
        /// The scene node this tween operates on
        /// </summary>
        public gxtISceneNode Node { get { return node; } }

        /// <summary>
        /// Mesh this tween operates on
        /// </summary>
        public gxtIMesh Mesh { get { return mesh; } }

        /// <summary>
        /// Adds a keyframe to the tween
        /// </summary>
        /// <param name="keyFrame">Keyframe</param>
        /// <returns>Index of the keyframe, or -1 if it could not be inserted</returns>
        public int AddKeyframe(gxtKeyframe keyFrame)
        {
            gxtDebug.Assert(keyFrame != null, "Cannot add null keyframe!");

            // greedy
            if (keyframes.Count > 0)
            {
                if (keyFrame.LocalTime > keyframes[keyframes.Count - 1].LocalTime)
                {
                    keyframes.Add(keyFrame);
                    return keyframes.Count - 1;
                }
            }

            int insertIndex = 0;
            for (int i = 0; i < keyframes.Count; ++i)
            {
                if (keyFrame.LocalTime > keyframes[i].LocalTime)
                    insertIndex = i + 1;
                else if (keyFrame.LocalTime < keyframes[i].LocalTime)
                    break;
                else
                    return -1;
            }
            keyframes.Insert(insertIndex, keyFrame);
            return insertIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="localTime"></param>
        /// <param name="interpolationType"></param>
        /// <returns></returns>
        public int CreateKeyframe(gxtAnimationPose pose, float localTime, gxtAnimationInterpolationType interpolationType = gxtKeyframe.DEFAULT_INTERPOLATION_TYPE)
        {
            gxtKeyframe kf = new gxtKeyframe(pose, localTime, interpolationType);
            return AddKeyframe(kf);
        }

        /// <summary>
        /// Removes a keyframe from the tween
        /// </summary>
        /// <param name="keyframe">Keyframe</param>
        /// <returns>If the keyframe was successfully removed</returns>
        public bool RemoveKeyframe(gxtKeyframe keyframe)
        {
            return keyframes.Remove(keyframe);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemoveKeyframe(int index)
        {
            if (index >= 0 && index < keyframes.Count)
            {
                keyframes.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the keyframe at the given index
        /// Can return null if the index is out of range
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public gxtKeyframe GetKeyframe(int index)
        {
            if (index >= 0 && index < keyframes.Count)
                return keyframes[index];
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="mesh"></param>
        public gxtAnimationClip(gxtISceneNode node, gxtIMesh mesh)
        {
            gxtDebug.Assert(mesh.Material != null, "Meshes used in animation must have an attached material!");
            this.node = node;
            this.keyframes = new List<gxtKeyframe>();
            this.mesh = mesh;
            uvCoords = new Vector2[mesh.NumVertices];
            currentFrame = 0;
        }

        /// <summary>
        /// Picks the proper poses and interpolates between them as needed
        /// </summary>
        /// <param name="t">T</param>
        /// <param name="backwards">If moving backwards thru the animation</param>
        public void Update(float t, bool backwards = false)
        {
            int startFrameIndex = -1;
            int nextIndex;
            if (!backwards)
            {
                // find start index
                // TODO: do local search from the current frame??
                for (int i = 0; i < keyframes.Count; ++i)
                {
                    if (t >= keyframes[i].LocalTime)
                        startFrameIndex = i;
                    else
                        break;
                }

                if (startFrameIndex == -1)
                    return;

                if (startFrameIndex != currentFrame)
                {
                    CleanFrame(startFrameIndex);
                }

                nextIndex = gxtMath.IMin(startFrameIndex + 1, keyframes.Count - 1);
                if (startFrameIndex == nextIndex)
                {
                    SetFrame(startFrameIndex);
                }
                else
                {
                    // otherwise interpolate between the two keyframes
                    float tweenDuration = keyframes[nextIndex].LocalTime - keyframes[startFrameIndex].LocalTime;
                    float tweenT = (t - keyframes[startFrameIndex].LocalTime) / tweenDuration;
                    Interpolate(tweenT, startFrameIndex, nextIndex);
                }
            }
            else
            {
                t += 1.0f;
                // same as above but in reverse
                for (int i = keyframes.Count - 1; i >= 0; --i)
                {
                    if (t <= keyframes[i].LocalTime)
                        startFrameIndex = i;
                    else
                        break;
                }

                if (startFrameIndex == -1)
                    return;

                if (startFrameIndex != currentFrame)
                {
                    CleanFrame(startFrameIndex);
                }

                nextIndex = gxtMath.IMax(startFrameIndex - 1, 0);
                if (startFrameIndex == nextIndex)
                {
                    SetFrame(startFrameIndex);
                }
                else
                {
                    float tweenDuration = keyframes[startFrameIndex].LocalTime - keyframes[nextIndex].LocalTime;
                    float tweenT = (t - keyframes[startFrameIndex].LocalTime) / tweenDuration;
                    Interpolate(gxtMath.Abs(tweenT), startFrameIndex, nextIndex);
                }
            }
        }

        /// <summary>
        /// Resets the clip back to it's initial state
        /// </summary>
        /// <param name="backwards"></param>
        public void Reset(bool backwards)
        {
            if (keyframes.Count != 0)
            {
                if (backwards)
                    SetFrame(keyframes.Count - 1);
                else
                    SetFrame(0);
            }
        }

        /// <summary>
        /// Sets the pose at the given keyframe index
        /// </summary>
        /// <param name="index"></param>
        private void SetFrame(int index)
        {
            gxtAnimationPose tmp = keyframes[index].AnimationPose;
            node.Position = tmp.Translation;
            node.Rotation = tmp.Rotation;
            node.Scale = tmp.Scale;
            mesh.Material.ColorOverlay = tmp.ColorOverlay;

            if (tmp.InterpolateUVCoords)
            {
                for (int i = 0; i < tmp.UVCoordinates.Length; ++i)
                {
                    this.uvCoords[i] = tmp.UVCoordinates[i];
                }
                mesh.SetTextureCoordinates(uvCoords);
            }
        }

        /// <summary>
        /// Internal to animation clips.  Sets up variables when a new 
        /// frame is reached.
        /// </summary>
        /// <param name="index"></param>
        private void CleanFrame(int index)
        {
            gxtDebug.Assert(index >= 0 && index < keyframes.Count);
            if (currentFrame != index)
            {
                mesh.Material.ColorOverlay = keyframes[index].AnimationPose.ColorOverlay;
                if (keyframes[index].AnimationPose.UVCoordinates != null)
                {
                    Array.Resize(ref uvCoords, keyframes[index].AnimationPose.UVCoordinates.Length);
                    for (int i = 0; i < uvCoords.Length; ++i)
                    {
                        uvCoords[i] = keyframes[index].AnimationPose.UVCoordinates[i];
                    }
                    mesh.SetTextureCoordinates(uvCoords);
                }
                currentFrame = index;
            }
        }

        // shared temporary interpolation values
        // used as intermediaries for every tween
        private static Vector2 tmpPosition, tmpScale;
        private static float tmpRotation;
        private static Color tmpColor;
        private static gxtAnimationPose poseA, poseB;

        /// <summary>
        /// Performs linear interpolation between two keyframes at the given indices and a value of t
        /// </summary>
        /// <param name="tweenT">T</param>
        /// <param name="keyFrameIndexA">Index A</param>
        /// <param name="keyFrameIndexB">Index B</param>
        private void Interpolate(float tweenT, int keyFrameIndexA, int keyFrameIndexB)
        {
            poseA = keyframes[keyFrameIndexA].AnimationPose;
            poseB = keyframes[keyFrameIndexB].AnimationPose;

            if (keyframes[keyFrameIndexA].InterpolationType == gxtAnimationInterpolationType.SMOOTH_STEP)
            {
                // smooth step
                tmpPosition = gxtMath.SmoothStep(poseA.Translation, poseB.Translation, tweenT);
                tmpRotation = gxtMath.SmoothStep(poseA.Rotation, poseB.Rotation, tweenT);
                tmpScale = gxtMath.SmoothStep(poseA.Scale, poseB.Scale, tweenT);
                if (poseA.InterpolateColorOverlay)
                {
                    tmpColor = gxtMath.SmoothStep(poseA.ColorOverlay, poseB.ColorOverlay, tweenT);
                    mesh.Material.ColorOverlay = tmpColor;
                }
                if (poseA.InterpolateUVCoords)
                {
                    gxtDebug.SlowAssert(poseA.UVCoordinates != null && poseB.UVCoordinates != null, "Cannot interpolate between null UV Coordinates!");
                    gxtDebug.SlowAssert(poseA.UVCoordinates.Length == poseB.UVCoordinates.Length, "Cannot interpolate between UV Coordinate arrays of different sizes!");
                    for (int i = 0; i < poseA.UVCoordinates.Length; ++i)
                    {
                        this.uvCoords[i] = gxtMath.SmoothStep(poseA.UVCoordinates[i], poseB.UVCoordinates[i], tweenT);
                    }
                    mesh.SetTextureCoordinates(uvCoords);
                }
            }
            else if (keyframes[keyFrameIndexA].InterpolationType == gxtAnimationInterpolationType.LERP)
            {
                // lerp
                tmpPosition = gxtMath.Lerp(poseA.Translation, poseB.Translation, tweenT);
                tmpRotation = gxtMath.Lerp(poseA.Rotation, poseB.Rotation, tweenT);
                tmpScale = gxtMath.Lerp(poseA.Scale, poseB.Scale, tweenT);
                if (poseA.InterpolateColorOverlay)
                {
                    tmpColor = Color.Lerp(poseA.ColorOverlay, poseB.ColorOverlay, tweenT);
                    mesh.Material.ColorOverlay = tmpColor;
                }
                if (poseA.InterpolateUVCoords)
                {
                    gxtDebug.SlowAssert(poseA.UVCoordinates != null && poseB.UVCoordinates != null, "Cannot interpolate between null UV Coordinates!");
                    gxtDebug.SlowAssert(poseA.UVCoordinates.Length == poseB.UVCoordinates.Length, "Cannot interpolate between UV Coordinate arrays of different sizes!");
                    for (int i = 0; i < poseA.UVCoordinates.Length; ++i)
                    {
                        this.uvCoords[i] = gxtMath.Lerp(poseA.UVCoordinates[i], poseB.UVCoordinates[i], tweenT);
                    }
                    mesh.SetTextureCoordinates(uvCoords);
                }
            }
            else
            {
                // smoother step
                tmpPosition = gxtMath.SmootherStep(poseA.Translation, poseB.Translation, tweenT);
                tmpRotation = gxtMath.SmootherStep(poseA.Rotation, poseB.Rotation, tweenT);
                tmpScale = gxtMath.SmootherStep(poseA.Scale, poseB.Scale, tweenT);
                if (poseA.InterpolateColorOverlay)
                {
                    tmpColor = gxtMath.SmootherStep(poseA.ColorOverlay, poseB.ColorOverlay, tweenT);
                    mesh.Material.ColorOverlay = tmpColor;
                }
                if (poseA.InterpolateUVCoords)
                {
                    gxtDebug.SlowAssert(poseA.UVCoordinates != null && poseB.UVCoordinates != null, "Cannot interpolate between null UV Coordinates!");
                    gxtDebug.SlowAssert(poseA.UVCoordinates.Length == poseB.UVCoordinates.Length, "Cannot interpolate between UV Coordinate arrays of different sizes!");
                    for (int i = 0; i < poseA.UVCoordinates.Length; ++i)
                    {
                        this.uvCoords[i] = gxtMath.SmootherStep(poseA.UVCoordinates[i], poseB.UVCoordinates[i], tweenT);
                    }
                    mesh.SetTextureCoordinates(uvCoords);
                }
            }

            // set interpolated values
            node.SetAll(ref tmpPosition, tmpRotation, ref tmpScale);
        }
    }
}
