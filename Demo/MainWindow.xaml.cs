using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wire3dEngine;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WireRender _render;
        private DispatcherTimer _updateTimer;

        private WireObject3D obj1;
        private WireObject3D obj2;
        private WireObject3D obj3;        

        public MainWindow()
        {
            InitializeComponent();
            
            InitRender();            
            _updateTimer = new DispatcherTimer(DispatcherPriority.Normal);
            _updateTimer.Interval = TimeSpan.FromSeconds(1 / 30.0);
            _updateTimer.Tick += _updateTimer_Tick;
            _updateTimer.Start();
        }

        void _updateTimer_Tick(object sender, EventArgs e)
        {
            DrawScene();
            UpdateScene();
        }

        void InitRender()
        {
            _render = new WireRender();

            obj1 = CreateCube();
            _render.AddObject(obj1);

            var obj1Transform = new Matrix3D();
            obj1Transform.Translate(new Vector3D(200, 0, 0));
            obj1Transform.Rotate(QuaternionUtils.Create(new Vector3D(0, 0, 1), 0));
            obj1.ApplyTransform(obj1Transform);

            obj2 = CreateCube();
            _render.AddObject(obj2);

            var obj2Transform = new Matrix3D();
            obj2Transform.Translate(new Vector3D(300, 50, 0));
            obj2.ApplyTransform(obj2Transform);

            obj3 = CreateCube();
            _render.AddObject(obj3);

            var obj3Transform = new Matrix3D();
            obj3Transform.Translate(new Vector3D(400, 0, 0));
            obj3.ApplyTransform(obj3Transform);


            var cameraTransform = new Matrix3D();
            cameraTransform.Rotate(QuaternionUtils.Create(new Vector3D(1, 0, 0), 0.1));
            cameraTransform.Translate(new Vector3D(-100, -400, -300));
            _render.ApplyCameraTransfrom(cameraTransform);
        }

        void DrawScene()
        {
            ClearCanvas();

            var second = Environment.TickCount / 1000;

            var visibleWires = _render.GetVisibleWires().ToArray();
            foreach (var wire in visibleWires)
            {
                DrawLine(wire.A, wire.B, Brushes.Black);
            }           
        }

        void UpdateScene()
        {
            var tetraTransform1 = new Matrix3D();
            tetraTransform1.Rotate(QuaternionUtils.Create(new Vector3D(0, 1, 0), 0.01));
            tetraTransform1.Rotate(QuaternionUtils.Create(new Vector3D(1, 0, 0), 0.01));
            tetraTransform1.Rotate(QuaternionUtils.Create(new Vector3D(0, 0, 1), 0.01));
            obj1.ApplyTransform(tetraTransform1);

            var tetraTransform2 = new Matrix3D();
            tetraTransform2.Rotate(QuaternionUtils.Create(new Vector3D(0, 1, 0), -0.01));
            tetraTransform2.Rotate(QuaternionUtils.Create(new Vector3D(1, 0, 0), 0.01));
            tetraTransform2.Rotate(QuaternionUtils.Create(new Vector3D(0, 0, 1), 0.01));
            obj2.ApplyTransform(tetraTransform2);
            
            var tetraTransform3 = new Matrix3D();
            tetraTransform3.Rotate(QuaternionUtils.Create(new Vector3D(0, 1, 0), 0.01));
            tetraTransform3.Rotate(QuaternionUtils.Create(new Vector3D(1, 0, 0), -0.01));
            tetraTransform3.Rotate(QuaternionUtils.Create(new Vector3D(0, 0, 1), 0.01));
            obj3.ApplyTransform(tetraTransform2);            
        }

        WireObject3D CreateLeftTriangle()
        {
            return new WireObject3D(
                new[]
                {
                    new Vector3D(0, -100, -50),
                    new Vector3D(-100, -100, 50),
                    new Vector3D(100, -100, 50),
                    new Vector3D(0, 100, 0)
                },
                new[]
                {                    
                    new ModelTriangle(0, 1, 3)                    
                },
                false);
        }

        WireObject3D CreateRightTriangle()
        {
            return new WireObject3D(
                new[]
                {
                    new Vector3D(0, -100, -50),
                    new Vector3D(-100, -100, 50),
                    new Vector3D(100, -100, 50),
                    new Vector3D(0, 100, 0)
                },
                new[]
                {
                    new ModelTriangle(0, 3, 2),
                },
                false);
        }

        WireObject3D CreateTetrahedron()
        {
            return new WireObject3D(
                new[]
                {
                    new Vector3D(0, -100, -50),
                    new Vector3D(-100, -100, 50),
                    new Vector3D(100, -100, 50),
                    new Vector3D(0, 100, 0)
                },
                new[]
                {
                    new ModelTriangle(0, 1, 2),
                    new ModelTriangle(0, 1, 3),
                    new ModelTriangle(0, 3, 2),
                    new ModelTriangle(1, 2, 3),
                },
                true);
        }

        WireObject3D CreateCube()
        {
            return new WireObject3D(
                new[]
                {
                    new Vector3D(-100, 100, -100),
                    new Vector3D(100, 100, -100),
                    new Vector3D(100, 100, 100),
                    new Vector3D(-100, 100, 100),

                    new Vector3D(-100, -100, -100),
                    new Vector3D(100, -100, -100),
                    new Vector3D(100, -100, 100),
                    new Vector3D(-100, -100, 100),
                },
                new[]
                {
                    new ModelTriangle(0, 3, 7),
                    new ModelTriangle(4, 0, 7),
                    new ModelTriangle(7, 3, 6),
                    new ModelTriangle(3, 2, 6),
                    new ModelTriangle(0, 1, 3),
                    new ModelTriangle(1, 2, 3),
                    new ModelTriangle(1, 0, 4),
                    new ModelTriangle(4, 1, 5),
                    new ModelTriangle(1, 6, 2),
                    new ModelTriangle(1, 5, 6),
                    new ModelTriangle(4, 6, 5),
                    new ModelTriangle(4, 7, 6),
                },
                true);
        }

        void ClearCanvas()
        {
            canvas.Children.Clear();
        }

        void DrawLine(Vector a, Vector b, Brush brush)
        {
            canvas.Children.Add(new Line()
            {
                X1 = a.X,
                Y1 = ActualHeight - a.Y,
                X2 = b.X,
                Y2 = ActualHeight - b.Y,
                Stroke = brush
            });
        }
    }
}
