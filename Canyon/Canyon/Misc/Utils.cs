using Microsoft.Xna.Framework;
using System;

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
        /// Normalize a vector only if the length isn't zero. This makes sure a division by zero doesn't occure.
        /// </summary>
        /// <param name="v">The vector to normalize.</param>
        public static Vector2 SafeNormalize(this Vector2 v)
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

        /// <summary>
        /// Convert the Vector to a usefull string.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string ToInfo(this Vector2 v)
        {
            return string.Format("{0:N}, {1:N} ({2:N})", v.X, v.Y, v.Length());
        }

        /// <summary>
        /// Return the Up vector for this orientation.
        /// 
        /// Vector3.Transform(Vector3.Up, orientation);
        /// </summary>
        /// <param name="orientation">The orientation</param>
        /// <returns>The transformed Up vector.</returns>
        public static Vector3 Up(this Quaternion orientation)
        {
            return Vector3.Transform(Vector3.Up, orientation);
        }

        /// <summary>
        /// Return the Forward vector for this orientation.
        /// 
        /// Vector3.Transform(Vector3.Forward, orientation);
        /// </summary>
        /// <param name="orientation">The orientation</param>
        /// <returns>The transformed Forward vector.</returns>
        public static Vector3 Forward(this Quaternion orientation)
        {
            return Vector3.Transform(Vector3.Forward, orientation);
        }

        /// <summary>
        /// Return the Right vector for this orientation.
        /// 
        /// Vector3.Transform(Vector3.Right, orientation);
        /// </summary>
        /// <param name="orientation">The orientation</param>
        /// <returns>The transformed Right vector.</returns>
        public static Vector3 Right(this Quaternion orientation)
        {
            return Vector3.Transform(Vector3.Right, orientation);
        }

        /// <summary>
        /// Return the Yaw value of a Quaternion.
        /// </summary>
        /// <param name="orientation">The orientation to calculate a yaw from.</param>
        /// <returns>The resulting Yaw value.</returns>
        public static float GetYaw(this Quaternion orientation)
        {
            return (float)Math.Asin(-2 * (orientation.X * orientation.Z + orientation.W * orientation.Y));
        }

        /// <summary>
        /// Return the Pitch value of a Quaternion.
        /// </summary>
        /// <param name="orientation">The orientation to calculate a pitch from.</param>
        /// <returns>The resulting Pitch value.</returns>
        public static float GetPitch(this Quaternion orientation)
        {
            return (float)Math.Atan2(2 * (orientation.Y * orientation.Z + orientation.W * orientation.X),
                orientation.W * orientation.W - orientation.X * orientation.X - orientation.Y * orientation.Y + orientation.Z * orientation.Z);
        }

        /// <summary>
        /// Return the Roll value of a Quaternion.
        /// </summary>
        /// <param name="orientation">The orientation to calculate a roll from.</param>
        /// <returns>The resulting Roll value.</returns>
        public static float GetRoll(this Quaternion orientation)
        {
            return (float)Math.Atan2(2 * (orientation.X * orientation.Y + orientation.W * orientation.Z),
                orientation.W * orientation.W + orientation.X * orientation.X - orientation.Y * orientation.Y - orientation.Z * orientation.Z);
        }

    }
}
