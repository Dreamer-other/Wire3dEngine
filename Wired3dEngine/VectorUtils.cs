using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Wire3dEngine
{
    public static class VectorUtils
    {
        public const float EPSILON = 0.0001f;

        public static bool IntersectTriangleAndSegment(RenderTriangle triangle, Vector3D a, Vector3D b, bool onlyCheckPlane,
            out Vector3D result)
        {
            double dummy;
            return IntersectTriangleAndSegment(triangle.A, triangle.B, triangle.C, a, b, onlyCheckPlane, out result, out dummy);
        }

        public static bool IntersectTriangleAndSegment(RenderTriangle triangle, Vector3D a, Vector3D b, bool onlyCheckPlane,
            out double dirK)
        {
            Vector3D dummy;
            return IntersectTriangleAndSegment(triangle.A, triangle.B, triangle.C, a, b, onlyCheckPlane, out dummy, out dirK);
        }

        public static bool IntersectTriangleAndSegment(Vector3D v0, Vector3D v1, Vector3D v2, Vector3D a, Vector3D b, bool onlyCheckPlane, out Vector3D result, out double dirK)
        {
            result = new Vector3D();
            dirK = 0;            
            var ab = (b - a);
            var segLen = ab.Length;

            if (segLen > 0)
            {
                var dir = ab / segLen;

                var orig = a;
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var pvec = Vector3D.CrossProduct(dir, edge2);
                var det = Vector3D.DotProduct(edge1, pvec);

                if (Math.Abs(det) < EPSILON) return false;

                var invDet = 1 / det;
                var tvec = orig - v0;
                var u = Vector3D.DotProduct(tvec, pvec) * invDet;

                if (!onlyCheckPlane && (u < 0 || u > 1))
                    return false;

                var qvec = Vector3D.CrossProduct(tvec, edge1);
                var v = Vector3D.DotProduct(dir, qvec) * invDet;

                if (!onlyCheckPlane && (v < 0 || u + v > 1))
                    return false;

                var dist = (Vector3D.DotProduct(edge2, qvec) * invDet);
                dirK = dist / segLen;

                if (dirK < 0 || dirK > 1)
                    return false;

                result = orig + dir * dist;

                return true;
            }
            else
            {
                return false;
            }

            // result = new Vector3D();
            // dirK = 0;
            // var dir = (b - a);
            // dir.Normalize();
            // var orig = a;
            // var v0v1 = v1 - v0;
            // var v0v2 = v2 - v0;

            // var N = Vector3D.CrossProduct(v0v1, v0v2);
            // var nDotRay = Vector3D.DotProduct(N, dir);

            // if (Math.Abs(Vector3D.DotProduct(N, dir)) < EPSILON) 
            //     return false; // ray parallel to triangle 

            // var d = Vector3D.DotProduct(N, v0);
            // dirK = -(Vector3D.DotProduct(N, orig) + d) / nDotRay;

            // if (dirK < 0 || dirK > 1)
            //     return false;

            // if (onlyCheckPlane)
            //     return true;

            // // inside-out test
            // var Phit = dir * dirK + orig;

            //// inside-out test edge0
            // var v0p = Phit - v0;
            // var v = Vector3D.DotProduct(N, Vector3D.CrossProduct(v0v1, v0p));
            // if (v < 0) return false; // P outside triangle

            // // inside-out test edge1
            // var v1p = Phit - v1;
            // var v1v2 = v2 - v1;
            // var w = Vector3D.DotProduct(N, Vector3D.CrossProduct(v1v2, v1p));
            // if (w < 0) return false; // P outside triangle

            // // inside-out test edge2
            // var v2p = Phit - v2;
            // var v2v0 = v0 - v2;
            // var u = Vector3D.DotProduct(N, Vector3D.CrossProduct(v2v0, v2p));
            // if (u < 0) return false; // P outside triangle

            // result = orig + dir * dirK;

            // return true;
        }

        public static bool IntersectSegments2D(Vector a, Vector b, Vector c, Vector d, out double ua)
        {
            ua = double.NaN;            

            var dir1 = b - a;
            var dir2 = d - c;

            var det = dir2.Y*dir1.X - dir2.X*dir1.Y;

            if (Math.Abs(det) < EPSILON)
                return false;

            var dy = a.Y - c.Y;
            var dx = a.X - c.X;
            var k = (dir2.X*dy - dir2.Y*dx) / det;

            if (0 > k || k > 1)
                return false;

            var ub = (dir1.X*dy - dir1.Y*dx) / det;
            if (0 > ub || ub > 1)
                return false;

            ua = k;

            return true;
        }

        public static bool IntersectTriangleAndSegment2D(Vector v0, Vector v1, Vector v2, Vector a, Vector b, out bool isAInside, out bool isBInside, out double k1, out double k2)
        {
            k1 = double.NaN;
            k2 = double.NaN;
            
            var av0 = v0 - a;
            var av1 = v1 - a;
            var av2 = v2 - a;

            var bv0 = v0 - b;
            var bv1 = v1 - b;
            var bv2 = v2 - b;

            var v01 = v1 - v0;
            var v21 = v2 - v1;
            var v20 = v0 - v2;
            
            var leftV01 = new Vector(-v01.Y, v01.X);
            var leftV12 = new Vector(-v21.Y, v21.X);
            var leftV20 = new Vector(-v20.Y, v20.X);

            bool? isAAtLeftV01 = IsGreaterZero(leftV01 * av0);
            bool? isAAtLeftV12 = IsGreaterZero(leftV12 * av1);
            bool? isAAtLeftV20 = IsGreaterZero(leftV20 * av2);

            bool? isBAtLeftV01 = IsGreaterZero(leftV01 * bv0);
            bool? isBAtLeftV12 = IsGreaterZero(leftV12 * bv1);
            bool? isBAtLeftV20 = IsGreaterZero(leftV20 * bv2);

            bool? notEdgeSideA = isAAtLeftV01 ?? isAAtLeftV12 ?? isAAtLeftV20;

            isAInside = notEdgeSideA == (isAAtLeftV01 ?? notEdgeSideA) && 
                notEdgeSideA == (isAAtLeftV12 ?? notEdgeSideA) && 
                notEdgeSideA == (isAAtLeftV20 ?? notEdgeSideA);

            bool? notEdgeSideB = isBAtLeftV01 ?? isBAtLeftV12 ?? isBAtLeftV20;

            isBInside = notEdgeSideB == (isBAtLeftV01 ?? notEdgeSideB) &&
                notEdgeSideB == (isBAtLeftV12 ?? notEdgeSideB) &&
                notEdgeSideB == (isBAtLeftV20 ?? notEdgeSideB);

            if (isAInside && isBInside)
                return true;

            double k;
            if (isAAtLeftV01 != null && isBAtLeftV01 != null && 
                IntersectSegments2D(a, b, v0, v1, out k1))
            {
                if (isAInside || isBInside) return true;
            }

            if (isAAtLeftV12 != null && isBAtLeftV12 != null && 
                IntersectSegments2D(a, b, v1, v2, out k))
            {
                if (double.IsNaN(k1)) k1 = k; else { k2 = k; goto _finish;}

                if (isAInside || isBInside) return true;
            }

            if (isAAtLeftV20 != null && isBAtLeftV20 != null && 
                IntersectSegments2D(a, b, v2, v0, out k))
            {
                if (double.IsNaN(k1)) k1 = k; else k2 = k;
            }

_finish:
            if (!double.IsNaN(k2) && k1 > k2)
            {
                var t = k2; k2 = k1; k1 = t;
            }

            return !double.IsNaN(k1) && (!double.IsNaN(k2) || isAInside || isBInside);
        }

        static bool? IsGreaterZero(double v)
        {
            if (v > EPSILON) return true;
            if (v < -EPSILON) return false;
            return null;
        }

        public static Vector To2D(this Vector3D v)
        {
            return new Vector(v.X, v.Y);
        }
    }
}
