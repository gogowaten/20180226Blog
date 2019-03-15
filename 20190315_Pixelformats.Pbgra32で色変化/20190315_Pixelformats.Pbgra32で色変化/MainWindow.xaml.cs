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
            imagePath = @"D:\ブログ用\テスト用画像\半透明2色.png";

            BitmapSource source;
            byte[] pixels;
            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Pbgra32, 96, 96);//半透明色が変化してしまう
            var Pbgra32 = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);

            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Bgra32, 96, 96);
            var Bgra32 = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);

            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Bgr32, 96, 96);
            var Bgr32 = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);

            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Rgb24, 96, 96);
            var Rgb24 = Color.FromRgb(pixels[0], pixels[1], pixels[2]);

            (pixels, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Bgr24, 96, 96);
            var Bgr24 = Color.FromRgb(pixels[2], pixels[1], pixels[0]);

            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
            int w = bitmapImage.PixelWidth;
            int h = bitmapImage.PixelHeight;
            var pf = bitmapImage.Format;//Bgra32
            int stride = pf.BitsPerPixel / 8 * w;
            pixels = new byte[stride * h];
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
