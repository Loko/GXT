#define GXT_RIGID_BODY_SLEEPING_ENABLED
//#undef GXT_RIGID_BODY_SLEEPING_ENABLED

using System;
using Microsoft.Xna.Framework;

namespace GXT.Physics
{
    /// <summary>
    /// Motion type which defines how the rigid body can be controlled 
    /// and updated
    /// </summary>
    public enum gxtRigidyBodyMotion
    {
        /// <summary>
        /// No velocity, acceleration, force, or torque.  However, may be 
        /// manually positioned and rotated.  Use this for things that are static 
        /// and/or animated.  Only use SetPosition() and SetRotation() calls on 
        /// fixed bodies.
        /// </summary>
        FIXED = 0,
        /// <summary>
        /// Considers linear and angular kinematics.  Thus velocity and acceleration, are considered,
        /// but nothing concerning mass, forces, or torque.
        /// </summary>
        KINEMATIC = 1,
        /// <summary>
        /// Considers everything, allowing for mass based force and torque dynamics in addition to 
        /// kinematics
        /// </summary>
        DYNAMIC = 2
    };


    /// <summary>
    /// A class which represents a rigid body in the physics world, absent of a geometrical 
    /// representation.  Instances of gxtRigidBody are meant to be added to the gxtPhysicsWorld 
    /// to take part in the simulation.
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtRigidBody
    {
        // should change
        private static float bodySleepEpsilon = 0.25f;

        /// <summary>
        /// Motion threshold at which all bodies can be put to sleep
        /// </summary>
        public static float SleepEpsilon { get { return bodySleepEpsilon; } set { bodySleepEpsilon = value; } }

        /// <summary>
        /// Calculates the rotational inertia for a rectangle with a given 
        /// width, height, and mass
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mass"></param>
        /// <returns></returns>
        public static float GetInertiaForRectangle(float width, float height, float mass)
        {
            return (mass * (height * height + width * width)) / 12.0f;
        }

        /// <summary>
        /// Calculates the rotational inertia for a circle with a given radius and mass
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="mass"></param>
        /// <returns></returns>
        public static float GetInertiaForCircle(float radius, float mass)
        {
            return 0.5f * mass * gxtMath.Pow(radius, 2);
        }

        /// <summary>
        /// Calculates the rotational inertia for a polygon with a given mass
        /// Passed in polygon does not need to be in local space
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="mass"></param>
        /// <returns></returns>
        public static float GetInertiaForPolygon(gxtPolygon polygon, float mass)
        {
            gxtDebug.Assert(polygon.NumVertices >= 3);
            
            // perform a deep copy so we don't corrupt the values of the polygon
            Vector2[] vertices = new Vector2[polygon.NumVertices];
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = polygon.v[i];
            }

            Vector2 centroid = polygon.GetCentroid();
            if (centroid != Vector2.Zero)
            {
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] -= centroid;
                }
                    //polygon.Translate(-centroid);
            }
            
            float denom = 0.0f;
            float numer = 0.0f;
            for (int j = vertices.Length - 1, i = 0; i < vertices.Length; j = i, i++)
            {
                float a = vertices[i].LengthSquared();
                float b = Vector2.Dot(vertices[i], vertices[j]);
                float c = vertices[j].LengthSquared();
                float d = gxtMath.Abs(gxtMath.Cross2D(vertices[j], vertices[i]));

                numer += d;
                denom += (a + b + c) * d;
            }
            return denom / (numer * 6.0f);
        }
        
        #if (GXT_RIGID_BODY_SLEEPING_ENABLED)
        /// <summary>
        /// Accumulated motion of the rigid body
        /// Used internally for sleep management
        /// </summary>
        private float motion;
        #endif


        private bool canSleep;
        private bool awake;

        private gxtRigidyBodyMotion motionType;
        private bool ignoreGravity;
        private bool fixedRotation;

        // inverse results in faster computations
        private float mass;
        private float invMass;
        private float rotationalInertia;
        private float invRotationalInertia;
        private float damping, angularDamping;

        private Vector2 position;
        private float rotation;

        private Vector2 acceleration;
        private Vector2 prevAcceleration;
        private Vector2 velocity;
        private float angularAccceleration;
        private float angularVelocity;
        private Vector2 force;
        private float torque;

        /// <summary>
        /// Determines if this rigid body can be put to sleep
        /// Should be set to false for important objects that 
        /// require constant attention (e.g. player characters)
        /// </summary>
        public bool CanSleep { get { return canSleep; } set { canSleep = value; } }

        /// <summary>
        /// Determines if the rigid body is currently awake.  If false 
        /// the rigid body is asleep, and will not bother processing 
        /// certain any dynamics dynamics
        /// </summary>
        public bool Awake { get { return awake; } set {
            if (canSleep && value)
                awake = true;
                #if (GXT_RIGID_BODY_SLEEPING_ENABLED)
                motion = bodySleepEpsilon * 2.0f;
                #endif
            }
        }


        /// <summary>
        /// MotionType of the rigid body
        /// The motion type determines if the body is 
        /// fixed in the world, has manually kinematic movement, 
        /// or is subject to forces/torques in the simulation
        /// </summary>
        public gxtRigidyBodyMotion MotionType { get { return motionType; } 
            set 
            {
                // no processing if the existing type
                if (motionType == value)
                    return;
                motionType = value;
                ClearKinematics();
                ClearForceAndTorque();
                Awake = true;
                if (motionType == gxtRigidyBodyMotion.KINEMATIC || motionType == gxtRigidyBodyMotion.FIXED)
                {
                    mass = 0.0f;
                    invMass = 0.0f;
                }
                else
                {
                    mass = 1.0f;
                    invMass = 1.0f;

                }
                rotationalInertia = 0.0f;
                invRotationalInertia = 0.0f;
            }
        }

        /// <summary>
        /// Is the motion type fixed?
        /// </summary>
        public bool IsFixed { get { return motionType == gxtRigidyBodyMotion.FIXED; } }

        /// <summary>
        /// Is the motion type kinematic?
        /// </summary>
        public bool IsKinematic { get { return motionType == gxtRigidyBodyMotion.KINEMATIC; } }

        /// <summary>
        /// Is the motion type dynamic?
        /// </summary>
        public bool IsDynamic { get { return motionType == gxtRigidyBodyMotion.DYNAMIC; } }

        /// <summary>
        /// Determines if this rigid body is affected by gravity
        /// </summary>
        public bool IgnoreGravity { get { return ignoreGravity; } set { ignoreGravity = value; } }

        /// <summary>
        /// Position of the body in world coordinates
        /// </summary>
        public Vector2 Position { get { return position; } set { Awake = true; position = value; } }

        /// <summary>
        /// Rotation of the body (in radians)
        /// </summary>
        public float Rotation { get { return rotation; } set { Awake = true; rotation = value; } }

        /// <summary>
        /// Prevents rotations, regardless of forces, and torque applied
        /// Rotations can still be manually specified by directly adjusting the Rotation property
        /// </summary>
        public bool FixedRotation { get { return fixedRotation; } 
            set 
            { 
                fixedRotation = value;
                if (value)
                {
                    invRotationalInertia = 0.0f;
                    rotationalInertia = 0.0f;
                }
            } 
        }

        /// <summary>
        /// Current linear acceleration of the rigid body
        /// </summary>
        public Vector2 Acceleration { get { return acceleration; } set { Awake = true; acceleration = value; } }

        /// <summary>
        /// Previous linear acceleration of the rigid body
        /// </summary>
        public Vector2 PrevAcceleration { get { return prevAcceleration; } }

        /// <summary>
        /// Current linear velocity of the rigid body 
        /// </summary>
        public Vector2 Velocity { get { return velocity; } set { Awake = true; velocity = value; } }
        
        /// <summary>
        /// Current angular acceleration of the rigid body
        /// </summary>
        public float AngularAcceleration { get { return angularAccceleration; } set { Awake = true; angularAccceleration = value; } }

        /// <summary>
        /// Current angular acceleration of the rigid body
        /// </summary>
        public float AngularVelocity { get { return angularVelocity; } set { Awake = true; angularVelocity = value; } }

        /// <summary>
        /// Total accumulation of forces acting on the rigid body
        /// </summary>
        public Vector2 Force { get { return force; } set { Awake = true; force = value; } }
        
        /// <summary>
        /// Total accumulation of torque acting on the body
        /// </summary>
        public float Torque { get { return torque; } set { Awake = true; torque = value; } }

        /// <summary>
        /// Mass of the rigid body
        /// Must be non-negative
        /// </summary>
        public float Mass { get { return mass; } 
            set 
            {
                if (motionType == gxtRigidyBodyMotion.DYNAMIC)
                {
                    gxtDebug.Assert(mass >= 0.0f, "Rigid Body cannot have negative mass");
                    mass = value;
                    if (value != 0.0f)
                        invMass = 1.0f / mass;
                    else
                        invMass = 0.0f;
                }
            } 
        }

        /// <summary>
        /// Inverse mass of the rigid body
        /// </summary>
        public float InverseMass { get { return invMass; } }

        /// <summary>
        /// The rotational inertia of the object
        /// </summary>
        public float Inertia { get { return rotationalInertia; } 
            set 
            {
                if (motionType == gxtRigidyBodyMotion.DYNAMIC && !fixedRotation)
                {
                    rotationalInertia = value;
                    invRotationalInertia = 1.0f / rotationalInertia;
                }
            } 
        }

        /// <summary>
        /// Inverse of rotational inertia
        /// </summary>
        public float InverseInertia { get { return invRotationalInertia; } }

        /// <summary>
        /// Linear Damping
        /// </summary>
        public float Damping { get { return damping; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f); damping = value; } }
        
        /// <summary>
        /// Angular Damping
        /// </summary>
        public float AngularDamping { get { return angularDamping; } set { gxtDebug.Assert(value >= 0.0f && value <= 1.0f); angularDamping = value; } }

        /// <summary>
        /// Bodies have no knowledge of geoms they might be attached to
        /// As such, you must set your own Inertia in the calling function
        /// Extra care is needed for compound bodies
        /// </summary>
        public gxtRigidBody()
        {
            canSleep = true;
            Awake = true;
            MotionType = gxtRigidyBodyMotion.DYNAMIC;
            Mass = 1.0f;
            rotationalInertia = 0.0f;
            invRotationalInertia = 0.0f;
            Damping = 1.0f;
            AngularDamping = 1.0f;
        }

        /// <summary>
        /// Performs calculations to determine final position and orientation 
        /// of the rigid body
        /// </summary>
        /// <param name="dt">Delta Time</param>
        public void Update(float dt)
        {
            // do not bother processing updates if the body is static or asleep
            if (!awake || motionType == gxtRigidyBodyMotion.FIXED) return;

            // update more strictly controlled motion of kinematic bodies
            if (motionType == gxtRigidyBodyMotion.KINEMATIC)
            {
                velocity += acceleration * dt;
                angularVelocity += angularAccceleration * dt;

                position += velocity * dt;
                rotation += angularVelocity * dt;
            }
            // dynamic (force and torque based) dynamics calculations
            else
            {
                gxtDebug.Assert(motionType == gxtRigidyBodyMotion.DYNAMIC);

                // update linear accel and linear velocity
                prevAcceleration = acceleration;
                acceleration = invMass * force;
                velocity += acceleration * dt;
                
                // update angular accel and angular velocity
                angularAccceleration = invRotationalInertia * torque;
                angularVelocity += angularAccceleration * dt;

                ClearForceAndTorque();
            }
        }

        /// <summary>
        /// Resolve final position and orientation given the velocities determined 
        /// in the body's update
        /// </summary>
        /// <param name="dt">Delta Time</param>
        public void Integrate(float dt)
        {
            if (!awake || motionType != gxtRigidyBodyMotion.DYNAMIC) return;

            angularVelocity *= gxtMath.Pow(angularDamping, dt);
            velocity *= gxtMath.Pow(damping, dt);

            position += velocity * dt;
            rotation += angularVelocity * dt;

            #if GXT_RIGID_BODY_SLEEPING_ENABLED
            // sleep management goes last
            if (canSleep)
            {
                float curMotion = Vector2.Dot(velocity, velocity) + angularVelocity * angularVelocity;
                float bias = gxtMath.Pow(0.5f, dt);
                motion = bias * curMotion + (1.0f - bias) * curMotion;

                if (motion < bodySleepEpsilon)
                {
                    //gxtLog.WriteLineV(gxtVerbosityLevel.INFORMATIONAL, "Body should be put to sleep");
                    Awake = false;
                }
                else if (motion > 10.0f * bodySleepEpsilon)
                {
                    // prevents overflow
                    motion = 10.0f * bodySleepEpsilon;
                }
            }
            #endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(Vector2 force)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Awake = true;
                this.force += force;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(ref Vector2 force)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Awake = true;
                this.force += force;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="torque"></param>
        public void ApplyTorque(float torque)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Awake = true;
                this.torque += torque;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        /// <param name="pt">Point in World Space</param>
        public void ApplyForceAtPoint(Vector2 force, Vector2 pt)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Awake = true;

                Vector2 localPt = pt - position;
                torque += gxtMath.Cross2D(localPt, force);
                this.force += force;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        /// <param name="localPt"></param>
        public void ApplyForceAtLocalPoint(Vector2 force, Vector2 localPt)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Awake = true;

                torque += gxtMath.Cross2D(localPt, force);
                this.force += force;
            }
        }

        public void Translate(Vector2 t)
        {
            Awake = true;
            position += t;
        }

        public void Translate(float tx, float ty)
        {
            Awake = true;
            position = new Vector2(position.X + tx, position.Y + ty);
        }


        public void Rotate(float rot)
        {
            Awake = true;
            rotation += rot;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearForce()
        {
            force = Vector2.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearTorque()
        {
            torque = 0.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearForceAndTorque()
        {
            force = Vector2.Zero;
            torque = 0.0f;
        }

        public void ClearKinematics()
        {
            acceleration = Vector2.Zero;
            velocity = Vector2.Zero;
            angularAccceleration = 0.0f;
            angularVelocity = 0.0f;
        }

        public void ApplyImpulse(Vector2 impulse, Vector2 pt)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Awake = true;
                velocity += impulse * invMass;
                angularVelocity += invRotationalInertia * gxtMath.Cross2D(pt - position, impulse);
            }
        }

        public void ApplyImpulseAtLocalPoint(Vector2 impulse, Vector2 localPt)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Awake = true;
                velocity += impulse * invMass;
                angularVelocity += invRotationalInertia * gxtMath.Cross2D(localPt, impulse);
            }
        }

        /*
        public void ApplyBiasImpulseAtWorldOffset(Vector2 impulseBias, Vector2 offset)
        {
            if (motionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Vector2 dv = impulseBias * invMass;
                linearVelocityBias += dv;
                float angImpulseBias = gxtMath.Cross2D(offset, impulseBias);
                angImpulseBias *= invRotationalInertia;
                angularVelocityBias += angImpulseBias;
            }
        }
        */
        
        public Vector2 GetVelocityAtWorldOffset(Vector2 offset)
        {
            Vector2 worldVel = gxtMath.Cross2D(angularVelocity, offset);
            return worldVel + velocity;
        }

        /*
        public Vector2 GetVelocityBiasAtWorldOffset(Vector2 offset)
        {
            Vector2 bias = gxtMath.Cross2D(angularVelocityBias, offset);
            return bias + linearVelocityBias; // bodies don't store this bias value, yet

        }
        */

        public void DebugDraw(Vector2 position)
        {
            string msg = string.Format("Force: {0}\nTorque: {1}\nAccel: {2}\nVel: {3}", force.ToString(), torque.ToString(), acceleration.ToString(), velocity.ToString());
            gxtDebugDrawer.Singleton.AddString(msg, position, Color.White, 0.0f);
        }
    }
}
