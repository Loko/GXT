using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A generic pair object-object (broadphase) intersection pair
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    public struct gxtBroadphaseCollisionPair<T>
    {
        public T objA, objB;

        /// <summary>
        /// Constructor taking pair of objects
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        public gxtBroadphaseCollisionPair(T a, T b)
        {
            // keeps order the same so equals works correctly
            if (a.GetHashCode() < b.GetHashCode())
            {
                objA = a;
                objB = b;
            }
            else
            {
                objA = b;
                objB = a;
            }
        }

        /// <summary>
        /// Equality override
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is gxtBroadphaseCollisionPair<T>)
                return Equals((gxtBroadphaseCollisionPair<T>)obj);
            return false;
        }

        /// <summary>
        /// IEquatable
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(gxtBroadphaseCollisionPair<T> other)
        {
            return other.objA.Equals(objA) && other.objB.Equals(objB);
        }

        /// <summary>
        /// HashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // multiplier prevents collisions
            const int MULTIPLIER = 10000;
            return (objA.GetHashCode() * MULTIPLIER + objB.GetHashCode());
        }
    }
}
