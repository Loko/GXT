using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GXT
{
    /// <summary>
    /// Oriented Bounding Box (2D)
    /// Has a center point, half-width extents and a local X and Y axis
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public struct gxtOBB : IEquatable<gxtOBB>
    {
        #region Fields
        [ContentSerializer]
        private Vector2 c;  // Center Point
        [ContentSerializer]
        private Vector2 r;  // Positive Half-Width Extents

        [ContentSerializerIgnore]
        public Vector2 localX; // Unit Vector For Local X Axis
        [ContentSerializerIgnore]
        public Vector2 localY; // Unit Vector For Local Y Axis

        /// <summary>
        /// Position of the center point
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 Position { get { return c; } set { c = value; } }

        /// <summary>
        /// Half width and height values from the center.  MUST BE POSITIVE!
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 Extents { get { return r; } set { r = value; } }

        /// <summary>
        /// Scalar Rotation of the OBB in radians
        /// </summary>
        public float Rotation
        {
            get
            {
                return gxtMath.Atan2(localX.Y, localX.X);
            }
            set
            {
                localX = new Vector2(gxtMath.Cos(value), gxtMath.Sin(value));
                localY = new Vector2(-localX.Y, localX.X);
            }
        }

        /// <summary>
        /// Full Width of the OBB
        /// </summary>
        public float Width { get { return r.X * 2.0f; } }

        /// <summary>
        /// Full Height of the OBB
        /// </summary>
        public float Height { get { return r.Y * 2.0f; } }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Clones another OBB
        /// </summary>
        /// <param name="obb">OBB to clone</param>
        public gxtOBB(gxtOBB obb)
        {
            c = obb.c;
            r = obb.r;
            localX = obb.localX;
            localY = obb.localY;
        }

        /// <summary>
        /// Constructors OBB with center point and half-width values from the center.
        /// Sets rotation to zero.
        /// </summary>
        /// <param name="center">Center point</param>
        /// <param name="extents">Half-width extents</param>
        public gxtOBB(Vector2 center, Vector2 extents)
        {
            c = center;
            r = extents;
            localX = new Vector2(1, 0);
            localY = new Vector2(0, 1);
        }

        /// <summary>
        /// Constructs OBB with precomputed center point, half-width values, and axises.
        /// </summary>
        /// <param name="center">Center point</param>
        /// <param name="extents">Half-width extents</param>
        /// <param name="xAxis">Normalized Local X Axis</param>
        /// <param name="yAxis">Normalized Local Y Axis</param>
        public gxtOBB(Vector2 center, Vector2 extents, Vector2 xAxis, Vector2 yAxis)
        {
            c = center;
            r = extents;
            localX = xAxis;
            localY = yAxis;
        }

        /// <summary>
        /// Constructs OBB with center point, half width values, and a given rotation (in radians).
        /// </summary>
        /// <param name="center">Center point</param>
        /// <param name="extents">Half-width extents</param>
        /// <param name="rotation">Init rotation (radians)</param>
        public gxtOBB(Vector2 center, Vector2 extents, float rotation)
        {
            c = center;
            r = extents;
            localX = new Vector2(1, 0);
            localY = new Vector2(0, 1);
            Rotation = rotation;
        }
        #endregion Constructors

        #region Update
        /// <summary>
        /// Translates the OBB by the given amount
        /// </summary>
        /// <param name="tans">Translation Vector</param>
        public void Translate(Vector2 trans)
        {
            c += trans;
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
        /// Checks if OBB is valid.  Tests will fail if any components are NaN
        /// and the axis do not have a unit length of 1
        /// </summary>
        /// <returns>Valid?</returns>
        public bool IsValid()
        {
            return !gxtMath.IsNaN(c.X) && !gxtMath.IsNaN(c.Y) && !gxtMath.IsNaN(localX.X) && !gxtMath.IsNaN(localX.Y) &&
                !gxtMath.IsNaN(localY.X) && !gxtMath.IsNaN(localY.Y) && !gxtMath.IsNaN(r.X) && !gxtMath.IsNaN(r.Y) &&
                gxtMath.Equals(localX.Length(), 1.0f, float.Epsilon) && gxtMath.Equals(localY.Length(), 1.0f, float.Epsilon);
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
        /// Finds closest point on OBB to a point in space.  If point
        /// is within OBB, this method will return the point itself.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Closest point on the OBB</returns>
        public Vector2 ClosestPtOnOBB(Vector2 point)
        {
            Vector2 diff = point - c;
            Vector2 result = c;
            float distX, distY;

            // Find and clamp x distance
            distX = Vector2.Dot(diff, localX);
            if (distX > r.X) distX = r.X;
            else if (distX < -r.X) distX = -r.X;
            // Multiply x distance by x axis
            result += distX * localX;

            // Find and clamp y distance
            distY = Vector2.Dot(diff, localY);
            if (distY > r.Y) distY = r.Y;
            else if (distY < -r.Y) distY = -r.Y;
            // Multiply y distance by y axis
            result += distY * localY;

            // Since we start from the center and project
            // outwards we don't need a special contains check
            return result;
        }

        /// <summary>
        /// Scalar positive distance from a point in space to the closest point
        /// on the OBB.  If point is inside the OBB, it will return 
        /// a distance of zero.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Scalar distance between point and OBB</returns>
        public float DistanceToPoint(Vector2 point)
        {
            return gxtMath.Sqrt(DistanceToPointSquared(point));
        }

        /// <summary>
        /// Squared version of DistanceToPoint.  Cheaper absent the square
        /// root routine.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>Squared scalar distance between point and OBB</returns>
        public float DistanceToPointSquared(Vector2 point)
        {
            Vector2 diff = point - c;
            float sqDist = 0.0f;

            float tmpDist, excess;

            // On X axis
            tmpDist = Vector2.Dot(diff, localX);
            excess = 0.0f;

            // Clamp X
            if (tmpDist < -r.X)
                excess = tmpDist + r.X;
            else if (tmpDist > r.X)
                excess = tmpDist - r.X;

            sqDist += excess * excess;

            // On Y axis
            // Reassign tmpDist and excess
            tmpDist = Vector2.Dot(diff, localY);
            excess = 0.0f;

            // Clamp Y
            if (tmpDist < -r.Y)
                excess = tmpDist + r.Y;
            else if (tmpDist > r.Y)
                excess = tmpDist - r.Y;

            sqDist += excess * excess;
            return sqDist;
        }
        #endregion ClosestPoint/Distance

        #region SAT
        /// <summary>
        /// Gets scalar value of OBB projected onto the given axis
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <param name="axis">Axis</param>
        /// <returns>ProjectedScalar</returns>
        private static float GetProjectedScalar(gxtOBB obb, Vector2 axis)
        {
            return obb.r.X * gxtMath.Abs(Vector2.Dot(axis, obb.localX)) + obb.r.Y * Math.Abs(Vector2.Dot(axis, obb.localY));
        }

        /// <summary>
        /// Gets scalar of this OBB projected onto a given axis
        /// </summary>
        /// <param name="axis">Axis</param>
        /// <returns>ProjectedScalar</returns>
        private float GetProjectedScalar(Vector2 axis)
        {
            return r.X * gxtMath.Abs(Vector2.Dot(axis, localX)) + r.Y * gxtMath.Abs(Vector2.Dot(axis, localY));
        }

        /// <summary>
        /// Checks if a seperating plane exists between the two given OBBs and axis
        /// </summary>
        /// <param name="a">OBB a</param>
        /// <param name="b">OBB b</param>
        /// <param name="axis">Axis</param>
        /// <returns>If Seperating Axis</returns>
        private static bool SeperatedOnAxis(gxtOBB a, gxtOBB b, Vector2 axis)
        {
            float projDist = gxtMath.Abs(Vector2.Dot(a.c - b.c, axis));
            return GetProjectedScalar(a, axis) + GetProjectedScalar(b, axis) < projDist;
        }

        /// <summary>
        /// Checks if a seperating plane exists between this OBB and the given axis
        /// </summary>
        /// <param name="a">OBB</param>
        /// <param name="axis">Axis</param>
        /// <returns>If Seperating Axis</returns>
        private bool SeperatedOnAxis(gxtOBB other, Vector2 axis)
        {
            float projDist = gxtMath.Abs(Vector2.Dot(c - other.c, axis));
            return GetProjectedScalar(axis) + other.GetProjectedScalar(axis) < projDist;
        }
        #endregion SAT

        #region Intersection/Containment
        /// <summary>
        /// Intersection test between this and another OBB
        /// </summary>
        /// <param name="other">Other OBB</param>
        /// <returns>If Intersecting</returns>
        public bool Intersects(gxtOBB other)
        {
            if (SeperatedOnAxis(other, localX)) return false;
            if (SeperatedOnAxis(other, localY)) return false;
            if (other.SeperatedOnAxis(this, other.localX)) return false;
            if (other.SeperatedOnAxis(this, other.localY)) return false;
            return true;
        }

        /// <summary>
        /// Static intersection test between two OBBs
        /// </summary>
        /// <param name="a">First OBB</param>
        /// <param name="b">Second OBB</param>
        /// <returns>If Intersecting</returns>
        public static bool Intersects(ref gxtOBB a, ref gxtOBB b)
        {
            if (SeperatedOnAxis(a, b, a.localX)) return false;
            if (SeperatedOnAxis(a, b, a.localY)) return false;
            if (SeperatedOnAxis(a, b, b.localX)) return false;
            if (SeperatedOnAxis(a, b, b.localY)) return false;
            return true;
        }

        /// <summary>
        /// Static intersection test between two OBBs.  Result determines 
        /// value of passed in boolean.
        /// </summary>
        /// <param name="a">First OBB</param>
        /// <param name="b">Second OBB</param>
        /// <param name="result">Result Boolean</param>
        public static void Intersects(gxtOBB a, gxtOBB b, out bool result)
        {
            if (SeperatedOnAxis(a, b, a.localX)) { result = false; return; }
            if (SeperatedOnAxis(a, b, a.localY)) { result = false; return; }
            if (SeperatedOnAxis(a, b, b.localX)) { result = false; return; }
            if (SeperatedOnAxis(a, b, b.localY)) { result = false; return; }
            result = true;
        }

        
        /// <summary>
        /// Intersection Test between this and a sphere
        /// </summary>
        /// <param name="sphere">Bounding Sphere</param>
        /// <returns>If Intersecting</returns>
        public bool IntersectsSphere(gxtSphere sphere)
        {
            Vector2 closestPoint = ClosestPtOnOBB(sphere.Position);
            return sphere.Contains(closestPoint);
        }
        

        /// <summary>
        /// Intersection Test between this and an AABB
        /// </summary>
        /// <param name="aabr">AABB</param>
        /// <returns>If Intersecting</returns>
        public bool IntersectsAABB(gxtAABB aabb)
        {
            return Intersects(new gxtOBB(aabb.Position, aabb.Extents));
        }

        /// <summary>
        /// Determines if point is within the bounds of the OBB
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <returns>If point is within OBB</returns>
        public bool Contains(Vector2 point)
        {
            Vector2 diff = point - c;
            if (gxtMath.Abs(Vector2.Dot(diff, localX)) > r.X) return false;
            if (gxtMath.Abs(Vector2.Dot(diff, localY)) > r.Y) return false;
            return true;
        }

        /// <summary>
        /// Determines if an entire OBB is contained within this OBB.
        /// </summary>
        /// <param name="other">Other OBB</param>
        /// <returns>If OBB is fully contained within this OBB</returns>
        public bool Contains(gxtOBB other)
        {
            Vector2 diff = other.c - c;
            float projDist = gxtMath.Abs(Vector2.Dot(diff, localX));
            if (other.GetProjectedScalar(localX) + projDist > GetProjectedScalar(localX)) return false;

            projDist = gxtMath.Abs(Vector2.Dot(diff, localY));
            if (other.GetProjectedScalar(localY) + projDist > GetProjectedScalar(localY)) return false;

            projDist = gxtMath.Abs(Vector2.Dot(diff, other.localX));
            if (other.GetProjectedScalar(localX) + projDist > GetProjectedScalar(other.localX)) return false;

            projDist = gxtMath.Abs(Vector2.Dot(diff, other.localY));
            if (other.GetProjectedScalar(localY) + projDist > GetProjectedScalar(other.localY)) return false;

            return true;
        }
        #endregion Intersection/Containment

        #region Operators/Overrides
        /// <summary>
        /// Compares another OBB for equality.  True if center point, half-width extents, 
        /// and orientation axis are all equal.
        /// </summary>
        /// <param name="other">Other OBB</param>
        /// <returns>If Equal</returns>
        public bool Equals(gxtOBB other)
        {
            return c.Equals(other.c) && r.Equals(other.r) && localX.Equals(other.localX) && localY.Equals(other.localY);
        }

        /// <summary>
        /// Compares object to this OBB.  Must be an OBB and have
        /// equal center points, halfwidth extents, and orientation axis 
        /// to be true.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>If Equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is gxtOBB)
                return Equals((gxtOBB)obj);
            else
                return false;
        }

        /// <summary>
        /// Hash code
        /// </summary>
        /// <returns>Hash code sum of center point, extents, and axis</returns>
        public override int GetHashCode()
        {
            return c.GetHashCode() + r.GetHashCode() + localX.GetHashCode() + localY.GetHashCode();
        }

        /// <summary>
        /// Equality operator override
        /// </summary>
        /// <param name="a">First OBB</param>
        /// <param name="b">Second OBB</param>
        /// <returns>If Equal</returns>
        public static bool operator ==(gxtOBB a, gxtOBB b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Inequality operator override
        /// </summary>
        /// <param name="a">First OBB</param>
        /// <param name="b">Second OBB</param>
        /// <returns>If Not Equal</returns>
        public static bool operator !=(gxtOBB a, gxtOBB b)
        {
            return !a.Equals(b);
        }
        #endregion Operators/Overrides
    }
}
