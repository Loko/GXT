#define GXT_DEBUG_DRAW_SIMPLEX
#define GXT_DEBUG_DRAW_CONTACTS
#define GXT_USE_PHYSICS_SCALING
#undef GXT_DEBUG_DRAW_SIMPLEX
//#undef GXT_DEBUG_DRAW_CONTACTS
//#undef GXT_USE_PHYSICS_SCALING

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
#if (GXT_USE_PHYSICS_SCALING)
using GXT.Physics;
#endif

namespace GXT
{
    public class gxtGJKCollider
    {
        // default maximum iterations
        public static readonly int MAX_ITERATIONS = 200;
        // default tolerance
        public static readonly float EPA_TOLERANCE = 1e-8F;//gxtMath.Sqrt(float.Epsilon);

        public static readonly int MAX_RAY_ITERATIONS = 50;


        private static int NextIndex(int i, int size)
        {
            if (i == size - 1)
                return 0;
            return i + 1;
        }

        private static int PrevIndex(int i, int size)
        {
            if (i == 0)
                return size - 1;
            return i - 1;
        }

        private static Vector2 GetEdge(List<Vector2> simplex, int i)
        {
            return simplex[NextIndex(i, simplex.Count)] - simplex[i];
        }

        private static Vector2 GetEdgeNormal(List<Vector2> simplex, int winding, int i)
        {
            Vector2 edge = GetEdge(simplex, i);
            if (winding < 0)
                edge = gxtMath.RightPerp(edge);
            else
                edge = gxtMath.LeftPerp(edge);
            edge.Normalize();
            return edge;
        }

        // PASS IN WINDING??
        private static Vector2 GetVertexNormal(List<Vector2> simplex, int winding, int i)
        {
            Vector2 e0 = GetEdgeNormal(simplex, winding, i);
            Vector2 e1 = GetEdgeNormal(simplex, winding, PrevIndex(i, simplex.Count));
            return Vector2.Normalize(e0 + e1);
        }

        /// <summary>
        /// Finds the farthest point in a given direction
        /// Search direction does not need to be normalized
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="nd"></param>
        /// <returns></returns>
        public static Vector2 FarthestPointInDirection(ref gxtPolygon polygon, Vector2 nd)
        {
            //gxtDebug.Assert(gxtMath.Equals(nd.Length(), 1.0f, float.Epsilon), "Direction vector must be normalized");
            int farthestIndex = 0;
            float farthestDistance = Vector2.Dot(polygon.v[0], nd);
            float tmpDistance;
            // info could be calculated here to affect search direction
            for (int i = 1; i < polygon.NumVertices; i++)
            {
                tmpDistance = Vector2.Dot(polygon.v[i], nd);
                if (tmpDistance > farthestDistance)
                {
                    farthestDistance = tmpDistance;
                    farthestIndex = i;
                }
            }
            return polygon.v[farthestIndex];
        }

        /// <summary>
        /// Creates a support point for the simplex given the polygons and a given search direction
        /// Used to build the simplex as part of the GJK and EPA algorithms
        /// </summary>
        /// <param name="polyA"></param>
        /// <param name="polyB"></param>
        /// <param name="nd"></param>
        /// <returns></returns>
        public static Vector2 SupportPt(ref gxtPolygon polyA, ref gxtPolygon polyB, Vector2 nd)
        {            
            Vector2 p1 = FarthestPointInDirection(ref polyA, nd);
            Vector2 p2 = FarthestPointInDirection(ref polyB, -nd);
            return p1 - p2;
        }

        /// <summary>
        /// Gets the farthest points in one call
        /// </summary>
        /// <param name="polyA"></param>
        /// <param name="polyAPt"></param>
        /// <param name="polyB"></param>
        /// <param name="polyBPt"></param>
        /// <param name="nd"></param>
        /// <returns></returns>
        public static Vector2 SupportPt(ref gxtPolygon polyA, out Vector2 polyAPt, ref gxtPolygon polyB, out Vector2 polyBPt, Vector2 nd)
        {
            polyAPt = FarthestPointInDirection(ref polyA, nd);
            polyBPt = FarthestPointInDirection(ref polyB, -nd);
            return polyAPt - polyBPt;
        }

        /// <summary>
        /// Determines if the simplex contains the origin if it is a triangle or 
        /// modifies the search direction if it is a line segment
        /// 
        /// This function may remove points from the simplex
        /// </summary>
        /// <param name="simplex"></param>
        /// <param name="nd"></param>
        /// <returns></returns>
        private static bool SimplexContainsOrigin(List<Vector2> simplex, ref Vector2 nd)
        {
            // last pt and its negation
            Vector2 a = simplex[simplex.Count - 1];
            Vector2 ao = -a;
            if (simplex.Count == 3)
            {
                // voroni regions cases for triangle
                Vector2 b = simplex[0];
                Vector2 c = simplex[1];
                Vector2 ab = b - a;
                Vector2 ac = c - a;
                Vector2 abPerp = gxtMath.TripleProduct(ac, ab, ab);
                Vector2 acPerp = gxtMath.TripleProduct(ab, ac, ac);
                float acLocation = Vector2.Dot(acPerp, ao);
                if (acLocation >= 0.0f)
                {
                    simplex.RemoveAt(1);
                    nd = acPerp;
                }
                else
                {
                    float abLocation = Vector2.Dot(abPerp, ao);
                    if (abLocation < 0.0f)
                    {
                        return true;
                    }
                    else
                    {
                        simplex.RemoveAt(0);
                        nd = abPerp;
                    }
                }
            }
            else
            {
                // assumed line segment case
                // set nd to equal the perp of the line segment facing the origin
                Vector2 b = simplex[0];
                Vector2 ab = b - a;
                nd = gxtMath.TripleProduct(ab, ao, ab);
                if (nd == Vector2.Zero)
                {
                    nd = gxtMath.RightPerp(ab);
                }
            }
            return false;
        }

