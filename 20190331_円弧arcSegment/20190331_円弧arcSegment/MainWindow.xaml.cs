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

namespace _20190331_円弧arcSegment
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            double radius = 50;
            Point center = new Point(radius, radius);

            MyWrapPanel.Children.Add(MakeBorder(radius, null));

            PathGeometry arc = MakeArcPathGeometry(center, radius, 0, 120, SweepDirection.Clockwise);
            MyWrapPanel2.Children.Add(MakePath(arc));

            PathGeometry pei = MakePieClip(center, radius, 120);
            MyWrapPanel2.Children.Add(MakePath(pei));
            MyWrapPanel2.Children.Add(MakeBorder(radius,pei));


            double start = 330;
            double stop = 30;
            //回転方向固定、時計回り
            arc = MakeArcPathGeometry(center, radius, start, stop, SweepDirection.Clockwise);
            pei = MakePieClip(center, radius, start, stop);

            MyWrapPanel2.Children.Add(MakePath(arc));
            MyWrapPanel2.Children.Add(MakePath(pei));
            MyWrapPanel2.Children.Add(MakeBorder(radius, pei));


            //回転方向任意
            arc = MakeArcPathGeometry(center, radius, start, stop, SweepDirection.Counterclockwise);
            pei = MakePieClip(center, radius, start, stop,SweepDirection.Counterclockwise);

            MyWrapPanel2.Children.Add(MakePath(arc));
            MyWrapPanel2.Children.Add(MakePath(pei));
            MyWrapPanel2.Children.Add(MakeBorder(radius, pei));




            ////arc.Clip = MakeArcClip(center, radius, 100);//開始角度固定0度
            ////arc.Clip = MakeArcClip(center, radius, 27, 291);
            //border.Clip = MakePieClip(center, radius, 330, 30, SweepDirection.Counterclockwise);
            ////arc.Clip = MakeTryangle();//三角形clip、テスト用
            //MyWrapPanel.Children.Add(border);

            //Path pa;
            //pa = new Path();
            //pa.Stroke = Brushes.YellowGreen;
            //pa.StrokeThickness = 1;
            //pa.Margin = new Thickness(4);
            //pa.Data = MakePieClip(center, radius, 330, 30, SweepDirection.Counterclockwise);
            //MyWrapPanel.Children.Add(pa);

            //pa = new Path();
            //pa.Stroke = Brushes.YellowGreen;
            //pa.StrokeThickness = 1;
            //pa.Margin = new Thickness(4);
            //pa.Data = MakeArcPathGeometry(center, radius, 330, 30, SweepDirection.Clockwise);
            //MyWrapPanel.Children.Add(pa);

            //pa = new Path();
            //pa.Stroke = Brushes.YellowGreen;
            //pa.StrokeThickness = 1;
            //pa.Margin = new Thickness(4);
            //pa.Data = MakeArcPathGeometry(center, radius, 330, 30, SweepDirection.Counterclockwise);
            //MyWrapPanel.Children.Add(pa);

        }

        private Border MakeBorder(double radius, PathGeometry clip)
        {
            Border border = new Border
            {
                Background = Brushes.LightCoral,
                Width = radius * 2.0,
                Height = radius * 2.0,
                Margin = new Thickness(4),
                Clip = clip
            };
            return border;
        }
        private Path MakePath(PathGeometry clip)
        {
            Path path;
            path = new Path
            {
                Stroke = Brushes.YellowGreen,
                StrokeThickness = 1,
                Margin = new Thickness(4),
                Data = clip
            };
            return path;
        }

        /// <summary>
        /// 円弧のPathGeometryを作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>        
        /// <param name="startDegrees">開始角度、0以上360以下で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360以下で指定</param>
        /// <param name="direction">回転方向、Clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry MakeArcPathGeometry(Point center, double radius, double startDegrees, double stopDegrees, SweepDirection direction)
        {
            Point start = MakePoint(startDegrees, center, radius);
            Point stop = MakePoint(stopDegrees, center, radius);
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDegrees - startDegrees : startDegrees - stopDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(radius, radius), 0, isLarge, direction, true);
            var fig = new PathFigure();
            fig.StartPoint = start;
            fig.Segments.Add(arc);
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }

        //完成形、回転方向を指定できるように
        /// <summary>
        /// 扇(pie)型のPathGeometryを作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>        
        /// <param name="startDegrees">開始角度、0以上360以下で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360以下で指定</param>
        /// <param name="direction">回転方向、Clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry MakePieClip(Point center, double radius, double startDegrees, double stopDegrees, SweepDirection direction)
        {
            Point start = MakePoint(startDegrees, center, radius);//始点座標
            Point stop = MakePoint(stopDegrees, center, radius);//終点座標
            //開始角度から終了角度までが180度を超えているかの判定
            //超えていたらArcSegmentのIsLargeArcをtrue、なければfalseで作成
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDegrees - startDegrees : startDegrees - stopDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(radius, radius), 0, isLarge, direction, true);

            //ArcSegmentの終点から中心点への直線をPolyLineSegmentで作成
            var ll = new PolyLineSegment();
            ll.Points.Add(stop);
            ll.Points.Add(center);

            //ArcSegmentとPolyLineSegmentを繋げるPathFigure作成
            var fig = new PathFigure();
            fig.StartPoint = start;//始点座標
            fig.Segments.Add(arc);//ArcSegment追加
            fig.Segments.Add(ll);//PolyLine
            fig.IsClosed = true;//Pathを閉じる、これはなくてもいいのかも

            //PathGeometryを作成してPathFigureを追加して完成
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }


        //任意の角度開始、回転方向は固定時計回り
        //330~10の場合、330を0にして、そこから40度とかにしたい
        /// <summary>
        /// 扇(pie)型のPathGeometryを作成、回転方向は固定時計回り
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>        
        /// <param name="startDegrees">開始角度、0以上360以下で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360以下で指定</param>
        /// <returns></returns>
        private PathGeometry MakePieClip(Point center, double radius, double startDegrees, double stopDegrees)
        {
            Point start = MakePoint(startDegrees, center, radius);
            Point stop = MakePoint(stopDegrees, center, radius);
            double diffDegrees = stopDegrees - startDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(radius, radius), 0, isLarge, SweepDirection.Clockwise, true);

            var ll = new PolyLineSegment();
            ll.Points.Add(stop);
            ll.Points.Add(center);

            var fig = new PathFigure();
            fig.StartPoint = start;
            fig.Segments.Add(arc);
            fig.Segments.Add(ll);
            fig.IsClosed = true;//これはなくてもいいのかも
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }


        // 開始角度は0度で固定、開始位置は中心座標から右、回転は時計回りで固定
        /// <summary>
        /// 扇(pie)型のPathGeometryを作成、開始角度は0度で固定、開始位置は中心座標から右、回転は時計回りで固定
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        /// <param name="degrees">終了角度、0以上360以下で指定</param>
        /// <returns></returns>
        private PathGeometry MakePieClip(Point center, double radius, double degrees)
        {
            Point start = new Point(center.X + radius, center.Y);

            if (degrees >= 360) { degrees = 359.99; }
            if (degrees < 0) { degrees = 0; }
            var stop = MakePoint(degrees, center, radius);
            bool isLarge = (degrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(radius, radius), 0, isLarge, SweepDirection.Clockwise, true);

            var ll = new PolyLineSegment();
            ll.Points.Add(stop);
            ll.Points.Add(center);

            var fig = new PathFigure();
            fig.StartPoint = start;
            fig.Segments.Add(arc);
            fig.Segments.Add(ll);
            fig.IsClosed = true;//これはなくてもいいのかも
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }

        ////中心座標、半径、終了角度
        //private ArcSegment MakeArcSegment(Point center, double radius, double degrees)
        //{
        //    if (degrees >= 360) { degrees = 359.99; }
        //    bool isLarge = (degrees > 180) ? true : false;
        //    Point stop = MakePoint(degrees, center, radius);
        //    return new ArcSegment(stop, new Size(radius, radius), 0, isLarge, SweepDirection.Clockwise, true);
        //}

        private Point MakePoint(double degrees, Point center, double radius)
        {
            if (degrees >= 360) { degrees = 359.99; }
            var rad = Radian(degrees);
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);
            var x = radius + cos * center.X;
            var y = radius + sin * center.Y;
            return new Point(x, y);
        }

        private double Radian(double degree)
        {
            return Math.PI / 180.0 * degree;
        }



        //三角形clipをLineSegmentで作成
        private PathGeometry MakeTryangle()
        {
            var l1 = new LineSegment(new Point(200, 200), true);
            var l2 = new LineSegment(new Point(0, 200), true);
            var fig = new PathFigure();
            fig.StartPoint = new Point(100, 100);
            fig.Segments.Add(l1);
            fig.Segments.Add(l2);
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }
    }
}
