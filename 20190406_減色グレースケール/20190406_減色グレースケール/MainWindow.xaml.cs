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

namespace _20190406_減色グレースケール
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string ImageFileFullPath;
        int ColorBit = 8;//1~8
        PixelFormat MyPixelFormat = PixelFormats.Indexed8;
        //BitmapSource MyBitmapGrayOrigin;
        BitmapSource MyBitmap;
        byte[] MyPixels;
        //BitmapPalette MyPalette;

        public MainWindow()
        {
            InitializeComponent();

            this.AllowDrop = true;
            KeyDown += MainWindow_KeyDown;
            Drop += MainWindow_Drop;
            MyTest(ColorBit);
        }

        private BitmapPalette MakePalette(int bit)
        {
            double rate = 255.0 / (Math.Pow(2, bit) - 1);
            int length = (int)Math.Pow(2, bit);
            var list = new List<Color>();
            byte value;
            for (int i = 0; i < length; i++)
            {
                value = (byte)(rate * i);
                list.Add(Color.FromRgb(value, value, value));
            }
            BitmapPalette palette = new BitmapPalette(list);
            return palette;
        }

        private BitmapPalette MakePalette2(int bit)
        {
            //1bit  2
            //2bit  4
            //3bit  8
            //4bit  16
            //5bit  32
            //6bit  64
            //7bit  128
            //8bit  256

            //index1    1bit
            //index2    2bit
            //index4    3~4bit
            //index8    5~8bit

            int div;
            if (bit == 1) { return null; }
            else if (bit == 2) { div = 2; }
            else div = (bit == 3 | bit == 4) ? 4 : 8;

            double rate = 255.0 / (Math.Pow(2, div) - 1);
            int length = (int)Math.Pow(2, div);
            var list = new List<Color>();
            byte value;
            for (int i = 0; i < length; i++)
            {
                value = (byte)(rate * i);
                list.Add(Color.FromRgb(value, value, value));
            }
            BitmapPalette palette = new BitmapPalette(list);
            return palette;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            var k = e.Key;
            var km = Keyboard.Modifiers;

            if ((km == ModifierKeys.Control) & (k == Key.S))
            {
                BitmapSource source = (BitmapSource)MyImage.Source;
                var aaa = new FormatConvertedBitmap(source, MyPixelFormat, MakePalette(ColorBit), 0);
                var bbb = new FormatConvertedBitmap(source, MyPixelFormat, MakePalette2(ColorBit), 0);

                var ddd = new FormatConvertedBitmap(source, MyPixelFormat, null, 0);

                var fff1 = new FormatConvertedBitmap(source, MyPixelFormat, new BitmapPalette(source, GetMaxColorCount()), 0);
                var fff2 = new FormatConvertedBitmap(source, MyPixelFormat, new BitmapPalette(source, (int)Math.Pow(2,ColorBit)), 0);

                int stride = source.PixelWidth;
                var vv = new byte[stride * source.PixelHeight];
                fff2.CopyPixels(vv, stride, 0);
                
                SaveImage(ddd);
            }

        }

        private void MyTest(int bit)
        {
            string filePath;
            filePath = @"D:\ブログ用\テスト用画像\NEC_8041_2017_05_09_午後わてん_96dpi.jpg";
            (MyPixels, MyBitmap) = MakeBitmapSourceAndByteArray(filePath, PixelFormats.Gray8, 96, 96);
            if (MyBitmap == null) { return; }

            ImageFileFullPath = filePath;
            MyImage.Source = ToReduceColor(MyPixels, ColorBit, MyBitmap);
        }

        private BitmapSource ToReduceColor(byte[] pixels, int bit, BitmapSource bitmap)
        {
            var pixelsNew = new byte[pixels.Length];
            var table = new byte[256];
            double step = 255.0 / (Math.Pow(2, bit) - 1);//1階調ぶんの値
            int shift = 8 - bit;
            for (int i = 0; i < 256; i++)
            {
                table[i] = (byte)((i >> shift) * step);
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                pixelsNew[i] = table[pixels[i]];
            }

            //SetPalette(ColorBit);
            //SetPalette2(ColorBit);

            var source = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, 96, 96, PixelFormats.Gray8, null, pixelsNew, bitmap.PixelWidth);
            return source;

        }



        #region

        private int GetMaxColorCount()
        {
            int count;
            if (MyPixelFormat == PixelFormats.BlackWhite | MyPixelFormat == PixelFormats.Indexed1) { count = 2; }
            else if (MyPixelFormat == PixelFormats.Indexed2) { count = 4; }
            else if (MyPixelFormat == PixelFormats.Indexed4) { count = 16; }
            else { count = 256; }
            return count;
        }
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            var (pixels, bitmap) = MakeBitmapSourceAndByteArray(filePath[0], PixelFormats.Gray8, 96, 96);

            if (bitmap == null)
            {
                MessageBox.Show("画像ファイルじゃないみたい");
            }
            else
            {
                ImageFileFullPath = filePath[0];
                MyPixels = pixels;
                MyBitmap = bitmap;
                MyImage.Source = ToReduceColor(pixels, ColorBit, bitmap);
            }
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

        private void SaveImage(BitmapSource source)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.tiff|*.tiff";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(ImageFileFullPath) + "_";
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(ImageFileFullPath);
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

                using (var fs = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    encoder.Save(fs);
                }
            }
        }
        #endregion

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton r = sender as RadioButton;
            ColorBit = int.Parse((string)r.Content);
            SetPixelFormat();
            //var neko = ToGray(MyPixels, ColorBit, MyBitmapSource);
            MyImage.Source = ToReduceColor(MyPixels, ColorBit, MyBitmap);
        }

        private void SetPixelFormat()
        {

            if (ColorBit == 1) { MyPixelFormat = PixelFormats.Indexed1; }
            else if (ColorBit == 2) { MyPixelFormat = PixelFormats.Indexed2; }
            else
            {
                MyPixelFormat = (ColorBit == 2) | (ColorBit == 3) ? PixelFormats.Indexed4 : PixelFormats.Indexed8;
            }
        }
    }
}
