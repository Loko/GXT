using System;
using Microsoft.Xna.Framework;

namespace GXT.Physics
{

    
    public class gxtSolver
    {

        private static float combinedFrictionCoefficient;
        private static float combinedRestitutionCoefficient;

        public static gxtFrictionType frictionType = gxtFrictionType.AVERAGE;

        private static float GetCombinedFriction(gxtRigidBody bodyA, gxtRigidBody bodyB)
        {
            if (frictionType == gxtFrictionType.AVERAGE)
                return (gxtMath.Sqrt(bodyA.GetFriction() * bodyB.GetFriction()));
            else
                return gxtMath.Min(bodyA.GetFriction(), bodyB.GetFriction());
        }

        private static float GetCombinedRestitution(gxtRigidBody bodyA, gxtRigidBody bodyB)
        {
            if (frictionType == gxtFrictionType.AVERAGE)
                return gxtMath.Sqrt(bodyA.GetRestitution() * bodyB.GetRestitution());
            else
                return gxtMath.Min(bodyA.GetFriction(), bodyB.GetFriction());
        }

        private static void PreStepImpulse(float invDt)
        {

        }
        /// <summary>
        /// Geoms perform no response 
        /// </summary>
        /// <param name="geomA"></param>
        /// <param name="geomB"></param>
        /// <param name="collisionResult"></param>
        /// <returns></returns>
        private static bool CheckGeomResponse(ref gxtGeom geomA, ref gxtGeom geomB, ref gxtCollisionResult collisionResult)
        {
            if (geomA.HasAttachedBody() || geomB.HasAttachedBody())
                return false;
            return true;
            /*
            if (!geomA.CollisionResponseEnabled)
            {
                if (!geomB.CollisionResponseEnabled)
                {
                    return true;
                }
                else
                {
                    geomB.Translate(collisionResult.Normal * -collisionResult.Depth);
                    return true;
                }
            }
            else if (!geomB.CollisionResponseEnabled)
            {
                geomA.Translate(collisionResult.Normal * -collisionResult.Depth);
                return true;
            }
            else
            {
                // either could be translated here, thinking of doing half for each
                geomA.Translate(collisionResult.Normal * -collisionResult.Depth);
                return true;
            }
            */
        }

        private static bool CheckOneRigidOneFixedResponse(ref gxtGeom geomA, ref gxtGeom geomB, ref gxtCollisionResult collisionResult, float dt)
        {
            if (geomA.RigidBody.MotionType == gxtRigidyBodyMotion.FIXED && geomB.RigidBody.MotionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                Vector2 prevForce = geomB.RigidBody.Mass * geomB.RigidBody.PrevAcceleration;
                Vector2 reflectedForce = gxtMath.GetReflection(prevForce, collisionResult.Normal);
                // used to arbitrarily be * 0.5
                Vector2 reflectedVelocity = gxtMath.GetReflection(geomB.RigidBody.Velocity, collisionResult.Normal) * combinedFrictionCoefficient;
                
                geomB.RigidBody.ClearAllKinematics();
                // eventually add at point
                //geomB.RigidBody.AddForceAtPoint(reflectedForce, collisionResult.ContactPointB);
                geomB.RigidBody.Velocity = reflectedVelocity;
                geomB.RigidBody.Translate(collisionResult.Normal * collisionResult.Depth);
                geomB.RigidBody.Update(dt);
                geomB.Update();
                return true;
            }
            else if (geomB.RigidBody.MotionType == gxtRigidyBodyMotion.FIXED && geomA.RigidBody.MotionType == gxtRigidyBodyMotion.DYNAMIC)
            {
                // worthless, it is cleared by this point
                Vector2 prevForce = geomA.RigidBody.Mass * geomA.RigidBody.PrevAcceleration;
                Vector2 reflectedForce = gxtMath.GetReflection(prevForce, collisionResult.Normal);
                Vector2 reflectedVelocity = gxtMath.GetReflection(geomA.RigidBody.Velocity, collisionResult.Normal) * combinedFrictionCoefficient;

                geomA.RigidBody.ClearAllKinematics();
                // eventually add at point
                //geomA.RigidBody.AddForceAtPoint(reflectedVelocity, collisionResult.ContactPointA);
                geomA.RigidBody.Velocity = reflectedVelocity;
                geomA.RigidBody.Translate(collisionResult.Normal * -collisionResult.Depth);
                geomA.RigidBody.Update(dt);
                geomA.Update();
                return true;
            }
            return false;
        }

