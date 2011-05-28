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

        public static bool OverlapBoxTriangle(Matrix world, Vector3 boxHalfsize, Vector3[] vertices)
        {
#if DEBUG
            if (vertices.Length != 3)
                throw new ArgumentException("Need three vertices for a triangle..");
#endif
            Vector3 v0, v1, v2, normal, e0, e1, e2;
            float min, max, d, p0, p1, p2, rad, fex, fey, fez;

            Matrix invert = Matrix.Invert(world);
            Vector3 boxCenter = world.Translation;
            
            v0 = Vector3.TransformNormal(vertices[0] - boxCenter, invert);
            v1 = Vector3.TransformNormal(vertices[1] - boxCenter, invert);
            v2 = Vector3.TransformNormal(vertices[2] - boxCenter, invert);
            e0 = v1 - v0;
            e1 = v2 - v1;
            e2 = v0 - v2;


            fex = Math.Abs(e0.X);
            fey = Math.Abs(e0.Y);
            fez = Math.Abs(e0.Z);

            // X01(e0[Z], e0[Y], fez, fey):
            p0 = e0.Z * v0.Y - e0.Y * v0.Z;
            p2 = e0.Z * v2.Y - e0.Y * v2.Z;
            min = p0; max = p2;
            if (p0 > p2)
                Swap( ref min, ref max );
            rad = fez * boxHalfsize.Y + fey * boxHalfsize.Z;
            if (min > rad || max < -rad)
                return false;

            // Y02(e0[Z], e0[X], fez, fex):
            p0 = -e0.Z * v0.X - e0.X * v0.Z;
            p2 = -e0.Z * v2.X - e0.X * v2.Z;
            min = p0; max = p2;
            if (p0 > p2)
                Swap(ref min, ref max);
            rad = fez * boxHalfsize.X + fex * boxHalfsize.Z;
            if (min > rad || max < -rad)
                return false;

            // Z12(e0[Y], e0[X], fey, fex):
            p1 = e0.Y * v1.X - e0.X * v1.Y;
            p2 = e0.Y * v2.X - e0.X * v2.Y;
            min = p2; max = p1;
            if (p2 > p1)
                Swap(ref min, ref max);
            rad = fey * boxHalfsize.X + fex * boxHalfsize.Y;
            if( min > rad || max <- rad)
                return false;


            fex = Math.Abs(e1.X);
            fey = Math.Abs(e1.Y);
            fez = Math.Abs(e1.Z);

            // X01(e1[Z], e1[Y], fez, fey):
            p0 = e1.Z * v0.Y - e1.Y * v0.Z;
            p2 = e1.Z * v2.Y - e1.Y * v2.Z;
            min = p0; max = p2;
            if (p0 > p2)
                Swap(ref min, ref max);
            rad = fez * boxHalfsize.Y + fey * boxHalfsize.Z;
            if (min > rad || max < -rad)
                return false;

            // Y02(e1[Z], e1[X], fez, fex):
            p0 = e1.Z * v0.X - e1.X * v0.Z;
            p2 = e1.Z * v2.X - e1.X * v2.Z;
            min = p0; max = p2;
            if (p0 > p2)
                Swap(ref min, ref max);
            rad = fez * boxHalfsize.X + fex * boxHalfsize.Z;
            if (min > rad || max < -rad)
                return false;

            // Z0(e1[Y], e1[X], fey, fex):
            p0 = e1.Y * v0.X - e1.X * v0.Y;
            p1 = e1.Y * v1.X - e1.X * v1.Y;
            min = p0; max = p1;
            if (p0 > p1)
                Swap(ref min, ref max);
            rad = fey * boxHalfsize.X + fex * boxHalfsize.Y;
            if (min > rad || max < -rad)
                return false;


            fex = Math.Abs(e2.X);
            fey = Math.Abs(e2.Y);
            fez = Math.Abs(e2.Z);

            // X2(e2[Z], e2[Y], fez, fey):
            p0 = e2.Z * v0.Y - e2.Y * v0.Z;
            p1 = e2.Z * v1.Y - e2.Y * v1.Z;
            min = p0; max = p1;
            if (p0 > p1)
                Swap(ref min, ref max);
            rad = fez * boxHalfsize.Y + fey * boxHalfsize.Z;
            if (min > rad || max < -rad)
                return false;

            // Y1(e2[Z], e2[X], fez, fex):
            p0 = -e2.Z * v0.X + e2.X * v0.Z;
            p1 = -e2.Z * v1.X + e2.X * v1.Z;
            min = p0; max = p1;
            if (p0 > p1)
                Swap(ref min, ref max);
            rad = fez * boxHalfsize.X + fex * boxHalfsize.Z;
            if (min > rad || max < -rad)
                return false;

            // Z12(e2[Y], e2[X], fey, fex):
            p1 = e2.Y * v1.X - e2.X * v1.Y;
            p2 = e2.Y * v2.X - e2.X * v2.Y;
            min = p2; max = p1;
            if (p2 > p1)
                Swap(ref min, ref max);
            rad = fey * boxHalfsize.X + fex * boxHalfsize.Y;
            if (min > rad || max < -rad)
                return false;


            FindMinMax(ref min, ref max, v0.X, v1.X, v2.X);
            if (min > boxHalfsize.X || max < -boxHalfsize.X)
                return false;

            FindMinMax(ref min, ref max, v0.Y, v1.Y, v2.Y);
            if (min > boxHalfsize.Y || max < -boxHalfsize.Y)
                return false;

            FindMinMax(ref min, ref max, v0.Z, v1.Z, v2.Z);
            if (min > boxHalfsize.Z || max < -boxHalfsize.Z)
                return false;


            normal = Vector3.Cross(e0, e1);
            d = -Vector3.Dot(normal, v0);
            if (!OverlayPlaneBox(normal, d, boxHalfsize))
                return false;


            return true;
        }

        public static bool OverlayPlaneBox(Vector3 normal, float d, Vector3 boxHalfsize )
        {
            Vector3 min, max;
            min.X = normal.X > 0.0f ? -boxHalfsize.X : boxHalfsize.X;
            max.X = -min.X;
            min.Y = normal.Y > 0.0f ? -boxHalfsize.Y : boxHalfsize.Y;
            max.Y = -min.Y;
            min.Z = normal.Z > 0.0f ? -boxHalfsize.Z : boxHalfsize.Z;
            max.Z = -min.Z;
            if (Vector3.Dot(normal, min) + d > 0.0f) 
                return false;
            if (Vector3.Dot(normal, max) + d >= 0.0f)
                return true;
            return false;
        }

        private static void Swap(ref float a, ref float b)
        {
            float t = a;
            a = b;
            b = t;
        }

        public static void FindMinMax(ref float min, ref float max, params float[] values)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                min = Math.Min(min, values[i]);
                max = Math.Max(max, values[i]);
            }
        }
    }
}
