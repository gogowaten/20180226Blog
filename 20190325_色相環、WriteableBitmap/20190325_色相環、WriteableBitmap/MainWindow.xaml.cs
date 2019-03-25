using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MyHSV;

namespace _20190325_色相環_WriteableBitmap
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            int width = 100;
            int height = 100;
            //赤Rect
            MyImage.Source = MakeRedRect(width, height);
            //色相環Rect
            MyImage2.Source = MakeHueRountRect(width, height);
            MyImage3.Source = MakeHueRountRect(width, height);
            MyImage4.Source = MakeHueRountRect(width, height);


            //切り抜いて円にする
            MyImage3.Clip = new EllipseGeometry(new Rect(0, 0, width, height));


            //切り抜いて円環にする、PathGeometry
            double ringWidth = 10;//リングの幅指定
            PathGeometry pg = new PathGeometry();
            //外側用Geometry作成追加
            pg.AddGeometry(new EllipseGeometry(new Rect(0, 0, width, height)));
            //内側用Geometry作成追加
            double xCenter = width / 2.0;//50
            double yCenter = height / 2.0;
            double xRadius = xCenter - ringWidth;//40
            double yRadius = yCenter - ringWidth;
            pg.AddGeometry(new EllipseGeometry(new Point(xCenter, yCenter), xRadius, yRadius));
            //切り抜き
            MyImage4.Clip = pg;

            //サイズ100ｘ100、リング幅10で決め打ち
            //PathGeometry pg = new PathGeometry();
            //pg.AddGeometry(new EllipseGeometry(new Rect(0, 0, width, height)));
            //pg.AddGeometry(new EllipseGeometry(new Point(50, 50), 40, 40));//決め打ち
            //MyImage4.Clip = pg;


        }

        /// <summary>
        /// pixelsFormats.Bgr24の赤一色のBitmap作成
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private BitmapSource MakeRedRect(int width, int height)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.BackBufferStride;//横一列のバイト数、24bit = 8byteに横ピクセル数をかけた値
            byte[] pixels = new byte[height * stride * 8];//*8はbyteをbitにするから
            for (int i = 0; i < pixels.Length; i += 3)//
            {
                pixels[i] = 255;//  red
                pixels[i + 1] = 0;//green
                pixels[i + 2] = 0;//blue
            }
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }

        /// <summary>
        /// pixelsFormats.Bgr24の色相環作成用のBitmap作成
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
    }
}
