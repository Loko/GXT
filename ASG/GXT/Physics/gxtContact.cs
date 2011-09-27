#define GXT_CONTACT_BIAS
#define GXT_APPLY_IMPULSES_BOTH_CONTACT_POINTS
#undef GXT_CONTACT_BIAS
#undef GXT_APPLY_IMPULSES_BOTH_CONTACT_POINTS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT.Physics
{
    public enum gxtFrictionType
    {
        AVERAGE = 0,
        AVERAGE_SQRT = 1,
        MINIMUM = 2
    };

    public class gxtContact : IComparable<gxtContact>
    {
        private bool enabled;
        private Vector2 normal;
        private float depth;
        private Vector2 contactPointA;
        private Vector2 contactPointB;
        private gxtGeom geomA, geomB;
        private Vector2 r1, r2;

        public bool Enabled { get { return enabled; } set { enabled = value; } }
        public Vector2 ContactPtA { get { return contactPointA; } }
        public Vector2 ContactPtB { get { return contactPointB; } }
        public Vector2 Normal { get { return normal; } }
        public float Depth { get { return depth; } }

        private float combinedFriction, combinedRestitution;
        private float massNormal, massTangent;
        
        #if GXT_CONTACT_BIAS
        private float normalVelocityBias, normalImpulseBias;
        #endif
        
        private float normalImpulse, tangentImpulse;
        private float bounceVelocity;

        public void SetupContact(gxtGeom ga, gxtGeom gb, ref gxtCollisionResult collisionResult, gxtFrictionType frictionType = gxtFrictionType.AVERAGE, bool enabledContact = true)
        {
            geomA = ga;
            geomB = gb;
            normal = collisionResult.Normal;
            depth = collisionResult.Depth;
            contactPointA = collisionResult.ContactPointA;
            contactPointB = collisionResult.ContactPointB;

            // friction mixing can be the average or the minimum
            if (frictionType == gxtFrictionType.AVERAGE)
                combinedFriction = (ga.GetFriction() + gb.GetFriction()) * 0.5f;
            else if (frictionType == gxtFrictionType.AVERAGE_SQRT)
                combinedFriction = gxtMath.Sqrt(ga.GetFriction() * gb.GetFriction());
            else
                combinedFriction = gxtMath.Min(ga.GetFriction(), gb.GetFriction());

            this.combinedRestitution = (ga.GetRestitution() + gb.GetRestitution()) * 0.5f;

            // calling function will pass in false for enabledContact if either has response disabled
            if (enabledContact)
                this.enabled = ShouldProcess(ga, gb);
            else
                this.enabled = false;
        }

        /// <summary>
        /// Determines if the contact should be enabled for solving based 
        /// on internal values
        /// </summary>
        /// <param name="ga"></param>
        /// <param name="gb"></param>
        /// <returns></returns>
        private bool ShouldProcess(gxtGeom ga, gxtGeom gb)
        {
            // ga is geom only case
            if (!ga.HasAttachedBody())
            {
                if (!gb.HasAttachedBody())
                    return false;
                return gb.RigidBody.IsDynamic;
            }

            // gb is geom only case
            if (!gb.HasAttachedBody())
            {
                if (!ga.HasAttachedBody())
                    return false;
                return ga.RigidBody.IsDynamic;
            }

            return ga.RigidBody.IsDynamic || gb.RigidBody.IsDynamic;
        }

        /// <summary>
        /// Note: will not perform correct response for geoms with no attached body
        /// </summary>
        /// <param name="invDt"></param>
        public void PreStepImpulse(float invDt)
        {
            // Do we do this for each contact point?  cpA and cpB?
            Vector2 crA = contactPointA - geomA.Position;
            Vector2 crB = contactPointB - geomB.Position;

            float crnA = Vector2.Dot(crA, this.normal);
            float crnB = Vector2.Dot(crB, this.normal);

            // if no attached rigid body, then we have to use zero for the inverseMass and inertia
            // inverse masses
            float imA = 0.0f, imB = 0.0f;
            // inverse inertias
            float iiA = 0.0f, iiB = 0.0f;

            bool geomAHasBody = false, geomBHasBody = false;

            if (geomA.HasAttachedBody())
            {
                imA = geomA.RigidBody.InverseMass;
                iiA = geomA.RigidBody.InverseInertia;
                geomAHasBody = true;
            }
            if (geomB.HasAttachedBody())
            {
                imB = geomB.RigidBody.InverseMass;
                iiB = geomB.RigidBody.InverseInertia;
                geomBHasBody = true;
            }

            float invMassSum = imA + imB;
            float cra2 = crA.LengthSquared();
            float crb2 = crB.LengthSquared();

            float kNormal = invMassSum + iiA * (cra2 - crnA * crnA) + iiB * (crb2 - crnB * crnB);
            this.massNormal = 1.0f / kNormal;   // change mass normal

            // mass tangent
            //float f1 = 1.0f;
            Vector2 tangent = gxtMath.Cross2D(this.normal, 1.0f);
            float rt1 = Vector2.Dot(crA, tangent);
            float rt2 = Vector2.Dot(crB, tangent);

            float kTangent = invMassSum;
            kTangent += iiA * (cra2 - rt1 * rt1) + iiB * (crb2 - rt2 * rt2);
            this.massTangent = 1.0f / kTangent;

            /* Not sure these allowed tolerances and bias factors are appropriate */
            #if GXT_CONTACT_BIAS
            float minVelBias = 0.005f + this.depth;
            if (0.0f < minVelBias)
                minVelBias = 0.0f;
            float biasFactor = 0.95f;  // make a constant
            this.normalVelocityBias = -biasFactor * invDt * minVelBias;
            #endif

            // moved to setup function
            //this.combinedRestitution = (geomA.GetRestitution() + geomB.GetRestitution()) * 0.5f;

            // calc bounce velocity
            Vector2 worldVelocityA = Vector2.Zero, worldVelocityB = Vector2.Zero;
            if (geomAHasBody)
                worldVelocityA = geomA.RigidBody.GetVelocityAtWorldOffset(crA);
            if (geomBHasBody)
                worldVelocityB = geomB.RigidBody.GetVelocityAtWorldOffset(crB);
            Vector2 dv = worldVelocityB - worldVelocityA;
            float dvn = Vector2.Dot(dv, this.normal);
            this.bounceVelocity = dvn * this.combinedRestitution;

            Vector2 ja = this.normal * this.normalImpulse;
            Vector2 jb = tangent * this.tangentImpulse;
            Vector2 impulse = ja + jb;

            // apply first impulse
            if (geomBHasBody)
                geomB.RigidBody.ApplyImpulse(impulse, contactPointB);
            if (geomAHasBody)
                geomA.RigidBody.ApplyImpulse(-impulse, contactPointA);

            //this.normalImpulseBias = 0.0f;
        }

        public void ApplyImpulse()
        {
            // In testing it appears we don't need to do this processing for each contact seperately
            this.r1 = contactPointA - geomA.Position;
            this.r2 = contactPointB - geomB.Position;

            bool geomAHasBody = geomA.HasAttachedBody(), geomBHasBody = geomB.HasAttachedBody();
            Vector2 worldVelocityA = Vector2.Zero, worldVelocityB = Vector2.Zero;

            if (geomAHasBody)
                worldVelocityA = geomA.RigidBody.GetVelocityAtWorldOffset(this.r1);
            if (geomBHasBody)
                worldVelocityB = geomB.RigidBody.GetVelocityAtWorldOffset(this.r2);
            Vector2 dv = worldVelocityB - worldVelocityA;
            float dvn = Vector2.Dot(dv, this.normal);

            float normImpulse = this.massNormal * -(dvn + this.bounceVelocity);
            float oldNormImpulse = this.normalImpulse;
            this.normalImpulse = oldNormImpulse + normImpulse;
            if (this.normalImpulse < 0.0f)
                this.normalImpulse = 0.0f;
            normImpulse = this.normalImpulse - oldNormImpulse;

            // really multiplying by the change in impulse
            // but this is what is applied on this iteration
            Vector2 impulse = this.normal * normImpulse;
            
            if (geomBHasBody)
                geomB.RigidBody.ApplyImpulse(impulse, contactPointB);
            if (geomAHasBody)
                geomA.RigidBody.ApplyImpulse(-impulse, contactPointA);

            // now find the difference in velocity bias
            // this code block has not been updated to support 
            // geoms without bodies yet...
            #if GXT_CONTACT_BIAS
            Vector2 vba = geomA.RigidBody.GetVelocityBiasAtWorldOffset(this.r1);
            Vector2 vbb = geomB.RigidBody.GetVelocityBiasAtWorldOffset(this.r2);
            Vector2 dvBias = vbb - vba;

            float normVelocityBias = Vector2.Dot(dvBias, this.normal);
            float normImpulseBias = this.massNormal * (-normVelocityBias + this.normalVelocityBias);
            float normImpulseBiasOld = this.normalImpulseBias;
            this.normalImpulseBias = normImpulseBias + normImpulseBiasOld;
            if (this.normalImpulseBias < 0.0f)
                this.normalImpulseBias = 0.0f;
            normImpulseBias = this.normalImpulseBias - normImpulseBiasOld;

            // apply bias at world offset
            Vector2 impulseBias = this.normal * normImpulseBias;
            
            geomB.RigidBody.ApplyBiasImpulseAtWorldOffset(impulseBias, r2);
            geomA.RigidBody.ApplyBiasImpulseAtWorldOffset(-impulseBias, r1);
            #endif

            float maxTangentImpulse = combinedFriction * this.normalImpulse;
            Vector2 tangent = gxtMath.Cross2D(this.normal, 1.0f);
            float vt = Vector2.Dot(dv, tangent);
            float tanImpulse = this.massTangent * (-vt);
            float tanImpulseOld = this.tangentImpulse;
            this.tangentImpulse = gxtMath.Clamp(tanImpulseOld + tanImpulse, -maxTangentImpulse, maxTangentImpulse);
            tanImpulse = this.tangentImpulse - tanImpulseOld;

            impulse = tanImpulse * gxtMath.Cross2D(this.normal, 1.0f);
            
            if (geomBHasBody)
                geomB.RigidBody.ApplyImpulse(impulse, contactPointB);
            if (geomAHasBody)
                geomA.RigidBody.ApplyImpulse(-impulse, contactPointA);
        }

        // we want to handle greatest depths first
        // hence why this compare to is backwards
        public int CompareTo(gxtContact c)
        {
            return c.depth.CompareTo(depth);
        }
    }
}