        private static bool SimplexContainsOrigin(List<Vector2> simplex, List<Vector2> pointsOnA, List<Vector2> pointsOnB, ref Vector2 nd)
        {
            // last pt and its negation
            Vector2 a = simplex[simplex.Count - 1];
            Vector2 ao = -a;
            if (simplex.Count == 3)
            {
                // voroni regions cases for triangle
                Vector2 b = simplex[0];
                Vector2 c = simplex[1];
                Vector2 ab = b - a;
                Vector2 ac = c - a;
                Vector2 abPerp = gxtMath.TripleProduct(ac, ab, ab);
                Vector2 acPerp = gxtMath.TripleProduct(ab, ac, ac);
                float acLocation = Vector2.Dot(acPerp, ao);
                if (acLocation >= 0.0f)
                {
                    simplex.RemoveAt(1);
                    pointsOnA.RemoveAt(1);
                    pointsOnB.RemoveAt(1);
                    nd = acPerp;
                }
                else
                {
                    float abLocation = Vector2.Dot(abPerp, ao);
                    if (abLocation < 0.0f)
                    {
                        return true;
                    }
                    else
                    {
                        simplex.RemoveAt(0);
                        pointsOnA.RemoveAt(0);
                        pointsOnB.RemoveAt(0);
                        nd = abPerp;
                    }
                }
            }
            else
            {
                // assumed line segment case
                // set nd to equal the perp of the line segment facing the origin
                Vector2 b = simplex[0];
                Vector2 ab = b - a;
                nd = gxtMath.TripleProduct(ab, ao, ab);
                if (nd == Vector2.Zero)
                {
                    nd = gxtMath.RightPerp(ab);
                }
            }
            return false;
        }


        public static Vector2 ClosestPointOnSegment(Vector2 p0, Vector2 p1, Vector2 pt)
        {
            Vector2 d = pt - p0;
            Vector2 line = p1 - p0;
            float ab2 = line.LengthSquared();
            if (ab2 <= float.Epsilon)
                return p0;
            float proj = Vector2.Dot(d, line);
            float t = proj / ab2;
            return p0 + line * t;
        }

        public static bool TriangleContainsOrigin(Vector2 a, Vector2 b, Vector2 c)
        {
            float sa = gxtMath.Cross2D(a, b);
            float sb = gxtMath.Cross2D(b, c);
            float sc = gxtMath.Cross2D(c, a);
            return (sa * sb > 0.0f && sa * sc > 0.0f);
        }

        public static void FindClosestEdge(List<Vector2> simplex, int winding, out int edgeIndex, out float distance, out Vector2 normal)
        {
            edgeIndex = simplex.Count;
            distance = float.MaxValue;
            normal = Vector2.Zero;
            
            for (int j = simplex.Count - 1, i = 0; i < simplex.Count; j = i, i++)
            {
                Vector2 edgeVector = simplex[i] - simplex[j];
                Vector2 edgeNormal;
                if (winding < 0)
                    edgeNormal = gxtMath.RightPerp(edgeVector);
                else
                    edgeNormal = gxtMath.LeftPerp(edgeVector);
                edgeNormal.Normalize();

                float edgeDistance = gxtMath.Abs(Vector2.Dot(simplex[j], edgeNormal));
                if (edgeDistance < distance)
                {
                    distance = edgeDistance;
                    normal = edgeNormal;
                    // might be j instead
                    edgeIndex = i;
                }
            }
        }

        public static void FindClosestSimplexPoints(List<Vector2> simplex, List<Vector2> pointsOnA, List<Vector2> pointsOnB, int winding, out Vector2 sa, out Vector2 sb, out int indexA, out int indexB)
        {
            gxtDebug.Assert(simplex.Count == 3 && pointsOnA.Count == 3 && pointsOnB.Count == 3);

            float distance = float.MaxValue;
            sa = simplex[2];
            sb = simplex[0];
            indexA = 2;
            indexB = 2;

            for (int j = simplex.Count - 1, i = 0; i < simplex.Count; j = i, i++)
            {
                Vector2 edgeVector = simplex[i] - simplex[j];
                Vector2 edgeNormal;
                if (winding < 0)
                    edgeNormal = gxtMath.RightPerp(edgeVector);
                else
                    edgeNormal = gxtMath.LeftPerp(edgeVector);
                edgeNormal.Normalize();

                float edgeDistance = gxtMath.Abs(Vector2.Dot(simplex[j], edgeNormal));
                if (edgeDistance < distance)
                {
                    sa = simplex[j];
                    sb = simplex[i];
                    indexA = j;
                    indexB = i;
                }
            }
        }

