#define GXT_MATH_TRIG_TABLES
#undef GXT_MATH_TRIG_TABLES

using System;
using Microsoft.Xna.Framework;

namespace GXT
{
    /// <summary>
    /// A custom collection of static math functions
    /// Many functions are aliases of those used in System.Math
    /// 
    /// The reasons for this class are as follows
    /// 1) One math class with all the required functionality (that included in System.Math, MathHelper, and more)
    /// 2) Prevents casting inside other code to floats, thereby improving readablility and preventing possible errors
    /// 3) Lookup tables for trig functions modeled after those used in Ogre and Wild Magic
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtMath
    {
        public const float PI = (float)Math.PI;
        public const float TWO_PI = 2.0f * PI;
        public const float PI_OVER_TWO = PI * 0.5f;
        public const float PI_OVER_FOUR = PI * 0.25f;
        public const float ONE_OVER_PI = 1.0f / PI;
        public const float ONE_OVER_TWO_PI = 1.0f / TWO_PI;

        public const float DEG_TO_RAD = PI / 180.0f;
        public const float RAD_TO_DEG = 180.0f / PI;
        public static readonly float LOG2 = (float)Math.Log(2.0);

        #if GXT_MATH_TRIG_TABLES
        private static int trigTableSize;
        /// <summary>
        /// Size of the trig lookup tables
        /// </summary>
        public static int TrigTableSize { get { return trigTableSize; } }

        /// <summary>
        /// Internal factor value used for table lookup
        /// </summary>
        private static float trigTableFactor;

        // lookup tables for trig functions
        private static float[] sinTable;
        private static float[] tanTable;
        #endif

        /// <summary>
        /// Initializes trig tables to a given size.  The larger the table, the more 
        /// accurate the values, but more memory will be used.  Passing in zero will 
        /// prevent the construction of the tables
        /// </summary>
        /// <param name="tableSize">Size of the trig table, zero to prevent building them</param>
        public static void Initialize(int tableSize = 4096)
        {
            #if GXT_MATH_TRIG_TABLES
            gxtDebug.Assert(tableSize >= 0, "Can't have a negative table size!");
            trigTableSize = tableSize;
            if (trigTableSize > 0)
            {
                trigTableFactor = TrigTableSize / TWO_PI;
                sinTable = new float[TrigTableSize];
                tanTable = new float[TrigTableSize];

                float angle;
                for (int i = 0; i < TrigTableSize; i++)
                {
                    angle = TWO_PI * i / TrigTableSize;
                    sinTable[i] = (float)Math.Sin(angle);
                    tanTable[i] = (float)Math.Tan(angle);
                }
            }
            #endif
        }

        #if GXT_MATH_TRIG_TABLES 
        /// <summary>
        /// Retreives sin value from the look up table
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static float SinFromTable(float angle)
        {
            if (angle >= 0)
            {
                return sinTable[((int)(angle * trigTableFactor)) % TrigTableSize];
            }
            else
            {
                return sinTable[TrigTableSize - (((int)(-angle * trigTableFactor)) % TrigTableSize) - 1];
            }
        }

        /// <summary>
        /// Retreives tan value from the lookup table
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static float TanFromTable(float angle)
        {
            return tanTable[((int)(angle * trigTableFactor)) % TrigTableSize];
        }
        #endif


        #region TrigFunctions
        public static float Sin(float value)
        {
            return (float)Math.Sin(value);
        }

        public static float Cos(float value)
        {
            return (float)Math.Cos(value);
        }

        public static float Tan(float value)
        {
            return (float)Math.Tan(value);
        }

        #if GXT_MATH_TRIG_TABLES
        public static float Sin(float value, bool useTable)
        {
            return (trigTableSize > 0 && useTable) ? SinFromTable(value) : Sin(value);
        }

        public static float Cos(float value, bool useTable)
        {
            return (trigTableSize > 0 && useTable) ? SinFromTable(value + PI_OVER_TWO) : Cos(value);
        }

        public static float Tan(float value, bool useTable)
        {
            return (trigTableSize > 0 && useTable) ? TanFromTable(value) : Tan(value);
        }
        #else
        public static float Sin(float value, bool useTable)
        {
            return Sin(value);
        }

        public static float Cos(float value, bool useTable)
        {
            return Cos(value);
        }

        public static float Tan(float value, bool useTable)
        {
            return Tan(value);
        }
        #endif

        public static float ASin(float value)
        {
            return (float)Math.Asin(value);
        }

        public static float ACos(float value)
        {
            return (float)Math.Acos(value);
        }

        public static float Atan(float value)
        {
            return (float)Math.Atan(value);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }
        #endregion TrigFunctions 

        #region Floor/Ceil
        public static int ICeil(float value)
        {
            return (int)Math.Ceiling(value);
        }

        public static int IFloor(float value)
        {
            return (int)Math.Floor(value);
        }

        public static float Ceil(float value)
        {
            return (float)Math.Ceiling(value);
        }

        public static float Floor(float value)
        {
            return (float)Math.Floor(value);
        }
        #endregion Floor/Ceil

        #region Abs/Sign
        public static float Abs(float value)
        {
            if (value < 0.0f)
                return -value;
            return value;
        }

        public static int IAbs(int value)
        {
            if (value < 0)
                return -value;
            return value;
        }

        public static int ISign(int value)
        {
            if (value < 0)
                return -1;
            if (value > 0)
                return 1;
            return 0;
        }

        public static float Sign(float value)
        {
            if (value < 0.0f)
                return -1.0f;
            if (value > 0.0f)
                return 1.0f;
            return 0.0f;
        }
        #endregion Abs/Sign

        #region NumericalRobustness
        /// <summary>
        /// Determines if the float is a valid value
        /// Used in many IsValid() object check functions
        /// </summary>
        /// <param name="value">float</param>
        /// <returns>Is Valid</returns>
        public static bool IsNaN(float value)
        {
            return float.IsNaN(value);
        }

        /// <summary>
        /// Determines if two floats are equal within a given tolerance
        /// </summary>
        /// <param name="a">float a</param>
        /// <param name="b">float b</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns>If Equal</returns>
        public static bool Equals(float a, float b, float tolerance)
        {
            return Math.Abs(a - b) <= tolerance;
        }

        /// <summary>
        /// Based on code explained on Christer Ericson's blog
        /// Credit to him and Erin Catto
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="absTolerance"></param>
        /// <param name="relTolerance"></param>
        /// <returns></returns>
        public static bool Equals(float a, float b, float absTolerance, float relTolerance)
        {
            return Abs(b - a) <= Max(absTolerance, relTolerance * Max(Abs(a), Abs(b)));
        }

        /*
        /// <summary>
        /// Determines if two vectors are equal within a given tolerance
        /// </summary>
        /// <param name="u">vector u</param>
        /// <param name="v">vector v</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns>If Equal</returns>
        public static bool Equals(Vector2 u, Vector2 v, float tolerance)
        {
            return Equals(u.X, v.X, tolerance) && Equals(u.Y, v.Y, tolerance);
        }
        */
        #endregion NumericalRobustness

        #region Base/Power
        public static float Sqr(float value)
        {
            return value * value;
        }

        public static float Pow(float value, int power)
        {
            while (power > 1)
            {
                value *= value;
                --power;
            }
            return value;
        }

        public static float Pow(float value, float power)
        {
            return (float)Math.Pow(value, value);
        }

        public static float Sqrt(float value)
        {
            return (float)Math.Sqrt(value);
        }

        public static float InvSqrt(float value)
        {
            return 1.0f / Sqrt(value);
        }

        /// <summary>
        /// A safe C# implementation of the Quake 3 fast inverse sqrt
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float FastInvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
            x = x * (1.5f - xhalf * x * x);
            return x;
        }

