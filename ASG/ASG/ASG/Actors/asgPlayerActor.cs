using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GXT;
using GXT.Rendering;
using GXT.Physics;
using GXT.Processes;

namespace ASG.Actors
{
    // delegates will inevitably go here

    public class asgPlayerActor
    {
        /* gxtIActor Members */
        protected bool enabled;
        protected gxtWorld world;
        protected Vector2 position;
        protected float rotation;
        protected gxtHashedString hashedString;

        public bool Enabled { get { return enabled; } set { enabled = value; } }
        public gxtWorld World { get { return world; } }
        public Vector2 Position { get { return position; } }
        public float Rotation { get { return rotation; } }
        public gxtHashedString Type { get { return hashedString; } }

        /* Custom members */

        // rendering
        gxtIEntity playerEntity;
        gxtIDrawable playerDrawable;
        gxtISceneNode playerSceneNode;

        // geometry
        gxtPolygon playerPolygon;
        gxtPolygon playerScaledPolygon;

        // bodies
        gxtRigidBody playerBody;

        // timers
        gxtStopWatch rayCastTimer;

        public void Initialize(Vector2 initPos, float initRot = 0.0f)
        {
            hashedString = new gxtHashedString("player_actor");
            this.position = initPos;
            this.rotation = gxtMath.WrapAngle(initRot);

            // in physics world units
            playerPolygon = gxtGeometry.CreateRectanglePolygon(2, 5.5f);

        }

        public void SetWorldPosition(Vector2 position)
        {

        }

        public void SetWorldRotation(float rotation)
        {

        }

        public void Translate(Vector2 t)
        {
            playerBody.Translate(t);
        }

        public gxtAABB GetAABB()
        {
            return gxtAABB.ZERO_EXTENTS_AABB;
        }

        public gxtOBB GetOBB()
        {
            return new gxtOBB();
        }

        public void Update(GameTime gameTime)
        {
            if (!enabled)
                return;
            position = playerBody.Position;
            rotation = playerBody.Rotation;
            // state based logic here only!
        }

        public void Unload()
        {
            // remove node and drawable
            playerSceneNode.DetachDrawable(playerDrawable);
            playerDrawable.Dispose();
            playerEntity.Dispose();
            world.RemoveSceneNode(playerSceneNode);

            // remove rigid bodies

            // remove geoms
        }
    }
}
