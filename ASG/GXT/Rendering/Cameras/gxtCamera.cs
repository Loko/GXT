using System;
using Microsoft.Xna.Framework;

namespace GXT.Rendering
{
    /// <summary>
    /// Camera, has positon, zoom, and rotation
    /// GetTransformation() Matrix must be
    /// passed into the spritebatch for this to have any effect
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtCamera : gxtICamera
    {
        #region Fields
        protected Matrix transform;
        protected Vector2 position;
        protected Vector2 halfWidthScreenExtents;
        protected bool dirty;
        protected float screenScale;
        protected float rotation;
        protected float zoom;

        /// <summary>
        /// Screen Scale, reccomended that you use the SetupScreenScale function.  Set manually at your own risk.
        /// </summary>
        public float ScreenScale { get { return screenScale; } set { gxtDebug.Assert(value >= float.Epsilon, "Must have a positive screen scale"); screenScale = value; } }

        /// <summary>
        /// Window Width
        /// </summary>
        protected int WindowWidth { get { return (int)halfWidthScreenExtents.X * 2; } private set { halfWidthScreenExtents = new Vector2(value / 2.0f, halfWidthScreenExtents.Y); } }

        /// <summary>
        /// Window Height
        /// </summary>
        protected int WindowHeight { get { return (int)halfWidthScreenExtents.Y * 2; } private set { halfWidthScreenExtents = new Vector2(halfWidthScreenExtents.X, value / 2.0f); } }

        /// <summary>
        /// Position
        /// </summary>
        public Vector2 Position { get { return position; } set { position = value; dirty = true; } }
        
        /// <summary>
        /// Rotation
        /// </summary>
        public float Rotation { get { return rotation; } set { rotation = value; dirty = true; } }

        /// <summary>
        /// The minimum allowed zoom level.  Negative values will distort the 
        /// scaling of the xform matrix.  There are no restrictions for the max level 
        /// of zoom.
        /// </summary>
        public const float MIN_CAMERA_SCALE = 0.05f;

        /// <summary>
        /// Zoom, cannot be set below MIN_CAMERA_SCALE
        /// Negative values cause issues with 
        /// transform matrix
        /// </summary>
        public float Zoom
        {
            get { return zoom; }
            set
            {
                if (screenScale + value < MIN_CAMERA_SCALE)
                {
                    zoom = -screenScale + MIN_CAMERA_SCALE;
                }
                else
                {
                    zoom = value;
                }
                dirty = true;
            }
        }
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Default constructor, does not register the camera with the display manager for changes in resolution
        /// </summary>
        public gxtCamera()
        {
            if (gxtDisplayManager.SingletonIsInitialized)
            {
                halfWidthScreenExtents = new Vector2(gxtDisplayManager.Singleton.ResolutionWidth * 0.5f, gxtDisplayManager.Singleton.ResolutionHeight * 0.5f);
                screenScale = gxtDisplayManager.Singleton.ResolutionWidth / (float)gxtDisplayManager.Singleton.TargetResolutionWidth;
            }
            else
            {
                halfWidthScreenExtents = new Vector2(gxtDisplayManager.DEFAULT_RESOLUTION_WIDTH * 0.5f, gxtDisplayManager.DEFAULT_RESOLUTION_HEIGHT * 0.5f);
                screenScale = 1.0f;
            }
            position = Vector2.Zero;
            Zoom = 0.0f;
            rotation = 0.0f;
            dirty = true;
        }

        /// <summary>
        /// Takes position, zoom, and rotation, but does not register the camera with the display manager
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="zoom">Zoom</param>
        /// <param name="registerWithDisplayManager"></param>
        public gxtCamera(Vector2 position, float rotation = 0.0f, float zoom = 0.0f, bool registerWithDisplayManager = false)
        {
            if (gxtDisplayManager.SingletonIsInitialized)
            {
                halfWidthScreenExtents = new Vector2(gxtDisplayManager.Singleton.ResolutionWidth * 0.5f, gxtDisplayManager.Singleton.ResolutionHeight * 0.5f);
                screenScale = gxtDisplayManager.Singleton.ResolutionWidth / (float)gxtDisplayManager.Singleton.TargetResolutionWidth;

                if (registerWithDisplayManager)
                    gxtDisplayManager.Singleton.RegisterCamera(this);
            }
            else
            {
                halfWidthScreenExtents = new Vector2(gxtDisplayManager.DEFAULT_RESOLUTION_WIDTH * 0.5f, gxtDisplayManager.DEFAULT_RESOLUTION_HEIGHT * 0.5f);
                screenScale = 1.0f;
                if (registerWithDisplayManager)
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cannot register the camera with display manager!  It hasn't been initialized yet!");
            }
            this.position = position;
            Zoom = zoom;
            this.rotation = rotation;
            dirty = true;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Moves the camera by a given amount in world space
        /// </summary>
        /// <param name="t">Translation Vector</param>
        public void Translate(Vector2 t)
        {
            Position += t;
        }

        /// <summary>
        /// Translate overload
        /// </summary>
        /// <param name="x">tx</param>
        /// <param name="y">ty</param>
        public void Translate(float x, float y)
        {
            Translate(new Vector2(x, y));
        }

        /// <summary>
        /// Local translation overload
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TranslateLocal(float x, float y)
        {
            TranslateLocal(new Vector2(x, y));
        }

        /// <summary>
        /// Translates the camera by the given amount relative to its local orientation
        /// </summary>
        /// <param name="t"></param>
        public void TranslateLocal(Vector2 t)
        {
            GetTransform();
            Matrix invTransform = Matrix.Invert(transform);
            Vector2 tWorld = Vector2.TransformNormal(t, invTransform);
            Position += tWorld;
        }

        /// <summary>
        /// Sets all the properties of the camera
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="zoom">Zoom</param>
        public void Set(Vector2 position, float rotation, float zoom)
        {
            Position = position;
            Rotation = rotation;
            Zoom = zoom;
        }

        /// <summary>
        /// Gets the AABB of the camera
        /// </summary>
        /// <returns></returns>
        public virtual gxtAABB GetViewAABB()
        {
            Vector2 r = halfWidthScreenExtents;
            float s = 1.0f / (screenScale + zoom);
            r *= s;
            gxtAABB scaledAABB = new gxtAABB(position, r);

            scaledAABB = scaledAABB.GetRotatedAABB(rotation);
            return scaledAABB;
        }

        /// <summary>
        /// Gets the OBB of the camera
        /// </summary>
        /// <returns></returns>
        public virtual gxtOBB GetViewOBB()
        {
            // the inverse of a rotation is just the negative rotation matrix
            Matrix xform = Matrix.CreateRotationZ(rotation);
            Matrix invTransform = Matrix.Invert(xform);
            float s = 1.0f / (screenScale + zoom);
            Vector2 r = halfWidthScreenExtents * s;
            Vector2 xAxis = Vector2.TransformNormal(Vector2.UnitX, invTransform);
            Vector2 yAxis = Vector2.TransformNormal(Vector2.UnitY, invTransform);
            return new gxtOBB(Position, r, xAxis, yAxis);
        }

        /// <summary>
        /// Gets the world space of a point relative to the top left corner of the 
        /// screen.  Pass in gxtMouse.GetPosition() to compare mouse position to other objects 
        /// in the scene.
        /// </summary>
        /// <param name="screenPos">Screen Position (relative to top left corner)</param>
        /// <returns>Position in World Space</returns>
        public virtual Vector2 GetVirtualMousePosition(Vector2 screenPos)
        {
            // update the transform and use the inverse to do the reverse
            // screen coordinates -> world coordinates
            GetTransform();
            Matrix invTransform = Matrix.Invert(transform);
            return Vector2.Transform(screenPos, invTransform);
        }

        /// <summary>
        /// Determines if an obb object is intersecting the camera obb
        /// </summary>
        /// <param name="obb"></param>
        /// <returns></returns>
        public bool IsOnScreen(gxtOBB obb)
        {
            return GetViewOBB().Intersects(obb);
        }

        /// <summary>
        /// Determines if an obb object is not intersecting the camera obb
        /// </summary>
        /// <param name="obb"></param>
        /// <returns></returns>
        public bool IsOffScreen(gxtOBB obb)
        {
            return !GetViewOBB().Intersects(obb);
        }

        /// <summary>
        /// Determines if a point is on the screen
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsOnScreen(Vector2 pt)
        {
            return GetViewOBB().Contains(pt);
        }

        /// <summary>
        /// Determines if a point is off the screen
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsOffScreen(Vector2 pt)
        {
            return !GetViewOBB().Contains(pt);
        }

        /// <summary>
        /// Determines if an object's obb is fully contained inside the 
        /// camera obb
        /// </summary>
        /// <param name="obb"></param>
        /// <returns></returns>
        public bool IsFullyOnScreen(gxtOBB obb)
        {
            return GetViewOBB().Contains(obb);
        }

        /// <summary>
        /// Clamps the camera to fit inside the specified boundaries
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="right">Right</param>
        /// <param name="bottom">Bottom</param>
        public void ClampBounds(float left, float top, float right, float bottom)
        {
            gxtAABB camAABB = GetViewAABB();
            float x = Position.X, y = Position.Y;
            if (camAABB.Left < left)
                x = left + camAABB.Extents.X;
            else if (camAABB.Right > right)
                x = right - camAABB.Extents.X;

            if (camAABB.Top < top)
                y = top + camAABB.Extents.Y;
            else if (camAABB.Bottom > bottom)
                y = bottom - camAABB.Extents.Y;

            Position = new Vector2(x, y);
        }


        /// <summary>
        /// Clamps the camera to fit inside the specified boundaries, with a padded margin
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="right">Right</param>
        /// <param name="bottom">Bottom</param>
        public void ClampBounds(float left, float top, float right, float bottom, float margin)
        {
            ClampBounds(left + margin, top + margin, right - margin, bottom - margin);
        }

        /// <summary>
        /// Handles scaling when the resolution is changed.  If handled properly, a change in 
        /// resolution should give the same view but with differences in the aspect ratio.
        /// </summary>
        /// <param name="manager">Display Manager</param>
        public void ResolutionChangedHandler(gxtDisplayManager manager)
        {
            SetupScreenScale(manager.ResolutionWidth, manager.ResolutionHeight, manager.TargetResolutionWidth, manager.TargetResolutionHeight);
        }

        /// <summary>
        /// Sets the screen scale given the actual and target values
        /// </summary>
        /// <param name="resolutionWidth">Width</param>
        /// <param name="resolutionHeight">Height</param>
        /// <param name="targetResolutionWidth">Target Width</param>
        /// <param name="targetResolutionHeight">Target Height</param>
        public void SetupScreenScale(int resolutionWidth, int resolutionHeight, int targetResolutionWidth, int targetResolutionHeight)
        {
            if (WindowWidth != resolutionWidth)
            {
                WindowWidth = resolutionWidth;
                WindowHeight = resolutionHeight;
                screenScale = resolutionWidth / (float)targetResolutionWidth;
                dirty = true;
            }
        }

        /// <summary>
        /// Gets View Matrix based on position, zoom, and rotation
        /// In this implementation the position is the center of the screen
        /// </summary>
        /// <returns>Transform Matrix</returns>
        public virtual Matrix GetTransform()
        {
            if (dirty)
            {
                transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0.0f)) *
                                Matrix.CreateScale(new Vector3(screenScale + zoom, screenScale + zoom, 1.0f)) *
                                Matrix.CreateRotationZ(rotation) *
                                Matrix.CreateTranslation(new Vector3(halfWidthScreenExtents.X, halfWidthScreenExtents.Y, 0.0f));
                dirty = false;
            }
            return transform;
        }
        #endregion Methods
    }
}
