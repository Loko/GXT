using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT.Physics
{
    public class gxtContactPair : IComparable<gxtContactPair>
    {
        private bool enabled;
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        private gxtGeom geomA, geomB;
        private Vector2 r1, r2;
        private float combinedFriction, combinedRestitution;

        private gxtSpecialContact contactA, contactB;
        public gxtSpecialContact ContactA { get { return contactA; } }
        public gxtSpecialContact ContactB { get { return contactB; } }

        public void SetupContact(gxtGeom ga, gxtGeom gb, ref gxtCollisionResult collisionResult, gxtFrictionType frictionType = gxtFrictionType.AVERAGE, bool enabledContact = true)
        {
            geomA = ga;
            geomB = gb;

            if (contactA == null)
                contactA = new gxtSpecialContact();
            if (contactB == null)
                contactB = new gxtSpecialContact();

            // setup the individual contacts
            contactA.SetupContact(ref collisionResult, true);
            contactB.SetupContact(ref collisionResult, true);

            // calculate shared values

            // friction mixing can be the average, sqrt product, or the minimum
            // recalculate just to be safe...their values could change while they are in contact
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
        /// Pre step both contact points
        /// </summary>
        /// <param name="invDt"></param>
        public void PreStepImpulse(float invDt)
        {
            PreStepContact(invDt, contactA);
            PreStepContact(invDt, contactB);
        }

        private void PreStepContact(float invDt, gxtSpecialContact contact)
        {
            // Do we do this for each contact point?  cpA and cpB?
            Vector2 crA = contact.Position - geomA.Position;
            Vector2 crB = contact.Position - geomB.Position;

            float crnA = Vector2.Dot(crA, contact.Normal);
            float crnB = Vector2.Dot(crB, contact.Normal);

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
            contact.MassNormal = 1.0f / kNormal;   // change mass normal

            // mass tangent
            //float f1 = 1.0f;
            Vector2 tangent = gxtMath.Cross2D(contact.Normal, 1.0f);
            float rt1 = Vector2.Dot(crA, tangent);
            float rt2 = Vector2.Dot(crB, tangent);

            float kTangent = invMassSum;
            kTangent += iiA * (cra2 - rt1 * rt1) + iiB * (crb2 - rt2 * rt2);
            contact.MassTangent = 1.0f / kTangent;

            /* Not sure these allowed tolerances and bias factors are appropriate */
            #if GXT_CONTACT_BIAS
            float minVelBias = 0.00005f + this.depth;
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
            float dvn = Vector2.Dot(dv, contact.Normal);
            contact.BounceVelocity = dvn * this.combinedRestitution;

            Vector2 ja = contact.Normal * contact.NormalImpulse;
            Vector2 jb = tangent * contact.TangentImpulse;
            Vector2 impulse = ja + jb;

            // apply first impulse
            if (geomBHasBody)
                geomB.RigidBody.ApplyImpulse(impulse, contact.Position);
            if (geomAHasBody)
                geomA.RigidBody.ApplyImpulse(-impulse, contact.Position);
        }

        public void ApplyImpulses()
        {
            ApplyContactImpulse(contactA);
            ApplyContactImpulse(contactB);
        }

        private void ApplyContactImpulse(gxtSpecialContact contact)
        {
            // In testing it appears we don't need to do this processing for each contact seperately
            this.r1 = contact.Position - geomA.Position;
            this.r2 = contact.Position - geomB.Position;

            bool geomAHasBody = geomA.HasAttachedBody(), geomBHasBody = geomB.HasAttachedBody();
            Vector2 worldVelocityA = Vector2.Zero, worldVelocityB = Vector2.Zero;

            if (geomAHasBody)
                worldVelocityA = geomA.RigidBody.GetVelocityAtWorldOffset(this.r1);
            if (geomBHasBody)
                worldVelocityB = geomB.RigidBody.GetVelocityAtWorldOffset(this.r2);
            Vector2 dv = worldVelocityB - worldVelocityA;
            float dvn = Vector2.Dot(dv, contact.Normal);

            float normImpulse = contact.MassNormal * -(dvn + contact.BounceVelocity);
            float oldNormImpulse = contact.NormalImpulse;
            contact.NormalImpulse = oldNormImpulse + normImpulse;
            if (contact.NormalImpulse < 0.0f)
                contact.NormalImpulse = 0.0f;
            normImpulse = contact.NormalImpulse - oldNormImpulse;

            // really multiplying by the change in impulse
            // but this is what is applied on this iteration
            Vector2 impulse = contact.Normal * normImpulse;

            if (geomBHasBody)
                geomB.RigidBody.ApplyImpulse(impulse, contact.Position);
            if (geomAHasBody)
                geomA.RigidBody.ApplyImpulse(-impulse, contact.Position);

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

            float maxTangentImpulse = combinedFriction * contact.NormalImpulse;
            Vector2 tangent = gxtMath.Cross2D(contact.Normal, 1.0f);
            float vt = Vector2.Dot(dv, tangent);
            float tanImpulse = contact.MassTangent * (-vt);
            float tanImpulseOld = contact.TangentImpulse;
            contact.TangentImpulse = gxtMath.Clamp(tanImpulseOld + tanImpulse, -maxTangentImpulse, maxTangentImpulse);
            tanImpulse = contact.TangentImpulse - tanImpulseOld;

            impulse = tanImpulse * gxtMath.Cross2D(contact.Normal, 1.0f);

            if (geomBHasBody)
                geomB.RigidBody.ApplyImpulse(impulse, contact.Position);
            if (geomAHasBody)
                geomA.RigidBody.ApplyImpulse(-impulse, contact.Position);
        }

        // the depths are the same for each contact
        public int CompareTo(gxtContactPair other)
        {
            return other.contactA.Depth.CompareTo(contactA.Depth);
        }
    }
}
