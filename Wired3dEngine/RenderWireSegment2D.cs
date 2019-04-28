using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wire3dEngine
{
    public struct RenderWireSegment2D
    {
        public RenderWireSegment2D(Vector a, Vector b)
        {
            A = a; B = b;
        }
        public Vector A, B;        
    }
}
