using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GXT.Rendering
{
    /// <summary>
    /// A Class To Handle Individual Debug Drawables
    /// Instances should only be created by the gxtDebugDrawer
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtDebugNode
    {
        /// <summary>
        /// Scene Node
        /// </summary>
        public gxtISceneNode Node { get; private set; }

        /// <summary>
        /// Timespan remaining until removal
        /// </summary>
        public TimeSpan TimeRemaining { get; private set; }

        /// <summary>
        /// The Scene Graph Id of this debug drawable belongs to
        /// </summary>
        public int SceneId { get; set; }

        /// <summary>
        /// Constructor initializing debugdrawable with given
        /// drawable and duration
        /// </summary>
        /// <param name="node">Scene Node</param>
        /// <param name="duration">Time Until Removal</param>
        /// <param name="id">Scene Graph id this drawable belongs to</param>
        public gxtDebugNode(gxtISceneNode node, TimeSpan duration, int id)
        {
            Node = node;
            TimeRemaining = duration;
            SceneId = id;
        }

        /// <summary>
        /// Updates remaining time
        /// Returns boolean indicating if time has expired
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        /// <returns>If time is expired</returns>
        public bool Update(GameTime gameTime)
        {
            TimeRemaining -= gameTime.ElapsedGameTime;
            return TimeRemaining <= TimeSpan.Zero;
        }
    }
}

