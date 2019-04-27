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

namespace _20190427_ガウス関数
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
            //MakeKernel1zi(1, 5);
            MakeKernel2zi(1, 5);
        }



        //private void MyTest()
        //{
        //    //string filePath = "";
        //    ImageFileFullPath = @"E:\オレ\雑誌スキャン\2003年pc雑誌\20030115_dosvmag_003.jpg";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_114.jpg";
        //    //ImageFileFullPath = @" D:\ブログ用\テスト用画像\border_row.bmp";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_003_.png";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_003_重.png";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\20030115_dosvmag_003_重_上半分.png";
        //    //ImageFileFullPath = @"D:\ブログ用\Lenna_(test_image).png";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\蓮の花.png";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\SIDBA\Girl.bmp";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\とり.png";
        //    //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\風車.png";


        //    (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Gray8, 96, 96);

        //    MyImageOrigin.Source = MyBitmapOrigin;
        //    MyPixelsOrigin = MyPixels;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stdev">standard deviation標準偏差</param>
        private void MakeKernel1zi(double stdev, int kernelSize)
        {
            var variance = stdev * stdev;//分散
            var f = 1 / Math.Sqrt(2 * Math.PI * variance);//expの前            
            int length = kernelSize / 2;//0からの距離
            double[] kernel = new double[kernelSize];
            //確率密度関数(ガウス関数)
            for (int i = 0; i < kernelSize; i++)
            {
                var f2 = -(Math.Pow(i - length, 2) / (2 * variance));//expの指数
                var f3 = f * Math.Pow(Math.E, f2);//自然対数の底^指数
                kernel[i] = f3;
            }

            //最低値が1になるように調整してから四捨五入
            double min = kernel[0];//最小値
            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] /= min;
                kernel[i] = Math.Round(kernel[i], MidpointRounding.AwayFromZero);
            }
        }

        private void MakeKernel2zi(double stdev, int kernelSize)
        {
            var variance = stdev * stdev;//分散
            var f = 1 / Math.Sqrt(2 * Math.PI * variance);//expの前            
            int length = kernelSize / 2;//0からの距離
            double[] temp = new double[kernelSize];
            //確率密度関数(ガウス関数)
            for (int i = 0; i < kernelSize; i++)
            {
                var f2 = -(Math.Pow(i - length, 2) / (2 * variance));//expの指数
                var f3 = f * Math.Pow(Math.E, f2);//自然対数の底^指数
                temp[i] = f3;
            }

            //最低値が1になるように調整してから四捨五入
            double min = temp[0] * temp[0];//最小値
            double[,] kernel = new double[kernelSize, kernelSize];
            for (int i = 0; i < temp.Length; i++)
            {
                for (int j = 0; j < temp.Length; j++)
                {
                    kernel[i, j] = temp[i] * temp[j] / min;
                    kernel[i, j] = Math.Round(kernel[i, j], MidpointRounding.AwayFromZero);
                }
            }
            
        }




        private (byte[] pixels, BitmapSource bitmap) Filterガウシアンフィルタ(int[,] weight, byte[] pixels, int width, int height)
        {
            ////重み
            //int[][] weight = new int[][] {
            //    new int[] { 1, 2, 1 },
            //    new int[] { 2, 4, 2 },
            //    new int[] { 1, 2, 1 } };

            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //注目ピクセルの値と、その周囲8ピクセルに重みをかけた合計の平均値を新しい値にする
                    int total = 0;
                    for (int i = -1; i < 2; i++)
                    {
                        int p = x + ((y + i) * stride);//注目ピクセルの位置
                        for (int j = -1; j < 2; j++)
                        {
                            total += pixels[p + j] * weight[i + 1, j + 1];
                        }
                    }

                    int average = total / 16;
                    filtered[x + y * stride] = (byte)average;
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
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

        //ぼかしフィルタ処理
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            //(byte[] pixels, BitmapSource bitmap) = Filter近傍8平均(
            //    MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            //MyImage.Source = bitmap;
            //MyPixels = pixels;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            //(byte[] pixels, BitmapSource bitmap) = Filter近傍8平均補正あり(
            //    MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            //MyImage.Source = bitmap;
            //MyPixels = pixels;

        }


        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            //(byte[] pixels, BitmapSource bitmap) = Filterガウシアンフィルタ(
            //    MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            //MyImage.Source = bitmap;
            //MyPixels = pixels;

        }


        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            //(byte[] pixels, BitmapSource bitmap) = Filterガウシアンフィルタ補正あり(
            //    MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            //MyImage.Source = bitmap;
            //MyPixels = pixels;

        }
    }
}
