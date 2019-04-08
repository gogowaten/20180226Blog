using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyHSV;

//画像の色相をバブルチャート風に表示するアプリ(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15926942.html

namespace _20190408_色相円の_グラフ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        const double MyRadius = 200.0;//色相画像半径
        Point MyCenter = new Point(MyRadius, MyRadius);//色相画像中心座標
        byte[] MyPixels;//画像のPixelの色情報、BitmapSourceのCopyPixelsより
        double[] MyHuesList;//全ピクセルの色相の配列
        double[] MyHuesListLimit;//全ピクセルの色相の配列(無彩色判定制限有り)
        int DivideCount = 120;//色相分割数
        //bool IsLimited;//制限有りのHueリストを使うかどうか

        public MainWindow()
        {
            InitializeComponent();

            this.AllowDrop = true;
            Drop += MainWindow_Drop;

            MyGrid.Children.Add(MakeAuxLine(MyCenter));
            MyHueImage.Source = MakeHueBitmap((int)(MyRadius * 2));
            MyHueImage.Clip = new RectangleGeometry(new Rect(0, 0, 0, 0));
            CheckBoxLimited.IsChecked = true;//初期は制限なしHueリストを使う

        }

        #region イベント
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            RadioButton rb = sender as RadioButton;
            int divCount = int.Parse((string)rb.Content);
            DivideCount = divCount;
            ChangeClip();//クリップ適用
            //MyHueImage.Clip = MakeClipEllipse(HuePixelCount(MyHuesList, divCount));
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            (byte[] pixels, BitmapSource bitmap) = MakeBitmapSourceAndByteArray(filePath[0], PixelFormats.Bgra32, 96, 96);

            if (bitmap == null)
            {
                MessageBox.Show("画像ファイルじゃないみたい");
            }
            else
            {
                MyPixels = pixels;
                //MyHuesList = GetHueList(pixels);
                //MyHuesListLimit = GetHueList(pixels, 0.05, 0.15);
                SetHueList(pixels, 0.05, 0.15);
                //var neko = HuePixelCount(MyHuesList, DivideCount);
                //MyHueImage.Clip = MakeClipEllipse(neko);
                MyImage.Source = bitmap;
                ChangeClip();//クリップ適用
            }
        }
        #endregion


        /// <summary>
        /// EllipseGeometryのクリップ作成
        /// </summary>
        /// <param name="hues">色相範囲ごとのピクセル数カウントした配列</param>
        /// <returns></returns>
        private Geometry MakeClipEllipse(int[] hues)
        {
            double max = hues.Max();
            //無彩色画像はmaxが0になっているはず、なので全クリップを返して終わり
            if (max == 0) { return new RectangleGeometry(new Rect(0, 0, 0, 0)); }

            Point center = new Point(MyRadius, MyRadius);
            double divDeg = 360.0 / hues.Length;//  1分割あたりの角度

            var clip = new PathGeometry();
            clip.FillRule = FillRule.Nonzero;
            //●の位置、中心から0.5～0.75の位置に配置
            double minDistance = MyRadius * 0.3725;//0.5
            double diffDistance = MyRadius * 0.8725 - minDistance;//0.75
            //●の面積、色相円の0.01～0.00001倍
            double maxArea = (Math.PI * MyRadius * MyRadius) * 0.01;
            double minArea = (Math.PI * MyRadius * MyRadius) * 0.00001;
            double diffArea = maxArea - minArea;

            //色相カウント数から●作成
            //配列のIndexが色相
            for (int i = 0; i < hues.Length; i++)
            {
                //ピクセル数が0ならパス
                if (hues[i] == 0) { continue; }

                //面積から半径を求める
                //面積=   パイ*半径^2
                //パイ*半径^2=  面積
                //半径^2= 面積/パイ
                //半径=   √(面積/パイ)

                //●の面積と、その半径
                double area = minArea + (diffArea * hues[i] / max);
                double radius = Math.Sqrt(area / Math.PI);
                //色相円中心からの距離、値が大きいほど遠くへ
                double distance = minDistance + (diffDistance * hues[i] / max);
                double degrees = i * divDeg;//●表示の角度
                Point clipCenter = MakePoint(degrees, center, distance);
                clip.AddGeometry(new EllipseGeometry(clipCenter, radius, radius));
            }
            return clip;
        }


        //補助線表示用のPath作成
        private Path MakeAuxLine(Point center)
        {

            var pg = new PathGeometry();
            pg.AddGeometry(new EllipseGeometry(center, center.X, center.Y));
            pg.AddGeometry(new EllipseGeometry(center, center.X * 3.0 / 4.0, center.Y * 3.0 / 4.0));
            pg.AddGeometry(new EllipseGeometry(center, center.X / 2.0, center.Y / 2.0));
            pg.AddGeometry(new EllipseGeometry(center, center.X / 4.0, center.Y / 4.0));
            var p = new Path();
            p.Stroke = Brushes.LightGray;
            p.StrokeThickness = 1.0;
            p.Opacity = 0.4;
            p.Data = pg;

            return p;
        }



        #region PathGeometry




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

        #endregion

        #region 色相環
        /// <summary>
        /// pixelsFormats.Rgb24の色相環作成用のBitmap作成
        /// 右が赤、時計回り
        /// </summary>
        /// <param name="size"></param>        
        /// <returns></returns>
        private BitmapSource MakeHueBitmap(int size)
        {
            var wb = new WriteableBitmap(size, size, 96, 96, PixelFormats.Rgb24, null);
            //色情報用のバイト配列作成
            int stride = wb.BackBufferStride;//横一列のバイト数、24bit = 8byteに横ピクセル数をかけた値
            byte[] pixels = new byte[size * stride * 8];//*8はbyteをbitにするから

            //100ｘ100のとき中心は50，50
            //ピクセル位置と画像の中心との差
            double xDiff = size / 2.0;
            double yDiff = size / 2.0;
            int p = 0;//今のピクセル位置の配列での位置
            for (int y = 0; y < size; y++)//y座標
            {
                for (int x = 0; x < size; x++)//x座標
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
            wb.WritePixels(new Int32Rect(0, 0, size, size), pixels, stride, 0);
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


        #region 画像系


        ////hueのリスト作成
        //private double[] GetHueList(byte[] pixels)
        //{
        //    double[] hueList = new double[pixels.Length / 4];
        //    int count = 0;
        //    for (int i = 0; i < pixels.Length; i += 4)
        //    {
        //        //ピクセルの色相取得
        //        hueList[count] = HSV.Color2HSV(pixels[i + 2], pixels[i + 1], pixels[i]).Hue;
        //        count++;
        //    }
        //    return hueList;
        //}

        ////hueのリスト作成、リミット付き
        ////彩度と明度が指定値以下なら無彩色判定        
        //private double[] GetHueList(byte[] pixels, double sLimit, double vLimit)
        //{
        //    double[] hueList = new double[pixels.Length / 4];
        //    int count = 0;
        //    for (int i = 0; i < pixels.Length; i += 4)
        //    {
        //        //ピクセルの色相取得
        //        HSV iHsv = HSV.Color2HSV(pixels[i + 2], pixels[i + 1], pixels[i]);
        //        var hue = (iHsv.Saturation < sLimit | iHsv.Value < vLimit) ? 360.0 : iHsv.Hue;
        //        hueList[count] = hue;
        //        count++;
        //    }
        //    return hueList;
        //}
        //hueのリスト作成、リミット付きは彩度と明度が指定値以下なら無彩色判定
        private void SetHueList(byte[] pixels,double sLimit,double vLimit)
        {
            MyHuesList = new double[pixels.Length / 4];
            MyHuesListLimit = new double[pixels.Length / 4];
            int count = 0;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                //ピクセルの色相取得
                HSV iHsv = HSV.Color2HSV(pixels[i + 2], pixels[i + 1], pixels[i]);
                MyHuesList[count] = iHsv.Hue;
                var hue = (iHsv.Saturation < sLimit | iHsv.Value < vLimit) ? 360.0 : iHsv.Hue;
                MyHuesListLimit[count] = hue;
                count++;
            }

        }

        /// <summary>
        /// GethueListから作成した色相の配列から色相の分割範囲ごとの数をカウント、
        /// 分割数divCountが4なら、360/4＝90度毎、範囲0(315~45)、範囲1(45~135)、範囲2(135~225)、範囲3(225~315)、
        /// 配列の「Index*360/分割数」が色相になる、4分割でIndex3なら、3*360/4=270、Index3の要素は色相270の範囲のもの
        /// <param name="hueList">色相の配列</param>
        /// <param name="divCount">分割数</param>
        /// <returns></returns>
        private int[] HuePixelCount(double[] hueList, int divCount)
        {
            int[] table = new int[divCount];
            double div = 360.0 / divCount;
            double divdiv = div / 2.0;
            for (int i = 0; i < hueList.Length; i++)
            {
                //ピクセルの色相取得
                double hue = hueList[i];
                if (hue == 360.0) { continue; }//色相360は無彩色なのでパス

                //色相の範囲ごとにカウント
                hue = Math.Floor((hue + divdiv) / div);
                hue = (hue >= divCount) ? 0 : hue;
                table[(int)hue]++;
            }
            return table;
        }



        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を取得、ピクセルフォーマットを指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] array, BitmapSource source) MakeBitmapSourceAndByteArray(string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            byte[] pixels = null;
            BitmapSource source = null;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var bf = BitmapFrame.Create(fs);

                    var convertedBitmap = new FormatConvertedBitmap(bf, pixelFormat, null, 0);
                    int w = convertedBitmap.PixelWidth;
                    int h = convertedBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    pixels = new byte[h * stride];
                    convertedBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = bf.DpiX; }
                    if (dpiY == 0) { dpiY = bf.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        convertedBitmap.Format,
                        convertedBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception)
            {
            }
            return (pixels, source);
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyHueImage.Clip = MakeClipEllipse(HuePixelCount(MyHuesListLimit, DivideCount));
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ChangeClip();
        }
        private void ChangeClip()
        {
            if (CheckBoxLimited.IsChecked==true)
            {
                MyHueImage.Clip = MakeClipEllipse(HuePixelCount(MyHuesListLimit, DivideCount));
            }
            else
            {
                MyHueImage.Clip = MakeClipEllipse(HuePixelCount(MyHuesList, DivideCount));
            }
        }
    }
}
