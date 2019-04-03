using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
//c# - WPF Doughnut ProgressBar - Stack Overflow
//https://stackoverflow.com/questions/36752183/wpf-doughnut-progressbar
//WPFで図形の円弧☽🌛、🍕扇形パイ形、🍩ドーナツ型(アーチ形)を表示してみた、ArcSegment(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15922445.html

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

            double radian = 50;//半径 
            Point center = new Point(radian, radian);//中心点
            double distance = 50;//中心点からの距離
            PathGeometry arcPathGeo;//円弧のPathGeometry


            //      時計回りに0～250度の円弧
            //中心点座標が半径と同じ、中心からの距離も半径と同じ
            arcPathGeo = ArcGeometry(center, distance, 0, 250, SweepDirection.Clockwise);
            MyStackPanel1.Children.Add(MakePath(arcPathGeo));

            //中心点座標が半径と同じ、中心からの距離が25
            arcPathGeo = ArcGeometry(center, 25, 0, 250, SweepDirection.Clockwise);
            MyStackPanel1.Children.Add(MakePath(arcPathGeo));

            //中心点座標が(100,100)、中心からの距離は半径と同じ
            arcPathGeo = ArcGeometry(new Point(100, 100), distance, 0, 250, SweepDirection.Clockwise);
            MyStackPanel1.Children.Add(MakePath(arcPathGeo));

            //中心点座標が(100,100)、中心からの距離が25
            arcPathGeo = ArcGeometry(new Point(100, 100), 25, 0, 250, SweepDirection.Clockwise);
            MyStackPanel1.Children.Add(MakePath(arcPathGeo));


            //反時計回りに100～330度
            arcPathGeo = ArcGeometry(center, radian, 100, 330, SweepDirection.Counterclockwise);
            MyStackPanel1.Children.Add(MakePath(arcPathGeo));


            //扇形、🍕
            PathGeometry piePahtGeo;
            piePahtGeo = PieGeometry(center, distance, 330, 30, SweepDirection.Clockwise);
            MyStackPanel1.Children.Add(MakePath(piePahtGeo));

            piePahtGeo = PieGeometry(center, distance, 330, 30, SweepDirection.Counterclockwise);
            MyStackPanel1.Children.Add(MakePath(piePahtGeo));


            //🍩
            PathGeometry donut;
            donut = DonutGeometry(center, 20, distance, 20, 300, SweepDirection.Clockwise);
            MyStackPanel1.Children.Add(MakePath(donut));

            donut = DonutGeometry(center, 20, distance, 20, 300, SweepDirection.Counterclockwise);
            MyStackPanel1.Children.Add(MakePath(donut));


            ////パックマン
            //System.Windows.Controls.WrapPanel wrap = new System.Windows.Controls.WrapPanel();
            //MyStackPanel1.Children.Add(wrap);
            //Path pacman = new Path();
            //pacman.Data = PieGeometry(new Point(100, 100), 100, 30, 330, SweepDirection.Clockwise);
            //pacman.Fill = Brushes.Yellow;
            //wrap.Children.Add(pacman);
            //MyStackPanel1.Background = Brushes.Black;

            ////エサ
            //for (int i = 0; i < 3; i++)
            //{
            //    Path esa = new Path();
            //    esa.Data = new EllipseGeometry(new Rect(new Size(20, 20)));
            //    esa.Fill = Brushes.Yellow;
            //    esa.Margin = new Thickness(-20, 0, 100, 0);
            //    esa.VerticalAlignment = VerticalAlignment.Center;
            //    wrap.Children.Add(esa);
            //}

        }


        private Path MakePath(PathGeometry geo)
        {
            Path path;
            path = new Path
            {
                Data = geo,
                Stroke = Brushes.PaleVioletRed,
                StrokeThickness = 4,
                Margin = new Thickness(4)
            };
            return path;
        }

        /// <summary>
        /// ドーナツ形、アーチ形のPathGeometry作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="width">幅</param>
        /// <param name="distance">中心からの距離</param>
        /// <param name="startDeg">開始角度、0以上360未満</param>
        /// <param name="stopDeg">終了角度、0以上360未満</param>
        /// <param name="direction">回転方向、clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry DonutGeometry(Point center, double width, double distance, double startDeg, double stopDeg, SweepDirection direction)
        {
            //外側の円弧終始点
            Point outSideStart = MakePoint(startDeg, center, distance);
            Point outSideStop = MakePoint(stopDeg, center, distance);

            //内側の円弧終始点は角度と回転方向が外側とは逆になる
            Point inSideStart = MakePoint(stopDeg, center, distance - width);
            Point inSideStop = MakePoint(startDeg, center, distance - width);

            //開始角度から終了角度までが180度を超えているかの判定
            //超えていたらArcSegmentのIsLargeArcをtrue、なければfalseで作成
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDeg - startDeg : startDeg - stopDeg;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;

            //arcSegment作成
            var outSideArc = new ArcSegment(outSideStop, new Size(distance, distance), 0, isLarge, direction, true);
            //内側のarcSegmentは回転方向を逆で作成
            var inDirection = (direction == SweepDirection.Clockwise) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            var inSideArc = new ArcSegment(inSideStop, new Size(distance - width, distance - width), 0, isLarge, inDirection, true);

            //PathFigure作成、外側から内側で作成している
            //2つのarcSegmentは、2本の直線(LineSegment)で繋げる
            var fig = new PathFigure();
            fig.StartPoint = outSideStart;
            fig.Segments.Add(outSideArc);
            fig.Segments.Add(new LineSegment(inSideStart, true));//外側終点から内側始点への直線
            fig.Segments.Add(inSideArc);
            fig.Segments.Add(new LineSegment(outSideStart, true));//内側終点から外側始点への直線
            fig.IsClosed = true;//Pathを閉じる必須

            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }

        /// <summary>
        /// 円弧のPathGeometryを作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="distance">中心点からの距離</param>
        /// <param name="startDegrees">開始角度、0以上360未満で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360未満で指定</param>
        /// <param name="direction">回転方向、Clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry ArcGeometry(Point center, double distance, double startDegrees, double stopDegrees, SweepDirection direction)
        {
            Point stop = MakePoint(stopDegrees, center, distance);//終点座標

            //IsLargeの判定、
            //開始角度から終了角度までが180度を超えていたらtrue、なければfalse
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDegrees - startDegrees : startDegrees - stopDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;

            //ArcSegment作成
            var arc = new ArcSegment(stop, new Size(distance, distance), 0, isLarge, direction, true);

            //PathFigure作成
            var fig = new PathFigure();
            Point start = MakePoint(startDegrees, center, distance);//始点座標
            fig.StartPoint = start;//始点座標をスタート地点に
            fig.Segments.Add(arc);//ArcSegment追加

            //PathGeometry作成、PathFigure追加
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }



        //完成形、回転方向を指定できるように
        /// <summary>
        /// 扇(pie)型のPathGeometryを作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="distance">中心点からの距離</param>
        /// <param name="startDegrees">開始角度、0以上360未満で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360未満で指定</param>
        /// <param name="direction">回転方向、Clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry PieGeometry(Point center, double distance, double startDegrees, double stopDegrees, SweepDirection direction)
        {
            Point start = MakePoint(startDegrees, center, distance);//始点座標
            Point stop = MakePoint(stopDegrees, center, distance);//終点座標
            //開始角度から終了角度までが180度を超えているかの判定
            //超えていたらArcSegmentのIsLargeArcをtrue、なければfalseで作成
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDegrees - startDegrees : startDegrees - stopDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(distance, distance), 0, isLarge, direction, true);

            //PathFigure作成
            //ArcSegmentとその両端と中心点をつなぐ直線LineSegment
            var fig = new PathFigure();
            fig.StartPoint = start;//始点座標
            fig.Segments.Add(arc);//ArcSegment追加
            fig.Segments.Add(new LineSegment(center, true));//円弧の終点から中心への直線
            fig.Segments.Add(new LineSegment(start, true));//中心から円弧の始点への直線
            fig.IsClosed = true;//Pathを閉じる、必須

            //PathGeometryを作成してPathFigureを追加して完成
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }

        #region 作る過程でできたもの、未使用

        //任意の角度開始、回転方向は固定時計回り
        //330~10の場合、330を0にして、そこから40度とかにしたい
        /// <summary>
        /// 扇(pie)型のPathGeometryを作成、回転方向は固定時計回り
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="distance">中心点からの距離</param>
        /// <param name="startDegrees">開始角度、0以上360未満で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360未満で指定</param>
        /// <returns></returns>
        private PathGeometry PieGeometry(Point center, double distance, double startDegrees, double stopDegrees)
        {
            Point start = MakePoint(startDegrees, center, distance);
            Point stop = MakePoint(stopDegrees, center, distance);
            double diffDegrees = stopDegrees - startDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(distance, distance), 0, isLarge, SweepDirection.Clockwise, true);

            var ll = new PolyLineSegment();
            ll.Points.Add(stop);
            ll.Points.Add(center);

            var fig = new PathFigure();
            fig.StartPoint = start;
            fig.Segments.Add(arc);
            fig.Segments.Add(ll);
            fig.IsClosed = true;//線を閉じる
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }


        // 開始角度は0度で固定、開始位置は中心座標から右、回転は時計回りで固定
        /// <summary>
        /// 扇(pie)型のPathGeometryを作成、開始角度は0度で固定、開始位置は中心座標から右、回転は時計回りで固定
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="distance">中心点からの距離</param>
        /// <param name="degrees">終了角度、0以上360未満で指定</param>
        /// <returns></returns>
        private PathGeometry PieGeometry(Point center, double distance, double degrees)
        {
            Point start = new Point(center.X + distance, center.Y);

            if (degrees >= 360) { degrees = 359.99; }
            if (degrees < 0) { degrees = 0; }
            var stop = MakePoint(degrees, center, distance);
            bool isLarge = (degrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(distance, distance), 0, isLarge, SweepDirection.Clockwise, true);

            var ll = new PolyLineSegment();
            ll.Points.Add(stop);
            ll.Points.Add(center);

            var fig = new PathFigure();
            fig.StartPoint = start;
            fig.Segments.Add(arc);
            fig.Segments.Add(ll);
            fig.IsClosed = true;//線を閉じる
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }
        #endregion


        /// <summary>
        /// 距離と角度からその座標を返す
        /// </summary>
        /// <param name="degrees">360以上は359.99になる</param>
        /// <param name="center">中心点</param>
        /// <param name="distance">中心点からの距離</param>
        /// <returns></returns>
        private Point MakePoint(double degrees, Point center, double distance)
        {
            if (degrees >= 360) { degrees = 359.99; }
            var rad = Radian(degrees);
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);
            var x = center.X + cos * distance;
            var y = center.Y + sin * distance;
            return new Point(x, y);
        }

        private double Radian(double degree)
        {
            return Math.PI / 180.0 * degree;
        }

    }
}
