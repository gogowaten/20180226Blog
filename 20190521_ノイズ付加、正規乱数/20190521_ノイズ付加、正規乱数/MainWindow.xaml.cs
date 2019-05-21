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

namespace _20190521_ノイズ付加_正規乱数
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string ImageFileFullPath;//ファイルパス、画像保存時に使う
        BitmapSource MyBitmapOrigin;//元画像、リセット用
        byte[] MyPixelsOrigin;//元画像、リセット用
        byte[] MyPixels;//処理後画像

        public MainWindow()
        {
            InitializeComponent();

            this.Drop += MainWindow_Drop;
            this.AllowDrop = true;
            //MyTest();

        }



        private void MyTest()
        {
            //string filePath = "";
            ImageFileFullPath = @"E:\オレ\雑誌スキャン\2003年pc雑誌\20030115_dosvmag_003.jpg";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_114.jpg";
            //ImageFileFullPath = @" D:\ブログ用\テスト用画像\border_row.bmp";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_003_.png";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_003_重.png";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_003_重_上半分.png";
            //ImageFileFullPath = @"D:\ブログ用\Lenna_(test_image).png";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\蓮の花.png";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\SIDBA\Girl.bmp";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\とり.png";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\風車.png";


            (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Gray8, 96, 96);

            MyImageOrigin.Source = MyBitmapOrigin;
            MyPixelsOrigin = MyPixels;
        }

        private double[] MakeRandom12(int count)
        {
            Random r = new Random();
            double[] myR = new double[count];
            for (int i = 0; i < count; i++)
            {
                double rand = 0;
                for (int j = 0; j < 12; j++)
                {
                    rand += r.NextDouble();
                }
                myR[i] = rand - 6.0;
            }
            return myR;
        }

        private (double[], double[]) MakeRandomボックスミュラー法(int count)
        {
            double[] cosR = new double[count];
            double[] sinR = new double[count];
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                var X = random.NextDouble();
                var Y = random.NextDouble();
                cosR[i] = Math.Sqrt(-2 * Math.Log(X)) * Math.Cos(2 * Math.PI * Y);
                sinR[i] = Math.Sqrt(-2 * Math.Log(X)) * Math.Sin(2 * Math.PI * Y);
            }
            return (cosR, sinR);
        }


        /// <summary>
        /// 画像にノイズ付加
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="sigma">標準偏差、0でノイズ付加なし</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Addノイズ12(byte[] pixels, int width, int height, double sigma)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる

            var random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int p = y * stride + x;
                    double v = sigma * MakeRandom() + pixels[p];
                    v = v < 0 ? 0 : v > 255 ? 255 : v;
                    filtered[p] = (byte)v;

                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));

            //疑似正規乱数生成
            double MakeRandom()
            {
                double temp = 0;
                for (int i = 0; i < 12; i++)
                {
                    temp += random.NextDouble();
                }
                return temp - 6.0;
            }
        }


        private (byte[] pixels, BitmapSource bitmap) Addボックスミュラー法cos(byte[] pixels, int width, int height, double sigma)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる

            var random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int p = y * stride + x;
                    double v = sigma * MakeRandom() + pixels[p];
                    v = v < 0 ? 0 : v > 255 ? 255 : v;
                    filtered[p] = (byte)v;

                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));

            //乱数生成、ボックス=ミュラー法
            double MakeRandom()
            {
                var x = random.NextDouble();
                var y = random.NextDouble();

                return Math.Sqrt(-2 * Math.Log(x)) * Math.Cos(2 * Math.PI * y);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rate">ノイズの割合、0.0～1.0で指定</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Addごま塩ノイズ(byte[] pixels, int width, int height, double rate)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる

            var random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int p = y * stride + x;
                    filtered[p] = pixels[p];
                    if (random.NextDouble() < rate)
                    {
                        if (random.NextDouble() < 0.5)
                        {
                            filtered[p] = 0;
                        }
                        filtered[p] = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;
                    }
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));

            //乱数生成、ボックス=ミュラー法
            double MakeRandom()
            {
                var x = random.NextDouble();
                var y = random.NextDouble();

                return Math.Sqrt(-2 * Math.Log(x)) * Math.Cos(2 * Math.PI * y);
            }
        }




        private void Kakunin(byte[] pixels)
        {
            double average;//平均
            double total = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                total += pixels[i];
            }
            average = total / pixels.Length;
            double variance;//分散
            total = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                total += Math.Pow(pixels[i] - average, 2.0);
            }
            variance = total / pixels.Length;
            double stdev = Math.Sqrt(variance);//標準偏差
            MessageBox.Show($"平均＝{ average}\n分散＝{variance}\n標準偏差＝{ stdev}");
        }

        #region その他

        //画像ファイルドロップ時の処理
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
                MyPixels = pixels;
                MyPixelsOrigin = pixels;
                MyBitmapOrigin = bitmap;
                MyImage.Source = bitmap;
                MyImageOrigin.Source = bitmap;
                ImageFileFullPath = filePath[0];
            }
        }


        //画像の保存
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

        //表示画像リセット
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyImage.Source = MyBitmapOrigin;
            MyPixels = MyPixelsOrigin;
        }

        //画像保存
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) { return; }
            //BitmapSource source = (BitmapSource)MyImage.Source;
            //SaveImage(new FormatConvertedBitmap(source, PixelFormats.Indexed4, new BitmapPalette(source, 16), 0));
            //SaveImage(new FormatConvertedBitmap(source, PixelFormats.Indexed4, null, 0));
            SaveImage((BitmapSource)MyImage.Source);
        }

        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double[] neko = MakeRandom12(1000);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var neko = MakeRandomボックスミュラー法(1000);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addノイズ12(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            Kakunin(MyPixels);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addボックスミュラー法cos(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addごま塩ノイズ(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value / SliderSTDEV.Maximum);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
