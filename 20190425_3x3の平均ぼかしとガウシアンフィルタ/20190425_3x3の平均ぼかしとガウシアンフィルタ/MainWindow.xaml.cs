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
//画像のぼかし処理、8近傍平均とガウスぼかし、グレースケールで処理(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15941552.html


namespace _20190425_3x3の平均ぼかしとガウシアンフィルタ
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
        /// ぼかしフィルタ、近傍8ピクセルとの平均値に変換、PixelFormat.Gray8専用
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filter近傍8平均(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //注目ピクセルの値と、その周囲8ピクセルの合計の平均値を新しい値にする
                    int total = 0;
                    int p = x + y * stride;//注目ピクセルの位置
                    total += pixels[p - stride - 1];//左上
                    total += pixels[p - stride];    //上
                    total += pixels[p - stride + 1];//右上
                    total += pixels[p - 1];     //左
                    total += pixels[p];         //注目ピクセル
                    total += pixels[p + 1];     //右
                    total += pixels[p + stride - 1];//左下
                    total += pixels[p + stride];    //下
                    total += pixels[p + stride + 1];//右下
                    int average = total / 9;
                    filtered[p] = (byte)average;
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }
        //↑と書き方が違うだけで同じ処理、未使用
        private (byte[] pixels, BitmapSource bitmap) Filter近傍8平均2(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //注目ピクセルの値と、その周囲8ピクセルの合計の平均値を新しい値にする
                    int total = 0;
                    for (int i = -1; i < 2; i++)
                    {
                        int p = x + ((y + i) * stride);//注目ピクセルの位置
                        for (int j = -1; j < 2; j++)
                        {
                            total += pixels[p + j];
                        }
                    }

                    int average = total / 9;
                    filtered[x + y * stride] = (byte)average;
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        //色補正あり版
        //誤差程度にきれい(正確)にぼやけるけど、処理時間は10倍以上かかる
        private (byte[] pixels, BitmapSource bitmap) Filter近傍8平均補正あり(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //注目ピクセルの値と、その周囲8ピクセルの2乗和の平均の平方根を新しい値にする
                    //((各値の2乗)/9)の平方根を新しい値にする
                    double total = 0;
                    int p = x + y * stride;//注目ピクセルの位置
                    total += Math.Pow(pixels[p - stride - 1], 2);//左上
                    total += Math.Pow(pixels[p - stride], 2);    //上
                    total += Math.Pow(pixels[p - stride + 1], 2);//右上
                    total += Math.Pow(pixels[p - 1], 2);     //左
                    total += Math.Pow(pixels[p], 2);         //注目ピクセル
                    total += Math.Pow(pixels[p + 1], 2);     //右
                    total += Math.Pow(pixels[p + stride - 1], 2);//左下
                    total += Math.Pow(pixels[p + stride], 2);    //下
                    total += Math.Pow(pixels[p + stride + 1], 2);//右下
                    double average = total / 9;
                    average = Math.Sqrt(average);
                    filtered[p] = (byte)average;
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }





        private (byte[] pixels, BitmapSource bitmap) Filterガウシアンフィルタ(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //注目ピクセルの値と、その周囲8ピクセルに重みをかけた合計の平均値を新しい値にする
                    int total = 0;
                    int p = x + y * stride;//注目ピクセルの位置
                    total += pixels[p - stride - 1];//左上
                    total += pixels[p - stride] * 2;//上
                    total += pixels[p - stride + 1];//右上
                    total += pixels[p - 1] * 2;     //左
                    total += pixels[p] * 4;         //注目ピクセル
                    total += pixels[p + 1] * 2;     //右
                    total += pixels[p + stride - 1];//左下
                    total += pixels[p + stride] * 2;//下
                    total += pixels[p + stride + 1];//右下
                    int average = total / 16;
                    filtered[p] = (byte)average;
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }
        //↑と書き方が違うだけで同じ処理、未使用
        private (byte[] pixels, BitmapSource bitmap) Filterガウシアンフィルタ2(byte[] pixels, int width, int height)
        {
            //重み
            int[][] weight = new int[][] {
                new int[] { 1, 2, 1 },
                new int[] { 2, 4, 2 },
                new int[] { 1, 2, 1 } };

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
                            total += pixels[p + j] * weight[i + 1][j + 1];
                        }
                    }

                    int average = total / 16;
                    filtered[x + y * stride] = (byte)average;
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        //色補正あり版
        //誤差程度にきれい(正確)にぼやけるけど、処理時間は10倍以上かかる

        private (byte[] pixels, BitmapSource bitmap) Filterガウシアンフィルタ補正あり(byte[] pixels, int width, int height)
        {
            //重み
            //1,2,1
            //2,4,2
            //1,2,1
            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //注目ピクセルの値と、その周囲8ピクセルの2乗和の平均の平方根を新しい値にする
                    //((各値の2乗*重み)/16)の平方根を新しい値にする
                    double total = 0;
                    int p = x + y * stride;//注目ピクセルの位置
                    total += Math.Pow(pixels[p - stride - 1], 2);//左上
                    total += Math.Pow(pixels[p - stride], 2) * 2;//上
                    total += Math.Pow(pixels[p - stride + 1], 2);//右上
                    total += Math.Pow(pixels[p - 1], 2) * 2;     //左
                    total += Math.Pow(pixels[p], 2) * 4;         //注目ピクセル
                    total += Math.Pow(pixels[p + 1], 2) * 2;     //右
                    total += Math.Pow(pixels[p + stride - 1], 2);//左下
                    total += Math.Pow(pixels[p + stride], 2) * 2;//下
                    total += Math.Pow(pixels[p + stride + 1], 2);//右下
                    double average = total / 16;
                    average = Math.Sqrt(average);
                    filtered[p] = (byte)average;
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
            (byte[] pixels, BitmapSource bitmap) = Filter近傍8平均(
                MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filter近傍8平均補正あり(
                MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }


        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterガウシアンフィルタ(
                MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }


        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterガウシアンフィルタ補正あり(
                MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }
    }
}
