using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GXT
{
    /// <summary>
    /// Container with built in operations for convex 
    /// counter clockwise 2D polygons
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: FIX ISCCW CALL
    public struct gxtPolygon : IEquatable<gxtPolygon>
    {
        /// <summary>
        /// Vertices
        /// </summary>
        public Vector2[] v;

        /// <summary>
        /// Size
        /// </summary>
        public int NumVertices { get { return v.Length; } }

        /// <summary>
        /// Sets up a polygon of the given size
        /// Make sure the vertices you specify later form 
        /// a convex counter clockwise polygon
        /// </summary>
        /// <param name="n"></param>
        public gxtPolygon(int n)
        {
            gxtDebug.Assert(n >= 3);
            v = new Vector2[n];
        }

        /// <summary>
        /// Sets up a polygon with an array of vertices
        /// </summary>
        /// <param name="vertices">Vertices</param>
        public gxtPolygon(Vector2[] vertices)
        {
            v = vertices;
            //gxtDebug.Assert(IsValid());
        }

        /// <summary>
        /// Sets up a polygon with a list of vertices
        /// </summary>
        /// <param name="vertices">Vertices</param>
        public gxtPolygon(List<Vector2> vertices)
        {
            v = vertices.ToArray();
            //gxtDebug.Assert(IsValid());
        }

        /// <summary>
        /// Sets up a polygon with an enumerator of vertices
        /// </summary>
        /// <param name="vertices">Vertices</param>
        public gxtPolygon(IEnumerator<Vector2> vertices)
        {
            List<Vector2> verts = new List<Vector2>();
            while (vertices.MoveNext())
            {
                verts.Add(vertices.Current);
            }
            v = verts.ToArray();
            //gxtDebug.Assert(IsValid());
        }

        public static gxtPolygon Copy(gxtPolygon p)
        {
            Vector2[] vertices = new Vector2[p.NumVertices];
            for (int i = 0; i < p.NumVertices; ++i)
            {
                vertices[i] = p.v[i];
            }
            return new gxtPolygon(vertices);
        }

        #region HelperFunctions
        /// <summary>
        /// Next vertex index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int NextIndex(int i)
        {
            gxtDebug.Assert(i >= 0 && i < v.Length);
            if (i == v.Length - 1)
                return 0;
            return i + 1;
        }

        /// <summary>
        /// Prev vertex index
        /// </summary>
        /// <param name="i">index</param>
        /// <returns></returns>
        public int PrevIndex(int i)
        {
            gxtDebug.Assert(i >= 0 && i < v.Length);
            if (i == 0)
                return v.Length - 1;
            return i - 1;
        }

        /// <summary>
        /// Gets edge segment formed by this and the 
        /// next vertex
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector2 GetEdge(int i)
        {
            return v[NextIndex(i)] - v[i];
        }

        /// <summary>
        /// Gets the midpoint on the edge segment
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector2 GetEdgeMidPoint(int i)
        {
            return v[i] + ((v[NextIndex(i)] - v[i]) * 0.5f);
        }

        /// <summary>
        /// Gets the edge normal for the indexed edge
        /// </summary>
        /// <param name="i">Edge Index</param>
        /// <returns>Edge Normal (normalized right perp of the edge)</returns>
        public Vector2 GetEdgeNormal(int i)
        {
            Vector2 e = GetEdge(i);
            float prevX = e.X;
            e.X = -e.Y;
            e.Y = prevX;
            e.Normalize();
            return e;
        }

        /// <summary>
        /// Gets the vertex normal, summing and then normalizing 
        /// the two adjacent edge normals
        /// </summary>
        /// <param name="i">Vertex Index</param>
        /// <returns>Normal at the vertex</returns>
        public Vector2 GetVertexNormal(int i)
        {
            Vector2 en0 = GetEdgeNormal(i);
            Vector2 en1 = GetEdgeNormal(PrevIndex(i));
            return Vector2.Normalize(en0 + en1);
        }
        #endregion HelperFunctions

        #region Transformations
        /// <summary>
        /// Transforms the vertices by the translation vector
        /// </summary>
        /// <param name="t">Translation</param>
        public void Translate(Vector2 t)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] += t;
            }
        }

        /// <summary>
        /// Transforms the vertices by the scale vector
        /// </summary>
        /// <param name="scale">Scale</param>
        public void Scale(Vector2 scale)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i].X *= scale.X;
                v[i].Y *= scale.Y;
            }
        }

        /// <summary>
        /// Transforms the vertices by the uniform scale factor
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(float scale)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] *= scale;
            }
        }

        /// <summary>
        /// Rotates the vertices by a given angle in radians
        /// </summary>
        /// <param name="rad">Radians</param>
        public void Rotate(float rad)
        {
            Matrix rotMat;
            Matrix.CreateRotationZ(rad, out rotMat);

            for (int i = 0; i < NumVertices; i++)
            {
                v[i] = Vector2.Transform(v[i], rotMat);
            }
        }
        #endregion Transformations

        #region Centroid/Convexity
        /// <summary>
        /// Calculates the signed area of the polygon
        /// </summary>
        /// <returns>Signed Area</returns>
        private float GetSignedArea()
        {
            float area = 0.0f;

            for (int j = NumVertices - 1, i = 0; i < NumVertices; j = i, i++)
            {
                area += v[j].X * v[i].Y;
                area -= v[j].Y * v[i].X;
            }
            area *= 0.5f;
            return area;
        }

        /// <summary>
        /// Calculates the unsigned area of the polygon
        /// </summary>
        /// <returns>Unsigned Area</returns>
        public float GetArea()
        {
            return gxtMath.Abs(GetSignedArea());
        }

        /// <summary>
        /// Determines if the polygon is in counter clockwise winding
        /// Cross products for every pair of successive vertices must be 
        /// negative for the polygon to be CCW
        /// </summary>
        /// <returns>if ccw</returns>
        public bool IsCCW()
        {
            if (NumVertices == 3)
                return gxtMath.IsCCWTriangle(v[0], v[1], v[2]);

            for (int j = NumVertices - 1, i = 0; i < NumVertices; j = i, i++)
            {
                if (gxtMath.Cross2D(v[j], v[i]) > float.Epsilon)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if the polygon meets the restrictions meaning 
        /// a min size of three vertices and counter clockwise ordering
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return v != null && v.Length >= 3 && IsCCW();
        }

        /// <summary>
        /// Gets the centroid given the signed 
        /// area of the polygon
        /// </summary>
        /// <param name="area">Signed Area</param>
        /// <returns>Centroid</returns>
        public Vector2 GetCentroid(float signedArea)
        {
            float cx = 0.0f, cy = 0.0f;
            float factor;
            int i;

            float area = gxtMath.Abs(signedArea);

            // if positive traverse verts backwards
            if (signedArea > 0)
            {
                for (i = NumVertices - 1; i >= 0; i--)
                {
                    int j = (i - 1) % NumVertices;

                    if (j < 0)
                        j += NumVertices;

                    factor = -(v[i].X * v[j].Y - v[j].X * v[i].Y);
                    cx += (v[i].X + v[j].X) * factor;
                    cy += (v[i].Y + v[j].Y) * factor;
                }

            }
            else
            {
                // otherwise traverse forwards
                for (i = 0; i < NumVertices; i++)
                {
                    int j = (i + 1) % NumVertices;

                    factor = -(v[i].X * v[j].Y - v[j].X * v[i].Y);
                    cx += (v[i].X + v[j].X) * factor;
                    cy += (v[i].Y + v[j].Y) * factor;
                }
            }

            area *= 6.0f;
            factor = 1.0f / area;
            cx *= factor;
            cy *= factor;

            return new Vector2(cx, cy);
        }

        /// <summary>
        /// Calculates the centroid of the polygon
        /// </summary>
        /// <returns>Centroid</returns>
        public Vector2 GetCentroid()
        {
            return GetCentroid(GetSignedArea());
        }
        #endregion Centroid/Convexity

        #region Equality
        /// <summary>
        /// Equality override
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>If equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is gxtPolygon)
                return Equals((gxtPolygon)obj);
            return false;
        }

        /// <summary>
        /// Equality override
        /// </summary>
        /// <param name="other">Polygon</param>
        /// <returns>If equal</returns>
        public bool Equals(gxtPolygon other)
        {
            if (NumVertices != other.NumVertices)
                return false;

            for (int i = 0; i < NumVertices; i++)
            {
                if (v[i] != other.v[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Equality Operator Override
        /// </summary>
        /// <param name="polyA">Polygon A</param>
        /// <param name="polyB">Polygon B</param>
        /// <returns>If equal</returns>
        public static bool operator ==(gxtPolygon polyA, gxtPolygon polyB)
        {
            return polyA.Equals(polyB);
        }

        /// <summary>
        /// Inequality Operator Override
        /// </summary>
        /// <param name="polyA">Polygon A</param>
        /// <param name="polyB">Polygon B</param>
        /// <returns></returns>
        public static bool operator !=(gxtPolygon polyA, gxtPolygon polyB)
        {
            return !polyA.Equals(polyB);
        }

        /// <summary>
        /// Hash Code Override
        /// </summary>
        /// <returns>Hash Code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion Equality
    }
}

