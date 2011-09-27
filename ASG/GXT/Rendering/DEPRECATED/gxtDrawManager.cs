using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    public class gxtDrawManager
    {
        /*
        public class gxtSAPBox
        {
            public Vector2 min;
            public Vector2 max;
            public gxtIDraw drawable;

            public gxtSAPBox(gxtIDraw drawable)
            {
                this.drawable = drawable;
            }

            public void Update()
            {
                gxtAABB aabb = drawable.GetAABB();
                this.min = aabb.Min;
                this.max = aabb.Max;
            }
        }

        private List<gxtSAPBox> drawableList;

        public void Initialize()
        {
            drawableList = new List<gxtSAPBox>();
        }

        private int FindIndex(float x)
        {
            int idx = 0;
            while (x > drawableList[idx].min.X)
            {
                idx++;
            }
            return idx;
            //
            int low = 0, high = drawableList.Count;
            int mid;
            do
            {
                mid = low + (high - low) / 2;
                if (drawableList[mid].min.X > minX)

                else if (minX < )
                    high = mid;
            } while (low < high);
            //
        }

        public void Add(gxtIDraw drawable)
        {
            // make associated sap box
            gxtSAPBox addBox = new gxtSAPBox(drawable);
            addBox.Update();

            int size = drawableList.Count;
            // if empty or greater than the rest of the collection add to end
            if (drawableList.Count == 0 || addBox.min.X >= drawableList[size - 1].min.X)
                drawableList.Add(addBox);
            // if less than rest of collection insert in front
            else if (addBox.min.X < drawableList[0].min.X)
                drawableList.Insert(0, addBox);
            else
            {
                int idx = 1;
                while (addBox.min.X > drawableList[idx].min.X)
                {
                    idx++;
                }
                drawableList.Insert(idx, addBox);
            }
        }

        public void Remove(gxtIDraw drawable)
        {
            float minX = drawable.GetAABB().Min.X;
            int low = 0, high = drawableList.Count - 1;
            int mid;
            
            do {
                mid = low + ((high - low) / 2);
                if (drawable == drawableList[mid])
                {
                    drawableList.RemoveAt(mid);
                    break;
                }
                if (minX > drawableList[mid].min.X)
                    mid = low + 1;
                else
                    high = mid - 1;
            } while (low < high);

            gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Drawable Was Not Removed From Draw Manager Because It Could Not Be Found");
        }


        private int CompareBoxes(gxtSAPBox x, gxtSAPBox y)
        {
            if (x.min.X < y.min.X)
                return -1;
            if (x.min.X > y.min.X)
                return 1;
            return 0;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < drawableList.Count; i++)
            {
                drawableList[i].Update();
            }
            drawableList.Sort(CompareBoxes);
        }

        private bool Intersect(ref Vector2 minA, ref Vector2 maxA, ref Vector2 minB, ref Vector2 maxB)
        {
            if (maxA.X < minB.X || minA.X > maxB.X) return false;
            if (maxA.Y < minB.Y || minA.Y > maxB.Y) return false;
            return true;
        }


        public void Draw(ref SpriteBatch spriteBatch, gxtAABB viewAABB)
        {
            // could conceivably call this numerous times for each camera
            Vector2 camMin = viewAABB.Min;
            Vector2 camMax = viewAABB.Max;
            int maxIdx = FindIndex(camMax.X);

        }
        */

        private List<gxtIDraw> drawableList;
        private List<gxtIDraw> removeList;

        //private List<gxtIDraw> drawList;

        #region Constructor/Init
        public gxtDrawManager() { }

        public void Initialize()
        {
            drawableList = new List<gxtIDraw>();
            removeList = new List<gxtIDraw>();
            //drawList = new List<gxtIDraw>();
        }

        public void Initialize(IEnumerable<gxtIDraw> drawables)
        {
            drawableList = new List<gxtIDraw>(drawables);
            removeList = new List<gxtIDraw>();
            //drawList = new List<gxtIDraw>();
        }
        #endregion Constructor/Init

        #region Add/Remove
        public void Remove(gxtIDraw drawable)
        {
            removeList.Add(drawable);
        }

        public void Add(gxtIDraw drawable)
        {
            drawableList.Add(drawable);
        }
        #endregion Add/Remove

        #region Removal/Clear
        private void ProcessedRemoved()
        {
            for (int i = 0; i < removeList.Count; i++)
            {
                drawableList.Remove(removeList[i]);
            }
        }

        private void ClearLists()
        {
            removeList.Clear();
            //drawList.Clear();
        }
        #endregion Removal/Clear

        #region Update/Cull
        public void Update()
        {
            ProcessedRemoved();
            ClearLists();
            //Cull(camAABB);
        }

        /*
        private void Cull(gxtAABB camAABB)
        {
            // brute force culling, for now
            for (int i = 0; i < drawableList.Count; i++)
            {
                if (gxtAABB.Intersects(camAABB, drawableList[i].GetAABB()))
                    drawList.Add(drawableList[i]);
            }
        }
        */
        #endregion Update/Cull

        #region Draw
        public void Draw(ref SpriteBatch spriteBatch, gxtAABB cameraAABB)
        {
            // brute force culling, for now
            for (int i = 0; i < drawableList.Count; i++)
            {
                if (gxtAABB.Intersects(cameraAABB, drawableList[i].GetAABB()))
                    drawableList[i].Draw(ref spriteBatch);
            }
        }
        #endregion Draw
        
    }
}
