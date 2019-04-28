using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Wire3dEngine
{
    public class WireRender
    {
        private List<WireObject3D> _objects = 
            new List<WireObject3D>();

        private Matrix3D _cameraMatrix;

        public WireRender()
        {
            _cameraMatrix = Matrix3D.Identity;
        }

        public void AddObject(WireObject3D obj)
        {
            _objects.Add(obj);
        }

        public void ApplyCameraTransfrom(Matrix3D m)
        {
            var invM = m * Matrix3D.Identity;
            invM.Invert();
            _cameraMatrix = invM * _cameraMatrix;
        }
        
        bool IntersectTriangles(RenderTriangle firstTrg, RenderTriangle secondTrg, ref RenderWireSegment segment)
        {
            Vector3D? firstPoint = null;
            Vector3D? secondPoint = null;
            Vector3D tmpPoint;

            if (VectorUtils.IntersectTriangleAndSegment(firstTrg, secondTrg.A, secondTrg.B, false, out tmpPoint))
            {
                firstPoint = tmpPoint;
            }

            if (VectorUtils.IntersectTriangleAndSegment(firstTrg, secondTrg.B, secondTrg.C, false, out tmpPoint))
            {
                if (firstPoint == null) { firstPoint = tmpPoint; } else { secondPoint = tmpPoint; goto _finish; }
            }

            if (VectorUtils.IntersectTriangleAndSegment(firstTrg, secondTrg.C, secondTrg.A, false, out tmpPoint))
            {
                if (firstPoint == null) { firstPoint = tmpPoint; } else { secondPoint = tmpPoint; goto _finish; }
            }

            if (VectorUtils.IntersectTriangleAndSegment(secondTrg, firstTrg.A, firstTrg.B, false, out tmpPoint))
            {
                if (firstPoint == null) { firstPoint = tmpPoint; } else { secondPoint = tmpPoint; goto _finish; }
            }

            if (VectorUtils.IntersectTriangleAndSegment(secondTrg, firstTrg.B, firstTrg.C, false, out tmpPoint))
            {
                if (firstPoint == null) { firstPoint = tmpPoint; } else { secondPoint = tmpPoint; goto _finish; }
            }

            if (VectorUtils.IntersectTriangleAndSegment(secondTrg, firstTrg.C, firstTrg.A, false, out tmpPoint))
            {
                if (firstPoint == null) { firstPoint = tmpPoint; } else { secondPoint = tmpPoint; goto _finish; }
            }            

            return false;

_finish:
            if(secondPoint == null)
                return false;

            segment = new RenderWireSegment(
                firstPoint.Value,
                secondPoint.Value,
                firstTrg.Index,
                secondTrg.Index,
                firstTrg.Obj,
                secondTrg.Obj);

            return true;
        }

        IEnumerable<RenderWireSegment> GenerateWiresByIntersectingTwoTrianglesList(
            List<RenderTriangle> firstList, List<RenderTriangle> secondList)
        {            
            for (int i = 0; i < firstList.Count; i++)
            {
                var firstTrg = firstList[i];
                for (int j = 0;j < secondList.Count; j++)
                {
                    var secondTrg = secondList[j];
                    if (firstTrg.Obj != secondTrg.Obj && firstTrg.BBox.Crossed(secondTrg.BBox))
                    {
                        var segment = new RenderWireSegment();
                        if (IntersectTriangles(firstTrg, secondTrg, ref segment))
                        {
                            yield return segment;
                        }
                    }
                }
            }
        }

        IEnumerable<RenderWireSegment> IntersectWiresAndTriangles(
            List<RenderWireSegment> wires, List<RenderTriangle> triangles)
        {            
            for (int i = 0; i < wires.Count; i++)
            {
                var segment = wires[i];
                List<double> intersectPoints = null;

                for (int j = 0;j < triangles.Count; j++)
                {
                    var triangle = triangles[j];
                    if (segment.Obj1 != triangle.Obj && segment.Obj2 != triangle.Obj && 
                        segment.BBox.Crossed(triangle.BBox))
                    {
                        double intersectPoint;
                        if (VectorUtils.IntersectTriangleAndSegment(triangle, segment.A, segment.B, true, out intersectPoint))
                        {
                            if(intersectPoints == null)
                                intersectPoints = new List<double>();

                            intersectPoints.Add(intersectPoint);
                        }
                    }                    
                }

                if (intersectPoints == null)
                {
                    yield return segment;
                }
                else
                {
                    intersectPoints.Sort();
                    
                    var prevPoint = segment.A;
                    var dir = segment.B - segment.A;
                    
                    foreach (var intersectPoint in intersectPoints)
                    {
                        var curPoint = segment.A + dir * intersectPoint;

                        yield return new RenderWireSegment(
                            prevPoint, 
                            curPoint, 
                            segment.FirstTriangle, 
                            segment.SecondTriangle, 
                            segment.Obj1, 
                            segment.Obj2);

                        prevPoint = curPoint;
                    }

                    yield return new RenderWireSegment(
                        prevPoint, 
                        segment.B, 
                        segment.FirstTriangle, 
                        segment.SecondTriangle,
                        segment.Obj1,
                        segment.Obj2);
                }
            }
        }

        IEnumerable<RenderWireSegment2D> FilterVisibleWires(List<RenderWireSegment> wires, List<RenderTriangle> triangles)
        {
            int wireIdx = 0;

            var segmentVisibleParts = new List<RenderWireSegment>();
            var newVisibleParts = new List<RenderWireSegment>();

            for (; wireIdx < wires.Count; wireIdx++)
            {                
                var segment = wires[wireIdx];

                segmentVisibleParts.Clear();
                segmentVisibleParts.Add(segment);                
                
                for (int trgIdx = 0;
                    trgIdx < triangles.Count && triangles[trgIdx].MinZ < segment.MaxZ;
                    trgIdx++)
                {
                    newVisibleParts.Clear();
                    var triangle = triangles[trgIdx];

                    foreach (var segmentPart in segmentVisibleParts)
                    {
                        bool isSegmentIntersect = false;

                        if (triangle.BBox.Crossed2D(segmentPart.BBox) && !segmentPart.IsOwnerTriangle(triangle.Index))
                        {
                            bool isOnGoodSide = true;

                            if (triangle.MaxZ > segmentPart.MinZ && triangle.MinZ < segmentPart.MaxZ)
                            {
                                var trgToSegCenter = segmentPart.BBox.Center - triangle.A;
                                var trgToZero = -triangle.A;

                                if (Math.Sign(Vector3D.DotProduct(trgToSegCenter, triangle.N)) ==
                                    Math.Sign(Vector3D.DotProduct(new Vector3D(0, 0, -1), triangle.N)))
                                {
                                    isOnGoodSide = false;
                                }
                            }

                            if (isOnGoodSide)
                            {
                                bool isAInside, isBInside;
                                double k1, k2;
                                if (VectorUtils.IntersectTriangleAndSegment2D(triangle.A.To2D(), triangle.B.To2D(), triangle.C.To2D(), segmentPart.A.To2D(), segmentPart.B.To2D(),
                                        out isAInside, out isBInside, out k1, out k2))
                                {
                                    double k = k1;

                                    if (!isAInside)
                                    {
                                        var newB = segmentPart.Interpolate(k);
                                        newVisibleParts.Add(
                                            new RenderWireSegment(
                                                segmentPart.A,
                                                newB,
                                                segmentPart.FirstTriangle,
                                                segmentPart.SecondTriangle,
                                                segmentPart.Obj1,
                                                segmentPart.Obj2));

                                        k = k2;
                                    }

                                    if (!isBInside)
                                    {
                                        var newA = segmentPart.Interpolate(k);
                                        newVisibleParts.Add(
                                            new RenderWireSegment(
                                                newA,
                                                segmentPart.B,
                                                segmentPart.FirstTriangle,
                                                segmentPart.SecondTriangle,
                                                segmentPart.Obj1,
                                                segmentPart.Obj2));
                                    }

                                    isSegmentIntersect = true;
                                }
                            }
                        }

                        if (!isSegmentIntersect)
                        {
                            newVisibleParts.Add(segmentPart);
                        }
                    }

                    segmentVisibleParts.Clear();
                    segmentVisibleParts.AddRange(newVisibleParts);
                }
                

                foreach(var visibleSegmentPart in segmentVisibleParts)
                {
                    yield return new RenderWireSegment2D(visibleSegmentPart.A.To2D(), visibleSegmentPart.B.To2D());
                }                
            }
        }

        public IEnumerable<RenderWireSegment2D> GetVisibleWires()
        {
            var listOfListsTriangles = _objects.
                Select(o => o.GetTrianglesToRender(_cameraMatrix).OrderBy(t => t.MinZ).ToList()).
                ToList();

            var allTriangles = listOfListsTriangles.SelectMany(l => l).ToList();

            var listOfListsWires = _objects.
                Select(o => o.GetWiresToRender(_cameraMatrix).OrderBy(t => t.MinZ).ToList()).
                ToList();

            var allWires = listOfListsWires.SelectMany(l => l).ToList();

            // ребра образованные пересечениями треугольников
            var wiresOfIntersecion = new List<RenderWireSegment>();
            for (int i = 0; i < listOfListsTriangles.Count - 1; i++)
            {
                for (int j = i + 1; j < listOfListsTriangles.Count; j++)
                {
                    var wiresByIntersect = GenerateWiresByIntersectingTwoTrianglesList(
                        listOfListsTriangles[i], listOfListsTriangles[j]).ToList();

                    foreach (var wire in wiresByIntersect)
                    {
                        wiresOfIntersecion.Add(wire);
                    }
                }
            }

            allWires.AddRange(wiresOfIntersecion);

            allWires = IntersectWiresAndTriangles(allWires, allTriangles).ToList();
            
            allWires.Sort(Comparer<RenderWireSegment>.Create((w1, w2) => w1.MinZ.CompareTo(w2.MinZ)));
            allTriangles.Sort(Comparer<RenderTriangle>.Create((w1, w2) => w1.MinZ.CompareTo(w2.MinZ)));

            return FilterVisibleWires(allWires, allTriangles);
        }
    }
}
