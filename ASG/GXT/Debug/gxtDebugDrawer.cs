using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GXT.Rendering;

namespace GXT
{
    /// <summary>
    /// A manager for in game debug drawing of primitives and text
    /// Extremely useful for testing purposes as it can be used to render actual geometry 
    /// without adding drawable functionality to game components which aren't meant to support it
    /// Debug drawing is inherently inefficient, so only use for testing purposes
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: dirty flags so isset function isn't called every time?
    // TODO: keep a current debug material?
    public class gxtDebugDrawer : gxtSingleton<gxtDebugDrawer>
    {
        #region Fields
        // basic flags
        private bool enabled;
        private bool visible;
        private bool fillGeometry;

        // scene management
        private int nextId;
        private int curId;
        // id -> SceneGraph
        private Dictionary<int, gxtSceneGraph> sceneGraphs;

        // collection of current debug nodes
        // they are removed when updates are performed
        private List<gxtDebugNode> debugNodes;

        // rendering properties, in virtual coordinates
        // these sizes are relative to the camera's zoom
        private float halfPtSize;
        private float rayLength;
        private float maxRayLength;
        private float rayArrowLength;
        private float rayAngleOffset;

        private float debugScale;
        private float debugRenderDepth;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Is the component enabled?  If not, no debug drawables will be added/updated.
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Flag for turning on/off debug rendering
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    for (int i = 0; i < NumDebugNodes; i++)
                    {
                        debugNodes[i].Node.SetVisibility(value, false);
                    }
                    visible = value;
                }
            }
        }

        /// <summary>
        /// Determines if debug renderable geometry is filled or drawn with lines and shells
        /// </summary>
        public bool FillGeometry { get { return fillGeometry; } set { fillGeometry = value; } }

        /// <summary>
        /// Current scene id
        /// </summary>
        public int CurrentSceneId { get { return curId; } set { curId = value; } }

        /// <summary>
        /// Gets a new unique id
        /// </summary>
        /// <returns>id</returns>
        public int GetNewId()
        {
            return nextId++;
        }

        /// <summary>
        /// The font used in AddString() function calls
        /// </summary>
        public SpriteFont DebugFont { get; set; }

        /// <summary>
        /// Gets the amount of active debug drawables
        /// </summary>
        public int NumDebugNodes { get { return debugNodes.Count; } }

        /// <summary>
        /// The diameter of circles created by AddPoint()
        /// </summary>
        public float PtSize { get { return halfPtSize * 2.0f; } set { gxtDebug.Assert(value >= 0.0f); halfPtSize = value * 0.5f; } }

        /// <summary>
        /// Default length, in virtual coordinates, of any drawn ray
        /// </summary>
        public float RayLength { get { return rayLength; } set { gxtDebug.Assert(value >= 0.0f); rayLength = value; } }

        /// <summary>
        /// Max length of a drawn ray
        /// </summary>
        public float MaxRayLength { get { return maxRayLength; } set { gxtDebug.Assert(value >= rayLength); maxRayLength = value; } }

        /// <summary>
        /// Drawn length of the arrow tips used to draw rays
        /// </summary>
        public float RayArrowLength { get { return rayArrowLength; } set { gxtDebug.Assert(value > 0.0f); rayArrowLength = value; } }

        /// <summary>
        /// Angle in radians the arrows are offset from the main line
        /// </summary>
        public float RayAngleOffset { get { return rayAngleOffset; } set { rayAngleOffset = value; } }

        /// <summary>
        /// Current scale factor to apply to debug drawables
        /// Very useful if you wish to draw objects in a different coordinate system (e.g. physics geoms)
        /// </summary>
        public float CurrentScale { get { return debugScale; } set { gxtDebug.Assert(value > 0.0f); debugScale = value; } }

        /// <summary>
        /// Current render depth to draw debug drawables 
        /// unless specified by a method that explictly takes that value
        /// </summary>
        public float CurrentRenderDepth { get { return debugRenderDepth; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f); debugRenderDepth = value; } }
        #endregion Properties

        #region Init/Scene
        /// <summary>
        /// Determines if the class has been initialized
        /// You should only call the Initialize() method once
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return debugNodes != null && sceneGraphs != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initEnabled">If enabled on init</param>
        /// <param name="initVisibility">If visible on init</param>
        /// <param name="scale">The current debug scale on init</param>
        /// <param name="renderDepth">The current render depth on init</param>
        /// <param name="debugFont">Font used for debug strings.  Can be null, but this won't allow for debug drawing of strings</param>
        /// <param name="fillGeometry">If geometry should be filled or drawn with lines</param>
        /// <param name="pointSize">The diameter of points rendered by the debug drawer</param>
        /// <param name="defaultRayLength">Default virtual size of a ray drawn by the debug drawer</param>
        /// <param name="maxRayLength">Max virtual size drawn by the debug drawer</param>
        /// <param name="rayArrowLength">Virtual size of arrow tips used to draw rays</param>
        /// <param name="rayAngleOffset">Angle offset of the arrows used to draw rays</param>
        public void Initialize(bool initEnabled = true, bool initVisibility = true, float scale = 1.0f, float renderDepth = 0.1f, SpriteFont debugFont = null, bool fillGeometry = false, 
            float pointSize = 6.0f, float defaultRayLength = 50.0f, float maxRayLength = 10000.0f, float rayArrowLength = 12.5f, float rayAngleOffset = 0.5f)
        {
            gxtDebug.Assert(!IsInitialized(), "The Debug Drawer Has Already Been Initialized!");

            Enabled = initEnabled;
            debugNodes = new List<gxtDebugNode>();
            Visible = initVisibility;

            this.halfPtSize = pointSize * 0.5f;
            this.rayLength = defaultRayLength;
            this.maxRayLength = maxRayLength;
            this.rayArrowLength = rayArrowLength;
            this.rayAngleOffset = rayAngleOffset;

            sceneGraphs = new Dictionary<int, gxtSceneGraph>();
            nextId = 0;

            DebugFont = debugFont;
            fillGeometry = false;
            
            debugScale = scale;
            debugRenderDepth = renderDepth;
        }

        /// <summary>
        /// Registers a scene graph with the debug drawer using the given id
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="sceneGraph">Scene Graph</param>
        public void AddSceneGraph(int id, gxtSceneGraph sceneGraph)
        {
            gxtDebug.Assert(!sceneGraphs.ContainsKey(id) && !sceneGraphs.ContainsValue(sceneGraph));
            sceneGraphs.Add(id, sceneGraph);
        }

        /// <summary>
        /// Removes a scene graph from the debug drawer with the given id
        /// </summary>
        /// <param name="id"></param>
        public void RemoveSceneGraph(int id)
        {
            gxtDebug.Assert(sceneGraphs.ContainsKey(id));
            sceneGraphs.Remove(id);
        }

        /// <summary>
        /// Removes everything from the debug drawing queue
        /// </summary>
        public void ClearAllDebugNodes()
        {
            for (int i = NumDebugNodes - 1; i >= 0; i--)
            {
                sceneGraphs[debugNodes[i].SceneId].RemoveNode(debugNodes[i].Node);
                debugNodes.RemoveAt(i);
            }
        }

        /// <summary>
        /// Removes everything from the debug drawing queue with the currently set id
        /// Note, this does not remove the associated scene graph
        /// </summary>
        public void ClearCurrentDebugNodes()
        {
            for (int i = NumDebugNodes - 1; i >= 0; i--)
            {
                if (debugNodes[i].SceneId == curId)
                {
                    sceneGraphs[curId].RemoveNode(debugNodes[i].Node);
                    debugNodes.RemoveAt(i);
                }
            }
        }
        #endregion Init/Scene

        /// <summary>
        /// Helper function, constructs a node attaches the debug drawable, and adds 
        /// it to the proper scene graph
        /// </summary>
        /// <param name="drawable">Drawable In Local Space</param>
        /// <param name="position">World Position</param>
        /// <param name="rotation">World Rotation</param>
        /// <param name="color">Color</param>
        /// <param name="duration">Debug Drawable LifeSpan</param>
        private void DebugAdd(gxtIDrawable drawable, Vector2 position, float rotation, TimeSpan duration)
        {
            gxtISceneNode node = new gxtSceneNode();
            node.Position = position;
            node.Rotation = rotation;
            node.AttachDrawable(drawable);

            gxtDebugNode debugN = new gxtDebugNode(node, duration, curId);
            sceneGraphs[curId].AddNode(node);
            debugNodes.Add(debugN);
        }


        #region AABB
        public void AddAABB(gxtAABB aabb, Color color)
        {
            AddAABB(aabb, color, debugRenderDepth);
        }
        
        /// <summary>
        /// Adds an AABB to the debug drawing queue
        /// </summary>
        /// <param name="aabb">AABB to draw</param>
        /// <param name="color">AABB color</param>
        /// <param name="renderDepth">Render Depth of the visual representation</param>
        public void AddAABB(gxtAABB aabb, Color color, float renderDepth)
        {
            AddAABB(aabb, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an AABB to the debug drawing queue for a given duration
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddAABB(gxtAABB aabb, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                gxtIMaterial aabbMaterial = new gxtMaterial(true, color, renderDepth);
                if (FillGeometry)
                {
                    gxtRectangle r = new gxtRectangle(aabb.Width * debugScale, aabb.Height * debugScale);
                    r.Material = aabbMaterial;
                    DebugAdd(r, aabb.Position * debugScale, 0.0f, duration);
                }
                else
                {
                    Vector2[] aabbVertices = gxtGeometry.CreateRectangleVertices(aabb.Width * debugScale, aabb.Height * debugScale);
                    gxtLineLoop lineLoop = new gxtLineLoop(aabbVertices, aabbMaterial, true);
                    DebugAdd(lineLoop, aabb.Position * debugScale, 0.0f, duration);
                }
            }
        }

        public void AddAABB(Vector2 min, Vector2 max, Color color)
        {
            AddAABB(min, max, color, debugRenderDepth);
        }

        /// <summary>
        /// Adds an AABB to the debug drawing queue (given min and max points)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddAABB(Vector2 min, Vector2 max, Color color, float renderDepth)
        {
            AddAABB(min, max, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an AABB to the debug drawing queue (given min and max points) for a given duration
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddAABB(Vector2 min, Vector2 max, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                gxtIMaterial aabbMaterial = new gxtMaterial(true, color, renderDepth);
                if (FillGeometry)
                {
                    Vector2 sz = max - min;
                    Vector2 pos = (max + min) * 0.5f;
                    gxtRectangle r = new gxtRectangle(sz.X * debugScale, sz.Y * debugScale, aabbMaterial);
                    DebugAdd(r, pos * debugScale, 0.0f, duration);
                }
                else
                {
                    Vector2 c = (max + min) * 0.5f;
                    Vector2[] aabbVertices = gxtGeometry.CreateRectangleVertices((max.X - min.X) * debugScale, (max.Y - min.Y) * debugScale);
                    gxtLineLoop lineLoop = new gxtLineLoop(aabbVertices, aabbMaterial, true);
                    DebugAdd(lineLoop, c * debugScale, 0.0f, duration);
                }
            }
        }
        #endregion AABB

        #region Line/Ray
        public void AddLine(Vector2 start, Vector2 end, Color color)
        {
            AddLine(start, end, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a line to the debug drawing queue
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddLine(Vector2 start, Vector2 end, Color color, float renderDepth)
        {
            AddLine(start, end, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a line to the debug drawing queue for a given duration
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddLine(Vector2 start, Vector2 end, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                gxtIMaterial lineMaterial = new gxtMaterial(true, color, renderDepth);
                gxtLine line = new gxtLine(start * debugScale, end * debugScale, lineMaterial);
                DebugAdd(line, Vector2.Zero, 0.0f, duration);
            }
        }

        public void AddRay(gxtRay ray, Color color)
        {
            AddRay(ray, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a ray to the debug drawing queue
        /// Will be drawn with default ray length
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddRay(gxtRay ray, Color color, float renderDepth)
        {
            AddRay(ray, color, renderDepth, TimeSpan.Zero);
        }

        public void AddRay(gxtRay ray, float t, Color color)
        {
            AddRay(ray, t, color, debugRenderDepth, TimeSpan.Zero);
        }

        public void AddRay(gxtRay ray, float t, Color color, float renderDepth)
        {
            AddRay(ray, t, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a ray to the debug drawing queue for the given duration
        /// Will be drawn with default ray length
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddRay(gxtRay ray, Color color, float renderDepth, TimeSpan duration)
        {
            AddRay(ray, rayLength, color, renderDepth, duration);
        }

        /// <summary>
        /// Adds a ray with the given length to the debug drawing queue for the given duration
        /// Ray length will be capped, for rendering purposes, at MaxRayLength
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="t"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddRay(gxtRay ray, float t, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                Vector2 drawOrigin = ray.Origin * debugScale;
                float drawLength = gxtMath.Min(t * debugScale, maxRayLength);
                Vector2 drawEndpoint = drawOrigin + ray.Direction * drawLength;
                
                float rayAngle = gxtMath.Atan2(ray.Direction.Y, ray.Direction.X);
                float rightTipAngle = rayAngle + gxtMath.PI_OVER_TWO + rayAngleOffset;
                Vector2 rightTipDirection = new Vector2(gxtMath.Cos(rightTipAngle), gxtMath.Sin(rightTipAngle));
                float leftTipAngle = rayAngle - gxtMath.PI_OVER_TWO - rayAngleOffset;
                Vector2 leftTipDirection = new Vector2(gxtMath.Cos(leftTipAngle), gxtMath.Sin(leftTipAngle));

                // origin, endpoint, right tip endpoint, left tip endpoint
                //          3
                //           \
                // 0 -------- 1 
                //           /
                //          2
                Vector2[] arrowVertices = new Vector2[] { drawOrigin, drawEndpoint, drawEndpoint + rightTipDirection * rayArrowLength, drawEndpoint + leftTipDirection * rayArrowLength };
                int[] arrowIndices = new int[] { 0, 1, 1, 2, 1, 3 };

                gxtIMaterial arrowMaterial = new gxtMaterial(true, color, renderDepth);
                gxtLineList arrow = new gxtLineList(arrowVertices, arrowIndices, arrowMaterial);
                DebugAdd(arrow, Vector2.Zero, 0.0f, duration);
            }
        }
        #endregion Line/Ray

        #region Polygon
        public void AddPolygon(gxtPolygon polygon, Color color)
        {
            AddPolygon(polygon, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an unfilled polygon to the debug drawing queue
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddPolygon(gxtPolygon polygon, Color color, float renderDepth)
        {
            AddPolygon(polygon, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an unfilled polygon to the debug drawing queue for a given duration
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddPolygon(gxtPolygon polygon, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                gxtIMaterial polygonMaterial = new gxtMaterial(true, color, renderDepth);

                // create a properly scaled copy of the polygon
                gxtPolygon polyCopy = gxtPolygon.Copy(polygon);
                if (debugScale != 1.0f)
                {
                    Vector2 centroid = polyCopy.GetCentroid();
                    if (!centroid.Equals(Vector2.Zero))
                    {
                        polyCopy.Translate(-centroid);
                        polyCopy.Scale(debugScale);
                        polyCopy.Translate(centroid * debugScale);
                    }
                }

                if (FillGeometry)
                {
                    gxtMesh polygonMesh = new gxtMesh(polyCopy.v, polygonMaterial, true);
                    DebugAdd(polygonMesh, Vector2.Zero, 0.0f, duration);
                }
                else
                {
                    gxtUserLineLoop lineLoop = new gxtUserLineLoop(polyCopy.v, polygonMaterial, true);
                    DebugAdd(lineLoop, Vector2.Zero, 0.0f, duration);
                }
            }
        }
        #endregion Polygon

        #region Axes
        public void AddAxes(Vector2 position, float rotation, Color xAxisColor, Color yAxisColor)
        {
            AddAxes(position, rotation, xAxisColor, yAxisColor, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="xAxisColor"></param>
        /// <param name="yAxisColor"></param>
        /// <param name="renderDepth"></param>
        public void AddAxes(Vector2 position, float rotation, Color xAxisColor, Color yAxisColor, float renderDepth)
        {
            AddAxes(position, rotation, xAxisColor, yAxisColor, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="xAxisColor"></param>
        /// <param name="yAxisColor"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddAxes(Vector2 position, float rotation, Color xAxisColor, Color yAxisColor, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                Vector2 xAxis, yAxis;
                gxtMath.GetAxesVectors(rotation, out xAxis, out yAxis);
                Vector2 start = position * debugScale;

                gxtIMaterial xAxisMaterial = new gxtMaterial(true, xAxisColor, renderDepth);
                gxtIMaterial yAxisMaterial = new gxtMaterial(true, yAxisColor, renderDepth);

                gxtLine drawableXAxis = new gxtLine(start, start + (xAxis * rayLength), xAxisMaterial);
                gxtLine drawableYAxis = new gxtLine(start, start + (yAxis * rayLength), yAxisMaterial);
                
                DebugAdd(drawableXAxis, Vector2.Zero, 0.0f, duration);
                DebugAdd(drawableYAxis, Vector2.Zero, 0.0f, duration);
            }
        }
        #endregion Axes

        #region Circle/Pt/Sphere
        public void AddCircle(Vector2 center, float radius, Color color)
        {
            AddCircle(center, radius, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a circle to the debug drawing queue
        /// Alias of AddSphere()
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddCircle(Vector2 center, float radius, Color color, float renderDepth)
        {
            AddCircle(center, radius, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a circle to the debug drawing queue for the given duration
        /// Alias of AddSphere()
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddCircle(Vector2 center, float radius, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                Vector2 c = center * debugScale;
                float r = radius * debugScale;

                gxtIMaterial circleMaterial = new gxtMaterial(true, color, renderDepth);
                gxtCircle circle;
                
                if (FillGeometry)
                    circle = new gxtCircle(r, circleMaterial, gxtCircleDrawMode.CIRCLE);
                else
                    circle = new gxtCircle(r, circleMaterial, gxtCircleDrawMode.SHELL);

                DebugAdd(circle, c, 0.0f, duration);
            }
        }

        public void AddSphere(gxtSphere sphere, float radius, Color color)
        {
            AddCircle(sphere.Position, sphere.Radius, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a spehere to the debug drawing queue
        /// Alias of AddCircle()
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddSphere(gxtSphere sphere, Color color, float renderDepth)
        {
            AddCircle(sphere.Position, sphere.Radius, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a spehere to the debug drawing queue for a given duration
        /// Alias of AddCircle()
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddSphere(gxtSphere sphere, Color color, float renderDepth, TimeSpan duration)
        {
            AddCircle(sphere.Position, sphere.Radius, color, renderDepth, duration);
        }

        public void AddPt(Vector2 pt, Color color)
        {
            AddPt(pt, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a point to the debug drawing queue
        /// Same as adding a circle/sphere with a radius of DebugDrawer.PtSize / 2
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddPt(Vector2 pt, Color color, float renderDepth)
        {
            AddPt(pt, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a point to the debug drawing queue for a given duration
        /// Same as adding a circle/sphere with a radius of DebugDrawer.PtSize / 2
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddPt(Vector2 pt, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                gxtIMaterial ptMaterial = new gxtMaterial(true, color, renderDepth);
                gxtCircle circle = new gxtCircle(halfPtSize, ptMaterial);
                DebugAdd(circle, pt * debugScale, 0.0f, duration);
            }
        }
        #endregion Circle/Pt/Sphere

        #region OBB
        public void AddOBB(gxtOBB obb, Color color)
        {
            AddOBB(obb, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an OBB to the debug drawing queue
        /// </summary>
        /// <param name="obb"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddOBB(gxtOBB obb, Color color, float renderDepth)
        {
            AddOBB(obb, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds an OBB to the debug drawing queue
        /// </summary>
        /// <param name="obb"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddOBB(gxtOBB obb, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet())
            {
                gxtIMaterial obbMaterial = new gxtMaterial(true, color, renderDepth);
                if (FillGeometry)
                {
                    gxtRectangle r = new gxtRectangle(obb.Width * debugScale, obb.Height * debugScale, obbMaterial);
                    DebugAdd(r, obb.Position * debugScale, obb.Rotation, duration);
                }
                else
                {
                    Vector2[] obbVertices = gxtGeometry.CreateRectangleVertices(obb.Width * debugScale, obb.Height * debugScale);
                    gxtLineLoop lineLoop = new gxtLineLoop(obbVertices, obbMaterial, true);
                    DebugAdd(lineLoop, obb.Position * debugScale, obb.Rotation, duration);
                }
            }
        }
        #endregion OBB

        #region String
        public void AddString(string txt, Vector2 pos, Color color)
        {
            AddString(txt, pos, color, debugRenderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a readable text string to the debug drawing queue
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="pos"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        public void AddString(string txt, Vector2 pos, Color color, float renderDepth)
        {
            AddString(txt, pos, color, renderDepth, TimeSpan.Zero);
        }

        /// <summary>
        /// Adds a readable text string to the debug drawing queue for a given duration
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="pos"></param>
        /// <param name="color"></param>
        /// <param name="renderDepth"></param>
        /// <param name="duration"></param>
        public void AddString(string txt, Vector2 pos, Color color, float renderDepth, TimeSpan duration)
        {
            if (IsSet(true))
            {
                gxtIMaterial tfMaterial = new gxtMaterial(true, color, renderDepth);
                gxtTextField tf = new gxtTextField(DebugFont, txt, tfMaterial);
                DebugAdd(tf, pos * debugScale, 0.0f, duration);
            }
        }
        #endregion String

        /// <summary>
        /// Updates the debug drawer.  This function will remove 
        /// debug drawables from the appropriate draw manager if their time 
        /// has expired
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public void Update(GameTime gameTime)
        {
            if (!Enabled) return;

            for (int i = 0; i < debugNodes.Count; i++)
            {
                if (debugNodes[i].Update(gameTime))
                {
                    sceneGraphs[debugNodes[i].SceneId].RemoveNode(debugNodes[i].Node);
                    debugNodes.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Checks if the debug drawer is ready to draw
        /// Logs warnings for problems if found
        /// </summary>
        /// <param name="checkFont">Optional check to see if the debug drawer is ready to draw text</param>
        /// <returns>If the manger should process a draw request</returns>
        private bool IsSet(bool checkFont = false)
        {
            bool set = true;
            if (!Enabled)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Debug Drawer Is Not Enabled: No Debug Drawables Will Be Added Or Updated!");
                set = false;
            }
            if (nextId == 0)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Debug Drawer Has No Attached Scene Graphs: No Debug Drawables Will Be Added!");
            }
            if (!sceneGraphs.ContainsKey(curId))
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "No Scene Graph Is Set For Id {0}: Debug Drawables Will Not Be Added!", curId);
                set = false;
            }
            if (checkFont && DebugFont == null)
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "No Debug Font Is Set: Debug Drawable Strings Will Not Be Added!");
                set = false;
            }
            return set;
        }
    }
}