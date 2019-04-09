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

namespace _20190409_画像フィルタ処理
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
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
            (MyPixels, MyBitmap) = MakeBitmapSourceAndByteArray(filePath, PixelFormats.Gray8, 96, 96);

            MyImageOrigin.Source = MyBitmap;
        }
        private BitmapSource Filter(byte[] pixels, int width, int height, int[][] weight, int div, int offset)
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
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width);
        }
        //速度は↑より誤差程度遅いので、わかりやすい↑を使ったほうがいい
        private BitmapSource Filter2(byte[] pixels, int width, int height, int[][] weight, int div, int offset)
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
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width);

        }

        //自身と周辺8画素の中央値をそのまま適用
        //ノイズ除去の効果があるけど、かなりぼやける
        //処理時間はぼかしの10倍
        private BitmapSource MedianFilter(byte[] pixels, int width, int height)
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
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width);
        }

        private BitmapSource TestFilter(byte[] pixels, int width, int height)
        {
            //ノイズ除去のつもりだったけどぼかし効果になった
            //自身と周辺8画素を比較して輝度が10以上離れていたら、周辺8画素の平均輝度に変更する
            byte threshold = 10;//小さいほどぼやける
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
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width);
        }

        private BitmapSource TestFilter2(byte[] pixels, int width, int height)
        {
            //ノイズ除去のつもりだったけどぼかし効果になった
            //自身と上下左右4画素を比較して輝度が10以上離れていたら、周辺4画素の平均輝度に変更する
            byte threshold = 10;//これ以上離れていたら平均値にする
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

                    double average = around.Average(z => z);
                    if (Math.Abs(average - current) > threshold)
                    {
                        current = (byte)average;
                    }
                    filtered[x + y * width] = current;
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width);
        }

        private BitmapSource TestFilter3(byte[] pixels, int width, int height)
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
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width);
        }

        private BitmapSource TestFilter4(byte[] pixels, int width, int height)
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
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width);
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

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            int[][] weight = new int[][] {
                new int[] { 0, 1, 0 },
                new int[] { 1, 1, 1 },
                new int[] { 0, 1, 0 } };
            int offset = 0;
            int div = 5;
            MyImage.Source = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight,
                weight, div, offset);
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
            MyImage.Source = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight,
                weight, div, offset);
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
            MyImage.Source = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight,
                weight, div, offset);
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
            MyImage.Source = Filter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight,
                weight, div, offset);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //メディアン、ノイズ除去
            MyImage.Source = MedianFilter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            int[][] weight = new int[][] {
                new int[] { 0, 1, 0 },
                new int[] { 1, 1, 1 },
                new int[] { 0, 1, 0 } };
            int offset = 0;
            int div = 5;
            MyImage.Source = Filter2(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight, weight, div, offset);

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            MyImage.Source = TestFilter(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            MyImage.Source = TestFilter2(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            MyImage.Source = TestFilter3(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {            
            MyImage.Source = TestFilter4(MyPixels, MyBitmap.PixelWidth, MyBitmap.PixelHeight);
        }
    }
}
