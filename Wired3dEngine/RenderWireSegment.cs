using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Wire3dEngine
{
    public struct RenderWireSegment
    {
        public Vector3D A, B;

        public int FirstTriangle, SecondTriangle;

        public BBox BBox;

        public WireObject3D Obj1, Obj2;
        
        public RenderWireSegment(Vector3D a, Vector3D b, int firstTriangle, int secondTriangle, WireObject3D obj1, WireObject3D obj2)
        {
            A = a; B = b;
            FirstTriangle = firstTriangle;
            SecondTriangle = secondTriangle;
            BBox = new BBox(a, b);
            Obj1 = obj1;
            Obj2 = obj2;
        }

        public bool IsOwnerTriangle(int triangleIndex)
        {
            return FirstTriangle == triangleIndex ||
                SecondTriangle == triangleIndex;
        }

        public double MinZ { get { return BBox.Min.Z; } }

        public double MaxZ { get { return BBox.Max.Z; } }

        public Vector3D Interpolate(double k)
        {
            return A + (B - A) * k;
        }        
    }
}