        public static float Log(float value)
        {
            return (float)Math.Log(value);
        }

        public static float LogN(float value, float n)
        {
            return (float)Math.Log(value, n);
        }

        public float Log2(float value)
        {
            return Log(value) / LOG2;
        }

        /// <summary>
        /// Efficiently determines if the number is a power of two with bit operations
        /// http://en.wikipedia.org/wiki/Power_of_two
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsPower2(int value)
        {
            return (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Efficiently finds the nearest power of two from the given value with bit 
        /// operations
        /// http://en.wikipedia.org/wiki/Power_of_two
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int NearestPower2(int value)
        {
            --value;
            value |= value >> 16;
            value |= value >> 8;
            value |= value >> 4;
            value |= value >> 2;
            value |= value >> 1;
            ++value;
            return value;
        }
        #endregion Base/Power

        #region Conversions
        public static float DegreesToRadians(float degrees)
        {
            return degrees * DEG_TO_RAD;
        }

        public static float RadiansToDegrees(float radians)
        {
            return radians * RAD_TO_DEG;
        }

        /// <summary>
        /// Normalizes the angle to be between PI and -PI
        /// </summary>
        /// <param name="radians">radians</param>
        /// <returns>Wrapped angle</returns>
        public static float WrapAngle(float radians)
        {
            radians += PI;
            radians -= Floor(radians * ONE_OVER_TWO_PI) * TWO_PI;
            radians -= PI;
            return radians;
        }
        #endregion Conversions

        #region Min/Max/Clamp
        public static float Saturate(float x)
        {
            if (x <= 0.0f)
                return 0.0f;
            else if (x >= 1.0f)
                return 1.0f;
            return x;
        }

        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value <= min)
                return min;
            if (value >= max)
                return max;
            return value;
        }

