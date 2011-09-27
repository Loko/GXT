using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT.Input
{
    /// <summary>
    /// This special compararer will avoid ugly/expensive casting
    /// Based on reading available here:
    /// http://www.xnawiki.com/index.php?title=Enumerations_as_Dictionary_Keys
    /// http://blog.nickgravelyn.com/2009/04/net-misconceptions-part-1/
    /// </summary>
    public class gxtPlayerIndexEqualityComparer : gxtSingleton<gxtPlayerIndexEqualityComparer>, IEqualityComparer<PlayerIndex>
    {
        /// <summary>
        /// Custom equals call, no casting
        /// </summary>
        /// <param name="x">PlayerIndex x</param>
        /// <param name="y">PlayerIndex y</param>
        /// <returns>Boolean Equality</returns>
        public bool Equals(PlayerIndex x, PlayerIndex y)
        {
            return x == y;
        }

        /// <summary>
        /// Hash code calculation
        /// </summary>
        /// <param name="obj">PlayerIndex obj</param>
        /// <returns>Hash Code</returns>
        public int GetHashCode(PlayerIndex obj)
        {
            return obj.GetHashCode();
        }
    }
}