        public static void FindClosestSimplexPoints(List<Vector2> simplex, List<Vector2> pointsOnA, List<Vector2> pointsOnB, int winding, out Vector2 sa, out Vector2 sb, out Vector2 ptA, out Vector2 ptB)
        {
            gxtDebug.Assert(simplex.Count == 3 && pointsOnA.Count == 3 && pointsOnB.Count == 3);

            float distance = float.MaxValue;
            sa = simplex[2];
            sb = simplex[0];
            ptA = pointsOnA[2];
            ptB = pointsOnB[2];

            for (int j = simplex.Count - 1, i = 0; i < simplex.Count; j = i, i++)
            {
                Vector2 edgeVector = simplex[i] - simplex[j];
                Vector2 edgeNormal;
                if (winding < 0)
                    edgeNormal = gxtMath.RightPerp(edgeVector);
                else
                    edgeNormal = gxtMath.LeftPerp(edgeVector);
                edgeNormal.Normalize();

                float edgeDistance = gxtMath.Abs(Vector2.Dot(simplex[j], edgeNormal));
                if (edgeDistance < distance)
                {
                    sa = simplex[j];
                    sb = simplex[i];
                    ptA = pointsOnA[j];
                    ptB = pointsOnB[j];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s0">Simplex Point 0 Of Closest Edge</param>
        /// <param name="s1">Simplex Point 1 Of Closest Edge</param>
        /// <param name="ptA0"></param>
        /// <param name="ptA1"></param>
        /// <param name="ptB0"></param>
        /// <param name="ptB1"></param>
        /// <param name="cpA"></param>
        /// <param name="cpB"></param>
        public static void FindClosestPointsHelper(Vector2 s0, Vector2 s1, Vector2 ptA0, Vector2 ptA1, Vector2 ptB0, Vector2 ptB1, out Vector2 cpA, out Vector2 cpB)
        {
            cpA = new Vector2();
            cpB = new Vector2();

            Vector2 lambda = s1 - s0;
            if (gxtMath.Abs(lambda.X) <= float.Epsilon && gxtMath.Abs(lambda.Y) <= float.Epsilon)
            {
                cpA = ptA0;
                cpB = ptB0;
            }
            else
            {
                float lambdaSq = lambda.LengthSquared();
                float l2 = -Vector2.Dot(lambda, s0) / lambdaSq;
                float l1 = 1.0f - l2;

                if (l1 < 0.0f)
                {
                    cpA = ptA1;
                    cpB = ptB1;
                }
                else if (l2 < 0.0f)
                {
                    cpA = ptA0;
                    cpA = ptB0;
                }
                else
                {
                    cpA = ptA0 * l1 + ptA1 * l2;
                    cpB = ptB0 * l1 + ptB1 * l2;
                    //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Interpolation Case");
                }
            }
        }

        public static void FindClosestPointsSpecial(List<Vector2> simplex, List<Vector2> pointsOnA, List<Vector2> pointsOnB, out Vector2 cpA, out Vector2 cpB)
        {
            Vector2 p0 = ClosestPointOnSegment(simplex[0], simplex[1], Vector2.Zero);
            Vector2 p1 = ClosestPointOnSegment(simplex[1], simplex[2], Vector2.Zero);
            Vector2 p2 = ClosestPointOnSegment(simplex[2], simplex[0], Vector2.Zero);

            float m0 = p0.LengthSquared();
            float m1 = p1.LengthSquared();
            float m2 = p2.LengthSquared();

            if (m0 < m1)
            {
                if (m0 < m2)
                {
                    // 0 and 1
                    FindClosestPointsHelper(simplex[0], simplex[1], pointsOnA[0], pointsOnA[1], pointsOnB[0], pointsOnB[1], out cpA, out cpB);
                }
                else
                {
                    // 2 and 0
                    FindClosestPointsHelper(simplex[2], simplex[0], pointsOnA[2], pointsOnA[0], pointsOnB[2], pointsOnB[0], out cpA, out cpB);
                }
            }
            else
            {
                if (m1 < m2)
                {
                    // 1 and 2
                    FindClosestPointsHelper(simplex[1], simplex[2], pointsOnA[1], pointsOnA[2], pointsOnB[1], pointsOnB[2], out cpA, out cpB);
                }
                else
                {
                    // 2 and 0
                    FindClosestPointsHelper(simplex[2], simplex[0], pointsOnA[2], pointsOnA[0], pointsOnB[2], pointsOnB[0], out cpA, out cpB);
                }
            }

        }

        public static void FindClosestPointsSpecial2(List<Vector2> simplex, List<Vector2> pointsOnA, List<Vector2> pointsOnB, out Vector2 cpA, out Vector2 cpB)
        {
            float minDistance = float.MaxValue;
            int index = simplex.Count - 1;

            for (int i = 0, j = simplex.Count - 1; i < simplex.Count; j = i, i++)
            {
                Vector2 edgePoint = ClosestPointOnSegment(simplex[j], simplex[i], Vector2.Zero);
                float edgeDistance = edgePoint.LengthSquared();
                if (edgeDistance < minDistance)
                {
                    minDistance = edgeDistance;
                    index = j;
                }
            }

            int nextIndex = (index + 1) % simplex.Count;
            FindClosestPointsHelper(simplex[index], simplex[nextIndex], pointsOnA[index], pointsOnA[nextIndex], pointsOnB[index], pointsOnB[nextIndex], out cpA, out cpB);
        }

        public static void FindClosestPolygonPoints(Vector2 sa, Vector2 sb, Vector2 polyAPt0, Vector2 polyAPt1, Vector2 polyBPt0, Vector2 polyBPt1, int winding, out Vector2 cpA, out Vector2 cpB)
        {
            Vector2 lambda = sb - sa;
            if (lambda == Vector2.Zero)
            {
                cpA = polyAPt0;
                cpB = polyBPt0;
            }
            else
            {
                float lambdaSq = Vector2.Dot(lambda, lambda);
                float lambda2 = Vector2.Dot(-lambda, sa) / lambdaSq;
                float lambda1 = 1.0f - lambda2;

                if (lambda1 < 0.0f)
                {
                    cpA = polyAPt1;
                    cpB = polyBPt1;
                }
                else if (lambda2 < 0.0f)
                {
                    cpA = polyAPt0;
                    cpB = polyBPt0;
                }
                else
                {
                    Vector2 pt1;
                    Vector2 pt2;
                    pt1 = (polyAPt0 * lambda1) + (polyAPt1 * lambda2);
                    pt2 = (polyBPt0 * lambda1) + (polyBPt1 * lambda2);
                    /*
                    pt1.X = polyAPt0.X * lambda1 + polyAPt1.X * lambda2;
                    pt1.Y = polyAPt0.Y * lambda1 + polyAPt1.Y * lambda2;

                    pt2.X = polyBPt0.X * lambda1 + polyBPt1.X * lambda2;
                    pt2.Y = polyBPt0.Y * lambda1 + polyBPt1.X * lambda2;
                    */
                    cpA = pt1;
                    cpB = pt2;
                    
                }
            }
        }

        public static void FindClosestPoints(List<Vector2> simplex, List<Vector2> pointsOnA, List<Vector2> pointsOnB, out Vector2 cpA, out Vector2 cpB)
        {
            cpA = new Vector2();
            cpB = new Vector2();

            // find lambda1 and lambda2
            Vector2 l = simplex[1] - simplex[0];

            // check if a and b are the same point
            if (l == Vector2.Zero)
            {
                // then the closest points are a or b support points
                cpA = pointsOnA[0];
                cpB = pointsOnB[0];
            }
            else
            {
                // otherwise compute lambda1 and lambda2
                float ll = l.LengthSquared();
                float l2 = Vector2.Dot(-l, simplex[0]) / ll;
                float l1 = 1.0f - l2;

                // check if either lambda1 or lambda2 is less than zero
                if (l1 < 0)
                {
                    // if lambda1 is less than zero then that means that
                    // the support points of the Minkowski point B are
                    // the closest points
                    cpA = pointsOnA[1];
                    cpB = pointsOnB[1];
                    //p1.set(b.p1);
                    //p2.set(b.p2);
                }
                else if (l2 < 0)
                {
                    // if lambda2 is less than zero then that means that
                    // the support points of the Minkowski point A are
                    // the closest points
                    cpA = pointsOnA[0];
                    cpB = pointsOnB[0];
                    //p1.set(a.p1);
                    //p2.set(a.p2);
                }
                else
                {
                    // compute the closest points using lambda1 and lambda2
                    // this is the expanded version of
                    // p1 = a.p1.multiply(l1).add(b.p1.multiply(l2));
                    // p2 = a.p2.multiply(l1).add(b.p2.multiply(l2));
                    cpA = (pointsOnA[0] * l1) + (pointsOnA[1] * l2);
                    cpB = (pointsOnB[0] * l1) + (pointsOnB[1] * l2);
                    /*
                    p1.x = a.p1.x * l1 + b.p1.x * l2;
                    p1.y = a.p1.y * l1 + b.p1.y * l2;
                    p2.x = a.p2.x * l1 + b.p2.x * l2;
                    p2.y = a.p2.y * l1 + b.p2.y * l2;
                    */
                }
            }
            // set the new points in the separation object
            //s.point1 = p1;
            //s.point2 = p2;
        }

        public static void FindClosestVertex(List<Vector2> simplex, int winding, out int edgeIndex, out float distance, out Vector2 normal)
        {
            edgeIndex = 0;
            distance = float.MaxValue;
            // refer to way edge normal is calculated in gxtPolygon
            normal = Vector2.Zero;
            float lengthSquared = float.MaxValue;
            float tmpDistance;
            Vector2 tmpNormal;
            for (int i = 0; i < simplex.Count; i++)
            {
                tmpNormal = GetVertexNormal(simplex, winding, i);
                tmpDistance = gxtMath.Abs(Vector2.Dot(simplex[i], tmpNormal));
                if (tmpDistance < lengthSquared)
                {
                    lengthSquared = tmpDistance;
                    edgeIndex = i;
                    normal = tmpNormal;
                }
            }
            distance = gxtMath.Sqrt(lengthSquared);
        }

        public static bool FindClosestFeature(List<Vector2> simplex, int winding, out int index, out float distance, out Vector2 normal)
        {
            int edgeIndex = 0;
            float edgeDistance = float.MaxValue;
            Vector2 edgeNormal = Vector2.Zero;

            int vertexIndex = 0;
            float vertexDistance = float.MaxValue;
            Vector2 vertexNormal = Vector2.Zero;
            
            FindClosestEdge(simplex, winding, out edgeIndex, out edgeDistance, out edgeNormal);
            //FindClosestVertex(simplex, winding, out vertexIndex, out vertexDistance, out vertexNormal);

            //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Edge Distance: {0}", edgeDistance);
            //gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Vertex Distance: {0}", vertexDistance);

            if (edgeDistance < vertexDistance)
            {
                distance = edgeDistance;
                index = edgeIndex;
                normal = edgeNormal;
                return false;
            }
            else
            {
                //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "vertex collision");
                //gxtDebugDrawer.Singleton.AddString("");
                distance = vertexDistance;
                index = vertexIndex;
                normal = vertexNormal;
                return true;
            }
        }

        public static bool Intersects(ref gxtPolygon polyA, ref gxtPolygon polyB, Vector2 nd)
        {
            List<Vector2> simplex = new List<Vector2>();

            simplex.Add(SupportPt(ref polyA, ref polyB, nd));
            nd = -nd;

            int iterations = 0;

            while (iterations < MAX_ITERATIONS)
            {
                simplex.Add(SupportPt(ref polyA, ref polyB, nd));

                if (Vector2.Dot(simplex[simplex.Count - 1], nd) <= 0.0f)
                    return false;
                else if (SimplexContainsOrigin(simplex, ref nd))
                    return true;

                iterations++;
            }

            return false;
        }

        /// <summary>
        /// Builds a simplex as part of GJK, determines collision between the two polygons
        /// Termination simplex can be used by EPA
        /// </summary>
        /// <param name="polyA"></param>
        /// <param name="polyB"></param>
        /// <param name="nd"></param>
        /// <param name="simplex"></param>
        /// <returns>If intersecting</returns>
        public static bool BuildSimplex(ref gxtPolygon polyA, ref gxtPolygon polyB, Vector2 nd, out List<Vector2> simplex)
        {
            simplex = new List<Vector2>();
            
            //add a support point
            simplex.Add(SupportPt(ref polyA, ref polyB, nd));
            nd = -nd;

            int iterations = 0;
            // gjk loop
            while (iterations < MAX_ITERATIONS)
            {
                // add a new support point
                simplex.Add(SupportPt(ref polyA, ref polyB, nd));
                // if worse we know they aren't colliding
                if (Vector2.Dot(simplex[simplex.Count - 1], nd) <= 0.0f)
                    return false;
                else if (SimplexContainsOrigin(simplex, ref nd))
                    return true;

                iterations++;
            }

            return false;
        }

        public static bool BuildSimplex(ref gxtPolygon polyA, ref gxtPolygon polyB, Vector2 nd, out List<Vector2> simplex, out List<Vector2> pointsOnA, out List<Vector2> pointsOnB)
        {
            simplex = new List<Vector2>();
            pointsOnA = new List<Vector2>();
            pointsOnB = new List<Vector2>();

            Vector2 ptA;
            Vector2 ptB;
            Vector2 supportPt = SupportPt(ref polyA, out ptA, ref polyB, out ptB, nd);
            simplex.Add(supportPt);
            pointsOnA.Add(ptA);
            pointsOnB.Add(ptB);

            nd = -nd;
            int iterations = 0;
            while (iterations < MAX_ITERATIONS)
            {
                supportPt = SupportPt(ref polyA, out ptA, ref polyB, out ptB, nd);
                
                simplex.Add(supportPt);
                pointsOnA.Add(ptA);
                pointsOnB.Add(ptB);
                // if worse we know they aren't colliding, so don't even bother
                // finding closest points
                if (Vector2.Dot(simplex[simplex.Count - 1], nd) <= 0.0f)
                    return false;
                // if true, we have an intersection
                // if not the search direction will be modified
                if (SimplexContainsOrigin(simplex, pointsOnA, pointsOnB, ref nd))
                    return true;

                iterations++;
            }

            return false;
        }

        public static bool Intersects(ref gxtPolygon polyA, Vector2 centroidA, ref gxtPolygon polyB, Vector2 centroidB)
        {
            Vector2 d = centroidB - centroidA;
            return Intersects(ref polyA, ref polyB, d);
        }

        public static bool Intersects(ref gxtPolygon polyA, ref gxtPolygon polyB)
        {
            // arbitrary search direction
            // strictly placeholder for now
            Vector2 d = Vector2.One;
            return Intersects(ref polyA, ref polyB, d);
        }

        
        public static float Distance(ref gxtPolygon polyA, ref gxtPolygon polyB, Vector2 nd, out Vector2 cpA, out Vector2 cpB)
        {
            cpA = Vector2.Zero;
            cpB = Vector2.Zero;
            //int icpA = 0;
            //int icpB = 0;

            List<Vector2> simplex = new List<Vector2>();
            List<Vector2> pointsOnA = new List<Vector2>();
            List<Vector2> pointsOnB = new List<Vector2>();

            Vector2 ptA;
            Vector2 ptB;
            
            // first search direction
            Vector2 supportPt = SupportPt(ref polyA, out ptA, ref polyB, out ptB, nd);
            simplex.Add(supportPt);
            pointsOnA.Add(ptA);
            pointsOnB.Add(ptB);

            // reverse search direction
            nd = -nd;
            Vector2 supportPt2 = SupportPt(ref polyA, out ptA, ref polyB, out ptB, nd);
            simplex.Add(supportPt2);
            pointsOnA.Add(ptA);
            pointsOnB.Add(ptB);

            //nd = ClosestPointOnSegment(simplex[0], simplex[1], Vector2.Zero);
            
            int iterations = 0;
            while (iterations < MAX_ITERATIONS)
            {
                nd = ClosestPointOnSegment(simplex[0], simplex[1], Vector2.Zero);
                nd = -nd;
                nd.Normalize();

                if (nd == Vector2.Zero)
                    return 0.0f;

                Vector2 supportPt3 = SupportPt(ref polyA, out ptA, ref polyB, out ptB, nd);
                simplex.Add(supportPt3);
                pointsOnA.Add(ptA);
                pointsOnB.Add(ptB);

                if (TriangleContainsOrigin(simplex[0], simplex[1], simplex[2]))
                    return 0.0f;

                float proj = Vector2.Dot(simplex[2], nd);
                if (proj - Vector2.Dot(simplex[0], nd) < float.Epsilon)
                {
                    FindClosestPoints(simplex, pointsOnA, pointsOnB, out cpA, out cpB);
                    return -proj;
                }
                // TODO: FINISH ALGORITHM
                if (simplex[0].LengthSquared() < simplex[1].LengthSquared())
                {
                    simplex[1] = simplex[2];
                }
                else
                {
                    simplex[0] = simplex[2];
                }
                iterations++;
            }

            return 0.0f;
        }
        
        /*
        public float Distance(ref gxtPolygon polyA, ref gxtPolygon polyB)
        {
            Vector2 d = Vector2.One;
            return Distance(ref polyA, ref polyB, d);
        }
        */
        public static float Distance(ref gxtPolygon polyA, Vector2 centroidA, ref gxtPolygon polyB, Vector2 centroidB, out Vector2 cpA, out Vector2 cpB)
        {
            Vector2 d = centroidB - centroidA;
            return Distance(ref polyA, ref polyB, d, out cpA, out cpB);
        }
        
        /// <summary>
        /// Gets the ordering
        /// 1 is clockwise
        /// -1 is counterclockwise
        /// </summary>
        /// <param name="simplex">Iteratively built simplex</param>
        /// <returns></returns>
        public static int GetWinding(List<Vector2> simplex)
        {
            for (int j = simplex.Count - 1, i = 0; i < simplex.Count; j = i, i++)
            {
                float cross = gxtMath.Cross2D(simplex[j], simplex[i]);
                if (cross > 0.0f)
                    return 1;
                if (cross < 0.0f)
                    return -1;
            }
            // if every cross product is zero we have a vertical line
            // this means the winding cannot be determined
            return 0;
        }

        /// <summary>
        /// Returns extensive collision information about the given polygons
        /// Will determine if intersecting by running the GJK algorithm
        /// It saves the terminating simplex and further uses it for the 
        /// EPA algorithm.  Returns the intersection depth and collision normal as 
        /// part of the gxtCollisionResult return struct.  The normal and depth will 
        /// not be calculated, and thus such information is not realiable, if the intersection 
        /// is false
        /// </summary>
        /// <param name="polyA">Polygon A</param>
        /// <param name="centroidA">Centroid of polygon A</param>
        /// <param name="polyB">Polygon B</param>
        /// <param name="centroidB">Centroid of polygon B</param>
        /// <returns>Collision Result</returns>
        public static gxtCollisionResult CollideOld(gxtPolygon polyA, Vector2 centroidA, gxtPolygon polyB, Vector2 centroidB)
        {
            // optimal search direction
            Vector2 direction = centroidB - centroidA;
            // simplex vector2 collection
            List<Vector2> simplex;
            // the result structure for the collision
            gxtCollisionResult result = new gxtCollisionResult();
            
            // finishes with terminating simplex from gjk
            // boolean return for intersection
            if (!BuildSimplex(ref polyA, ref polyB, direction, out simplex))
            {
                result.Intersection = false;
                return result;
            }
            else
            {
                result.Intersection = true;
            }

            // iterations
            int iterations = 0;

            // holder edge information
            Vector2 normal;
            float distance;
            int index;
            
            // calculate the ordering (CW/CCW)
            // of the simplex.  only calc once
            // reusable in loop
            int winding = GetWinding(simplex);

            // epa loop
            while (iterations < MAX_ITERATIONS)
            {
                // find closest edge to origin info
                //FindClosestEdge(simplex, out index, out distance, out normal);
                if (FindClosestFeature(simplex, winding, out index, out distance, out normal))
                {
                    //gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "vertex collision");
                    result.Normal = normal;
                    result.Depth = distance;
                    //result.EdgeIndex = index;
                    //DebugDrawSimplex(simplex, Color.Blue, 0.0f);
                    return result;
                }

                // TODO: COMPARE TO VERTEX VALUES
                // REALLY LOOKING FOR CLOSEST *FEATURE*

                
                // support point to (possibly) add to the simplex
                Vector2 pt = SupportPt(ref polyA, ref polyB, normal);

                float d = Vector2.Dot(pt, normal);
                // if less than our tolerance
                // will return this information
                if (d - distance < EPA_TOLERANCE)
                {
                    result.Normal = normal;
                    result.Depth = distance;
                    //result.EdgeIndex = index;
                    // TEMPORARY
                    //DebugDrawSimplex(simplex, Color.Green, 0.0f);
                    //gxtDebugDrawer.Singleton.AddLine(Vector2.Zero, result.Normal * 30, Color.Gray, 0.0f);
                    return result;
                }
                // otherwise add to the simplex
                else
                {
                    simplex.Insert(index, pt);
                    iterations++;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns extensive collision information about the given polygons
        /// Will determine if intersecting by running the GJK algorithm
        /// It saves the terminating simplex and further uses it for the 
        /// EPA algorithm.  Returns the intersection depth and collision normal as 
        /// part of the gxtCollisionResult return struct.  The normal and depth will 
        /// not be calculated, and thus such information is not realiable, if the intersection 
        /// is false
        /// </summary>
        /// <param name="polyA">Polygon A</param>
        /// <param name="centroidA">Centroid of polygon A</param>
        /// <param name="polyB">Polygon B</param>
        /// <param name="centroidB">Centroid of polygon B</param>
        /// <returns>Collision Result</returns>
        public static gxtCollisionResult Collide(gxtPolygon polyA, Vector2 centroidA, gxtPolygon polyB, Vector2 centroidB)
        {
            // optimal search direction
            Vector2 direction = centroidB - centroidA;
            // simplex vector2 collection
            List<Vector2> simplex;
            List<Vector2> pointsOnA;
            List<Vector2> pointsOnB;
            // the result structure for the collision
            gxtCollisionResult result = new gxtCollisionResult();

            // finishes with terminating simplex from gjk
            // boolean return for intersection
            if (!BuildSimplex(ref polyA, ref polyB, direction, out simplex, out pointsOnA, out pointsOnB))
            {
                result.Intersection = false;
                return result;
            }
            else
            {
                result.Intersection = true;
            }


            // iterations
            int iterations = 0;

            // holder edge information
            Vector2 normal;
            float distance;
            int index;
            Vector2 cpA, cpB;

            /*
            FindClosestPointsSpecial(simplex, pointsOnA, pointsOnB, out cpA, out cpB);
            
            #if (GXT_DEBUG_DRAW_CONTACTS)
            gxtDebugDrawer.Singleton.AddPt(cpA * gxtPhysicsWorld.PHYSICS_SCALE, Color.GreenYellow, 0.0f);
            gxtDebugDrawer.Singleton.AddPt(cpB * gxtPhysicsWorld.PHYSICS_SCALE, Color.GreenYellow, 0.0f);
            #endif

            result.ContactPointA = cpA;
            result.ContactPointB = cpB;
            */

            // calc ordering once, reusable in loop
            int winding = GetWinding(simplex);

            // epa loop
            while (iterations < MAX_ITERATIONS)
            {
                // find closest edge to origin info
                // returns true if it is a vertex

                FindClosestFeature(simplex, winding, out index, out distance, out normal);
                /*
                if (FindClosestFeature(simplex, winding, out index, out distance, out normal))
                {
                    result.Normal = normal;
                    result.Depth = distance;

                    int nextIndex = (index + 1) % simplex.Count;
                    FindClosestPointsHelper(simplex[index], simplex[nextIndex], pointsOnA[index], pointsOnA[nextIndex], pointsOnB[index], pointsOnB[nextIndex], out cpA, out cpB);

                    result.ContactPointA = cpA;
                    result.ContactPointB = cpB;

                    #if (GXT_DEBUG_DRAW_CONTACTS)
                    gxtDebugDrawer.Singleton.AddPt(cpA * gxtPhysicsWorld.PHYSICS_SCALE, Color.Red, 0.0f);
                    gxtDebugDrawer.Singleton.AddPt(cpB * gxtPhysicsWorld.PHYSICS_SCALE, Color.Red, 0.0f);
                    #endif

                    #if GXT_DEBUG_DRAW_SIMPLEX
                    DebugDrawSimplex(simplex, Color.Blue, 0.0f);
                    #endif
                    
                    return result;
                }
                */

                // TODO: COMPARE TO VERTEX VALUES
                // REALLY LOOKING FOR CLOSEST *FEATURE*

                // support point to (possibly) add to the simplex
                //Vector2 pt = SupportPt(ref polyA, ref polyB, normal);
                Vector2 ptA, ptB;
                Vector2 pt = SupportPt(ref polyA, out ptA, ref polyB, out ptB, normal); 

                float d = Vector2.Dot(pt, normal);
                // if less than our tolerance
                // will return this information
                if (d - distance < EPA_TOLERANCE)
                {
                    result.Normal = normal;
                    result.Depth = distance;

                    FindClosestPointsSpecial2(simplex, pointsOnA, pointsOnB, out cpA, out cpB);
                    result.ContactPointA = cpA;
                    result.ContactPointB = cpB;

                    //result.EdgeIndex = index;
                    
                    #if GXT_DEBUG_DRAW_SIMPLEX
                    DebugDrawSimplex(simplex, Color.Green, 0.0f);
                    #endif

                    #if (GXT_DEBUG_DRAW_CONTACTS)
                    gxtDebugDrawer.Singleton.AddPt(cpA * gxtPhysicsWorld.PHYSICS_SCALE, Color.Red, 0.0f);
                    gxtDebugDrawer.Singleton.AddPt(cpB * gxtPhysicsWorld.PHYSICS_SCALE, Color.Red, 0.0f);
                    #endif
                    
                    return result;
                }
                // otherwise add to the simplex
                else
                {
                    simplex.Insert(index, pt);
                    // could remove
                    pointsOnA.Insert(index, ptA);
                    pointsOnB.Insert(index, ptB);
                    iterations++;
                }
            }

            return result;
        }

        /// <summary>
        /// http://objectmix.com/graphics/132701-ray-line-segment-intersection-2d.html#post460607
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool RayIntersectsSegment(gxtRay ray, Vector2 pt0, Vector2 pt1)
        {
            return RayIntersectsSegment(ray, pt0, pt1, float.MaxValue);
        }

        public static bool RayIntersectsSegment(gxtRay ray, Vector2 pt0, Vector2 pt1, float tmax)
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

        public static bool RayIntersectsSegment(gxtRay ray, Vector2 pt0, Vector2 pt1, float tmax, out float t)
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


        public static bool RayCast(gxtRay ray, gxtPolygon polygon)
        {
            //if (polygon.Contains(ray.Origin))
            //    return false;

            int crossings = 0;
            for (int j = polygon.NumVertices - 1, i = 0; i < polygon.NumVertices; j = i, i++)
            {
                if (RayIntersectsSegment(ray, polygon.v[j], polygon.v[i]))
                    crossings++;
            }
            return crossings > 0 && crossings % 2 == 0;
        }

        public static bool RayCast(gxtRay ray, gxtPolygon polygon, float tmax, out float t)
        {
            t = float.MaxValue;
            bool intersection = false;
            int crossings = 0;
            float distance;
            for (int j = polygon.NumVertices - 1, i = 0; i < polygon.NumVertices; j = i, i++)
            {
                if (RayIntersectsSegment(ray, polygon.v[j], polygon.v[i], float.MaxValue, out distance))
                {
                    crossings++;
                    if (distance < tmax)
                        intersection = true;
                    if (distance < t)
                        t = distance;
                }
            }
            return intersection && crossings > 0 && crossings % 2 == 0;
        }

        public static bool RayCast(gxtRay ray, gxtPolygon polygon, float tmax, out float t, out Vector2 pt, out Vector2 normal)
        {
            t = float.MaxValue;
            pt = ray.Origin;
            normal = ray.Direction;

            bool intersection = false;
            // temp holder for segment distance
            float distance;
            int crossings = 0;

            for (int j = polygon.NumVertices - 1, i = 0; i < polygon.NumVertices; j = i, i++)
            {
                if (RayIntersectsSegment(ray, polygon.v[j], polygon.v[i], float.MaxValue, out distance))
                {
                    crossings++;
                    if (distance < t && distance <= tmax)
                    {
                        intersection = true;

                        t = distance;
                        pt = ray.GetPoint(t);

                        Vector2 edge = polygon.v[i] - polygon.v[j];
                        normal = Vector2.Normalize(gxtMath.RightPerp(edge));
                        //normal = gxtMath.GetReflection(ray.Direction, edgeNormal);
                    }
                }
            }
            return intersection && crossings > 0 && crossings % 2 == 0;
        }

        public static bool Intersects(ref gxtPolygon polygon, Vector2 centroid, gxtSphere sphere)
        {
            if (Contains(polygon, sphere.Position))
                return true;
            float r2 = sphere.Radius * sphere.Radius;
            for (int j = polygon.NumVertices - 1, i = 0; i < polygon.NumVertices; j = i, i++)
            {
                Vector2 edgeVector = polygon.v[i] - polygon.v[j];
                Vector2 edgeNormal = Vector2.Normalize(gxtMath.RightPerp(edgeVector));
                float edgeDistance = gxtMath.Abs(Vector2.Dot(polygon.v[j], edgeNormal));
                // void PointEdgeDistance(const Vector& P, const Vector& E0, const Vector& E1, Vector& Q, float& dist2){    Vector D = P - E0;    Vector E = E1 - E0;    float e2 = E * E;    float ed = E * D;    float t  = (ed / e2);       t = (t < 0.0)? 0.0f : (t > 1.0f)? : 1.0f : t;    Q = E0 + t * E;    Vector PQ = Q - P;    dist2 = PQ * PQ;}
                if (edgeDistance <= r2)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// An efficient contains point function whoich uses a binary search 
        /// and assumes the input of a counter clockwise polygon
        /// </summary>
        /// <param name="polygon">Polygon</param>
        /// <param name="pt">Pt</param>
        /// <returns>If Inside</returns>
        public static bool Contains(gxtPolygon polygon, Vector2 pt)
        {
            int low = 0, high = polygon.NumVertices;
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

        #if GXT_DEBUG_DRAW_SIMPLEX
        public static void DebugDrawSimplex(List<Vector2> simplex, Color color, float depth)
        {
            Vector2[] vertices = simplex.ToArray();
            
            #if GXT_USE_PHYSICS_SCALING
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] *= gxtPhysicsWorld.PHYSICS_SCALE;
            }
            #endif

            gxtPolygon poly = new gxtPolygon(vertices);
            gxtDebugDrawer.Singleton.AddPolygon(poly, color, 0.0f);
            for (int i = 0; i < poly.NumVertices; i++)
            {
                gxtDebugDrawer.Singleton.AddPt(poly.v[i], Color.WhiteSmoke, 0.0f);
                gxtDebugDrawer.Singleton.AddString(i.ToString(), poly.v[i], Color.White, 0.0f);
            }
        }
        #endif
    }
}
