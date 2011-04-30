﻿using Microsoft.Xna.Framework;

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
    }
}
