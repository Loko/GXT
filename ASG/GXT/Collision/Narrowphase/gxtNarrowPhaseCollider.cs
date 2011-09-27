using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A base narrow phase collider
    /// Has common algorithms for point containment and raycasting
    /// Specific object - object intersection algorithms (e.g. GJK, SAT) can 
    /// be implemented in a sub class
    /// </summary>
    public abstract class gxtNarrowPhaseCollider : gxtINarrowPhaseCollider
    {
        //public const int DEFAULT_MAX_ITERATIONS = 75;
        //public const float DEFAULT_TOLERANCE = 1e-8f;

        public abstract int MaxIterations { get; set; }
        public abstract float Tolerance { get; set; }

        public gxtNarrowPhaseCollider()
        {
            //MaxIterations = 75;
            //Tolerance = 1e-8f;
        }

        #region Contains
        /// <summary>
        /// An efficient contains point function whoich uses a binary search 
        /// and assumes the input of a counter clockwise polygon
        /// </summary>
        /// <param name="polygon">Polygon</param>
        /// <param name="pt">Pt</param>
        /// <returns>If Inside</returns>
        public virtual bool Contains(ref gxtPolygon polygon, Vector2 pt)
        {
            int low = 0, high = polygon.v.Length;
            do
            {
                int mid = (low + high) / 2;
                if (gxtMath.IsCCWTriangle(polygon.v[0], polygon.v[mid], pt))
                    low = mid;
                else
                    high = mid;
            } while (low + 1 < high);

            if (low == 0 || high == polygon.v.Length) return false;

            return gxtMath.IsCCWTriangle(polygon.v[low], polygon.v[high], pt);
        }
        #endregion Contains

        public abstract bool Intersects(ref gxtPolygon polygonA, Vector2 centroidA, ref gxtPolygon polygonB, Vector2 centroidB);

        public abstract bool Collide(ref gxtPolygon polygonA, Vector2 centroidA, ref gxtPolygon polygonB, out gxtCollisionResult collisionResult);

        #region RayCasting
        public bool RayIntersectsSegment(gxtRay ray, Vector2 pt0, Vector2 pt1, float tmax = float.MaxValue)
        {
            Vector2 seg = pt1 - pt0;
            Vector2 segPerp = gxtMath.LeftPerp(seg);
            float perpDotd = Vector2.Dot(ray.Direction, segPerp);
            if (gxtMath.Equals(perpDotd, 0.0f, float.Epsilon))
                return false;

            Vector2 d = pt0 - ray.Origin;

            float t = Vector2.Dot(segPerp, d) / perpDotd;
            float s = Vector2.Dot(gxtMath.LeftPerp(ray.Direction), d) / perpDotd;

            return t >= 0.0f && t <= tmax && s >= 0.0f && s <= 1.0f;
        }

        /// <summary>
        /// A ray - line segment intersection test
        /// Used as a support function for polygon raycasting
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="pt0">First Point of the Segment</param>
        /// <param name="pt1">Second Point of the Segment</param>
        /// <param name="t">Distance</param>
        /// <param name="tmax">Max Distance</param>
        /// <returns>If intersecting</returns>
        public bool RayIntersectsSegment(gxtRay ray, Vector2 pt0, Vector2 pt1, out float t, float tmax = float.MaxValue)
        {
            Vector2 seg = pt1 - pt0;
            Vector2 segPerp = gxtMath.LeftPerp(seg);
            float perpDotd = Vector2.Dot(ray.Direction, segPerp);
            if (gxtMath.Equals(perpDotd, 0.0f, float.Epsilon))
            {
                t = float.MaxValue;
                return false;
            }

            Vector2 d = pt0 - ray.Origin;

            t = Vector2.Dot(segPerp, d) / perpDotd;
            float s = Vector2.Dot(gxtMath.LeftPerp(ray.Direction), d) / perpDotd;

            return t >= 0.0f && t <= tmax && s >= 0.0f && s <= 1.0f;
        }

        /// <summary>
        /// Performs a ray cast on the polygon given an optional tmax value
        /// Only process the information in the rayHit structure if Intersection = true,
        /// which is specified in the gxtRayHit instance and the return value.  If the ray origin is 
        /// inside the Polygon it is not considered an intersection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="polygon">Polygon</param>
        /// <param name="rayHit">Ray Hit Info</param>
        /// <param name="tmax">Max T Value</param>
        /// <returns>If Intersecting</returns>
        public virtual bool RayCast(gxtRay ray, ref gxtPolygon polygon, out gxtRayHit rayHit, float tmax = float.MaxValue)
        {
            rayHit = new gxtRayHit();
            rayHit.Distance = tmax;

            // if a crossing is within tmax
            bool intersection = false;
            // temp holder for segment distance
            float distance;
            // number of crossings, regardless of tmax
            int crossings = 0;

            // log a message when we run a raycast on a very detailed polygon
            // to indicate that the results may not be perfect
            int testIterations = polygon.NumVertices;
            if (MaxIterations < polygon.NumVertices)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Polygon vertices exceeds max collider iterations.  Not all segments will be tested!");
                testIterations = MaxIterations;
            }

            for (int j = polygon.NumVertices - 1, i = 0; i < testIterations; j = i, i++)
            {
                if (RayIntersectsSegment(ray, polygon.v[j], polygon.v[i], out distance))
                {
                    crossings++;
                    if (distance <= rayHit.Distance)
                    {
                        intersection = true;

                        rayHit.Distance = distance;
                        rayHit.Point = ray.GetPoint(distance);

                        // right perp assumes CCW polygon winding
                        Vector2 edge = polygon.v[j] - polygon.v[i];
                        rayHit.Normal = Vector2.Normalize(gxtMath.RightPerp(edge));
                    }
                }
            }
            // raycast algorithm
            rayHit.Intersection = intersection && crossings > 0 && crossings % 2 == 0;
            return rayHit.Intersection;
        }
        #endregion RayCasting
    }
}
