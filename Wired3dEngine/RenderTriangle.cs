using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Wire3dEngine
{
    public struct RenderTriangle
    {
        public RenderTriangle(Vector3D a, Vector3D b, Vector3D c, int index, WireObject3D obj)
            :this()
        {
            A = a; B = b; C = c;
            N = Vector3D.CrossProduct(B - A, C - A);
            N.Normalize();

            BBox = new BBox(a, b, c);
            Index = index;
            Obj = obj;
        }

        public int Index;

        public Vector3D A, B, C, N;        

        public BBox BBox;

        public WireObject3D Obj;

        public double MinZ { get { return BBox.Min.Z; } }

        public double MaxZ { get { return BBox.Max.Z; } }        
    }
}
