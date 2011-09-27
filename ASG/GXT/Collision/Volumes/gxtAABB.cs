using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GXT
{
    /// <summary>
    /// Axis Aligned Bounding Box (2D)
    /// Uses the center-extents representation
    /// Very effective for fast broadphase tests
    /// 
    /// Author: Jeff Lansing
    /// </summary>

    /*
     * -----------------------------------------
     * |                                       |
     * |                                       |
     * |                                       |
     * |                                       |
     * |                c                      |
     * |                . - - - - r.X - - - -  |
     * |                |                      |
     * |                |                      |
     * |               r.Y                     |
     * |                |                      |
     * |                |                      |
     * -----------------------------------------
     */
    public struct gxtAABB : IEquatable<gxtAABB>
    {
        #region Fields
        [ContentSerializer]
        private Vector2 c;  // Center Point
        [ContentSerializer]
        private Vector2 r;  // Positive Half-Width Extents

        /// <summary>
        /// Position of the center point
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 Position { get { return c; } set { c = value; } }

        /// <summary>
        /// Half width and height values from the center
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 Extents { get { return r; } set { r = value; } }

        /// <summary>
        /// Min (Top Left) Vert of the AABB
        /// </summary>
        public Vector2 Min { get { return c - r; } }

        /// <summary>
        /// Max (Bottom Right) Vert of the AABB
        /// </summary>
        public Vector2 Max { get { return c + r; } }

        /// <summary>
        /// Scalar side positions
        /// </summary>
        public float Left { get { return c.X - r.X; } }
        public float Top { get { return c.Y - r.Y; } }
        public float Right { get { return c.X + r.X; } }
        public float Bottom { get { return c.Y + r.Y; } }

        /// <summary>
        /// Dimensions
        /// </summary>
        public float Width { get { return r.X * 2.0f; } }
        public float Height { get { return r.Y * 2.0f; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Clones another AABB
        /// </summary>
        /// <param name="aabb">AABB to clone</param>
        public gxtAABB(gxtAABB aabb)
        {
            c = aabb.c;
            r = aabb.r;
        }

        /// <summary>
        /// Constructs AABB with center point and half-width values from center
        /// </summary>
        /// <param name="c">Center point</param>
        /// <param name="r">Half-Width Extents</param>
        public gxtAABB(Vector2 center, Vector2 extents)
        {
            this.c = center;
            this.r = extents;
        }

        /// <summary>
        /// Constructs AABB with center point and half width values from center (as floats)
        /// </summary>
        /// <param name="center">Center point</param>
        /// <param name="halfWidth">Half Width</param>
        /// <param name="halfHeight">Half Height</param>
        public gxtAABB(Vector2 center, float halfWidth, float halfHeight)
        {
            this.c = center;
            this.r = new Vector2(halfWidth, halfHeight);
        }
        #endregion Constructors

        #region Update
        /// <summary>
        /// Translates the position of the AABB by the given amount
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
        /// Updates with newly returned AABB given the position, orientation,
        /// and the untransformed local AABB.
        /// </summary>
        /// <param name="position">Position of underlying geom</param>
        /// <param name="rotation">Rotation of underlying geom</param>
        /// <param name="localAABR">Local untransformed AABB</param>
        /// <returns>Transformed AABB</returns>
        public static gxtAABB Update(Vector2 position, float rotation, gxtAABB localAABB)
        {
            Vector2 c = position;
            float cos = gxtMath.Cos(rotation); 
            float sin = gxtMath.Sin(rotation);

            float cX = cos * localAABB.c.X - sin * localAABB.c.Y;
            float cY = sin * localAABB.c.X + cos * localAABB.c.Y;

            c += new Vector2(cX, cY);

            float absCos = gxtMath.Abs(cos);
            float absSin = gxtMath.Abs(sin);

            Vector2 r = new Vector2(localAABB.r.X * absCos + localAABB.r.Y * absSin, localAABB.r.X * absSin + localAABB.r.Y * absCos);

            return new gxtAABB(c, r);
        }

        /// <summary>
        /// Merges two AABBs into one
        /// </summary>
        /// <param name="a">AABB A</param>
        /// <param name="b">AABB B</param>
        /// <returns>Merge AABB</returns>
        public static gxtAABB Merge(gxtAABB a, gxtAABB b)
        {
            float minX = gxtMath.Min(a.Left, b.Left);
            float minY = gxtMath.Min(a.Top, b.Top);
            float maxX = gxtMath.Max(a.Right, b.Right);
            float maxY = gxtMath.Max(a.Bottom, b.Bottom);
            Vector2 center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            return new gxtAABB(center, new Vector2(maxX - center.X, maxY - center.Y));
        }

        /// <summary>
        /// Gets this AABB with an applied rotation.  Only use if this is the
        /// untransformed AABB and you are certain the origin of rotation for the
        /// underlying geometry is the same as the AABB
        /// </summary>
        /// <param name="rotation">rotation (in radians)</param>
        /// <returns>AABB after rotation</returns>
        public gxtAABB GetRotatedAABB(float rotation)
        {
            float cos = gxtMath.Abs(gxtMath.Cos(rotation));
            float sin = gxtMath.Abs(gxtMath.Sin(rotation));
            return new gxtAABB(c, new Vector2(r.X * cos + r.Y * sin, r.X * sin + r.Y * cos));
        }

        /// <summary>
        /// Checks if AABB is valid.  Tests will fail if numbers are corrupted.
        /// Useful for debugging purposes.
        /// </summary>
        /// <returns>Valid numbers</returns>
        public bool IsValid()
        {
            return !gxtMath.IsNaN(c.X) && !gxtMath.IsNaN(c.Y) && !gxtMath.IsNaN(r.X) && !gxtMath.IsNaN(r.Y);
        }

        /// <summary>
        /// Checks if box is empty
        /// </summary>
        /// <returns>Negative extents?</returns>
        public bool IsEmpty()
        {
            return r.X < 0 || r.Y < 0;
        }
        #endregion Update

        #region ClosestPoint/Distance
        /// <summary>
        /// Finds closest point on AABB to a point in space.  If point
        /// is within AABB, this method will return the point itself.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Closest point on the AABB</returns>
        public Vector2 ClosestPtOnAABB(Vector2 point)
        {
            float x = point.X, y = point.Y;

            // clamp x
            if (x < Left) x = Left;
            else if (x > Right) x = Right;

            // clamp y
            if (y < Top) y = Top;
            else if (y > Bottom) y = Bottom;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Scalar positive distance from a point in space to the closest point
        /// on the AABB.  If point is inside the AABB, it will return 
        /// a distance of zero.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Scalar distance between point and AABB</returns>
        public float DistanceToPoint(Vector2 point)
        {
            return gxtMath.Sqrt(DistanceToPointSquared(point));
        }

        /// <summary>
        /// Squared version of DistanceToPoint.  Cheaper absent the square
        /// root routine.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Squared scalar distance between point and AABB</returns>
        public float DistanceToPointSquared(Vector2 point)
        {
            float sqDist = 0.0f;

            if (point.X < Left) sqDist += (Left - point.X) * (Left - point.X);
            else if (point.X > Right) sqDist += (point.X - Right) * (point.X - Right);

            if (point.Y < Top) sqDist += (Top - point.Y) * (Top - point.Y);
            else if (point.Y > Bottom) sqDist += (point.Y - Bottom) * (point.Y - Bottom);

            return sqDist;
        }
        #endregion ClosestPoint/Distance

        #region Intersection/Containment
        /// <summary>
        /// Intersection test between this and another AABB
        /// </summary>
        /// <param name="other">Other AABB</param>
        /// <returns>If Intersecting</returns>
        public bool Intersects(gxtAABB other)
        {
            if (gxtMath.Abs(c.X - other.c.X) > r.X + other.r.X) return false;
            if (gxtMath.Abs(c.Y - other.c.Y) > r.Y + other.r.Y) return false;
            return true;
        }

        /// <summary>
        /// Static intersection test between two AABBs
        /// </summary>
        /// <param name="a">First AABB</param>
        /// <param name="b">Second AABB</param>
        /// <returns>If Intersecting</returns>
        public static bool Intersects(gxtAABB a, gxtAABB b)
        {
            if (gxtMath.Abs(a.c.X - b.c.X) > a.r.X + b.r.X) return false;
            if (gxtMath.Abs(a.c.Y - b.c.Y) > a.r.Y + b.r.Y) return false;
            return true;
        }

        /// <summary>
        /// Static intersection test between two AABBs.  Result determines 
        /// value of passed in boolean.
        /// </summary>
        /// <param name="a">First AABB</param>
        /// <param name="b">Second AABB</param>
        public static bool Intersects(ref gxtAABB a, ref gxtAABB b)
        {
            if (gxtMath.Abs(a.c.X - b.c.X) > a.r.X + b.r.X) return false;
            if (gxtMath.Abs(a.c.Y - b.c.Y) > a.r.Y + b.r.Y) return false;
            return true;
        }

        
        /// <summary>
        /// Intersection Test between this and a sphere
        /// </summary>
        /// <param name="sphere">Bounding Sphere</param>
        /// <returns>If Intersecting</returns>
        public bool IntersectsSphere(gxtSphere sphere)
        {
            return DistanceToPointSquared(sphere.Position) <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Intersection test between this and an OBB
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <returns>If intersecting</returns>
        public bool IntersectsOBB(gxtOBB obb)
        {
            return obb.IntersectsAABB(this);
        }

        /// <summary>
        /// Intersection test between this and an AABB
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="tmax"></param>
        /// <param name="insideIsCollision"></param>
        /// <returns></returns>
        public bool IntersectsRay(gxtRay ray, float tmax = float.MaxValue, bool insideIsCollision = false)
        {
            return ray.IntersectsAABB(this, tmax, insideIsCollision);
        }

        public bool IntersectsRay(gxtRay ray, out float t, out Vector2 pt, float tmax = float.MaxValue, bool insideIsCollision = false)
        {
            return ray.IntersectsAABB(this, out t, out pt, tmax, insideIsCollision);
        }

        /// <summary>
        /// Determines if a point is within the bounds of the AABB
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>If point is within AABB</returns>
        public bool Contains(Vector2 point)
        {
            if (gxtMath.Abs(point.X - c.X) > r.X) return false;
            if (gxtMath.Abs(point.Y - c.Y) > r.Y) return false;
            return true;
        }

        /// <summary>
        /// Determines if an entire AABB is contained within this AABB
        /// </summary>
        /// <param name="other">Other AABB</param>
        /// <returns>If AABB is fully within this AABB</returns>
        public bool Contains(gxtAABB other)
        {
            if (gxtMath.Abs(c.X - other.c.X) + other.r.X > r.X) return false;
            if (gxtMath.Abs(c.Y - other.c.Y) + other.r.Y > r.Y) return false;
            return true;
        }
        #endregion Intersection/Containment

        #region Operators/Overrides
        /// <summary>
        /// Compares another AABB for equality.  True if center points and half-width
        /// extents are both equal.
        /// </summary>
        /// <param name="other">Other AABB</param>
        /// <returns>If Equal</returns>
        public bool Equals(gxtAABB other)
        {
            return c.Equals(other.c) && r.Equals(other.r);
        }

        /// <summary>
        /// Compares object to this AABB.  Must be AABB and have
        /// equal center points and halfwidth extents to be true.
        /// </summary>
        /// <param name="o">Object to compare</param>
        /// <returns>If Equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is gxtAABB)
                return Equals((gxtAABB)obj);
            else
                return false;
        }

        /// <summary>
        /// Hash code
        /// </summary>
        /// <returns>Hash code sum of center point and half width extents</returns>
        public override int GetHashCode()
        {
            return c.GetHashCode() + r.GetHashCode();
        }

        /// <summary>
        /// Equality operator override
        /// </summary>
        /// <param name="a">First AABB</param>
        /// <param name="b">Second AABB</param>
        /// <returns>If Equal</returns>
        public static bool operator ==(gxtAABB a, gxtAABB b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Inequality operator override
        /// </summary>
        /// <param name="a">First AABB</param>
        /// <param name="b">Second AABB</param>
        /// <returns>If Not Equal</returns>
        public static bool operator !=(gxtAABB a, gxtAABB b)
        {
            return !a.Equals(b);
        }
        #endregion Operators/Overrides

        public static readonly gxtAABB ZERO_EXTENTS_AABB = new gxtAABB(Vector2.Zero, Vector2.Zero);
        public static readonly gxtAABB MAX_EXTENTS_AABB = new gxtAABB(Vector2.Zero, new Vector2(float.MaxValue));
        public static readonly gxtAABB MIN_EXTENTS_AABB = new gxtAABB(Vector2.Zero, new Vector2(float.MinValue));
    }
}
