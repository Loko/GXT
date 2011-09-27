using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;

namespace GXT
{
    public class gxtGJKColliderInstance : gxtNarrowPhaseCollider
    {
        public const int DEFAULT_MAX_GJK_ITERATIONS = 100;
        public const float DEFAULT_MAX_GJK_TOLERANCE = float.Epsilon; //gxtMath.Sqrt(float.Epsilon);
        
        private int maxIterations;
        private float tolerance;

        public override int MaxIterations { get { return maxIterations; } set { gxtDebug.Assert(value >= 5, "Must Have At Least 5 Iterations"); maxIterations = value; } }
        public override float Tolerance { get { return tolerance; } set { gxtDebug.Assert(value >= 0.0f, "Must have a positive collision tolerance"); tolerance = value; } }

        public gxtGJKColliderInstance()
        {
            MaxIterations = DEFAULT_MAX_GJK_ITERATIONS;
            Tolerance = DEFAULT_MAX_GJK_TOLERANCE;
        }

        #region SupportFunctions
        /// <summary>
        /// Finds the farthest point in a given direction
        /// Search direction does not need to be normalized
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="nd"></param>
        /// <returns></returns>
        public Vector2 FarthestPointInDirection(ref gxtPolygon polygon, Vector2 nd)
        {
            int farthestIndex = 0;
            float farthestDistance = Vector2.Dot(polygon.v[0], nd);
            float tmpDistance;
            // finds largest scalar porjection onto nd
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
        public Vector2 SupportPt(ref gxtPolygon polyA, ref gxtPolygon polyB, Vector2 nd)
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
        public Vector2 SupportPt(ref gxtPolygon polyA, out Vector2 polyAPt, ref gxtPolygon polyB, out Vector2 polyBPt, Vector2 nd)
        {
            polyAPt = FarthestPointInDirection(ref polyA, nd);
            polyBPt = FarthestPointInDirection(ref polyB, -nd);
            return polyAPt - polyBPt;
        }
        #endregion SupportFunctions

        #region SimplexContainsOrigin
        /// <summary>
        /// Determines if the simplex contains the origin if it is a triangle or 
        /// modifies the search direction if it is a line segment
        /// 
        /// This function may remove points from the simplex
        /// </summary>
        /// <param name="simplex"></param>
        /// <param name="nd"></param>
        /// <returns></returns>
        private bool SimplexContainsOrigin(List<Vector2> simplex, ref Vector2 nd)
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

        private bool SimplexContainsOrigin(List<Vector2> simplex, List<Vector2> pointsOnA, List<Vector2> pointsOnB, ref Vector2 nd)
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
        #endregion SimplexContainsOrigin

        public override bool Intersects(ref gxtPolygon polygonA, Vector2 centroidA, ref gxtPolygon polygonB, Vector2 centroidB)
        {
            throw new NotImplementedException();
        }

        public override bool Collide(ref gxtPolygon polygonA, Vector2 centroidA, ref gxtPolygon polygonB, out gxtCollisionResult collisionResult)
        {
            throw new NotImplementedException();
        }
    }
}
