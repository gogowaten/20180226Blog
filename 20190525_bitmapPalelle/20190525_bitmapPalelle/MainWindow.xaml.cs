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

//ピクセルフォーマットがindexed(パレット付き)の画像
//pngで保存はPngBitmapEncoder
//2色の画像をピクセルフォーマットindexed8で作成して
//pngで保存するとビットの深さ1のファイルが作成される
//確認するとパレットも作成されていたので、これはindexed1だと思う
//256ｘ256で8.35KB
//なのでわざわざindexed1の画像を作成しなくても良さそう


//indexed8以外のindexedの画像を作成するのは難しい
//indexed8は最大色数が256色、つまり1byte
//なので画像をbyte配列にしたときに、要素1つに付き1ピクセル
//対してindexed1だと要素1つに付き128ピクセルになる

//2色のRgb24からではindexed1にはならない
//普通のビットの深さ24のPngファイルが作成される
//256ｘ256で23.6KB

namespace _20190525_bitmapPalelle
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyOriginBitmapSource;
        string ImageFileFullPath;
        BitmapPalette MyOriginalPalette;

        public MainWindow()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            MyOriginBitmapSource = CreateGrayScaleBitmap();
            MyImageOrigin.Source = MyOriginBitmapSource;
        }

        //グレースケール画像作成
        private BitmapSource CreateGrayScaleBitmap()
        {
            int width = 32;
            int height = 256;
            int stride = 256;
            byte[] pixels = new byte[height * stride];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * stride + x] = (byte)y;
                }
            }
            var gray = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(gray, PixelFormats.Rgb24, null, 0);
            return convertedBitmap;
        }

        private BitmapSource Create2ColorsIndexed8Bitmap(Color c1, Color c2)
        {
            int width = 256;
            int height = 256;
            int stride = 256;
            byte[] pixels = new byte[height * stride];
            Random random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * stride + x] = (byte)random.Next(0, 2);//0 or 1
                }
            }
            List<Color> colors = new List<Color> { c1, c2 };
            BitmapPalette palette = new BitmapPalette(colors);
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Indexed8, palette, pixels, stride);
        }

        private BitmapSource Create4ColorsIndexed8Bitmap(Color c1, Color c2, Color c3, Color c4)
        {
            int width = 256;
            int height = 256;
            int stride = 256;
            byte[] pixels = new byte[height * stride];
            Random random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * stride + x] = (byte)random.Next(0, 4);//0 ~ 3
                }
            }
            List<Color> colors = new List<Color> { c1, c2, c3, c4 };
            BitmapPalette palette = new BitmapPalette(colors);
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Indexed8, palette, pixels, stride);
        }

        private BitmapSource Create2ColorsRgb24Bitmap(Color c1, Color c2)
        {
            int width = 256;
            int height = 256;
            int stride = width * 3;
            byte[] pixels = new byte[height * stride];
            Random random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x += 3)
                {
                    int p = y * stride + x;
                    pixels[p] = c1.R;
                    pixels[p + 1] = c1.G;
                    pixels[p + 2] = c1.B;
                    if (random.NextDouble() < 0.5)
                    {
                        pixels[p] = c2.R;
                        pixels[p + 1] = c2.G;
                        pixels[p + 2] = c2.B;

                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Rgb24, null, pixels, stride);
        }


        private void test2(BitmapSource source)
        {
            BitmapPalette palette = new BitmapPalette(source, 16);
            var cb = new FormatConvertedBitmap(source, PixelFormats.Indexed8, null, 0);

        }
        private void indexed8(BitmapSource source)
        {
            var cb = new FormatConvertedBitmap(source, PixelFormats.Indexed8, null, 0);
            MyImage.Source = cb;
            //誤差拡散になる
            //グレースケール画像は0-255までを等分割した17ごとの色になる
            //赤のSV画像は8ごとの色になる、153色になった、indexed8のmaxは256色
        }
        private void test()
        {
            MyImage.Source = MyImageOrigin.Source;
        }

        #region その他
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            var (pixels, bitmap, palette) = MakeBitmapSourceAndByteArrayAndPalette(filePath[0], PixelFormats.Rgb24, 96, 96);

            if (bitmap == null)
            {
                MessageBox.Show("画像ファイルじゃないみたい");
            }
            else
            {
                //MyPixels = pixels;
                //MyPixelsOrigin = pixels;

                ImageFileFullPath = filePath[0];
                MyOriginBitmapSource = bitmap;
                MyImageOrigin.Source = MyOriginBitmapSource;
                MyOriginalPalette = palette;
            }
        }

        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を取得とパレットを返す、ピクセルフォーマットを指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] array, BitmapSource source, BitmapPalette palette) MakeBitmapSourceAndByteArrayAndPalette(string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            byte[] pixels = null;
            BitmapSource source = null;
            BitmapPalette palette = null;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var bf = BitmapFrame.Create(fs);
                    palette = bf.Palette;

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
            return (pixels, source, palette);
        }


        //画像の保存
        private void SaveImage(BitmapSource source, string fullpath)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.tiff|*.tiff";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(fullpath) + "_";
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(fullpath);
            if (saveFileDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                if (saveFileDialog.FilterIndex == 1)
                {
                    encoder = new PngBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 2)
                {
                    encoder = new BmpBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 3)
                {
                    encoder = new TiffBitmapEncoder();
                }
                encoder.Frames.Add(BitmapFrame.Create(source));
                //encoder.Palette = source.Palette;

                using (var fs = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    encoder.Save(fs);
                }
            }
        }

        //画像クリックで元画像と処理後画像の切り替え
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int aa = Panel.GetZIndex(MyImage);
            Panel.SetZIndex(MyImageOrigin, aa + 1);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int aa = Panel.GetZIndex(MyImage);
            Panel.SetZIndex(MyImageOrigin, aa - 1);
        }

        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //画像保存
            if (MyImage.Source == null) { return; }
            SaveImage((BitmapSource)MyImage.Source, ImageFileFullPath);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            indexed8(MyOriginBitmapSource);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Create2ColorsIndexed8Bitmap(Colors.Cyan, Colors.Magenta);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Create4ColorsIndexed8Bitmap(Colors.Cyan, Colors.Magenta, Colors.Red, Colors.Yellow);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Create2ColorsRgb24Bitmap(Colors.Cyan, Colors.Magenta);
        }
    }
}
