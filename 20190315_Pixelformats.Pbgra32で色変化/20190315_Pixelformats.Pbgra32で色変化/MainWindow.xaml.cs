using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _20190315_Pixelformats.Pbgra32で色変化
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            string imagePath = "";
            imagePath = @"D:\ブログ用\テスト用画像\半透明A100R30G122B224.png";


            byte[] pixels;

            BitmapSource bitmapPbgra32;
            (pixels, bitmapPbgra32) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Pbgra32);
            var Pbgra32 = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            MyImagePbgra32.Source = bitmapPbgra32;

            BitmapSource bitmapRgra32;
            (pixels, bitmapRgra32) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Bgra32);
            var Bgra32 = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            MyImageBgra32.Source = bitmapRgra32;

            BitmapSource source;
            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Bgr32);
            var Bgr32 = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);

            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Rgb24);
            var Rgb24 = Color.FromRgb(pixels[0], pixels[1], pixels[2]);

            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Bgr24);
            var Bgr24 = Color.FromRgb(pixels[2], pixels[1], pixels[0]);

            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
            var pf = bitmapImage.Format;//Bgra32
            int stride = pf.BitsPerPixel / 8 * bitmapImage.PixelWidth;
            pixels = new byte[stride * bitmapImage.PixelHeight];
            bitmapImage.CopyPixels(pixels, stride, 0);
            var auto = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);

        }



        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を取得、ピクセルフォーマットを指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] array, BitmapSource source) MakeByteArrayAndSourceFromImageFile(string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
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


    }
}
