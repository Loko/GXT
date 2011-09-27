#define GXT_CHECK_HASH_COLLISIONS
//#undef GXT_CHECK_HASH_COLLISIONS

using System;
#if (GXT_CHECK_HASH_COLLISIONS)
using System.Collections.Generic;
#endif
namespace GXT
{
    /// <summary>
    /// Delegate for any hash function
    /// </summary>
    /// <param name="str">string</param>
    /// <returns>Long Hash Id</returns>
	public delegate uint gxtStringHashFunction(string str);

    /// <summary>
    /// A class for managing hashed strings ids
    /// Your own hash function can be swapped in by changing 
    /// the static HashFunction delegate.  Make sure you
    /// use the same hash function or the comparisons will no 
    /// longer be valid
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    // TODO: HOW DO WE HANDLE COLLISIONS?
	public class gxtHashedString : IEquatable<gxtHashedString>
	{
		private string str;
		private uint id;
		
        /// <summary>
        /// The human readable string
        /// </summary>
		public string String { get { return str; } }

        /// <summary>
        /// Hashed Id
        /// </summary>
		public uint Id { get { return id; } }

        private static gxtStringHashFunction hashFunction = JenkinsStringHashFunction;
        /// <summary>
        /// The hash function used to get hash codes for all HashedStrings 
        /// </summary>
        public static gxtStringHashFunction HashFunction { get { return hashFunction; } set { hashFunction = value; } }
		
        /// <summary>
        /// Takes a readable string
        /// </summary>
        /// <param name="s">string</param>
		public gxtHashedString(string s)
		{
			str = s;
			id = Hash(str);
		}
		
        /// <summary>
        /// A simple hash function based on the 
        /// One At A Time Algorithm
        /// </summary>
        /// <param name="str">c# string</param>
        /// <returns>hash id</returns>
		public static uint JenkinsStringHashFunction(string str)
		{
			gxtDebug.Assert(str != null, "Null String Passed Into JenkinsStringHashFunction");

            int len = str.Length;
            gxtDebug.Assert(len != 0, "Empty String Passed Into JenkinsStringHashFunction");

            uint hash = 0;
			for (int i = 0; i < len; i++)
			{
				hash += (uint)str[i];
				hash += (hash << 10);
				hash ^= (hash >> 16);
			}
			hash += (hash << 3);
			hash ^= (hash >> 11);
			hash += (hash << 15);
			return hash;
		}
		
		public static uint Hash(string s)
		{
			gxtDebug.Assert(hashFunction != null);
			uint hashId = hashFunction(s);
            
            #if (GXT_CHECK_HASH_COLLISIONS)
            // check to see if the id is in use, and if it is, that it's the same string
            if (collisionMap.ContainsKey(hashId))
            {
                if (collisionMap[hashId] != s)
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "There has been a collision with your hashed strings!");
                    gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "The hash values for \"{0}\" and \"{1}\" are both {2}!");
                    gxtLog.WriteLineV(gxtVerbosityLevel.CRITICAL, "Please pick a different name for either item!");
                    // it would be very bad to assert inside of a tool or editor
                    // such tools should really have their own way of tracking collisions
                    //gxtDebug.Assert(false, "Hash String Collision!");
                }
            }
            #endif
			
            return hashId;
		}
		
        // comparison functions
        // could expand to compare ids and strings with operators, not sure if that's a good idea tho
        public static bool operator ==(gxtHashedString a, gxtHashedString b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(gxtHashedString a, gxtHashedString b)
        {
            return a.id != b.id;
        }

        public override bool Equals(object obj)
        {
            if (obj is gxtHashedString)
                return Equals((gxtHashedString)obj);
            return false;
        }

        public bool Equals(gxtHashedString other)
        {
            return id == other.id;
        }

        public override int GetHashCode()
        {
            return (int)id;
        }

        public static bool operator <(gxtHashedString a, gxtHashedString b)
        {
            return a.id < b.id;
        }

        public static bool operator <=(gxtHashedString a, gxtHashedString b)
        {
            return a.id <= b.id;
        }

        public static bool operator >(gxtHashedString a, gxtHashedString b)
        {
            return a.id > b.id;
        }

        public static bool operator >=(gxtHashedString a, gxtHashedString b)
        {
            return a.id >= b.id;
        }

        #if (GXT_CHECK_HASH_COLLISIONS)
        private static Dictionary<uint, string> collisionMap = new Dictionary<uint, string>();         
        #endif
    }
}