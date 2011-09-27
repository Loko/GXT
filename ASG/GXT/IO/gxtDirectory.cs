using System;
using System.Collections.Generic;
using System.IO;

namespace GXT.IO
{
    /// <summary>
    /// This class will eventually be a platform independent layer for directory management on the 
    /// PC and the XBOX 360.  It will require more research on how IO works on the 360.  For now it is just a stub.
    /// </summary>
    public class gxtDirectory
    {
        public static string GetCurrentDirectory()
        {
            return "";
            //#if WINDOWS
            //#endif

            //#elif XBOX
            
            //#endif
        }

        public static void CreateDirectory(string path)
        {

        }

        // static methods for delete, copy, move, etc. etc.
        // seperate file class, does more or less the same thing
        // resource manager also goes in this directory/namespace
        // need to research how XNA Storage class for xbox 360 works
    }
}
