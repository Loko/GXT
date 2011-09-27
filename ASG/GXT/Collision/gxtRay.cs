using System;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A structure for holding the properties of a 2D ray with an origin 
    /// and a normalized direction vector
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public struct gxtRay
    {
        private Vector2 origin;
        private Vector2 direction;

        /// <summary>
        /// Origin
        /// </summary>
        public Vector2 Origin { get { return origin; } set { origin = value; } }

        /// <summary>
        /// Normalized Direction
        /// </summary>
        public Vector2 Direction { get { return direction; } set { direction = value; } }

        /// <summary>
        /// Constructs a ray with the given origin and direction
        /// </summary>
        /// <param name="origin">Start Point of the Ray</param>
        /// <param name="direction">Normalized Direction of the Ray</param>
        public gxtRay(Vector2 origin, Vector2 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        /// <summary>
        /// Gets a point on the ray at the given distance
        /// </summary>
        /// <param name="t">Distance</param>
        /// <returns>Point Along the Ray</returns>
        public Vector2 GetPoint(float t)
        {
            gxtDebug.Assert(t >= 0.0f);
            return origin + direction * t;
        }

        /// <summary>
        /// A fast boolean intersection test that omits an expensive square root call
        /// However, it does not return values for t and the contact point
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <returns>If intersecting</returns>
        public bool IntersectsSphere(gxtSphere sphere)
        {
            Vector2 m = origin - sphere.Position;
            float c = m.LengthSquared() - sphere.Radius * sphere.Radius;

            if (c <= 0.0f)
                return true;

            float b = Vector2.Dot(m, direction);

            if (b > 0.0f)
                return false;

            float disc = b * b - c;
            if (disc < 0.0f)
                return false;

            return true;
        }

        /// <summary>
        /// Sphere intersection test which calculates t and the intersection point
        /// The normal can later be found with Normalize(pt - sphere.Position)
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="t">Max Distance</param>
        /// <param name="pt">Contact Point</param>
        /// <returns>If intersecting</returns>
        public bool IntersectsSphere(gxtSphere sphere, out float t, out Vector2 pt)
        {
            Vector2 m = origin - sphere.Position;
            float b = Vector2.Dot(m, direction);
            float c = m.LengthSquared();

            if (c > 0.0f && b > 0.0f)
            {
                t = 0.0f;
                pt = origin;
                return false;
            }
            
            float discr = b * b - c;
            if (discr < 0.0f)
            {
                t = 0.0f;
                pt = origin;
                return false;
            }

            t = -b - gxtMath.Sqrt(discr);
            if (t < 0.0f)
                t = 0.0f;
            pt = GetPoint(t);
            return true;
        }

        /// <summary>
        /// A simple boolean ray-aabb intersection test with options 
        /// for max values of t and inside/outside checks
        /// </summary>
        /// <param name="aabb">AABB</param>
        /// <param name="tmax">Max Distance</param>
        /// <param name="insideIsCollision">If the ray origin inside the AABB still counts as a collision</param>
        /// <returns>If intersecting</returns>
        public bool IntersectsAABB(gxtAABB aabb, float tmax = float.MaxValue, bool insideIsCollision = false)
        {
            gxtDebug.Assert(tmax >= 0.0f);

            // special case, confirms a collision if the origin is inside the AABB
            // only set insideIsCollision to true if the AABB is for a more complex 
            // narrowphase shape (e.g. a polygon)
            if (insideIsCollision)
                if (aabb.Contains(origin))
                    return true;

            Vector2 absDirection = new Vector2(gxtMath.Abs(direction.X), gxtMath.Abs(direction.Y));
            
            Vector2 aabbMin = aabb.Min;
            Vector2 aabbMax = aabb.Max;

            float tmin = float.MinValue;
            //float tmax = float.MaxValue;

            if (absDirection.X < float.Epsilon)
            {
                // then we know it is parallel
                if (origin.X < aabbMin.X || origin.X > aabbMax.X)
                {
                    return false;
                }
            }
            else
            {
                float oneOverDX = 1.0f / direction.X;
                float t1X = (aabbMin.X - origin.X) * oneOverDX;
                float t2X = (aabbMax.X - origin.X) * oneOverDX;

                if (t1X > t2X)
                {
                    float holderTX = t1X;
                    t1X = t2X;
                    t2X = holderTX;
                }
                if (t1X > tmin)
                {
                    tmin = t1X;
                }

                tmax = gxtMath.Min(tmax, t2X);
                
                if (tmin > tmax)
                    return false;
            }
            if (absDirection.Y < float.Epsilon)
            {
                // then we know it is parallel
                if (origin.Y < aabbMin.Y || origin.Y > aabbMax.Y)
                {
                    return false;
                }
            }
            else
            {
                float oneOverDY = 1.0f / direction.Y;
                float t1Y = (aabbMin.Y - origin.Y) * oneOverDY;
                float t2Y = (aabbMax.Y - origin.Y) * oneOverDY;

                if (t1Y > t2Y)
                {
                    float holderTY = t1Y;
                    t1Y = t2Y;
                    t2Y = holderTY;
                }
                if (t1Y > tmin)
                {
                    tmin = t1Y;
                }

                tmax = gxtMath.Min(tmax, t2Y);

                if (tmin > tmax)
                    return false;
            }

            if (tmin < 0.0f || tmax < tmin)
                return false;

            return true;
        }

        /// <summary>
        /// Checks for intersection with the given AABB
        /// Distance and contact point are also calculated
        /// </summary>
        /// <param name="aabb">AABB</param>
        /// <param name="t">Distance of contact</param>
        /// <param name="pt">Contact Point</param>
        /// <param name="tmax">Max T</param>
        /// <param name="insideIsCollision">If an origin inside the AABB still counts as a collision</param>
        /// <returns>If intersecting</returns>
        public bool IntersectsAABB(gxtAABB aabb, out float t, out Vector2 pt, float tmax = float.MaxValue, bool insideIsCollision = false)
        {
            gxtDebug.Assert(tmax >= 0.0f);

            pt = origin;
            t = 0.0f;

            // special case, confirms a collision if the origin is inside the AABB
            // only set insideIsCollision to true if the AABB is for a more complex 
            // narrowphase shape (e.g. a polygon)
            if (insideIsCollision)
                if (aabb.Contains(origin))
                    return true;

            Vector2 absDirection = new Vector2(gxtMath.Abs(direction.X), gxtMath.Abs(direction.Y));

            Vector2 aabbMin = aabb.Min;
            Vector2 aabbMax = aabb.Max;

            float tmin = float.MinValue;

            if (absDirection.X < float.Epsilon)
            {
                // then we know it is parallel
                if (origin.X < aabbMin.X || origin.X > aabbMax.X)
                {
                    return false;
                }
            }
            else
            {
                float oneOverDX = 1.0f / direction.X;
                float t1X = (aabbMin.X - origin.X) * oneOverDX;
                float t2X = (aabbMax.X - origin.X) * oneOverDX;

                if (t1X > t2X)
                {
                    float holderTX = t1X;
                    t1X = t2X;
                    t2X = holderTX;
                }
                if (t1X > tmin)
                {
                    tmin = t1X;
                }

                tmax = gxtMath.Min(tmax, t2X);

                if (tmin > tmax)
                    return false;
            }
            if (absDirection.Y < float.Epsilon)
            {
                // then we know it is parallel
                if (origin.Y < aabbMin.Y || origin.Y > aabbMax.Y)
                {
                    return false;
                }
            }
            else
            {
                float oneOverDY = 1.0f / direction.Y;
                float t1Y = (aabbMin.Y - origin.Y) * oneOverDY;
                float t2Y = (aabbMax.Y - origin.Y) * oneOverDY;

                if (t1Y > t2Y)
                {
                    float holderTY = t1Y;
                    t1Y = t2Y;
                    t2Y = holderTY;
                }
                if (t1Y > tmin)
                {
                    tmin = t1Y;
                }

                tmax = gxtMath.Min(tmax, t2Y);

                if (tmin > tmax)
                    return false;
            }

            if (tmin < 0.0f || tmax < tmin)
                return false;

            t = tmin;
            pt = GetPoint(t);
            return true;
        }
    }
}
