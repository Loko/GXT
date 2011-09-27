using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT
{
    public interface gxtINarrowPhaseCollider
    {
        int MaxIterations { get; set; }
        float Tolerance { get; set; }

        bool Contains(ref gxtPolygon polygon, Vector2 pt);
        bool RayCast(gxtRay ray, ref gxtPolygon polygon, out gxtRayHit rayHit, float tmax = float.MaxValue);
        // intersects method without centroid?
        bool Intersects(ref gxtPolygon polygonA, Vector2 centroidA, ref gxtPolygon polygonB, Vector2 centroidB);
        // collide method without centroid?
        bool Collide(ref gxtPolygon polygonA, Vector2 centroidA, ref gxtPolygon polygonB, out gxtCollisionResult collisionResult);
    }
}
