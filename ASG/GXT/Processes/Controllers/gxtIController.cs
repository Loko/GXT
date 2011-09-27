using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GXT.Processes
{
    public interface gxtIController : gxtIUpdate 
    {
        Type GetTargetType();
        bool QueriesInput { get; }
        void LateUpdate(GameTime gameTime);
    }
}
