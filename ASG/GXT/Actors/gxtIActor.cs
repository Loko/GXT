using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A simple interface for world actors 
    /// intended to be added to instances of 
    /// gxtWorld
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public interface gxtIActor : gxtIUpdate
    {
        /// <summary>
        /// World Actor Is In
        /// </summary>
        gxtWorld World { get; }

        /// <summary>
        /// Position
        /// </summary>
        Vector2 Position { get; set; }
        
        /// <summary>
        /// Rotation
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Actor Type
        /// </summary>
        gxtHashedString Type { get; }

        // The following variables are under consideration
        // to be enforced for all actors
        //bool IsPhysical { get; }
        //bool IsGeometrical { get; }
        //gxtAABB GetAABB();
        //gxtOBB GetOBB();

        /// <summary>
        /// Disposes of the actor and removes 
        /// all associated components from the world
        /// </summary>
        void UnloadActor();
    }
}
