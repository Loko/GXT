using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GXT.Physics;


namespace GXT
{
    public class gxtSweepAndPruneArrayCollider
    {
        public class gxtSAPBox
        {
            public Vector2 min;
            public Vector2 max;
            public gxtGeom geom;

            public gxtSAPBox(gxtGeom geom)
            {
                this.geom = geom;
            }

            public void Update()
            {
                gxtAABB aabb = geom.AABB;
                this.min = aabb.Min;
                this.max = aabb.Max;
            }
        }

        public List<gxtSAPBox> colliderList;

        public void Initialize()
        {
            colliderList = new List<gxtSAPBox>();
        }

        public void Add(gxtGeom geom)
        {
            // make associated sap box
            gxtSAPBox addBox = new gxtSAPBox(geom);
            addBox.Update();

            int size = colliderList.Count;
            // if empty or greater than the rest of the collection add to end
            if (colliderList.Count == 0 || addBox.min.X >= colliderList[size - 1].min.X)
                colliderList.Add(addBox);
            // if less than rest of collection insert in front
            else if (addBox.min.X < colliderList[0].min.X)
                colliderList.Insert(0, addBox);
            else
            {
                int idx = 1;
                while (addBox.min.X > colliderList[idx].min.X)
                {
                    idx++;
                }
                colliderList.Insert(idx, addBox);
            }
        }

        public void Remove(gxtGeom geom)
        {
            float minX = geom.AABB.Min.X;
            int low = 0, high = colliderList.Count - 1;
            int mid;

            do
            {
                mid = low + ((high - low) / 2);
                if (geom == colliderList[mid].geom)
                {
                    colliderList.RemoveAt(mid);
                    return;
                }
                if (minX > colliderList[mid].min.X)
                    mid = low + 1;
                else
                    high = mid - 1;
            } while (low < high);

            gxtLog.WriteLineV(VerbosityLevel.WARNING, "Geom Was Not Found");
        }

        private int CompareBoxes(gxtSAPBox x, gxtSAPBox y)
        {
            if (x.min.X < y.min.X)
                return -1;
            if (x.min.X > y.min.X)
                return 1;
            return 0;
        }

        public void Sort()
        {
            for (int i = 0; i < colliderList.Count; i++)
            {
                colliderList[i].Update();
            }
            colliderList.Sort(CompareBoxes);
        }

        private bool Intersect(ref Vector2 minA, ref Vector2 maxA, ref Vector2 minB, ref Vector2 maxB)
        {
            if (maxA.X < minB.X || minA.X > maxB.X) return false;
            if (maxA.Y < minB.Y || minA.Y > maxB.Y) return false;
            return true;
        }

        public virtual bool CanPrune(int idxA, int idxB)
        {
            if (colliderList[idxA].geom.CollisionEnabled || colliderList[idxB].geom.CollisionEnabled) return true;
            if ((colliderList[idxA].geom.CollisionGroups & colliderList[idxB].geom.CollidesWithGroups) == gxtCollisionGroup.NONE ||
                        (colliderList[idxB].geom.CollidesWithGroups & colliderList[idxA].geom.CollisionGroups) == gxtCollisionGroup.NONE) return true;
            return false;
        }

        public void GetCollisionPairs(ref List<gxtGeomTestPair> testPairs)
        {
            for (int i = 0; i < colliderList.Count; i++)
            {
                for (int j = i + 1; j < colliderList.Count; j++)
                {
                    if (colliderList[j].min.X > colliderList[i].max.X)
                        break;
                    else if (!CanPrune(i, j))
                        if (Intersect(ref colliderList[i].min, ref colliderList[i].max, ref colliderList[j].min, ref colliderList[j].max))
                            testPairs.Add(new gxtGeomTestPair(i, j));
                }
            }
        }
    }
}
