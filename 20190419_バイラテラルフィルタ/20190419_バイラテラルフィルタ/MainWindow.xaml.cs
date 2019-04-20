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

namespace _20190419_バイラテラルフィルタ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string ImageFileFullPath;
        BitmapSource MyBitmapOrigin;
        byte[] MyPixelsOrigin;//元画像、リセット用
        byte[] MyPixels;//

        public MainWindow()
        {
            InitializeComponent();

            this.Drop += MainWindow_Drop;
            MyTest();
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
            ImageFileFullPath = @"D:\ブログ用\テスト用画像\SIDBA\Girl.bmp";
            ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\とり.png";
            ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\風車.png";


            (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Gray8, 96, 96);

            MyImageOrigin.Source = MyBitmapOrigin;
            MyPixelsOrigin = MyPixels;
        }

        private (byte[] pixels, BitmapSource bitmap) Filter5x5(byte[] pixels, int width, int height, int[][] weight, int div)
        {
            if (div == 0) { return (pixels, MyBitmapOrigin); }
            byte[] filtered = new byte[pixels.Length];
            int p;
            //上下左右2ラインは処理しない
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    double v = 0.0;
                    p = x + y * width;
                    int pp;
                    for (int a = 0; a < 5; a++)
                    {
                        for (int b = 0; b < 5; b++)
                        {
                            pp = (x + b - 2) + ((y + a - 2) * width);
                            v += pixels[pp] * weight[a][b];
                        }
                    }

                    v /= div;
                    v = (v > 255) ? 255 : (v < 0) ? 0 : v;
                    filtered[p] = (byte)v;
                }
            }

            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixels, BitmapSource bitmap) Filter5x5バイラテラル偽(byte[] pixels, int width, int height, int[][] weight, int div)
        {
            if (div == 0) { return (pixels, MyBitmapOrigin); }
            byte[] filtered = new byte[pixels.Length];
            int p;
            byte pValue;
            byte ppValue;
            double diffValue;//輝度差

            //上下左右2ラインは処理しない
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    double v = 0.0;
                    p = x + y * width;
                    pValue = pixels[p];

                    int pp;
                    for (int a = 0; a < 5; a++)
                    {
                        for (int b = 0; b < 5; b++)
                        {
                            //中心との輝度差が少ないほど重みをつける
                            pp = (x + b - 2) + ((y + a - 2) * width);
                            ppValue = pixels[pp];
                            diffValue = Math.Abs(ppValue - pValue);
                            var weightValue = (255 - diffValue) / 255;//輝度差による重み0.0～1.0
                            v += ppValue * weight[a][b] * weightValue;
                        }
                    }

                    v /= div;
                    v = (v > 255) ? 255 : (v < 0) ? 0 : v;
                    filtered[p] = (byte)v;
                }
            }

            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixels, BitmapSource bitmap) Filter5x5バイラテラル(byte[] pixels, int width, int height, int[][] weight, int div)
        {
            if (div == 0) { return (pixels, MyBitmapOrigin); }
            byte[] filtered = new byte[pixels.Length];
            int p;
            byte pValue;//処理中の中心輝度
            byte ppValue;//5x5の処理中の輝度            
            const double SD = 0.5;//輝度差による重みに使う係数、大きくするとぼやける1.0だとガウシアンと同じくらいぼやける、0.5くらいがいい
            double exp2;
            //double nekoWeight;
            int totalValue;//5x5ピクセルの合計輝度
            double pNormValue, ppNormValue;//正規化した輝度
            //上下左右2ラインは処理しない
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    double v = 0.0;
                    p = x + y * width;

                    (double[][] nekow, double nekodiv) = GetWeightAndDiv2(SD, weight, pixels, width, p);
                    int pp;
                    for (int a = 0; a < 5; a++)
                    {
                        for (int b = 0; b < 5; b++)
                        {
                            pp = (x + b - 2) + ((y + a - 2) * width);
                            ppValue = pixels[pp];
                            v += ppValue * nekow[a][b];
                            
                        }
                    }

                    v /= nekodiv;
                    v = (v > 255) ? 255 : (v < 0) ? 0 : v;
                    filtered[p] = (byte)v;
                }
            }

            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        private (double[][], double) GetWeightAndDiv2(double SD, int[][] gaussian, byte[] pixels, int stride, int p)
        {
            double[][] weight = new double[5][];//入れ物
            //5x5ピクセルの値とその合計を取得
            (byte[][] range, int totalValue) = Get5x5PixlesAndTolal(pixels, stride, p);

            double pValue = range[2][2] / 255.0;//中心の値
            double div = 0;
            double exp2, rate;
            for (int y = 0; y < 5; y++)
            {
                weight[y] = new double[5];
                for (int x = 0; x < 5; x++)
                {
                    exp2 = -(Math.Pow(pValue - (range[y][x] / 255.0), 2)
                        / (2 * SD * SD));
                    rate = Math.Pow(totalValue, exp2) * gaussian[y][x];
                    weight[y][x] = rate;
                    div += rate;

                    double a1 = pValue - range[y][x];
                    double a2 = Math.Pow(a1, 2);
                    double a3 = 2 * Math.Pow(SD, 2);
                    double a4 = -(a2 / a3);
                }
            }
            return (weight, div);
        }


        private (byte[] pixels, BitmapSource bitmap) Filter5x5バイラテラル2(byte[] pixels, int width, int height, int[][] weight, int div)
        {
            //上の2つをまとめたもの

            if (div == 0) { return (pixels, MyBitmapOrigin); }
            byte[] filtered = new byte[pixels.Length];
            int p;
            byte pValue;//処理中の中心輝度
            byte ppValue;//5x5の処理中の輝度            
            const double SD = 0.5;//輝度差による重みに使う係数、大きくするとぼやける1.0だとガウシアンと同じくらいぼやける、0.5くらいがいい
            double exp2;
            //double nekoWeight;
            int totalValue;//5x5ピクセルの合計輝度
            double pNormValue, ppNormValue;//正規化した輝度
            double nekoWeight;//新しいウェイト
            byte[][] range;
            //上下左右2ラインは処理しない
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    double v = 0.0;
                    p = x + y * width;
                    //5x5ピクセルの値とその合計を取得
                    (range, totalValue) = Get5x5PixlesAndTolal(pixels, width, p);
                    pValue = pixels[p];
                    int pp;
                    for (int a = 0; a < 5; a++)
                    {
                        for (int b = 0; b < 5; b++)
                        {
                            exp2 = -(Math.Pow(pValue - (range[a][b] / 255.0), 2.0)
                                / (2 * SD * SD));
                            var rate = Math.Pow(totalValue, exp2) * weight[a][b];


                            pp = (x + b - 2) + ((y + a - 2) * width);
                            ppValue = pixels[pp];
                            //v += ppValue * nekow[a][b];

                        }
                    }

                    //v /= nekodiv;
                    v = (v > 255) ? 255 : (v < 0) ? 0 : v;
                    filtered[p] = (byte)v;
                }
            }

            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        //指定したピクセルを中心にした5x5ピクセルの値とその合計を返す
        private (byte[][], int) Get5x5PixlesAndTolal(byte[] pixels, int stride, int p)
        {
            byte[][] vs = new byte[5][];
            int total = 0;
            for (int y = 0; y < 5; y++)
            {
                int yy = (y - 2) * stride;
                vs[y] = new byte[5];
                for (int x = 0; x < 5; x++)
                {
                    vs[y][x] = pixels[p + yy + x - 2];
                    total += pixels[p + yy + x - 2];
                }
            }
            return (vs, total);
        }




        #region その他

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
                MyBitmapOrigin = bitmap;
                MyImage.Source = MyBitmapOrigin;
                MyImageOrigin.Source = MyBitmapOrigin;
                MyPixelsOrigin = MyPixels;
            }
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



        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyImage.Source = MyBitmapOrigin;
            MyPixels = MyPixelsOrigin;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            BitmapSource source = (BitmapSource)MyImage.Source;
            //SaveImage(new FormatConvertedBitmap(source, PixelFormats.Indexed4, new BitmapPalette(source, 16), 0));
            //SaveImage(new FormatConvertedBitmap(source, PixelFormats.Indexed4, null, 0));
            SaveImage((BitmapSource)MyImage.Source);
        }

        private void Button_Click_バイラテラル(object sender, RoutedEventArgs e)
        {
            int[][] weight = new int[][] { new int[] { 1, 4, 6, 4, 1 }, new int[] { 4, 16, 24, 16, 4 }, new int[] { 6, 24, 36, 24, 6 }, new int[] { 4, 16, 24, 16, 4 }, new int[] { 1, 4, 6, 4, 1 } };
            (byte[] pixels, BitmapSource bitmap) = Filter5x5バイラテラル(MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight, weight, 256);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_ガウシアン(object sender, RoutedEventArgs e)
        {
            int[][] weight = new int[][] { new int[] { 1, 4, 6, 4, 1 }, new int[] { 4, 16, 24, 16, 4 }, new int[] { 6, 24, 36, 24, 6 }, new int[] { 4, 16, 24, 16, 4 }, new int[] { 1, 4, 6, 4, 1 } };
            (byte[] pixels, BitmapSource bitmap) = Filter5x5(MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight, weight, 256);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
