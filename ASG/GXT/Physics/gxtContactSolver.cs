using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GXT.Physics
{
    // unused
    public class gxtContactSolver
    {
        private int maxIterations;
        public int MaxIterations { get { return maxIterations; } set { gxtDebug.Assert(value > 5, "Must have a sensible amount of contacts to resolve!"); maxIterations = value; } }

        private List<gxtContact> contactList;

        public void SortContacts(List<gxtContact> contacts)
        {
            contactList = contacts;
            // do insertion sort by penetration depth
            // log if the max iterations is less than the amount of contacts
        }

        public void PreStepImpulses(float dt)
        {

        }

        public void ApplyImpulses(float dt)
        {

        }
    }
}
