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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20190324_グラデーションブラシ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var myBrush = new LinearGradientBrush();
            myBrush.StartPoint = new Point(0.5, 0.0);
            myBrush.EndPoint = new Point(0.5, 1.0);
            myBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
            myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.33));
            myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 0.66));
            myBrush.GradientStops.Add(new GradientStop(Colors.Cyan, 1.0));
            //0 60 120 180
            //MyBorder.Background = myBrush;
            

            var egeo = new EllipseGeometry(new Rect(new Size(200, 200)));

            //MyPath.Data = new EllipseGeometry(new Rect(new Size(200, 200)));
            //MyPath.Stroke = myBrush;
            //MyPath.StrokeThickness = 10;

            var gd = new GeometryDrawing();
            //gd.Geometry = egeo;
            Pen myPen = new Pen();
            myPen.Thickness = 12;
            myPen.Brush = myBrush;
            gd.Pen = myPen;

            var pg = new PathGeometry();
            var pf = new PathFigure();
            pf.StartPoint = new Point(0, 0);
            var seg = new ArcSegment();
            seg.Point = new Point(50, 0);
            seg.Size = new Size(100, 100);
            seg.SweepDirection = SweepDirection.Clockwise;
            seg.IsLargeArc = true;
            seg.RotationAngle = 0;

            pf.Segments.Add(seg);
            pg.Figures.Add(pf);
            gd.Geometry = pg;
            

            MyImage.Stretch = Stretch.None;

            var dImg = new DrawingImage();
            dImg.Drawing = gd;
            MyImage.Source = dImg;


            var bb = new RadialGradientBrush();
            //bb.MappingMode

            MyPath1.Stroke = myBrush;

            myBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0.9, 0.57735),
            };
            myBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
            myBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));
            MyPath2.Stroke = myBrush;

            myBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5, 0.1),
                EndPoint = new Point(0.5, 0.9),
            };
            myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            //myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 1.0));
            MyPath3.Stroke = myBrush;

            myBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(1.0, 0.57735),
                EndPoint = new Point(0.0, 0.9),
            };
            myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 0.0));
            //myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.Cyan, 1.0));
            MyPath4.Stroke = myBrush;

            myBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.9, 1.0),
                EndPoint = new Point(0.0, 0.57735),
            };
            myBrush.GradientStops.Add(new GradientStop(Colors.Cyan, 0.0));
            //myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.Blue, 1.0));
            MyPath5.Stroke = myBrush;

            myBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5,1.0),
                EndPoint = new Point(0.5, 0.1),
            };
            myBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0.0));
            //myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.Magenta, 1.0));
            MyPath6.Stroke = myBrush;

            myBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.0, 0.57735),
                EndPoint = new Point(1.0, 0.0),
            };
            myBrush.GradientStops.Add(new GradientStop(Colors.Magenta, 0.0));
            //myBrush.GradientStops.Add(new GradientStop(Colors.Lime, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
            MyPath7.Stroke = myBrush;

        }
    }
}
