/*
using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


namespace GXT.Rendering
{
    /// <summary>
    /// Class for drawable line given a start and endposition
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtLine : gxtIDraw
    {
        #region Fields
        /// <summary>
        /// Draw It?
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Start position of the line
        /// </summary>
        public Vector2 Start { get; set; }

        /// <summary>
        /// The end position of the line
        /// </summary>
        public Vector2 End { get; set; }

        /// <summary>
        /// Depth
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Color of the line
        /// </summary>
        public Color ColorOverlay { get; set; }
        public byte Alpha { get { return ColorOverlay.A; } set { ColorOverlay = new Color(ColorOverlay.R, ColorOverlay.G, ColorOverlay.B, value); } }

        /// <summary>
        /// Thickness of the line
        /// </summary>
        public float LineThickness { get; set; }

        //public Vector2 Origin { get { return new Vector2(0, 0.5f); } }  // maybe get rid of this??
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Parameterless constructor.  XML only!
        /// </summary>
        public gxtLine() { }

        /// <summary>
        /// Takes start and enpoint for the line
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        public gxtLine(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
            Visible = true;
            ColorOverlay = Color.White;
            LineThickness = 1.5f;
            Depth = 0;
        }

        /// <summary>
        /// Takes start, end point, and color
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <param name="colorOverlay">Color</param>
        public gxtLine(Vector2 start, Vector2 end, Color colorOverlay)
        {
            Start = start;
            End = end;
            Visible = true;
            Depth = 0;
            ColorOverlay = colorOverlay;
            LineThickness = 1.5f;
        }

        /// <summary>
        /// Takes start, end point, color, and line thickness
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <param name="colorOverlay">Color</param>
        /// <param name="lineThickness">Line Thickness</param>
        public gxtLine(Vector2 start, Vector2 end, Color colorOverlay, float lineThickness)
        {
            Start = start;
            End = end;
            Visible = true;
            Depth = 0;
            ColorOverlay = colorOverlay;
            LineThickness = lineThickness;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="colorOverlay"></param>
        /// <param name="lineThickness"></param>
        /// <param name="depth"></param>
        public gxtLine(Vector2 start, Vector2 end, Color colorOverlay, float lineThickness, float depth)
        {
            Start = start;
            End = end;
            Visible = true;
            Depth = depth;
            ColorOverlay = colorOverlay;
            LineThickness = lineThickness;
        }
        #endregion Constructors

        #region Culling
        public gxtAABB GetAABB()
        {
            Vector2 c = (Start + End) * 0.5f;
            float rX = gxtMath.Abs(End.X - c.X);
            float rY = gxtMath.Abs(End.Y - c.Y);
            return new gxtAABB(c, new Vector2(rX, rY));
        }

        public gxtOBB GetOBB()
        {
            // Could likely be improved, but not a crucial optimization
            Vector2 d = End - Start;
            Vector2 localX = Vector2.Normalize(d);
            Vector2 localY = new Vector2(-d.Y, d.X);
            Vector2 r = new Vector2(d.Length() * 0.5f, LineThickness * 0.5f);
            Vector2 c = (End + Start) * 0.5f;
            return new gxtOBB(c, r, localX, localY);
        }
        #endregion Culling

        #region Draw
        /// <summary>
        /// Draws the line
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        public void Draw(ref SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            Vector2 d = End - Start;
            float angle = gxtMath.Atan2(d.Y, d.X);
            float dist = d.Length() + 1;

            spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, Start, null, ColorOverlay, angle, new Vector2(0, 0.5f),
                new Vector2(dist, LineThickness), SpriteEffects.None, Depth);
        }

        public static void Draw(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float depth, float lineThickness)
        {
            Vector2 d = end - start;
            float angle = gxtMath.Atan2(d.Y, d.X);
            float dist = d.Length() + 1;

            spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, start, null, color, angle, new Vector2(0, 0.5f),
                new Vector2(dist, lineThickness), SpriteEffects.None, depth);
        }
        #endregion Draw
    }

    public struct gxtPtPair
    {
        public Vector2 start;
        public Vector2 end;

        public gxtPtPair(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }
    }

    
    public class gxtLineBatch : gxtIDraw
    {
        public bool Visible { get; set; }
        public float Depth { get; set; }
        public Color ColorOverlay { get; set; }
        public float LineThickness { get; set; }
        private List<Vector2> pts;

        private gxtAABB aabb;

        public gxtLineBatch()
        {
            ColorOverlay = Color.White;
            Depth = 0;
            LineThickness = 1.15f;
            Visible = true;
            pts = new List<Vector2>();
            aabb = new gxtAABB(Vector2.Zero, new Vector2(float.NegativeInfinity));
        }

        public gxtLineBatch(List<gxtPtPair> ptPairs, Color color)
        {
            ColorOverlay = color;
            Depth = 0;
            LineThickness = 1.15f;
            Visible = true;
            pts = new List<Vector2>();
            aabb = new gxtAABB(Vector2.Zero, new Vector2(float.NegativeInfinity));
        }

        private bool IsExtremity(Vector2 pt)
        {
            if (pt.X <= aabb.Left || pt.X >= aabb.Right)
                return true;
            if (pt.Y <= aabb.Top || pt.Y >= aabb.Bottom)
                return true;
            return false;
        }

        public void Add(gxtPtPair ptPair)
        {
            // store min and max to avoid sub calc
            Vector2 min = aabb.Min;
            Vector2 max = aabb.Max;

            // find new min/max values
            min.X = gxtMath.Min3(min.X, ptPair.start.X, ptPair.end.X);
            max.X = gxtMath.Max3(max.X, ptPair.start.X, ptPair.end.X);
            min.Y = gxtMath.Min3(min.Y, ptPair.start.Y, ptPair.end.Y);
            max.Y = gxtMath.Max3(max.Y, ptPair.start.Y, ptPair.end.Y);

            // calc new aabb
            Vector2 c = (min + max) * 0.5f;
            aabb = new gxtAABB(c, max - c);

            // add points
            pts.Add(ptPair.start);
            pts.Add(ptPair.end);
        }

        public bool Remove(gxtPtPair ptPair)
        {
            int rmIdx = FindIndex(ptPair);
            if (rmIdx == -1) return false;
            RemoveAt(rmIdx);
            if (pts.Count == 0) // empty aabb
                aabb = new gxtAABB(Vector2.Zero, new Vector2(float.NegativeInfinity));
            if (IsExtremity(ptPair.start) || IsExtremity(ptPair.end))   // if not extremity don't recalc
                aabb = gxtGeometry.ComputeAABB(pts.ToArray());
            return true;
        }

        private void RemoveAt(int i)
        {
            pts.RemoveAt(i);
            pts.RemoveAt(i);
        }

        public void Clear()
        {
            pts.Clear();
            aabb = new gxtAABB(Vector2.Zero, new Vector2(float.NegativeInfinity));
        }

        public int NumLines()
        {
            return pts.Count / 2;
        }

        public bool IsValid()
        {
            return pts.Count % 2 == 0;
        }

        private int FindIndex(gxtPtPair ptPair)
        {
            for (int i = 0; i < pts.Count; i += 2)
            {
                if (pts[i] == ptPair.start)
                    if (pts[i + 1] == ptPair.end)
                        return i;
            }
            return -1;
        }

        /// <summary>
        /// returns aabb which is adjusted on add/remove ops
        /// </summary>
        /// <returns></returns>
        public gxtAABB GetAABB()
        {
            return aabb;
        }

        public gxtOBB GetOBB()
        {
            return new gxtOBB(aabb.Position, aabb.Extents);
        }

        public void Draw(ref SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            for (int i = 1; i < pts.Count; i += 2)
            {
                Vector2 d = pts[i] - pts[i - 1];
                float ang = gxtMath.Atan2(d.Y, d.X);
                float dist = d.Length() + 1;
                spriteBatch.Draw(gxtPrimitiveManager.Singleton.PixelTexture, pts[i - 1], null, ColorOverlay, ang, new Vector2(0, 0.5f), new Vector2(dist, LineThickness), SpriteEffects.None, Depth);
            }
        }
    }
}
*/