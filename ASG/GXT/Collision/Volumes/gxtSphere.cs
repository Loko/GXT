using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GXT
{
    /// <summary>
    /// Bounding Sphere (2D)
    /// Has a center point and a radius which extends in every direction
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public struct gxtSphere : IEquatable<gxtSphere>
    {
        #region Fields
        [ContentSerializer]
        private Vector2 c;  // Center point
        [ContentSerializer]
        private float r;    // Radius

        /// <summary>
        /// Position of the center point
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 Position { get { return c; } set { c = value; } }

        /// <summary>
        /// Radius.  MUST BE POSITIVE!
        /// </summary>
        [ContentSerializerIgnore]
        public float Radius { get { return r; } set { r = value; } }

        /// <summary>
        /// Diameter of the Sphere
        /// </summary>
        public float Diameter { get { return r * 2.0f; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Clones another Sphere
        /// </summary>
        /// <param name="sphere">Sphere to clone</param>
        public gxtSphere(gxtSphere sphere)
        {
            c = sphere.c;
            r = sphere.r;
        }

        /// <summary>
        /// Constructs a sphere with given center point and radius
        /// </summary>
        /// <param name="center">Center point</param>
        /// <param name="radius">Radius</param>
        public gxtSphere(Vector2 center, float radius)
        {
            this.c = center;
            this.r = radius;
        }
        #endregion Constructors

        #region Update
        /// <summary>
        /// Translates the position of the Sphere by the given amount
        /// </summary>
        /// <param name="trans">Translation Vector</param>
        public void Translate(Vector2 t)
        {
            c += t;
        }

        /// <summary>
        /// Translation overload
        /// </summary>
        /// <param name="x">X translation</param>
        /// <param name="y">Y translation</param>
        public void Translate(float x, float y)
        {
            Translate(new Vector2(x, y));
        }

        /// <summary>
        /// Updates with newly returned sphere given position
        /// </summary>
        /// <param name="position">Position of underlying geom</param>
        /// <param name="rotation">Rotation of underlying geom</param>
        /// <param name="localSphere">Local untransformed sphere</param>
        /// <returns>Transformed sphere</returns>
        public static gxtSphere Update(Vector2 position, float rotation, gxtSphere localSphere)
        {
            Vector2 c = position;
            float cos = gxtMath.Cos(rotation);
            float sin = gxtMath.Sin(rotation);

            float cX = cos * localSphere.c.X - sin * localSphere.c.Y;
            float cY = sin * localSphere.c.X + cos * localSphere.c.Y;

            return new gxtSphere(c + new Vector2(cX, cY), localSphere.r);
        }

        /// <summary>
        /// Merges two spheres
        /// </summary>
        /// <param name="a">Sphere A</param>
        /// <param name="b">Sphere B</param>
        /// <returns>Merged sphere</returns>
        public static gxtSphere Merge(gxtSphere a, gxtSphere b)
        {
            Vector2 d = a.c - b.c;
            float distSq = d.LengthSquared();
            if (gxtMath.Sqrt(a.r - b.r) >= distSq)
            {
                if (a.r >= b.r)
                    return a;
                else
                    return b;
            }
            else
            {
                float dist = gxtMath.Sqrt(distSq);
                float r = (dist + a.r + b.r) * 0.5f;
                Vector2 c = a.c;
                if (dist > 0.0f)
                    c += ((r - a.r) / dist) * d;
                return new gxtSphere(c, r);
            }
        }

        /// <summary>
        /// Checks if sphere is valid.  Tests will fail if numbers are corrupted.
        /// Useful for debugging purposes.
        /// </summary>
        /// <returns>Valid numbers</returns>
        public bool IsValid()
        {
            return !gxtMath.IsNaN(c.X) && !gxtMath.IsNaN(c.Y) && !gxtMath.IsNaN(r);
        }

        /// <summary>
        /// Checks if sphere is empty
        /// </summary>
        /// <returns>Negative radius?</returns>
        public bool IsEmpty()
        {
            return r < 0.0f;
        }
        #endregion Update

        #region ClosestPoint/Distance
        /// <summary>
        /// Finds closest point on the sphere to a point in space.  If point is within
        /// the sphere, this method will return the point itself.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Closest point on the sphere</returns>
        public Vector2 ClosestPtOnSphere(Vector2 point)
        {
            Vector2 diff = c - point;
            float dist = diff.Length();

            if (dist <= r)
                return point;   // Check if point is inside sphere
            else
                return point + ((dist - r) / dist) * diff;  // adds point to component vector
        }

        /// <summary>
        /// Scalar positive distance from a point in space to the closest point
        /// on the sphere.  If point is inside it will return a distance of zero.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Scalar distance between point and sphere</returns>
        public float DistanceToPoint(Vector2 point)
        {
            float dist = Vector2.Distance(c, point);
            if (dist <= r)
                return 0.0f;
            else
                return dist - r;
        }

        /// <summary>
        /// Squared version of of DistanceToPoint.  May be useful for some calculations involving 
        /// intersection/containment with other volumes but is not cheaper than the standard distance to
        /// point method in this struct and generally should be avoided.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Squared scalar distance between point and sphere</returns>
        public float DistanceToPointSquared(Vector2 point)
        {
            float dist = DistanceToPoint(point);
            return dist * dist;
            /*
             * This is how the math would work in an isolated DistanceSquared Function
             * But it's faster to just square the value of DistanceToPoint() 
            float dist = Vector2.Distance(c, point);
            if (dist <= r)
                return 0.0f;
            else
                return (dist * dist) - (2.0f * dist * r) + (r * r);
            */
        }
        #endregion ClosestPoint/Distance

        #region Intersection/Containment
        /// <summary>
        /// Intersection test between this and another sphere
        /// </summary>
        /// <param name="other">Other sphere</param>
        /// <returns>If intersecting</returns>
        public bool Intersects(gxtSphere other)
        {
            float radiusSum = r + other.r;
            float dX = c.X - other.c.X, dY = c.Y - other.c.Y;
            return dX * dX + dY * dY <= radiusSum * radiusSum;
        }

        /// <summary>
        /// Intersection test between this and another AABB
        /// </summary>
        /// <param name="aabb">AABB</param>
        /// <returns>If intersecting</returns>
        public bool IntersectsAABB(gxtAABB aabb)
        {
            return aabb.DistanceToPointSquared(c) <= r * r;
        }

        /// <summary>
        /// Intersection test between this and another OBB
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <returns>If intersecting</returns>
        public bool IntersectsOBB(gxtOBB obb)
        {
            return obb.IntersectsSphere(this);
        }

        /// <summary>
        /// Static intersection test between two spheres.
        /// </summary>
        /// <param name="a">First sphere</param>
        /// <param name="b">Second sphere</param>
        /// <returns>If Intersecting</returns>
        public static bool Intersects(gxtSphere a, gxtSphere b)
        {
            float radiusSum = a.r + b.r;
            float dX = a.c.X - b.c.X, dY = a.c.Y - b.c.Y;
            return dX * dX + dY * dY <= radiusSum * radiusSum;
        }

        /// <summary>
        /// Static intersection test between two spheres.  Result determines
        /// value of passed in boolean.
        /// </summary>
        /// <param name="a">First AABB</param>
        /// <param name="b">Second AABB</param>
        /// <param name="result">Result Boolean</param>
        public static void Intersects(gxtSphere a, gxtSphere b, out bool result)
        {
            float radiusSum = a.r + b.r;
            result = Vector2.DistanceSquared(a.c, b.c) <= radiusSum * radiusSum;
        }

        /// <summary>
        /// Determines if a point is within the bounds of the sphere.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>If point is within this sphere</returns>
        public bool Contains(Vector2 point)
        {
            return Vector2.DistanceSquared(c, point) <= r * r;
        }

        /// <summary>
        /// Determines if an entire sphere is contained within this sphere.
        /// </summary>
        /// <param name="other">Other sphere</param>
        /// <returns>If sphere is fully within this sphere</returns>
        public bool Contains(gxtSphere other)
        {
            float dist = Vector2.Distance(c, other.c);
            return dist + other.r <= r;
        }
        #endregion Intersection/Containment

        #region Operators/Overrides
        /// <summary>
        /// Compares another sphere for equality.  True if center point and radius are
        /// both equal.
        /// </summary>
        /// <param name="other">Other sphere</param>
        /// <returns>If Equal</returns>
        public bool Equals(gxtSphere other)
        {
            return c.Equals(other.c) && r.Equals(other.r);
        }

        /// <summary>
        /// Compares object to this sphere.  Must be a sphere and have
        /// equal center points and radii.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>If Equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is gxtSphere)
                return Equals((gxtSphere)obj);
            else
                return false;
        }

        /// <summary>
        /// Hash code
        /// </summary>
        /// <returns>Hash code sum of center point and radius</returns>
        public override int GetHashCode()
        {
            return c.GetHashCode() + r.GetHashCode();
        }

        /// <summary>
        /// Equality operator ovverride
        /// </summary>
        /// <param name="a">First sphere</param>
        /// <param name="b">Second sphere</param>
        /// <returns>If Equal</returns>
        public static bool operator ==(gxtSphere a, gxtSphere b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Inequality operator override
        /// </summary>
        /// <param name="a">First sphere</param>
        /// <param name="b">Second sphere</param>
        /// <returns>If Not Equal</returns>
        public static bool operator !=(gxtSphere a, gxtSphere b)
        {
            return !a.Equals(b);
        }
        #endregion Operators/Overrides
    }
}
