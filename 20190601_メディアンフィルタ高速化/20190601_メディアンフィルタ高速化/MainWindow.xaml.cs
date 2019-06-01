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
using System.Diagnostics;

//https://www.ipsj.or.jp/award/9faeag0000004f1r-att/LI_9.pdf
//３×３メディアンフィルタの高速アルゴリズム
//A Fast Algorithm for 3x3 Median Filtering
//浜村倫行† 　入江文平†
//            参照渡し - C# によるプログラミング入門 | ++C++; // 未確認飛行 C
//https://ufcpp.net/study/csharp/sp_ref.html
namespace _20190601_メディアンフィルタ高速化
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
        }


        /// <summary>
        /// 3x3byteの配列の並べ替え、3つごとで並べ替える
        ///  0  1  2    ←列番号
        /// [0][3][6]   []はインデックス
        /// [1][4][7]
        /// [2][5][8]
        /// この配置で見立てて、列の中を並べ替える
        /// </summary>
        /// <param name="pixels"></param>
        private void MedianSort3x3Byte(ref byte[] pixels)
        {
            byte temp;
            for (int i = 0; i < 9; i += 3)
            {
                if (pixels[i] >= pixels[i + 1])
                {
                    // 0 1

                    if (pixels[i + 1] >= pixels[i + 2])
                    {
                        // 0 1 2
                        //順番そのまま
                    }
                    else if (pixels[i + 0] >= pixels[i + 2])// 0 >= 1 and 2 >= 1
                    {
                        // 0 2 1
                        temp = pixels[i + 1];
                        pixels[i + 1] = pixels[i + 2];
                        pixels[i + 2] = temp;
                    }
                    else
                    {
                        // 2 0 1
                        temp = pixels[i + 2];
                        pixels[i + 2] = pixels[i + 1];
                        pixels[i + 1] = pixels[i];
                        pixels[i] = temp;
                    }
                }
                else
                {
                    // 1 0
                    if (pixels[i + 2] >= pixels[i + 1])
                    {
                        // 2 1 0
                        temp = pixels[i];
                        pixels[i] = pixels[i + 2];
                        pixels[i + 2] = temp;
                    }
                    else if (pixels[i + 0] >= pixels[i + 2])// 1 0 and 1 2
                    {
                        // 1 0 2
                        temp = pixels[i];
                        pixels[i] = pixels[i + 1];
                        pixels[i + 1] = temp;
                    }
                    else
                    {
                        // 1 2 0
                        temp = pixels[i];
                        pixels[i] = pixels[i + 1];
                        pixels[i + 1] = pixels[i + 2];
                        pixels[i + 2] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// 中央値を取得
        /// </summary>
        /// <param name="v">並べ替え済みの配列</param>
        /// <returns></returns>
        private byte MedianFind(byte[] v)
        {
            if ((v[2] >= v[4] && v[4] >= v[6]) || (v[6] >= v[4] && v[4] >= v[2]))
            {
                return v[4];
            }
            else if (v[2] >= v[4] && v[6] >= v[4])
            {
                return Math.Min(v[2], Math.Min(v[3], v[6]));
            }
            else
            {
                return Math.Max(v[2], Math.Max(v[5], v[6]));
            }
        }


        /// <summary>
        /// BitmapSourcクラスのCopyPixelsで得られるbyte型配列に
        /// メディアンフィルタをかける
        /// ピクセルフォーマットGray8(グレースケール画像)専用
        /// </summary>
        /// <param name="pixels">画像の輝度値配列</param>
        /// <param name="width">画像の横ピクセル数</param>
        /// <param name="height">画像の縦ピクセル数</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化1(
            byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            //一行のbyte数、Gray8は1ピクセルあたり1byteなのでwidthとおなじ
            int stride = width;
            byte[] v = new byte[9];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = x + y * stride;//注目ピクセルの位置
                    v[0] = pixels[p - stride - 1];//注目ピクセルの左上
                    v[1] = pixels[p - 1];//左
                    v[2] = pixels[p + stride - 1];//左下
                    v[3] = pixels[p - stride];//上
                    v[4] = pixels[p];
                    v[5] = pixels[p + stride];//下
                    v[6] = pixels[p - stride + 1];//右上
                    v[7] = pixels[p + 1];//右
                    v[8] = pixels[p + stride + 1];//右下

                    //ソートして中央の値(5番目)を新しい値にする
                    //数値ソート
                    MedianSort3x3Byte(ref v);
                    //MedianSortByte(ref v, 0);//これより↑のほうが速い
                    //MedianSortByte(ref v, 3);
                    //MedianSortByte(ref v, 6);

                    //unitソート
                    MedianSortUnit(ref v);
                    //中央値取得して新しい値にする
                    filtered[p] = MedianFind(v);

                    //LINQなら1行で済むけど、5～10倍時間がかかる、遅すぎ
                    //filtered[p] = v.OrderBy(z => z).ToList()[4];
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }





        /// <summary>
        /// 並べ替え、配列の中の3つを大きい順にする
        /// </summary>
        /// <param name="v">配列</param>
        /// <param name="begin">開始インデックス、この番号から3つ分を並べ替える</param>
        private void MedianSortByte(ref byte[] v, int begin)
        {
            byte temp;
            if (v[begin] >= v[begin + 1])
            {
                // 0 1
                if (v[begin + 1] >= v[begin + 2])
                {
                    // 0 1 2
                    //順番そのまま
                }
                else if (v[begin + 0] >= v[begin + 2])// 0 >= 1 and 2 >= 1
                {
                    // 0 2 1
                    temp = v[begin + 1];
                    v[begin + 1] = v[begin + 2];
                    v[begin + 2] = temp;
                }
                else
                {
                    // 2 0 1
                    temp = v[begin + 2];
                    v[begin + 2] = v[begin + 1];
                    v[begin + 1] = v[begin];
                    v[begin] = temp;
                }
            }
            else
            {
                // 1 0
                if (v[begin + 2] >= v[begin + 1])
                {
                    // 2 1 0
                    temp = v[begin];
                    v[begin] = v[begin + 2];
                    v[begin + 2] = temp;
                }
                else if (v[begin + 0] >= v[begin + 2])// 1 0 and 1 2
                {
                    // 1 0 2
                    temp = v[begin];
                    v[begin] = v[begin + 1];
                    v[begin + 1] = temp;
                }
                else
                {
                    // 1 2 0
                    temp = v[begin];
                    v[begin] = v[begin + 1];
                    v[begin + 1] = v[begin + 2];
                    v[begin + 2] = temp;
                }
            }
        }


        /// <summary>
        ///  0  1  2    ←列番号
        /// [0][3][6]
        /// [1][4][7]
        /// [2][5][8]
        /// この配置で列を並べ替え、左から大きい順
        /// インデックス[1][4][7]を比較して並べ替え
        /// </summary>
        /// <param name="v"></param>
        private void MedianSortUnit(ref byte[] v)
        {
            byte t1, t2, t3;
            if (v[1] >= v[4])
            {
                // 0 > 1
                if (v[4] >= v[7])
                {
                    // 0 1 2 入れ替えなし
                }
                else if (v[1] >= v[7])
                {
                    // 0 2 1 は1と2を入れ替え
                    t1 = v[3];
                    t2 = v[4];
                    t3 = v[5];
                    v[3] = v[6];
                    v[4] = v[7];
                    v[5] = v[8];
                    v[6] = t1;
                    v[7] = t2;
                    v[8] = t3;
                }
                else
                {
                    // 2 0 1
                    t1 = v[3];
                    t2 = v[4];
                    t3 = v[5];
                    v[3] = v[0];
                    v[4] = v[1];
                    v[5] = v[2];
                    v[0] = v[6];
                    v[1] = v[7];
                    v[2] = v[8];
                    v[6] = t1;
                    v[7] = t2;
                    v[8] = t3;
                }
            }
            else
            {
                // 1 > 0
                if (v[7] >= v[4])
                {
                    // 2 1 0
                    t1 = v[0];
                    t2 = v[1];
                    t3 = v[2];
                    v[0] = v[6];
                    v[1] = v[7];
                    v[2] = v[8];
                    v[6] = t1;
                    v[7] = t2;
                    v[8] = t3;
                }
                else if (v[1] >= v[7])
                {
                    // 1 0 2
                    t1 = v[3];
                    t2 = v[4];
                    t3 = v[5];
                    v[3] = v[0];
                    v[4] = v[1];
                    v[5] = v[2];
                    v[0] = t1;
                    v[1] = t2;
                    v[2] = t3;
                }
                else
                {
                    // 1 2 0
                    t1 = v[3];
                    t2 = v[4];
                    t3 = v[5];
                    v[3] = v[6];
                    v[4] = v[7];
                    v[5] = v[8];
                    v[6] = v[0];
                    v[7] = v[1];
                    v[8] = v[2];
                    v[0] = t1;
                    v[1] = t2;
                    v[2] = t3;
                }
            }
        }


        //ようやくできた！！！！！！
        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化5(
            byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数
            byte[] v = new byte[9];//3x3、自身と周囲8近傍用
            byte t3, t4, t5, t6, t7, t8;//一時記録用

            for (int y = 1; y < height - 1; y++)
            {
                //今の行(y)の左端とその上下の値を配列の中央列に入れてソート
                int pp = stride * y;//今の行の左端のインデックス
                v[3] = pixels[pp - stride];//左端の一行上
                v[4] = pixels[pp];//左端
                v[5] = pixels[pp + stride];//左端の下
                MedianSortByte(ref v, 3);//ソート
                //今の行(y)の2列目とその上下の値を配列の右列に入れてソート
                v[6] = pixels[pp - stride + 1];//今の行の左端の一行上の右
                v[7] = pixels[pp + 1];
                v[8] = pixels[pp + stride + 1];
                MedianSortByte(ref v, 6);

                //ソートの結果を値コピー
                t3 = v[3];
                t4 = v[4];
                t5 = v[5];
                t6 = v[6];
                t7 = v[7];
                t8 = v[8];

                for (int x = 1; x < width - 1; x++)
                {
                    //中央列と右列を左側にシフト
                    v[0] = t3;
                    v[1] = t4;
                    v[2] = t5;
                    v[3] = t6;
                    v[4] = t7;
                    v[5] = t8;

                    int p = x + y * stride;//注目ピクセルの位置
                    //右列
                    v[6] = pixels[p - stride + 1];
                    v[7] = pixels[p + 1];
                    v[8] = pixels[p + stride + 1];
                    MedianSortByte(ref v, 6);//byte sort

                    //中央列と右列は次回に使うので記録
                    t3 = v[3];
                    t4 = v[4];
                    t5 = v[5];
                    t6 = v[6];
                    t7 = v[7];
                    t8 = v[8];

                    MedianSortUnit(ref v);//unit sort
                    //中央値取得、設定                    
                    filtered[p] = MedianFind(v);
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }



        /// <summary>
        /// ピクセルフォーマットGray8専用
        /// </summary>
        /// <param name="pixels">ピクセルの値の配列</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filterメディアン(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            byte[] v = new byte[9];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = x + y * stride;//注目ピクセルの位置
                    v[0] = pixels[p - stride - 1];//注目ピクセルの左上
                    v[1] = pixels[p - stride];//上
                    v[2] = pixels[p - stride + 1];//右上
                    v[3] = pixels[p - 1];//左
                    v[4] = pixels[p];
                    v[5] = pixels[p + 1];//右
                    v[6] = pixels[p + stride - 1];//左下
                    v[7] = pixels[p + stride];//下
                    v[8] = pixels[p + stride + 1];//右下
                    //ソートして中央の値(5番目)を新しい値にする
                    filtered[p] = v.OrderBy(z => z).ToList()[4];
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //クリップボードから画像を貼り付け
            var source = Clipboard.GetImage();
            if (source == null)
            {
                MessageBox.Show("(クリップボードに画像は)ないです");
            }
            else
            {
                //クリップボードに画像があったらピクセルフォーマットをGray8に変換して取り込む
                int w = source.PixelWidth;
                int h = source.PixelHeight;
                int stride = w;
                var gray = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
                byte[] pixels = new byte[h * stride];
                gray.CopyPixels(pixels, stride, 0);
                MyPixels = pixels;
                MyPixelsOrigin = pixels;
                MyBitmapOrigin = gray;
                MyImage.Source = gray;
                MyImageOrigin.Source = gray;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //クリップボードに画像をコピー
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            var sw = new Stopwatch();
            sw.Start();
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            sw.Stop();
            MyTextBlock1.Text = $"処理時間 = {sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }


        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            var sw = new Stopwatch();
            sw.Start();
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン高速化1(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            sw.Stop();
            MyTextBlock2.Text = $"処理時間 = {sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            var sw = new Stopwatch();
            sw.Start();
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン高速化5(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            sw.Stop();
            MyTextBlock3.Text = $"処理時間 = {sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
