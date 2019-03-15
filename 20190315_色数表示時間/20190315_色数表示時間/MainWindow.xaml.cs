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

namespace _20190315_色数表示時間
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] SourceByte;//画像のピクセルデータ
        const int MyLoopCount = 10;//計測ループ回数

        public MainWindow()
        {
            InitializeComponent();

            this.AllowDrop = true;//ファイルのドロップ許可
            Drop += MainWindow_Drop;
            string imagePath = "";
            imagePath = @"D:\ブログ用\チェック用2\NEC_6221_2019_02_24_午後わてん_half.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\2x1白黒.png";
            imagePath = @"E:\オレ\携帯\2019\NEC_6293.JPG";

            BitmapSource source;
            //(SourceByte, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Pbgra32, 96, 96);//半透明色が変化してしまう
            (SourceByte, source) = MakeByteArrayAndSourceFromImageFile(imagePath, PixelFormats.Bgra32, 96, 96);
            MyImage.Source = source;

            Button1.Click += Button1_Click;

        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            BitmapSource source;
            (SourceByte, source) = MakeByteArrayAndSourceFromImageFile(filePath[0], PixelFormats.Pbgra32, 96, 96);
            if (source == null)
            {
                MessageBox.Show("画像ファイルじゃないみたい");
                Button1.IsEnabled = false;
            }
            Button1.IsEnabled = true;
            MyImage.Source = source;

        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Time計測();

        }

        private void Time計測()
        {
            List<Func<byte[], int>> funcs = new List<Func<byte[], int>>();
            funcs.Add(Count1配列とビットシフト);
            funcs.Add(Count2配列と掛け算足し算);
            funcs.Add(Count3Dictionary);
            funcs.Add(Count4DictionaryKyeCount);
            funcs.Add(Count5Dictionary32bpp);
            funcs.Add(Count6Dictionary32bppとビットシフト);

            List<TextBlock> textBlocks = new List<TextBlock>();
            textBlocks.Add(TextBlock1);
            textBlocks.Add(TextBlock2);
            textBlocks.Add(TextBlock3);
            textBlocks.Add(TextBlock4);
            textBlocks.Add(TextBlock5);
            textBlocks.Add(TextBlock6);

            var sw = new Stopwatch();
            int count = 0;
            for (int i = 0; i < funcs.Count; i++)
            {
                sw.Restart();
                for (int j = 0; j < 10; j++)
                {
                    count = funcs[i](SourceByte);
                }
                sw.Stop();
                long tt = sw.Elapsed.Ticks / 10;
                var total = new TimeSpan(tt);

                textBlocks[i].Text = $"{total.Seconds}.{total.Milliseconds.ToString("000")}秒：{count}色";
            }
        }


        #region
        private int Count1配列とビットシフト(byte[] pixels)
        {
            int[] colorInt = new int[256 * 256 * 256];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                colorInt[pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16)]++;//bgr
            }
            int count = 0;
            for (int i = 0; i < colorInt.Length; i++)
            {
                if (colorInt[i] != 0) { count++; }
            }
            return count;
        }

        private int Count2配列と掛け算足し算(byte[] pixels)
        {
            int[] colorInt = new int[256 * 256 * 256];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                colorInt[pixels[i] + (pixels[i + 1] * 256) + (pixels[i + 2] * 65536)]++;//bgr
            }
            int count = 0;
            for (int i = 0; i < colorInt.Length; i++)
            {
                if (colorInt[i] != 0) { count++; }
            }
            return count;
        }

        private int Count3Dictionary(byte[] pixels)
        {
            var table = new Dictionary<int, int>();
            int key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = pixels[i] + (pixels[i + 1] * 256) + (pixels[i + 2] * 65536);
                if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
            }
            return table.Count;
        }

        private int Count4DictionaryKyeCount(byte[] pixels)
        {
            var table = new Dictionary<int, int>();
            int key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = pixels[i] + (pixels[i + 1] * 256) + (pixels[i + 2] * 65536);
                if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
            }
            Dictionary<int, int>.KeyCollection k = table.Keys;
            return k.Count;
        }

        private int Count5Dictionary32bpp(byte[] pixels)
        {
            var table = new Dictionary<uint, int>();//uint
            uint key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = (uint)(pixels[i] + (pixels[i + 1] * 256) + (pixels[i + 2] * 65536) + (pixels[i + 3] * 65536 * 256));
                if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
            }
            return table.Count;
        }

        private int Count6Dictionary32bppとビットシフト(byte[] pixels)
        {
            var table = new Dictionary<uint, int>();//uint
            uint key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = (uint)(pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16) | (pixels[i + 3] << 24));
                if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
            }
            return table.Count;
        }


        #endregion




        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を取得、ピクセルフォーマットを指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] array, BitmapSource source) MakeByteArrayAndSourceFromImageFile(string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
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


        #region 今回は未使用の関数

        //Bitmapsourceをbyte[]に変換
        private byte[] MakeByteArrayFromBpp(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = (width * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[height * stride];
            source.CopyPixels(pixels, stride, 0);
            return pixels;
        }


        //画像ファイルをdpiを指定してBitmapsourceに変換
        private BitmapSource MakeBitmapSourceFromImagePath(string filePath, double dpiX = 0, double dpiY = 0)
        {
            BitmapSource source = null;
            try
            {
                using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var bf = BitmapFrame.Create(stream);
                    int w = bf.PixelWidth;
                    int h = bf.PixelHeight;
                    int stride = (w * bf.Format.BitsPerPixel + 7) / 8;
                    byte[] pixels = new byte[h * stride];
                    bf.CopyPixels(pixels, stride, 0);
                    if (dpiX == 0) dpiX = 96.0;
                    if (dpiY == 0) dpiY = 96.0;
                    source = BitmapSource.Create(w, h, dpiX, dpiY, bf.Format, bf.Palette, pixels, stride);
                }
            }
            catch (Exception)
            {
                //throw;
            }
            return source;
        }


        private int CountColor(BitmapSource source)
        {
            byte[] pixels = MakeByteArrayFromBpp(source);
            int bpp = source.Format.BitsPerPixel;
            int count = 0;
            if (bpp <= 8)
            {

            }
            else if (bpp <= 24)
            {

            }
            else
            {
                
            }
            return count;
        }


        #endregion



        #region テスト確認用関数なので未使用
        private void MyTest確認用_カウント用配列の要素数限界()
        {
            //32bitだと配列の要素数は256*256*256*256必要だけど、この要素数で配列はエラーになる
            //なので透明度は無視してRGBだけの256*256*256の配列で数える

            Color siro = Colors.White;
            byte r, g, b, a;
            r = siro.R; g = siro.G; b = siro.B; a = siro.A;
            uint ui24 = (uint)(r | g << 8 | b << 16);//uintを使う意味がない
            uint ui32 = (uint)(r | g << 8 | b << 16 | g << 24);//本当はこれを使いたい
            int i24 = r | g << 8 | b << 16;//これを使う
            int i32 = r | g << 8 | b << 16 | g << 24;//intでは溢れる
        }

        #endregion


    }
}
