using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Wire3dEngine
{
    public class WireObject3D
    {
        private ModelTriangle[] _modelTriangles;
        private Vector3D[] _vertexes;
        private Matrix3D _matrix;
        private ModelWireSegment[] _modelWires;

        public WireObject3D(Vector3D[] vertexes, ModelTriangle[] modelTriangles, bool hideFlatEdges)
        {
            _matrix = Matrix3D.Identity;
            _vertexes = vertexes;
            _modelTriangles = modelTriangles;
            
            _modelWires = GenerateWiresByTriangles(vertexes, modelTriangles, hideFlatEdges);            
        }

        public void ApplyTransform(Matrix3D m)
        {
            _matrix = m*_matrix;
        }

        public IEnumerable<RenderTriangle> GetTrianglesToRender(Matrix3D invCameraMatrix)
        {
            var finalMatrix = _matrix * invCameraMatrix;
            foreach (var trinagle in _modelTriangles)
            {
                yield return new RenderTriangle(
                    (Vector3D)finalMatrix.Transform((Point3D)_vertexes[trinagle.A]),
                    (Vector3D)finalMatrix.Transform((Point3D)_vertexes[trinagle.B]),
                    (Vector3D)finalMatrix.Transform((Point3D)_vertexes[trinagle.C]),
                    trinagle.Index,
                    this);
            }
        }

        public IEnumerable<RenderWireSegment> GetWiresToRender(Matrix3D invCameraMatrix)
        {
            var finalMatrix = _matrix * invCameraMatrix;
            foreach (var wire in _modelWires)
            {
                yield return new RenderWireSegment(
                    (Vector3D)finalMatrix.Transform((Point3D)_vertexes[wire.A]),
                    (Vector3D)finalMatrix.Transform((Point3D)_vertexes[wire.B]), 
                    wire.FirstTriangle, 
                    wire.SecondTriangle,
                    this,
                    null);
            }
        }

        static IEnumerable<Edge> GetEdgesByTriangle(ModelTriangle trg)
        {
            yield return new Edge(trg.A, trg.B);
            yield return new Edge(trg.B, trg.C);
            yield return new Edge(trg.C, trg.A);
        }

        static ModelWireSegment[] GenerateWiresByTriangles(Vector3D[] vertexes, ModelTriangle[] triangles, bool hideFlatEdges)
        {
            var edgeToTrgs = new Dictionary<Edge, TrianglesPair>();            

            foreach (var trg in triangles)
            {
                foreach (var edgeToTrg in GetEdgesByTriangle(trg))
                {
                    TrianglesPair trgPair;
                    if (edgeToTrgs.TryGetValue(edgeToTrg, out trgPair))
                    {
                        Debug.Assert(trgPair.Trg2 == null);
                        trgPair.Trg2 = trg;
                    }
                    else
                    {
                        trgPair = new TrianglesPair();                        
                        trgPair.Trg1 = trg;

                        edgeToTrgs.Add(edgeToTrg, trgPair);
                    }
                }                               
            }

            return edgeToTrgs.
                Where(kv =>
                {
                    if (hideFlatEdges)
                    {
                        var edge = kv.Value;
                        if (edge.Trg2 != null)
                        {
                            var t1 = edge.Trg1;
                            var t2 = edge.Trg2;

                            var a1 = vertexes[t1.A] - vertexes[t1.C];
                            var b1 = vertexes[t1.B] - vertexes[t1.C];

                            var a2 = vertexes[t2.A] - vertexes[t2.C];
                            var b2 = vertexes[t2.B] - vertexes[t2.C];

                            var n1 = Vector3D.CrossProduct(a1, b1);

                            if (Math.Abs(Vector3D.DotProduct(n1, a2)) < VectorUtils.EPSILON && 
                                Math.Abs(Vector3D.DotProduct(n1, b2)) < VectorUtils.EPSILON)
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }).
                Select(kv => new ModelWireSegment(kv.Key.A, kv.Key.B, kv.Value.Trg1Index, kv.Value.Trg2Index)).
                ToArray();
        }

        class TrianglesPair
        {
            public ModelTriangle Trg1 { get; set; }
            public ModelTriangle Trg2 { get; set; }            
            public int Trg1Index { get { return Trg1 != null ? Trg1.Index : 0; } }
            public int Trg2Index { get { return Trg2 != null ? Trg2.Index : 0; } }
        }

        struct Edge
        {
            public Edge(int a, int b)
            {
                A = Math.Min(a, b);
                B = Math.Max(a, b);
            }
            public int A;
            public int B;
        }
    }

    public class ModelWireSegment : IEqualityComparer<ModelWireSegment>
    {
        public ModelWireSegment(int a, int b, int firstTriangle, int secondTriangle)            
        {
            A = Math.Min(a, b); B = Math.Max(a, b);
            FirstTriangle = firstTriangle;
            SecondTriangle = secondTriangle;
        }

        public int A { get; private set; }
        public int B { get; private set; }
        public int FirstTriangle { get; private set; }
        public int SecondTriangle { get; private set; }

        public bool Equals(ModelWireSegment x, ModelWireSegment y)
        {
            return x.A == y.A && x.B == y.B;
        }

        public int GetHashCode(ModelWireSegment obj)
        {
            return (A << 16) | B;
        }
    }

    public class ModelTriangle
    {
        public ModelTriangle(int a, int b, int c)        
        {
            A = a; B = b; C = c;
            Index = ++IndexCounter;
        }

        public int A, B, C;

        public int Index { get; private set; }

        private static int IndexCounter = 0;
    }
}