        public static int IClamp(int value, int min, int max)
        {
            if (value <= min)
                return min;
            if (value >= max)
                return max;
            return value;
        }

        public static T Max<T>(T x, T y) where T : IComparable<T>
        {
            if (x.CompareTo(y) > 0)
                return x;
            return y;
        }

        public static float Max(float x, float y)
        {
            if (x >= y)
                return x;
            return y;
        }

        public static int IMax(int x, int y)
        {
            if (x >= y)
                return x;
            return y;
        }

        public static T Min<T>(T x, T y) where T : IComparable<T>
        {
            if (x.CompareTo(y) < 0)
                return x;
            return y;
        }

        public static float Min(float x, float y)
        {
            if (x <= y)
                return x;
            return y;
        }

        public static int IMin(int x, int y)
        {
            if (x <= y)
                return x;
            return y;
        }

        public static float Min3(float v0, float v1, float v2)
        {
            if (v0 < v1)
            {
                if (v0 < v2)
                    return v0;
                else
                    return v2;
            }
            if (v1 < v2)
                return v1;
            else
                return v2;
        }

        public static float Max3(float v0, float v1, float v2)
        {
            if (v0 > v1)
            {
                if (v0 > v2)
                    return v0;
                else
                    return v2;
            }
            if (v1 > v2)
                return v1;
            else
                return v2;
        }
        #endregion Min/Max/Clamp

        #region Interpolation
        public static float Lerp(float a, float b, float t)
        {
            if (t <= 0.0f)
                return a;
            if (t >= 1.0f)
                return b;
            return a + ((b - a) * t);
        }

        public static float SmoothStep(float a, float b, float t)
        {
            float x = t * t * (3.0f - 2.0f * t);
            return a + ((b - a) * x);
        }

        public static float SmootherStep(float a, float b, float t)
        {
            float x = t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
            return a + ((b - a) * x);
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            if (t <= 0.0f)
                return a;
            if (t >= 1.0f)
                return b;
            return a + ((b - a) * t);
        }

        public static Vector2 SmoothStep(Vector2 a, Vector2 b, float t)
        {
            float x = t * t * (3.0f - 2.0f * t);
            return a + ((b - a) * x);
        }

        public static Vector2 SmootherStep(Vector2 a, Vector2 b, float t)
        {
            float x = t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
            return a + ((b - a) * x);
        }

        public static Color SmootherStep(Color a, Color b, float t)
        {
            Vector4 cva = a.ToVector4();
            Vector4 cvb = b.ToVector4();
            Vector4 cvr = new Vector4(SmootherStep(cva.X, cvb.X, t), SmootherStep(cva.Y, cvb.Y, t), SmootherStep(cva.Z, cvb.Z, t), SmootherStep(cva.W, cvb.W, t));
            return new Color(cvr);
        }

        public static Color SmoothStep(Color a, Color b, float t)
        {
            Vector4 cva = a.ToVector4();
            Vector4 cvb = b.ToVector4();
            Vector4 cvr = new Vector4(SmoothStep(cva.X, cvb.X, t), SmoothStep(cva.Y, cvb.Y, t), SmoothStep(cva.Z, cvb.Z, t), SmoothStep(cva.W, cvb.W, t));
            return new Color(cvr);
        }
        #endregion Interpolation

