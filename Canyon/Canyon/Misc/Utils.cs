using Microsoft.Xna.Framework;

namespace Canyon.Misc
{
    /// <summary>
    /// Common global ultilities.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Normalize a vector only if the length isn't zero. This makes sure a division by zero doesn't occure.
        /// </summary>
        /// <param name="v">The vector to normalize.</param>
        public static Vector3 SafeNormalize(this Vector3 v)
        {
            if (v.LengthSquared() > 0)
                v.Normalize();
            return v;
        }

        /// <summary>
        /// Set's the Vector's Y axis to Zero.
        /// </summary>
        /// <param name="v">The vector to flatten</param>
        /// <returns>Same vector with Y == 0</returns>
        public static Vector3 Flatten(this Vector3 v)
        {
            return v * new Vector3(1, 0, 1);
        }

        public static bool IsValid(this Vector2 v)
        {
            if (float.IsInfinity(v.X) || float.IsInfinity(v.Y))
                return false;
            if (float.IsNaN(v.X) || float.IsNaN(v.Y))
                return false;
            return true;
        }

        /// <summary>
        /// Convert the Vector to a usefull string.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string ToInfo(this Vector3 v)
        {
            return string.Format("{0:N}, {1:N}, {2:N} ({3:N})", v.X, v.Y, v.Z, v.Length());
        }
    }
}