        public static bool CheckRigidResponse(ref gxtGeom geomA, ref gxtGeom geomB, ref gxtCollisionResult collisionResult, float dt)
        {
            if (!geomA.HasAttachedBody() && !geomB.HasAttachedBody())
                return false;
            if (geomA.RigidBody.MotionType == gxtRigidyBodyMotion.FIXED && geomB.RigidBody.MotionType == gxtRigidyBodyMotion.FIXED)
                return true;
            if (!geomA.CollisionResponseEnabled && !geomB.CollisionResponseEnabled)
                return true;
            
            /* Begin Pre Step */
            Vector2 crA = collisionResult.ContactPointA - geomA.RigidBody.Position;
            Vector2 crB = collisionResult.ContactPointB - geomB.RigidBody.Position;

            float crnA = Vector2.Dot(crA, collisionResult.Normal);
            float crnB = Vector2.Dot(crB, collisionResult.Normal);

            float invMassSum = geomA.RigidBody.InverseMass + geomB.RigidBody.InverseMass;
            float cra2 = crA.LengthSquared();
            float crb2 = crB.LengthSquared();

            float fNorm = invMassSum + geomA.RigidBody.InverseInertia * (cra2 - crnA * crnA) + geomB.RigidBody.InverseInertia * (crb2 - crnB * crnB);
            float massNormal = 1.0f / fNorm;

            float f1 = 1.0f;
            Vector2 tangent = gxtMath.Cross2D(collisionResult.Normal, f1);
            float rt1 = Vector2.Dot(crA, tangent);
            float rt2 = Vector2.Dot(crB, tangent);

            float ktangent = invMassSum;

            ktangent += geomA.RigidBody.InverseInertia * (cra2 - rt1 * rt1) +
                geomB.RigidBody.InverseInertia * (crb2 - rt2 * rt2);

            float massTangent = 1.0f / ktangent;
            Vector2 worldVelocityA = geomA.RigidBody.GetVelocityAtWorldOffset(collisionResult.ContactPointA);
            Vector2 worldVelocityB = geomB.RigidBody.GetVelocityAtWorldOffset(collisionResult.ContactPointB);
            Vector2 dv = worldVelocityB - worldVelocityA;

            float dvn = Vector2.Dot(dv, collisionResult.Normal);
            float bounceVelocity = dvn * combinedRestitutionCoefficient;

            float normalImpulse = 0.0f;
            float tangentImpulse = 0.0f;
            Vector2 ja = collisionResult.Normal * normalImpulse;
            Vector2 jb = tangent * tangentImpulse;
            Vector2 impulse = ja + jb;
            geomB.RigidBody.ApplyImpulse(impulse, crB);
            impulse = -impulse;
            geomA.RigidBody.ApplyImpulse(impulse, crA);
            /* End Pre Step */
            /* Begin Post Step */

            // 1st update contact point position for both bodies
            // cra - bodyA.pos
            // crb - bodyB.pos

            // get velocity from contact point
            // get difference between velocities at contact point
            // get difference along normals
            float tmpNormalImpulse = massNormal * -(dvn + bounceVelocity);
            
            normalImpulse = gxtMath.Max(tmpNormalImpulse + normalImpulse, 0.0f);
            float dni = normalImpulse - tmpNormalImpulse;

            Vector2 impulse2 = collisionResult.Normal * tmpNormalImpulse;

            geomB.RigidBody.ApplyImpulse(impulse2, collisionResult.ContactPointB);

            impulse2 = -impulse2;
            geomA.RigidBody.ApplyImpulse(impulse2, collisionResult.ContactPointA);

            // 0s are old normal impulse bias
            float normalVelocityBias = Vector2.Dot(dv, collisionResult.Normal);
            float normalImpulseBias = massNormal * (-normalVelocityBias + 0.0f);
            normalImpulseBias = gxtMath.Max(normalImpulseBias + 0.0f, 0.0f);

            Vector2 impulseBias = collisionResult.Normal * normalImpulseBias;
            geomB.RigidBody.ApplyImpulse(impulseBias, collisionResult.ContactPointB);
            impulseBias = -impulseBias;
            geomA.RigidBody.ApplyImpulse(impulseBias, collisionResult.ContactPointA);

            normalImpulseBias = massNormal * (-normalVelocityBias + 0.0f);
            normalImpulseBias = gxtMath.Max(normalImpulseBias, 0.0f);

            Vector2 biasedImpulse = collisionResult.Normal * normalImpulseBias;
            geomB.RigidBody.ApplyImpulse(biasedImpulse, collisionResult.ContactPointB);
            biasedImpulse = -biasedImpulse;
            geomA.RigidBody.ApplyImpulse(biasedImpulse, collisionResult.ContactPointA);

            float maxTangentImpulse = combinedFrictionCoefficient * normalImpulse;
            f1 = 1.0f;
            Vector2 normCrossF = gxtMath.Cross2D(collisionResult.Normal, f1);
            float dvDotTangent = Vector2.Dot(dv, tangent);
            tangentImpulse = massTangent * (-dvDotTangent);
            tangentImpulse = gxtMath.Clamp(tangentImpulse, -maxTangentImpulse, maxTangentImpulse);

            Vector2 frictionImpulse = tangent * tangentImpulse;
            geomB.RigidBody.ApplyImpulse(frictionImpulse, collisionResult.ContactPointB);
            frictionImpulse = -frictionImpulse;
            geomA.RigidBody.ApplyImpulse(frictionImpulse, collisionResult.ContactPointA);


            /* End Post Step */
            /*
            geomA.RigidBody.Update(dt);
            geomB.RigidBody.Update(dt);
            geomA.Update();
            geomB.Update();
            */
            //gxtLog.WriteLineV(VerbosityLevel.INFORMATIONAL, "in collide");

            /*
            Vector2 relativeVelocity = geomA.RigidBody.Velocity - geomB.RigidBody.Velocity;
            // relative velocity along normal
            if (!gxtMath.Equals(relativeVelocity, Vector2.Zero, float.Epsilon))
            {
                float rvn = Vector2.Dot(relativeVelocity, collisionResult.Normal);
                if (!gxtMath.Equals(rvn, 0.0f, float.Epsilon))
                {
                    float f = -(1.0f + combinedRestitutionCoefficient) * rvn;
                    //float nn = Vector2.Dot(collisionResult.Normal, collisionResult.Normal);
                    float invMassSum = geomA.RigidBody.InverseMass + geomB.RigidBody.InverseMass;
                    float denom = Vector2.Dot(collisionResult.Normal, collisionResult.Normal * invMassSum);
                    float invja = 1.0f / (geomA.RigidBody.AngularAcceleration * geomA.RigidBody.Mass);
                    float invjb = 1.0f / (geomB.RigidBody.AngularAcceleration * geomB.RigidBody.Mass);
                    float ja = invja * gxtMath.Cross2D(collisionResult.Normal, collisionResult.ContactPointA);
                    //ja *= gxtMath.Cross2D(collisionResult.ContactPointA, collisionResult.Normal);
                    //ja = gxtMath.Cross2D();
                    float jb = invjb * gxtMath.Cross2D(collisionResult.Normal, collisionResult.ContactPointB);
                    //jb *= gxtMath.Cross2D(collisionResult.ContactPointB, collisionResult.Normal);
                    //float impulseDotN = collisionResult.Normal * (ja + jb);
                    //denom *= (ja * jb);
                    float fcopy = f;
                    f /= denom;
                    float foma = f / geomA.RigidBody.Mass;
                    float fomb = f / geomB.RigidBody.Mass;
                    
                    Vector2 rigidAFinalVelocity = geomA.RigidBody.Velocity + collisionResult.Normal * foma;
                    Vector2 rigidBFinalVelocity = geomB.RigidBody.Velocity - collisionResult.Normal * fomb;

                    geomA.RigidBody.SetVelocity(rigidAFinalVelocity);
                    geomB.RigidBody.SetVelocity(rigidBFinalVelocity);

                    float angularMomentumA = geomA.RigidBody.AngularVelocity * geomA.RigidBody.Mass;
                    float angularMomentumB = geomB.RigidBody.AngularVelocity * geomB.RigidBody.Mass;
                    Vector2 fn = f * collisionResult.Normal;
                    float finalAngularA = angularMomentumA + gxtMath.Cross2D(collisionResult.ContactPointA, fn);
                    geomA.RigidBody.SetAngularVelocity(finalAngularA * geomA.RigidBody.InverseMass);
                    float finalAngularB = angularMomentumB - gxtMath.Cross2D(collisionResult.ContactPointB, fn);
                    geomB.RigidBody.SetAngularVelocity(finalAngularB * geomB.RigidBody.InverseMass);
                    //geomA.RigidBody.Set
                    //geomA.RigidBody.ApplyImpulse(rigidAFinalVelocity, collisionResult.ContactPointA);
                    //geomB.RigidBody.ApplyImpulse(rigidBFinalVelocity, collisionResult.ContactPointB);
                }
                else
                {

                }
            }
            geomA.RigidBody.Translate(collisionResult.Normal * -collisionResult.Depth);
            geomB.RigidBody.Translate(collisionResult.Normal * collisionResult.Depth);
            // translate it out??
            geomB.RigidBody.Update(dt);
            geomA.RigidBody.Update(dt);
            */

            return true;
        }

        public static void Solve(gxtGeom geomA, gxtGeom geomB, gxtCollisionResult collisionResult, float dt)
        {
            // no intersection means there's nothing to solve
            gxtDebug.Assert(collisionResult.Intersection);
            if (geomA.HasAttachedBody() && geomB.HasAttachedBody())
            {
                combinedFrictionCoefficient = GetCombinedFriction(geomA.RigidBody, geomB.RigidBody);
                combinedRestitutionCoefficient = GetCombinedRestitution(geomA.RigidBody, geomB.RigidBody);
            }
            // early return for trigger geoms
            if (CheckGeomResponse(ref geomA, ref geomB, ref collisionResult))
                return;
            // earlier exit for one fixed, one rigid
            if (CheckOneRigidOneFixedResponse(ref geomA, ref geomB, ref collisionResult, dt))
                return;
            if (CheckRigidResponse(ref geomA, ref geomB, ref collisionResult, dt))
                return;
        }
    }
}