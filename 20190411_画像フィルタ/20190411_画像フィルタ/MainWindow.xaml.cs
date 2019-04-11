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

namespace _20190411_画像フィルタ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] MyPixelsOrigin;
        byte[] MyPixels;
        BitmapSource MyBitmap;

        public MainWindow()
        {
            InitializeComponent();
            AllowDrop = true;
            Drop += MainWindow_Drop;

            MyTest();
        }

        private void MyTest()
        {
            string filePath = "";
            filePath = @"E:\オレ\雑誌スキャン\2003年pc雑誌\20030115_dosvmag_003.jpg";
            //filePath = @" D:\ブログ用\テスト用画像\border_row.bmp";
            (MyPixels, MyBitmap) = MakeBitmapSourceAndByteArray(filePath, PixelFormats.Gray8, 96, 96);

            MyImageOrigin.Source = MyBitmap;
            MyPixelsOrigin = MyPixels;
        }

        private (byte[] pixels, BitmapSource bitmap) Filter(byte[] pixels, int width, int height, int[][] weight, int div, int offset)
        {
            //int[][] weight = new int[][] {
            //    new int[] { 0, 1, 0 },
            //    new int[] { 1, 1, 1 },
            //    new int[] { 0, 1, 0 } };
            //int offset = 0;
            //int div = 5;
            byte[] filtered = new byte[pixels.Length];
            int p;
            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double v = 0.0;
                    p = x + y * width;
                    int pp;
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            pp = (x + b - 1) + ((y + a - 1) * width);
                            v += pixels[pp] * weight[a][b];
                        }
                    }
                    v /= div;
                    v += offset;
                    v = (v > 255) ? 255 : (v < 0) ? 0 : v;
                    filtered[p] = (byte)v;
                }
            }

            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }
        //速度は↑より誤差程度遅いので、わかりやすい↑を使ったほうがいい
        private (byte[] pixesl, BitmapSource bitmap) Filter2(byte[] pixels, int width, int height, int[][] weight, int div, int offset)
        {
            byte[] filtered = new byte[pixels.Length];
            int stride = 1 * width;

            //めんどくさいので上下左右1ピクセルは処理しない            
            for (int i = stride + 1; i < pixels.Length - stride - 1; i++)
            {
                //右端ならcount＋2して次へ
                int mod = i % stride;
                if (mod == width - 1) { i += 2; continue; }

                double v = 0.0;
                int pp;
                for (int a = -1; a < 2; a++)
                {
                    for (int b = -1; b < 2; b++)
                    {
                        pp = i + b + (a * stride);
                        v += pixels[pp] * weight[a + 1][b + 1];
                    }
                }
                v /= div;
                v += offset;
                v = (v > 255) ? 255 : (v < 0) ? 0 : v;
                filtered[i] = (byte)v;

            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));

        }

        //メディアンフィルタ、自身と周辺8画素の中央値をそのまま適用
        //ノイズ除去の効果があるけど、かなりぼやける
        //処理時間はぼかしの10倍
        private (byte[] pixesl, BitmapSource bitmap) MedianFilter(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];
            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int pp;
                    List<byte> sort = new List<byte>();
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            pp = (x + b - 1) + ((y + a - 1) * width);
                            sort.Add(pixels[pp]);
                        }
                    }
                    //ソートして中央値を採用
                    var sorted = sort.OrderBy(z => z);
                    filtered[x + y * width] = sorted.ToList()[4];
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixesl, BitmapSource bitmap) TestFilter(byte[] pixels, int width, int height, byte threshold)
        {
            //ノイズ除去のつもりだったけどぼかし効果になった
            //自身と周辺8画素を比較して輝度が10以上離れていたら、周辺8画素の平均輝度に変更する
            //byte threshold = 10;//小さいほどぼやける
            byte[] filtered = new byte[pixels.Length];
            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int pp;
                    List<byte> around = new List<byte>();
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            pp = (x + b - 1) + ((y + a - 1) * width);
                            around.Add(pixels[pp]);
                        }
                    }
                    byte current = around[4];//自身の値
                    around.RemoveAt(4);//自身の値を除去して平均値を求める
                    double average = around.Average(z => z);
                    if (Math.Abs(average - current) > threshold) { current = (byte)average; }
                    filtered[x + y * width] = current;
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixesl, BitmapSource bitmap) Filter上下左右(byte[] pixels, int width, int height, byte threshold)
        {
            //上下左右の平均との差がしきい値以上ならノイズと判定して
            //平均値に変更する

            byte[] filtered = new byte[pixels.Length];
            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    byte current = pixels[y * width + x];//自身の値
                    double ave = 0;
                    ave += pixels[(y - 1) * width + x];//top
                    ave += pixels[(y + 1) * width + x];//bottom
                    ave += pixels[y * width + x - 1];//left
                    ave += pixels[y * width + x + 1];//right
                    ave /= 4;
                    //double average = around.Average(z => z);
                    if (Math.Abs(ave - current) > threshold)
                    {
                        current = (byte)ave;
                    }
                    filtered[x + y * width] = current;
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixesl, BitmapSource bitmap) TestFilter3(byte[] pixels, int width, int height)
        {
            //イマイチ
            //ノイズ除去のつもりだったけどぼかし効果になった
            //自身と斜めの4画素を比較して輝度が閾値以上離れていたら、周辺4画素の平均輝度に変更する
            byte threshold = 100;//これ以上離れていたら平均値にする
            byte[] filtered = new byte[pixels.Length];
            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    List<byte> around = new List<byte>();
                    byte current = pixels[y * width + x];//自身の値
                    around.Add(pixels[(y - 1) * width + x - 1]);//topLeft
                    around.Add(pixels[(y + 1) * width + x - 1]);//bottomLeft
                    around.Add(pixels[(y - 1) * width + x + 1]);//topRignt
                    around.Add(pixels[(y + 1) * width + x + 1]);//bottomRight

                    double average = around.Average(z => z);
                    if (Math.Abs(average - current) > threshold)
                    {
                        current = (byte)average;
                    }
                    filtered[x + y * width] = current;
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixesl, BitmapSource bitmap) TestFilter4(byte[] pixels, int width, int height)
        {
            //かなりいまいち
            //ノイズ除去のつもりだったけどぼかし効果になった
            //自身と上下左右4画素を比較して輝度が10以上離れていたら、周辺4画素の平均輝度に変更する
            //上下左右どれかに自身と似た輝度があれば、そのままにする
            byte threshold = 10;//これ以上離れていたら平均値にする
            byte similar = 1;//似た輝度の閾値、これ以下なら似ている
            byte[] filtered = new byte[pixels.Length];
            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    List<byte> around = new List<byte>();
                    byte current = pixels[y * width + x];//自身の値
                    around.Add(pixels[(y - 1) * width + x]);//top
                    around.Add(pixels[(y + 1) * width + x]);//bottom
                    around.Add(pixels[y * width + x - 1]);//left
                    around.Add(pixels[y * width + x + 1]);//right
                    //平均値からかけ離れていて
                    double average = around.Average(z => z);
                    double diff = Math.Abs(average - current);
                    if (diff > threshold)
                    {
                        //上下左右のどれか
                        if (similar < Math.Abs(around[0] - current) &&
                            similar < Math.Abs(around[1] - current) &&
                            similar < Math.Abs(around[2] - current) &&
                            similar < Math.Abs(around[3] - current))
                        {
                            //平均値に変更
                            current = (byte)average;
                        }
                    }
                    filtered[x + y * width] = current;
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        //エッジ抽出後、メディアンフィルタ
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアンとメディアン(byte[] pixels, int width, int height)
        {
            //ラプラシアン、エッジ抽出
            int[][] weight = new int[][] {
                new int[] { 0, -1, 0 },
                new int[] { -1, 4, -1 },
                new int[] { 0, -1, 0 } };

            byte threshold = 100;//絶対値でこれ以上のエッジならメディアンフィルタで変更しない
            bool[] IsFilter = new bool[pixels.Length];//判定結果用
            int p;
            //めんどくさいので上下左右1ピクセルは処理しない
            //ラプラシアンでエッジ判定
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int edge = 0;
                    p = x + y * width;
                    int pp;
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            pp = (x + b - 1) + ((y + a - 1) * width);
                            edge += pixels[pp] * weight[a][b];
                        }
                    }
                    //エッジ判定結果
                    byte moto = pixels[p];
                    int diff = Math.Abs(edge - moto);
                    //IsFilter[p] = (Math.Abs(edge) < threshold) ? true : false;
                    if (Math.Abs(edge) < threshold) { IsFilter[p] = true; }

                    ////差が小さければメディアンフィルタする(true)
                    //if (diff < threshold) { IsFilter[p] = true; }
                    //else IsFilter[p] = false;
                }
            }
            //メディアンフィルタ
            byte[] filtered = new byte[pixels.Length];
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    p = x + y * width;
                    //エッジはパス
                    if (IsFilter[p] == false)
                    {
                        filtered[p] = pixels[p];
                        continue;//パス
                    }

                    int pp;
                    List<byte> sort = new List<byte>();
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            pp = (x + b - 1) + ((y + a - 1) * width);
                            sort.Add(pixels[pp]);
                        }
                    }
                    //ソートして中央値を採用
                    var sorted = sort.OrderBy(z => z);
                    filtered[p] = sorted.ToList()[4];
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        private (byte[] pixesl, BitmapSource bitmap) Filter上下左右2(byte[] pixels, int width, int height, byte threshold)
        {
            //上下左右の平均との差がしきい値以上ならノイズと判定して
            //平均値に変更するけど
            //左右の色と似ているか、上下の色が似ていたら変更しない、第2しきい値
            byte similer = 50;//これ以下なら変更なし
            byte[] filtered = new byte[pixels.Length];

            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {

                    byte current = pixels[y * width + x];//自身の値
                    double ave = 0;
                    int top = pixels[(y - 1) * width + x];//top
                    int bottom = pixels[(y + 1) * width + x];//bottom
                    int left = pixels[y * width + x - 1];//left
                    int right = pixels[y * width + x + 1];//right

                    int dTop = Math.Abs(current - top);
                    int dBottom = Math.Abs(current - bottom);
                    int dLeft = Math.Abs(current - left);
                    int dRight = Math.Abs(current - right);
                    if ((dTop < similer & dBottom < similer) | (dLeft < similer & dRight < similer))
                    {
                    }
                    else if (Math.Abs(ave - current) < threshold)
                    {
                        current = (byte)((top + bottom + left + right) / 4);
                    }
                    filtered[x + y * width] = current;
                    if (top == bottom) { }
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        //エッジ抽出後、上下左右フィルタ
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアンと上下左右(byte[] pixels, int width, int height, int threshold)
        {
            //ラプラシアン、エッジ抽出
            int[][] weight = new int[][] {
                new int[] { 0, -1, 0 },
                new int[] { -1, 4, -1 },
                new int[] { 0, -1, 0 } };

            bool[] IsFilter = new bool[pixels.Length];//判定結果用
            int p;
            //めんどくさいので上下左右1ピクセルは処理しない
            //ラプラシアンでエッジ判定
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int edge = 0;
                    p = x + y * width;
                    int pp;
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            pp = (x + b - 1) + ((y + a - 1) * width);
                            edge += pixels[pp] * weight[a][b];
                        }
                    }
                    //エッジ判定結果
                    byte moto = pixels[p];
                    int diff = Math.Abs(edge - moto);
                    //IsFilter[p] = (Math.Abs(edge) < threshold) ? true : false;
                    if (Math.Abs(edge) < threshold) { IsFilter[p] = true; }

                    ////差が小さければフィルタする(true)
                    //if (diff < threshold) { IsFilter[p] = true; }
                    //else IsFilter[p] = false;
                }
            }

            var neko = IsFilter.Count(zz => zz == false);
            byte[] filtered = new byte[pixels.Length];
            //めんどくさいので上下左右1ピクセルは処理しない
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    p = x + (y * width);
                    //エッジはパス
                    if (IsFilter[p] == false)
                    {
                        filtered[p] = pixels[p];
                        continue;//パス
                    }

                    //ノイズなら平均値に変更
                    byte current = pixels[p];//自身の値                    
                    int top = pixels[(y - 1) * width + x];//top
                    int bottom = pixels[(y + 1) * width + x];//bottom
                    int left = pixels[p - 1];//left
                    int right = pixels[p + 1];//right
                    double ave = (top + bottom + left + right) / 4;
                    double diff = Math.Abs(current - ave);

                    if (Math.Abs(ave - current) > 1)
                    {
                        current = (byte)ave;
                    }
                    filtered[p] = current;
                }
            }
            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
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

                MyPixels = pixels;
                MyBitmap = bitmap;
                MyImage.Source = MyBitmap;
                MyImageOrigin.Source = MyBitmap;
                MyPixelsOrigin = MyPixels;
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

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            //リセット
            MyImage.Source = MyImageOrigin.Source;
            MyPixels = MyPixelsOrigin;
        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            int[][] weight = new int[][] {
                new int[] { 0, 1, 0 },
                new int[] { 1, 1, 1 },
                new int[] { 0, 1, 0 } };
            int offset = 0;
            int div = 5;
            (byte[] pixels, BitmapSource bitmap) = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight, weight, div, offset);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ラプラシアン、エッジ抽出
            int[][] weight = new int[][] {
                new int[] { 0, -1, 0 },
                new int[] { -1, 4, -1 },
                new int[] { 0, -1, 0 } };
            int offset = 0;
            int div = 1;
            (byte[] pixels, BitmapSource bitmap) = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight,
                weight, div, offset);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //シャープネス
            int[][] weight = new int[][] {
                new int[] { 0, -1, 0 },
                new int[] { -1, 5, -1 },
                new int[] { 0, -1, 0 } };
            int offset = 0;
            int div = 1;
            (byte[] pixels, BitmapSource bitmap) = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight,
                weight, div, offset);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //エンボス
            int[][] weight = new int[][] {
                new int[] { -1, 0, 0 },
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 0 } };
            int offset = 128;
            int div = 1;
            (byte[] pixels, BitmapSource bitmap) = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight,
                weight, div, offset);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //メディアン、ノイズ除去
            (byte[] pixels, BitmapSource bitmap) = MedianFilter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            int[][] weight = new int[][] {
                new int[] { 0, 1, 0 },
                new int[] { 1, 1, 1 },
                new int[] { 0, 1, 0 } };
            int offset = 0;
            int div = 5;
            (byte[] pixels, BitmapSource bitmap) = Filter2(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight, weight, div, offset);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            (byte[] pixels, BitmapSource bitmap) = TestFilter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight, 10);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            //上下左右
            (byte[] pixels, BitmapSource bitmap) = Filter上下左右(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight, 100);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            (byte[] pixels, BitmapSource bitmap) = TestFilter3(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            (byte[] pixels, BitmapSource bitmap) = TestFilter4(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            //ラプラシアン、エッジ抽出
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアンとメディアン(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            (byte[] pixels, BitmapSource bitmap) = Filter上下左右2(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight, 100);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアンと上下左右(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight, 100);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
