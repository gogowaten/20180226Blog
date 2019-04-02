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
using MyHSV;

namespace _20190402_色相円の棒グラフ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyTest();

        }

        private void MyTest()
        {
            double radius = 100;
            Point center = new Point(radius, radius);
            SweepDirection direction = SweepDirection.Clockwise;

            int side = (int)radius * 2;
            AddHueImage(side, null);

            PathGeometry clip1 = PieGeometry(center, radius, 0, 50, direction);
            AddHueImage(side, clip1);

            PathGeometry clip2 = PieGeometry(center, radius, 0, 50, direction);

        }

        private void AddHueImage(int side, PathGeometry clip)
        {
            Image img = new Image
            {
                Source = MakeHueRountRect(side, side),
                Clip = clip,
                Stretch = Stretch.None
            };

            MyWrapPanel.Children.Add(img);
        }

        #region PathGeometry
        /// <summary>
        /// 円弧のPathGeometryを作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="distance">中心点からの距離</param>        
        /// <param name="startDegrees">開始角度、0以上360以下で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360以下で指定</param>
        /// <param name="direction">回転方向、Clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry ArcPathGeometry(Point center, double distance, double startDegrees, double stopDegrees, SweepDirection direction)
        {
            Point start = MakePoint(startDegrees, center, distance);
            Point stop = MakePoint(stopDegrees, center, distance);
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDegrees - startDegrees : startDegrees - stopDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(distance, distance), 0, isLarge, direction, true);
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
        /// <param name="distance">中心点からの距離</param>        
        /// <param name="startDegrees">開始角度、0以上360以下で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360以下で指定</param>
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

            //ArcSegmentの終点から中心点への直線をPolyLineSegmentで作成
            var ll = new PolyLineSegment();
            ll.Points.Add(stop);
            ll.Points.Add(center);

            //ArcSegmentとPolyLineSegmentを繋げるPathFigure作成
            var fig = new PathFigure();
            fig.StartPoint = start;//始点座標は開始角度
            fig.Segments.Add(arc);//ArcSegment追加
            fig.Segments.Add(ll);//PolyLine
            fig.IsClosed = true;//Pathを閉じる

            //PathGeometryを作成してPathFigureを追加して完成
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }


        /// <summary>
        /// 指定角度、中心点、半径の座標を返す
        /// </summary>
        /// <param name="degrees">360以上は359.99になる</param>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
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

        #endregion

        #region 色相環
        /// <summary>
        /// pixelsFormats.Rgb24の色相環作成用のBitmap作成
        /// 右が赤、時計回り
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private BitmapSource MakeHueRountRect(int width, int height)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            //色情報用のバイト配列作成
            int stride = wb.BackBufferStride;//横一列のバイト数、24bit = 8byteに横ピクセル数をかけた値
            byte[] pixels = new byte[height * stride * 8];//*8はbyteをbitにするから

            //100ｘ100のとき中心は50，50
            //ピクセル位置と画像の中心との差
            int xDiff = width / 2;
            int yDiff = height / 2;
            int p = 0;//今のピクセル位置の配列での位置
            for (int y = 0; y < height; y++)//y座標
            {
                for (int x = 0; x < width; x++)//x座標
                {
                    //今の位置の角度を取得、これが色相になる
                    double radian = Math.Atan2(y - yDiff, x - xDiff);
                    double kakudo = Degrees(radian);
                    //色相をColorに変換
                    Color c = HSV.HSV2Color(kakudo, 1.0, 1.0);
                    //バイト配列に色情報を書き込み
                    p = y * stride + x * 3;
                    pixels[p] = c.R;
                    pixels[p + 1] = c.G;
                    pixels[p + 2] = c.B;
                }
            }
            //バイト配列をBitmapに書き込み
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }


        //ラジアンを0～360の角度に変換
        public double Degrees(double radian)
        {
            double deg = radian / Math.PI * 180;
            if (deg < 0) deg += 360;
            return deg;
        }
        #endregion


    }
}
