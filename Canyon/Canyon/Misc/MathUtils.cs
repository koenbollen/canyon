using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Canyon.Misc
{
    public static class MathUtils
    {
        /// <summary>
        /// This method tries to intersect a ray with a triangle.
        /// </summary>
        /// <param name="ray">The ray to cast (direction is normal)</param>
        /// <param name="vertices">3 vectices for the triangle</param>
        /// <param name="frac">Result fraction, intersect point is: ray.Position + (ray.Direction * frac)</param>
        /// <returns></returns>
        public static bool IntersectRayTriangle(Ray ray, Vector3[] vertices, ref float frac)
        {
#if DEBUG
            if (vertices.Length != 3)
                throw new ArgumentException("Need three vertices!");
#endif
            Vector3 edge0 = vertices[1] - vertices[0];
            Vector3 edge1 = vertices[2] - vertices[0];

            Vector3 pvec = Vector3.Cross(ray.Direction, edge1);

            float det = Vector3.Dot(edge0, pvec);
            if (det < 0.0001f)
                return false;

            Vector3 tvec = ray.Position - vertices[0];
            float u = Vector3.Dot(tvec, pvec);
            if (u < 0.0f || u > det)
                return false;

            Vector3 qvec = Vector3.Cross(tvec, edge0);

            float v = Vector3.Dot(ray.Direction, qvec);
            if (v < 0.0f || u + v > det)
                return false;

            frac = Vector3.Dot(edge1, qvec);
            float fInvDet = 1.0f / det;
            frac *= fInvDet;

            edge0.Draw(vertices[0], Color.Violet);
            edge1.Draw(vertices[0], Color.Brown);

            return true;
        }
    }
}
