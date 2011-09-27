using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A type of texture mapping
    /// </summary>
    public enum gxtTextureCoordinateType
    {
        CLAMP = 0,
        WRAP = 1
    };

    /// <summary>
    /// A collection of functions for geometry computations
    /// Creation/Conversion methods for and between bounding volumes
    /// and polygons
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: NEED A SPECIAL RETURN IF THINGS FAIL
    // Can't be a null return because polygons can't be null
    // either rework the CreatePolygon functions to return a boolean and take the polygon as a parameter
    // or figure something else out
    public static class gxtGeometry
    {
        /// <summary>
        /// Finds the most min and max points given a point set
        /// </summary>
        /// <param name="min">min of ptset</param>
        /// <param name="max">max of ptset</param>
        /// <param name="ptSet">array of points</param>
        public static void GetExtremePoints(out Vector2 min, out Vector2 max, Vector2[] ptSet)
        {
            gxtDebug.Assert(ptSet.Length > 0);

            int minX = 0, maxX = 0, minY = 0, maxY = 0;
            for (int i = 1; i < ptSet.Length; i++)
            {
                if (ptSet[i].X < ptSet[minX].X) minX = i;
                else if (ptSet[i].X > ptSet[maxX].X) maxX = i;
                if (ptSet[i].Y < ptSet[minY].Y) minY = i;
                else if (ptSet[i].Y > ptSet[maxY].Y) maxY = i;
            }
            min = new Vector2(ptSet[minX].X, ptSet[minY].Y);
            max = new Vector2(ptSet[maxX].X, ptSet[maxY].Y);
        }

        /// <summary>
        /// Finds the most min and max points given a point set
        /// Works with a generic enumerator
        /// </summary>
        /// <param name="min">min of ptset</param>
        /// <param name="max">max of ptset</param>
        /// <param name="ptSet">array of points</param>
        public static void GetExtremePoints(out Vector2 min, out Vector2 max, IEnumerator<Vector2> ptSet)
        {
            ptSet.MoveNext();
            float minX = ptSet.Current.X, maxX = ptSet.Current.X, minY = ptSet.Current.Y, maxY = ptSet.Current.Y;

            while (ptSet.MoveNext())
            {
                if (ptSet.Current.X < minX) minX = ptSet.Current.X;
                else if (ptSet.Current.X > maxX) maxX = ptSet.Current.X;
                if (ptSet.Current.Y < minY) minY = ptSet.Current.Y;
                else if (ptSet.Current.Y > maxY) maxY = ptSet.Current.Y;
            }
            min = new Vector2(minX, minY);
            max = new Vector2(maxX, maxY);
        }

        /// <summary>
        /// Finds the most distant points in the point set
        /// </summary>
        /// <param name="min">index of min point</param>
        /// <param name="max">index of max point</param>
        /// <param name="ptSet">array of points</param>
        public static void GetMostDistantPoints(out int min, out int max, Vector2[] ptSet)
        {
            int minX = 0, maxX = 0, minY = 0, maxY = 0;
            for (int i = 1; i < ptSet.Length; i++)
            {
                if (ptSet[i].X < ptSet[minX].X) minX = i;
                else if (ptSet[i].X > ptSet[maxX].X) maxX = i;
                if (ptSet[i].Y < ptSet[minY].Y) minY = i;
                else if (ptSet[i].Y > ptSet[maxY].Y) maxY = i;
            }

            float distSqX = Vector2.DistanceSquared(ptSet[maxX], ptSet[minX]);
            float distSqY = Vector2.DistanceSquared(ptSet[maxY], ptSet[minY]);

            max = maxX;
            min = minX;
            // Compares distance
            if (distSqY > distSqX)
            {
                max = maxY;
                min = minY;
            }
        }

        /// <summary>
        /// Finds the most distant points in the point set
        /// </summary>
        /// <param name="min">index of min point</param>
        /// <param name="max">index of max point</param>
        /// <param name="ptSet">array of points</param>
        public static void GetMostDistantPoints(out Vector2 min, out Vector2 max, IEnumerator<Vector2> ptSet)
        {
            // set to first element
            ptSet.MoveNext();

            // set everything to value of first vector2
            Vector2 minX, maxX, minY, maxY;
            minX = maxX = minY = maxY = new Vector2(ptSet.Current.X, ptSet.Current.Y);
            
            // loop and compare
            while (ptSet.MoveNext())
            {
                if (ptSet.Current.X < minX.X) minX = ptSet.Current;
                else if (ptSet.Current.X > maxX.X) maxX = ptSet.Current;
                if (ptSet.Current.Y < minY.Y) minY = ptSet.Current;
                else if (ptSet.Current.Y > maxY.Y) maxY = ptSet.Current;
            }

            // compare squared distances of x and y points
            float distSqX = Vector2.DistanceSquared(maxX, minX);
            float distSqY = Vector2.DistanceSquared(maxY, minY);
            // set to x
            max = maxX;
            min = minX;
            // compare on y
            if (distSqY > distSqX)
            {
                max = maxY;
                min = minY;
            }
        }

        /// <summary>
        /// Constructs a sphere from the most distant points on the point set
        /// </summary>
        /// <param name="sphere">sphere</param>
        /// <param name="ptSet">array of points</param>
        public static gxtSphere SphereFromDistantPoints(Vector2[] ptSet)
        {
            int min, max;
            GetMostDistantPoints(out min, out max, ptSet);
            Vector2 c = (ptSet[min] + ptSet[max]) * 0.5f;
            float r = Vector2.Distance(ptSet[max], c);
            return new gxtSphere(c, r);
        }

        /// <summary>
        /// Enlarges the sphere to include itself and the outside point
        /// if it is outside the existing sphere.
        /// </summary>
        /// <param name="sphere">sphere</param>
        /// <param name="pt">Pt in set</param>
        public static void MakeSphereIncludePt(ref gxtSphere sphere, Vector2 pt)
        {
            Vector2 diff = pt - sphere.Position;
            float distSq = diff.LengthSquared();
            // Contains check
            if (distSq > sphere.Radius * sphere.Radius)
            {
                float dist = gxtMath.Sqrt(distSq);
                // New radius half the sum of this distance and old
                // Works because position will also be adjusted
                float adjRadius = (sphere.Radius + dist) * 0.5f;
                // Adjust position by component vector
                sphere.Position += diff * ((adjRadius - sphere.Radius) / dist);
                sphere.Radius = adjRadius;
            }
        }

        /// <summary>
        /// Computes an AABB given a set of vertices/points
        /// </summary>
        /// <param name="ptSet">array of points</param>
        /// <returns>AABB for point set</returns>
        public static gxtAABB ComputeAABB(Vector2[] ptSet)
        {
            Vector2 min, max;
            GetExtremePoints(out min, out max, ptSet);
            Vector2 c = (min + max) * 0.5f;
            Vector2 r = max - c;
            return new gxtAABB(c, r);
        }

        /// <summary>
        /// Computes an AABB given a set of vertices/points
        /// Works with an enumerator
        /// </summary>
        /// <param name="ptSet">enumerator of points</param>
        /// <returns>AABB for point set</returns>
        public static gxtAABB ComputeAABB(IEnumerator<Vector2> ptSet)
        {
            Vector2 min, max;
            GetExtremePoints(out min, out max, ptSet);
            Vector2 c = (min + max) * 0.5f;
            Vector2 r = max - c;
            return new gxtAABB(c, r);
        }

        /// <summary>
        /// Computes a sphere given a set of vertices/points
        /// </summary>
        /// <param name="ptSet">array of points</param>
        /// <returns>sphere for the point set</returns>
        public static gxtSphere ComputeSphere(Vector2[] ptSet)
        {
            //  Find most distant points on AABR
            int min, max;
            GetMostDistantPoints(out min, out max, ptSet);

            // Constructs initial circle from distant points
            Vector2 c = (ptSet[min] + ptSet[max]) * 0.5f;
            float r = Vector2.Distance(ptSet[max], c);
            gxtSphere sphere = new gxtSphere(c, r);

            // Check to include all points in circle
            for (int i = 0; i < ptSet.Length; i++)
            {
                MakeSphereIncludePt(ref sphere, ptSet[i]);
            }
            return sphere;
        }

        /// <summary>
        /// Computes a sphere given a set of vertices/points
        /// </summary>
        /// <param name="ptSet">enumerator of points</param>
        /// <returns>sphere for the point set</returns>
        public static gxtSphere ComputeSphere(IEnumerator<Vector2> ptSet)
        {
            //  Find most distant points on AABB
            Vector2 min, max;
            GetMostDistantPoints(out min, out max, ptSet);

            // Constructs initial sphere from distant points
            Vector2 c = (min + max) * 0.5f;
            float r = Vector2.Distance(max, c);
            gxtSphere sphere = new gxtSphere(c, r);

            ptSet.Reset();
            // Check to include all points in circle
            while (ptSet.MoveNext())
            {
                MakeSphereIncludePt(ref sphere, ptSet.Current);
            }
            return sphere;
        }

        /// <summary>
        /// Computes an OBB given a set of vertices/points
        /// </summary>
        /// <param name="ptSet">array of points</param>
        /// <returns>OBB for point set</returns>
        public static gxtOBB ComputeOBB(Vector2[] ptSet)
        {
            // Area comparison value
            float minArea = float.MaxValue;

            // Instantiates members of OBB
            Vector2 c = new Vector2();
            Vector2 r = new Vector2();
            Vector2 localX = new Vector2();
            Vector2 localY = new Vector2();

            // Setting up the for loop this way avoids modulo and lets you iterate once smoothly
            for (int i = 0, j = ptSet.Length - 1; i < ptSet.Length; j = i, i++)
            {
                // Normalized edge axis
                Vector2 e = ptSet[i] - ptSet[j];
                e.Normalize();
                // Perpendicular edge axis
                Vector2 ePerp = new Vector2(-e.Y, e.X);

                // The min and max projections along the edge axis and its perpendicular axis
                float minE = 0.0f, minEperp = 0.0f, maxE = 0.0f, maxEperp = 0.0f;
                // Project all points along these two axes
                for (int k = 0; k < ptSet.Length; k++)
                {
                    Vector2 diff = ptSet[k] - ptSet[j];
                    // Project along edge axis
                    // Store min and max scalar projections on this axis
                    float dot = Vector2.Dot(diff, e);
                    if (dot < minE) minE = dot;
                    if (dot > maxE) maxE = dot;
                    // Same for perpendicular axis
                    dot = Vector2.Dot(diff, ePerp);
                    if (dot < minEperp) minEperp = dot;
                    if (dot > maxEperp) maxEperp = dot;
                }

                // Calculate area of rectangle, compare to existing minimum
                float area = (maxE - minE) * (maxEperp - minEperp);

                if (area < minArea)
                {
                    // Store new min
                    minArea = area;
                    // Like finding the center of an AABB
                    // Start with first edge point, project over to new maximum along each axis
                    // Divide by two
                    c = ptSet[j] + 0.5f * ((minE + maxE) * e + (minEperp + maxEperp) * ePerp);
                    // Half-Width Extents
                    r = new Vector2((maxE - minE) * 0.5f, (maxEperp - minEperp) * 0.5f);
                    // New axes for the OBB
                    localX = e;
                    localY = ePerp;
                }
            }
            return new gxtOBB(c, r, localX, localY);
        }

        /*
        private static float CalculateArea(IEnumerator<Vector2> ptSet, Vector2 pt)
        {
                float minE = 0.0f, minEperp = 0.0f, maxE = 0.0f, maxEperp = 0.0f;
                ptSet.Reset();
                while (ptSet.MoveNext())
                {
                    Vector2 diff = ptSet.Current - prev;
                    // Project along edge axis
                    // Store min and max scalar projections on this axis
                    float dot = Vector2.Dot(diff, e);
                    if (dot < minE) minE = dot;
                    if (dot > maxE) maxE = dot;
                    // Same for perpendicular axis
                    dot = Vector2.Dot(diff, ePerp);
                    if (dot < minEperp) minEperp = dot;
                    if (dot > maxEperp) maxEperp = dot;
                }

                // Calculate area of rectangle, compare to existing minimum
                float area = (maxE - minE) * (maxEperp - minEperp);
        }
        

        public static gxtOBB ComputeOBB(IEnumerator<Vector2> ptSet)
        {
            // Area comparison value
            float minArea = float.MaxValue;

            // Instantiates members of OBB
            Vector2 c = new Vector2();
            Vector2 r = new Vector2();
            Vector2 localX = new Vector2();
            Vector2 localY = new Vector2();

            while (ptSet.MoveNext())
            {
                Vector2 prev = ptSet.Current;
                ptSet.MoveNext();
                Vector2 cur = ptSet.Current;

                Vector2 e = cur - prev;
                e.Normalize();
                Vector2 ePerp = new Vector2(-e.Y, e.X);

                float minE = 0.0f, minEperp = 0.0f, maxE = 0.0f, maxEperp = 0.0f;
                ptSet.Reset();
                while (ptSet.MoveNext())
                {
                    Vector2 diff = ptSet.Current - prev;
                    // Project along edge axis
                    // Store min and max scalar projections on this axis
                    float dot = Vector2.Dot(diff, e);
                    if (dot < minE) minE = dot;
                    if (dot > maxE) maxE = dot;
                    // Same for perpendicular axis
                    dot = Vector2.Dot(diff, ePerp);
                    if (dot < minEperp) minEperp = dot;
                    if (dot > maxEperp) maxEperp = dot;
                }

                // Calculate area of rectangle, compare to existing minimum
                float area = (maxE - minE) * (maxEperp - minEperp);

                if (area < minArea)
                {
                    // Store new min
                    minArea = area;
                    // Like finding the center of an AABB
                    // Start with first edge point, project over to new maximum along each axis
                    // Divide by two
                    c = prev + 0.5f * ((minE + maxE) * e + (minEperp + maxEperp) * ePerp);
                    // Half-Width Extents
                    r = new Vector2((maxE - minE) * 0.5f, (maxEperp - minEperp) * 0.5f);
                    // New axes for the OBB
                    localX = e;
                    localY = ePerp;
                }
            }
            
            // Setting up the for loop this way avoids modulo and lets you iterate once smoothly
            for (int i = 0, j = ptSet.Length - 1; i < ptSet.Length; j = i, i++)
            {
                // Normalized edge axis
                Vector2 e = ptSet[i] - ptSet[j];
                e.Normalize();
                // Perpendicular edge axis
                Vector2 ePerp = new Vector2(-e.Y, e.X);

                // The min and max projections along the edge axis and its perpendicular axis
                float minE = 0.0f, minEperp = 0.0f, maxE = 0.0f, maxEperp = 0.0f;
                // Project all points along these two axes
                for (int k = 0; k < ptSet.Length; k++)
                {
                    Vector2 diff = ptSet[k] - ptSet[j];
                    // Project along edge axis
                    // Store min and max scalar projections on this axis
                    float dot = Vector2.Dot(diff, e);
                    if (dot < minE) minE = dot;
                    if (dot > maxE) maxE = dot;
                    // Same for perpendicular axis
                    dot = Vector2.Dot(diff, ePerp);
                    if (dot < minEperp) minEperp = dot;
                    if (dot > maxEperp) maxEperp = dot;
                }

                // Calculate area of rectangle, compare to existing minimum
                float area = (maxE - minE) * (maxEperp - minEperp);

                if (area < minArea)
                {
                    // Store new min
                    minArea = area;
                    // Like finding the center of an AABB
                    // Start with first edge point, project over to new maximum along each axis
                    // Divide by two
                    c = ptSet[j] + 0.5f * ((minE + maxE) * e + (minEperp + maxEperp) * ePerp);
                    // Half-Width Extents
                    r = new Vector2((maxE - minE) * 0.5f, (maxEperp - minEperp) * 0.5f);
                    // New axes for the OBB
                    localX = e;
                    localY = ePerp;
                }
            }
            
            return new gxtOBB(c, r, localX, localY);
        }
        */

        /// <summary>
        /// Converts an AABB to a valid polygon with 4 vertices
        /// </summary>
        /// <param name="aabb">AABB</param>
        /// <returns>Polygon</returns>
        public static gxtPolygon ComputePolygonFromAABB(gxtAABB aabb)
        {
            return new gxtPolygon(ComputeVerticesFromAABB(aabb));
        }

        /// <summary>
        /// Equivalent to ComputePolygonFromAABB() but returns the data 
        /// as an array of vertices instead
        /// </summary>
        /// <param name="aabb"></param>
        /// <returns></returns>
        public static Vector2[] ComputeVerticesFromAABB(gxtAABB aabb)
        {
            Vector2[] verts = new Vector2[4];
            
            Vector2 c = aabb.Position;
            Vector2 r = aabb.Extents;
            
            verts[0] = c - r;
            verts[1] = c + new Vector2(-r.X, r.Y);
            verts[2] = c + r;
            verts[3] = c + new Vector2(r.X, -r.Y);
            
            return verts;
        }

        /// <summary>
        /// Converts an OBB to a valid polygon with 4 vertices
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <returns>Polygon</returns>
        public static gxtPolygon ComputePolygonFromOBB(gxtOBB obb)
        {
            return new gxtPolygon(ComputeVerticesFromOBB(obb));
        }

        public static Vector2[] ComputeVerticesFromOBB(gxtOBB obb)
        {
            Vector2[] verts = new Vector2[4];

            Vector2 c = obb.Position;
            Vector2 r = obb.Extents;
            Vector2 localX = obb.localX;
            Vector2 localY = obb.localY;

            verts[0] = (c - (r.X * localX) - (r.Y * localY));
            verts[1] = (c - (r.X * localX) + (r.Y * localY));
            verts[2] = (c + (r.X * localX) + (r.Y * localY));
            verts[3] = (c + (r.X * localX) - (r.Y * localY));

            return verts;
        }

        /// <summary>
        /// Creates a box polygon, centered at the origin, and aligned with the 
        /// world axis, with the given width and height
        /// </summary>
        /// <param name="width">Full Width</param>
        /// <param name="height">Full Height</param>
        /// <returns>Rectangle Polygon</returns>
        public static gxtPolygon CreateRectanglePolygon(float width, float height)
        {
            Vector2[] verts = CreateRectangleVertices(width, height);
            return new gxtPolygon(verts);
        }

        /// <summary>
        /// Creates a box polygon, centered at the origin, and aligned with the 
        /// world axis, with the given width and height
        /// </summary>
        /// <param name="width">Full Width</param>
        /// <param name="height">Full Height</param>
        /// <returns>Rectangle Vertices</returns>
        public static Vector2[] CreateRectangleVertices(float width, float height)
        {
            gxtDebug.Assert(width >= 0.0f && height >= 0.0f, "Rectangle must have positive width and height");

            Vector2[] verts = new Vector2[4];

            float rX = width * 0.5f;
            float rY = height * 0.5f;
            verts[0] = new Vector2(-rX, -rY);
            verts[1] = new Vector2(-rX, rY);
            verts[2] = new Vector2(rX, rY);
            verts[3] = new Vector2(rX, -rY);

            return verts;
        }

        /// <summary>
        /// Creates a circular approximation in polygonal form with the given radius 
        /// and number of sides
        /// </summary>
        /// <param name="radius">Radius of the circular polygon</param>
        /// <param name="n">Num Vertices in the approximation</param>
        /// <returns></returns>
        public static gxtPolygon CreateCirclePolygon(float radius, int n)
        {   
            return new gxtPolygon(CreateCircleVertices(radius, n));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Vector2[] CreateCircleVertices(float radius, int n)
        {
            gxtDebug.Assert(radius > 0.0f, "Radius must be positive!");
            gxtDebug.Assert(n >= 3, "N must be at least 3");

            Vector2[] verts = new Vector2[n];

            float step = -gxtMath.TWO_PI / ((float)n);
            for (int i = 0; i < n; ++i)
            {
                verts[i] = new Vector2((gxtMath.Cos(step * i)) * radius, (gxtMath.Sin(step * i)) * radius);
            }

            return verts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static gxtPolygon CreateRoundedRectanglePolygon(float width, float height, float rx, float ry)
        {
            return new gxtPolygon(CreateRoundedRectangleVertices(width, height, rx, ry));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <returns></returns>
        public static Vector2[] CreateRoundedRectangleVertices(float width, float height, float rx, float ry)
        {
            gxtDebug.Assert(width >= 0.0f && height >= 0.0f && rx >= 0.0f && ry >= 0.0f, "All Values Used To Create A Rounded Rectangle Must Be Positive");

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            gxtDebug.Assert((rx > halfWidth + float.Epsilon) || (ry > halfHeight + float.Epsilon), "The radii of the rounded rectangle cannot exceed its full width/height");

            Vector2[] verts = new Vector2[8];

            verts[0] = new Vector2(halfWidth - rx, -halfHeight);
            verts[1] = new Vector2(halfWidth, -halfHeight + ry);
            verts[2] = new Vector2(halfWidth, halfHeight - ry);
            verts[3] = new Vector2(halfWidth - rx, halfHeight);
            verts[4] = new Vector2(-halfWidth + rx, halfHeight);
            verts[5] = new Vector2(-halfWidth, halfHeight - ry);
            verts[6] = new Vector2(-halfWidth, -halfHeight + ry);
            verts[7] = new Vector2(-halfWidth + rx, -halfHeight);

            return verts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static gxtPolygon CreateEllipsePolygon(float rx, float ry, int n)
        {
            return new gxtPolygon(CreateEllipseVertices(rx, ry, n));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Vector2[] CreateEllipseVertices(float rx, float ry, int n)
        {
            gxtDebug.Assert(n >= 4, "At least 4 vertices must be used to create the ellipse polygon");

            Vector2[] verts = new Vector2[n];
            float step = -gxtMath.TWO_PI / ((float)n);
            for (int i = 0; i < n; ++i)
            {
                verts[i] = new Vector2(rx * gxtMath.Cos(step * i), ry * gxtMath.Sin(step * i));
            }

            return verts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        /// <param name="endEdges"></param>
        /// <returns></returns>
        public static gxtPolygon CreateCapsulePolygon(float height, float radius, int endEdges)
        {
            return new gxtPolygon(CreateCapsuleVertices(height, radius, endEdges));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        /// <param name="endEdges"></param>
        /// <returns></returns>
        public static Vector2[] CreateCapsuleVertices(float height, float radius, int endEdges)
        {
            gxtDebug.Assert(radius >= 0.0f);
            gxtDebug.Assert(endEdges >= 0);
            Vector2[] verts = new Vector2[4 + endEdges * 2];
            float halfHeight = height * 0.5f;
            // top left
            verts[0] = new Vector2(-radius, -halfHeight);
            // bot left
            verts[1] = new Vector2(-radius, halfHeight);

            float step = gxtMath.PI / ((float)endEdges + 1);
            int insertOffset = 2;
            for (int i = 0; i < endEdges; ++i)
            {
                verts[i + insertOffset] = new Vector2(radius * -gxtMath.Cos(step * (i + 1)), halfHeight + radius * gxtMath.Sin(step * (i + 1)));
            }

            insertOffset += endEdges;
            // bot right
            verts[insertOffset++] = new Vector2(radius, halfHeight);
            // top right
            verts[insertOffset++] = new Vector2(radius, -halfHeight);

            for (int i = 0; i < endEdges; ++i)
            {
                verts[i + insertOffset] = new Vector2(radius * gxtMath.Cos(step * (i + 1)), -halfHeight - radius * gxtMath.Sin(step * (i + 1)));
            }

            return verts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="uvType"></param>
        /// <param name="translateToOrigin"></param>
        /// <returns></returns>
        public static Vector2[] CalculateTextureCoordinates(Vector2[] verts, int textureWidth, int textureHeight, gxtTextureCoordinateType uvType)
        {
            return CalculateTextureCoordinates(verts, textureWidth, textureHeight, Vector2.One, 0.0f, uvType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="textureWidth"></param>
        /// <param name="textureHeight"></param>
        /// <param name="textureScale"></param>
        /// <param name="textureRotation"></param>
        /// <param name="uvType"></param>
        /// <param name="translateToOrigin"></param>
        /// <returns></returns>
        public static Vector2[] CalculateTextureCoordinates(Vector2[] verts, int textureWidth, int textureHeight, Vector2 textureScale, float textureRotation, gxtTextureCoordinateType uvType)
        {
            gxtDebug.Assert(verts != null && verts.Length >= 2);
            gxtDebug.Assert(textureWidth >= 0 && textureHeight >= 0);
            gxtDebug.Assert(!gxtMath.Equals(textureScale.X, 0.0f, float.Epsilon) && !gxtMath.Equals(textureScale.Y, 0.0f, float.Epsilon));

            gxtAABB vertsAABB = gxtGeometry.ComputeAABB(verts);
            Vector2 min = new Vector2(verts[0].X, verts[0].Y);
            for (int i = 1; i < verts.Length; ++i)
            {
                if (verts[i].X < min.X)
                    min.X = verts[i].X;
                if (verts[i].Y < min.Y)
                    min.Y = verts[i].Y;
            }

            float sw = textureWidth * textureScale.X;
            float sh = textureHeight * textureScale.Y;
            Vector2 oneOverSizeVector = new Vector2(1.0f / sw, 1.0f / sh);
            Vector2 xAxis, yAxis;
            gxtMath.GetAxesVectors(textureRotation, out xAxis, out yAxis);

            Vector2[] textureCoords = new Vector2[verts.Length];
            if (uvType == gxtTextureCoordinateType.CLAMP)
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    Vector2 pos = new Vector2(verts[i].X, verts[i].Y);
                    Vector2 projPos = new Vector2(Vector2.Dot(pos - min, xAxis), Vector2.Dot(pos - min, yAxis));
                    textureCoords[i] = new Vector2(gxtMath.Clamp(projPos.X * oneOverSizeVector.X, 0.0f, 1.0f), gxtMath.Clamp(projPos.Y * oneOverSizeVector.Y, 0.0f, 1.0f));
                }
            }
            else
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    Vector2 pos = new Vector2(verts[i].X, verts[i].Y);
                    Vector2 projPos = new Vector2(Vector2.Dot(pos - min, xAxis), Vector2.Dot(pos - min, yAxis));
                    textureCoords[i] = new Vector2((projPos.X * oneOverSizeVector.X), (projPos.Y * oneOverSizeVector.Y));
                }
            }
            return textureCoords;
        }

        // convex hull methods??
    }
}
