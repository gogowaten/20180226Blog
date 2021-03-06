﻿using System;
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
using System.Diagnostics;

//画像ぼかし処理、普通のぼかし処理では画像によってイマイチな結果になる(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15940003.html

namespace _20190422_ぼかし処理カラー
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string ImageFileFullPath;//ファイルパス、画像保存時に使う
        BitmapSource MyBitmapOrigin;//元画像、リセット用
        byte[] MyPixelsOrigin;//元画像のピクセルの色データ配列、リセット用
        byte[] MyPixels;//処理後画像のピクセルの色データ配列

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


        //    (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Rgb24, 96, 96);

        //    MyImageOrigin.Source = MyBitmapOrigin;
        //    MyPixelsOrigin = MyPixels;
        //}

        /// <summary>
        /// ぼかしフィルタ、上下左右との平均値に変換、PixelFormat.Rgb24
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filter上下左右(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width * 3;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //注目ピクセルの値と、その上下左右の合計の平均値を新しい値にする
                    int p = x * 3 + y * stride;//注目ピクセルの位置
                    //RGBそれぞれを処理
                    for (int i = 0; i < 3; i++)
                    {
                        filtered[p + i] = (byte)GetAverage(p + i);
                    }
                }
            }
            //注目ピクセルの値と、その上下左右の合計の平均値を新しい値にする
            int GetAverage(int p)
            {
                int total = 0;
                total += pixels[p - stride];//上
                total += pixels[p - 3];     //左
                total += pixels[p];         //注目ピクセル
                total += pixels[p + 3];     //右
                total += pixels[p + stride];//下
                return total / 5;
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }

        /// <summary>
        /// ぼかし処理、PixelFormats.Rgb24専用、色の補正をするのできれいなぼかしになるはず、そのぶん処理は重い
        /// </summary>
        /// <param name="pixels">ピクセルの色データ配列</param>
        /// <param name="width">画像の横ピクセル数</param>
        /// <param name="height">縦ピクセル数</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filter上下左右補正あり(byte[] pixels, int width, int height)
        {
            var sw = new Stopwatch();
            sw.Start();

            byte[] filtered = new byte[pixels.Length];//処理結果用
            int stride = width * 3;//一行のbyte数
            //上下左右1ラインは処理しない(めんどくさい)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = x * 3 + y * stride;//注目ピクセルの位置
                    //RGBそれぞれを処理
                    for (int i = 0; i < 3; i++)
                    {
                        filtered[p + i] = (byte)GetNewValue(p + i);
                    }
                }
            }

            //注目ピクセルの値と、その上下左右それぞれの2乗の合計の平均値の平方根を新しい値にする
            int GetNewValue(int p)
            {
                double total = 0;
                total += Math.Pow(pixels[p - stride], 2);//上
                total += Math.Pow(pixels[p - 3], 2);     //左
                total += Math.Pow(pixels[p], 2);         //注目ピクセル
                total += Math.Pow(pixels[p + 3], 2);     //右
                total += Math.Pow(pixels[p + stride], 2);//下
                //return MySqrtInt((int)(total / 5));// 1.6秒
                //return (int)MySqrt1(total / 5);//     1.4秒
                //return (int)MySqrt1(total / 5, 1);//  1.3秒
                return (int)Math.Sqrt(total / 5);//     1.1秒
            }

            sw.Stop();
            MyTextBlock.Text = $"time = {sw.Elapsed.TotalSeconds.ToString("0.000")}秒";

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }

        /// <summary>
        /// 平方根の近似値を求める
        /// </summary>
        /// <param name="v">平方根を求める対象の数値</param>
        /// <param name="acceptable">0より大きい数値で指定、誤差の許容値、これ以下になったら返す</param>
        /// <returns></returns>
        private double MySqrt1(double v, double acceptable = 0.0001)
        {
            double x = 127.5;//初期値
            double x2;
            while (true)
            {
                x2 = (x + (v / x)) / 2.0;
                if (Math.Abs(x2 - x) <= acceptable) break;
                x = x2;
            }
            return x2;
        }

        /// <summary>
        /// 整数だけで計算する平方根
        /// </summary>
        /// <param name="v">平方根を求める対象の数値</param>
        /// <param name="acceptable">1以上で指定、誤差の許容値、これ以下になったら返す</param>
        /// <returns></returns>
        private int MySqrtInt(int v, int acceptable = 1)
        {
            int x = 127;
            int x2;
            while (true)
            {
                x2 = (x + (v / x)) >> 1;//>> 1はビットシフト、意味は/2
                if (Math.Abs(x2 - x) <= acceptable) break;
                x = x2;
            }
            return x2;
        }

        #region その他

        //画像ファイルドロップ時の処理
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            var (pixels, bitmap) = MakeBitmapSourceAndByteArray(filePath[0], PixelFormats.Rgb24, 96, 96);

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

        //ぼかしフィルタ処理、補正なし
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filter上下左右(
                MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        //補正あり
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filter上下左右補正あり(
                MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
