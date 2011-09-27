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
    /*
    public enum gxtFrictionType
    {
        AVERAGE = 0,
        AVERAGE_SQRT = 1,
        MINIMUM = 2
    };
    */

    public class gxtSpecialContact //: IComparable<gxtContact>
    {
        private Vector2 position, normal;
        private float depth;

        public Vector2 Position { get { return position; } }
        public Vector2 Normal { get { return normal; } }
        public float Depth { get { return depth; } }

        private float massNormal, massTangent;
        private float normalImpulse, tangentImpulse;
        private float bounceVelocity;

        public float MassNormal { get { return massNormal; } set { massNormal = value; } }
        public float MassTangent { get { return massTangent; } set { massTangent = value; } }
        public float NormalImpulse { get { return normalImpulse; } set { normalImpulse = value; } }
        public float TangentImpulse { get { return tangentImpulse; } set { tangentImpulse = value; } }
        public float BounceVelocity { get { return bounceVelocity; } set { bounceVelocity = value; } }

        public void SetupContact(ref gxtCollisionResult cresult, bool isContactA)
        {
            if (isContactA)
                this.position = cresult.ContactPointA;
            else
                this.position = cresult.ContactPointB;
            this.normal = cresult.Normal;
            this.depth = cresult.Depth;
        }
    }
}
