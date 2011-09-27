using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GXT
{
    public interface gxtIBroadPhaseCollider<T>
    {
        void Initialize();
        gxtBroadphaseCollisionPair<T>[] GetCollisionPairs();
        void AddObject(T obj, ref gxtAABB aabb);
        void UpdateObject(T obj, ref gxtAABB aabb);
        bool RemoveObject(T obj);
        // test(aabb)
        // raycast
        // raycast all
        // testall(pt)
    }
}