        #region VectorOperations
        public static float Cross2D(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static Vector2 Cross2D(Vector2 a, float s)
        {
            return new Vector2(s * a.Y, -s * a.X);
        }

        public static Vector2 Cross2D(float s, Vector2 a)
        {
            return new Vector2(-s * a.Y, s * a.X);
        }

        public static Vector2 TripleProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 r = new Vector2();
            float ac = Vector2.Dot(a, c);
            float bc = Vector2.Dot(b, c);
            r.X = b.X * ac - a.X * bc;
            r.Y = b.Y * ac - a.Y * bc; 
            return r;
        }

        public static Vector2 GetRotatedVector(Vector2 v, float rad)
        {
            float cos = Cos(rad);
            float sin = Sin(rad);
            return new Vector2(v.X * cos - v.Y * sin, v.Y * cos + v.X * sin);
        }

        public static void RotateVector(ref Vector2 v, float rad)
        {
            float cos = Cos(rad);
            float sin = Sin(rad);
            float tmpX = v.X;
            v.X = tmpX * cos - v.Y * sin;
            v.Y = v.Y * cos + tmpX * sin;
        }

        public static void RotateVectors(ref Vector2 u, ref Vector2 v, float rad)
        {
            float cos = Cos(rad);
            float sin = Sin(rad);
            // rotate u
            float tmpX = u.X;
            u.X = tmpX * cos - u.Y * sin;
            u.Y = u.Y * cos + tmpX * sin;
            // rotate v
            tmpX = v.X;
            v.X = tmpX * cos - v.Y * sin;
            v.Y = v.Y * cos + tmpX * sin;
        }

        public static float AbsDot(Vector2 u, Vector2 v)
        {
            float x = u.X * v.X;
            if (x < 0.0f)
                x = -x;
            float y = u.Y * v.Y;
            if (y < 0.0f)
                y = -y;
            return x * x + y * y;
        }

        public static bool IsCCWTriangle(Vector2 a, Vector2 b, Vector2 c)
        {
            return ((a.X - b.X) * (b.Y - c.Y) - (b.X - c.X) * (a.Y - b.Y) < 0);
        }

        public static Vector2 RightPerp(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }

        public static Vector2 LeftPerp(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        public static Vector2 GetReflection(Vector2 v, Vector2 n)
        {
            return v - 2.0f * Vector2.Dot(v, n) * n;
        }

        public static void GetAxesVectors(float rotation, out Vector2 xAxis, out Vector2 yAxis)
        {
            xAxis = new Vector2(Cos(rotation), Sin(rotation));
            yAxis = new Vector2(-xAxis.Y, xAxis.X);
        }

        public static void Decompose2D(ref Matrix m, out Vector2 translation, out Vector2 scale, out float rotation)
        {
            // translation is easy, just take it directly from the 4th row
            translation = new Vector2(m.M41, m.M42);

            Vector2 xAxis = new Vector2(m.M11, m.M12);
            Vector2 yAxis = new Vector2(m.M21, m.M22);
            // find abs scale
            float sx, sy;
            sx = xAxis.Length();
            sy = yAxis.Length();
            float oosx = 1.0f / sx;
            float oosy = 1.0f / sy;
            // this multiplication effectively strips the scaling
            // leaving us with a normalized rotation matrix
            xAxis *= sx;
            yAxis *= sy;
            // find determinant of this 2d normalized matrix
            // if it is negative we have at least one negative scale
            float det = xAxis.X * yAxis.Y - yAxis.X * xAxis.Y;
            if (det < 0.0f)
            {
                sx = -sx;
                sy = -sy;
                //xAxis *= -1.0f;
                rotation = gxtMath.Atan2(xAxis.Y, xAxis.X);
                /*if (rotation > gxtMath.PI)
                {
                    rotation -= gxtMath.PI;
                    sy = -sy;
                    sx = -sx;
                }
                */
                scale = new Vector2(sx, sy);
            }
            else
            {
                rotation = gxtMath.Atan2(xAxis.Y, xAxis.X);
                scale = new Vector2(sx, sy);
            }
        }
        #endregion VectorOps
    }
}
