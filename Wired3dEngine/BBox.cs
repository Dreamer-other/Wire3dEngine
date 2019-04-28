using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Wire3dEngine
{
    public struct BBox
    {
        public Vector3D Min;
        public Vector3D Max;

        public BBox(Vector3D a, Vector3D b)
        {
            Min = new Vector3D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
            Max = new Vector3D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        public BBox(Vector3D a, Vector3D b, Vector3D c)
        {
            Min = new Vector3D(
                Math.Min(Math.Min(a.X, b.X), c.X),
                Math.Min(Math.Min(a.Y, b.Y), c.Y),
                Math.Min(Math.Min(a.Z, b.Z), c.Z));

            Max = new Vector3D(
                Math.Max(Math.Max(a.X, b.X), c.X),
                Math.Max(Math.Max(a.Y, b.Y), c.Y),
                Math.Max(Math.Max(a.Z, b.Z), c.Z));
        }

        public BBox(Vector3D[] vertexes)
        {
            Min = new Vector3D(
                vertexes.Min(v => v.X), 
                vertexes.Min(v => v.Y), 
                vertexes.Min(v => v.Z));

            Max = new Vector3D(
                vertexes.Max(v => v.X),
                vertexes.Max(v => v.Y),
                vertexes.Max(v => v.Z));
        }

        public Vector3D Center { get { return (Min + Max) / 2; } }

        public bool Crossed(BBox box)
        {
            return Min.X <= box.Max.X && Max.X >= box.Min.X &&
                Min.Y <= box.Max.Y && Max.Y >= box.Min.Y &&
                Min.Z <= box.Max.Z && Max.Z >= box.Min.Z;
        }

        public bool Crossed2D(BBox box)
        {
            return Min.X <= box.Max.X && Max.X >= box.Min.X &&
                Min.Y <= box.Max.Y && Max.Y >= box.Min.Y;
        }
    }
}
