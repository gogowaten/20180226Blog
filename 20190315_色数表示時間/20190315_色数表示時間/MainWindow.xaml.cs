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
        const int MyLoopCount = 5;//計測ループ回数

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
            Button2.Click += Button2_Click;

            int c = Count11配列とビットシフト_24bitParallel(SourceByte);
        }
        //        C#でDictionaryの値によるソート - Coniglioの忘備録
        //http://coniglio.hateblo.jp/entry/2014/06/05/214330

        //各色カウントして表示、上位10色下位10色
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<uint, int> tabel = CountADictionary_32bitとビットシフト各色カウント(SourceByte);
            IOrderedEnumerable<KeyValuePair<uint, int>> sorted = tabel.OrderByDescending((x) => x.Value);
            //foreach (KeyValuePair<uint, int> item in sorted)
            //{
            //    uint key = item.Key;
            //    //    a       r        g        b
            //    //00000000_00000000_00000000_00000000
            //    byte b = (byte)(key & 0x000000FF);
            //    byte g = (byte)(key >> 8 & 0x0000FF);
            //    byte r = (byte)(key >> 16 & 0x00FF);
            //    byte a = (byte)(key >> 24);
            //    Color iro = Color.FromArgb(a, r, g, b);
            //    Console.WriteLine($"{iro.ToString()}_{item.Value}");
            //}

            IEnumerable<KeyValuePair<uint, int>> top10 = sorted.Take(10);//上位10色
            foreach (KeyValuePair<uint, int> item in top10)
            {
                uint key = item.Key;
                //    a       r        g        b
                //00000000_00000000_00000000_00000000
                byte b = (byte)(key & 0x000000FF);
                byte g = (byte)(key >> 8 & 0x0000FF);
                byte r = (byte)(key >> 16 & 0x00FF);
                byte a = (byte)(key >> 24);
                Color iro = Color.FromArgb(a, r, g, b);
                Console.WriteLine($"{iro.ToString()}_{item.Value}");
            }

            var bottom10 = sorted.Skip(sorted.Count() - 10);
            foreach (KeyValuePair<uint, int> item in bottom10)
            {
                uint key = item.Key;
                //    a       r        g        b
                //00000000_00000000_00000000_00000000
                byte b = (byte)(key & 0x000000FF);
                byte g = (byte)(key >> 8 & 0x0000FF);
                byte r = (byte)(key >> 16 & 0x00FF);
                byte a = (byte)(key >> 24);
                Color iro = Color.FromArgb(a, r, g, b);
                Console.WriteLine($"{iro.ToString()}_{item.Value}");
            }

        }


        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Time計測();
        }


        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            BitmapSource source;
            (SourceByte, source) = MakeByteArrayAndSourceFromImageFile(filePath[0], PixelFormats.Bgra32, 96, 96);
            if (source == null)
            {
                MessageBox.Show("画像ファイルじゃないみたい");
                Button1.IsEnabled = false;
            }
            Button1.IsEnabled = true;
            MyImage.Source = source;

        }

        private void Time計測()
        {
            List<Func<byte[], int>> funcs = new List<Func<byte[], int>>();
            funcs.Add(Count1配列とビットシフト_24bit);
            funcs.Add(Count2配列と掛け算足し算_24bit);
            funcs.Add(Count3Dictionary_24bit);
            funcs.Add(Count5Dictionary_24bitビットシフト);
            funcs.Add(Count4Dictionary_24bit_KeyCount);
            funcs.Add(Count6Dictionary_32bit);
            funcs.Add(Count7Dictionary_32bitとビットシフト);
            //funcs.Add(Count8Concurrent32bppとビットシフト);
            funcs.Add(Count9DictionaryIsAlpha_24or32bit);
            funcs.Add(Count10DictionaryIsAlpha_24or32bitビットシフト);
            funcs.Add(Count11配列とビットシフト_24bitParallel);
            funcs.Add(Count14ConcurrentDictionaryBoolean_32bpp);

            List<TextBlock> textBlocks = new List<TextBlock>();
            textBlocks.Add(TextBlock1);
            textBlocks.Add(TextBlock2);
            textBlocks.Add(TextBlock3);
            textBlocks.Add(TextBlock4);
            textBlocks.Add(TextBlock5);
            textBlocks.Add(TextBlock6);
            //textBlocks.Add(TextBlock7);
            textBlocks.Add(TextBlock8);
            textBlocks.Add(TextBlock9);
            textBlocks.Add(TextBlock10);
            textBlocks.Add(TextBlock11);
            textBlocks.Add(TextBlock12);

            var sw = new Stopwatch();
            int count = 0;
            for (int i = 0; i < funcs.Count; i++)
            {
                sw.Restart();
                for (int j = 0; j < MyLoopCount; j++)
                {
                    count = funcs[i](SourceByte);
                }
                sw.Stop();
                long tt = sw.Elapsed.Ticks / MyLoopCount;
                var total = new TimeSpan(tt);

                textBlocks[i].Text = $"{total.Seconds}.{total.Milliseconds.ToString("000")}秒：{count}色 {funcs[i].Method.Name}";
            }
        }


        #region
        private int Count1配列とビットシフト_24bit(byte[] pixels)
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

        private int Count2配列と掛け算足し算_24bit(byte[] pixels)
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

        private int Count3Dictionary_24bit(byte[] pixels)
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

        private int Count4Dictionary_24bit_KeyCount(byte[] pixels)
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

        private int Count5Dictionary_24bitビットシフト(byte[] pixels)
        {
            var table = new Dictionary<int, int>();
            int key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16);
                if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
            }
            return table.Count;
        }

        private int Count6Dictionary_32bit(byte[] pixels)
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

        private int Count7Dictionary_32bitとビットシフト(byte[] pixels)
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

        //全ピクセルを詰め込んでから重複を取り除いたリストをカウントする
        //遅い、メモリを大量に消費する
        private int Count8Concurrent32bppとビットシフト(byte[] pixels)
        {
            var concurrent = new System.Collections.Concurrent.ConcurrentBag<uint>();
            Parallel.For(0, pixels.Length / 4, i =>
              {
                  concurrent.Add((uint)(pixels[i * 4] | (pixels[i * 4 + 1] << 8) | (pixels[i * 4 + 2] << 16) | (pixels[i * 4 + 3] << 24)));
              });
            return concurrent.Distinct().ToArray().Length;
        }

        //半透明ピクセルがあった場合だけ32bitでカウント
        private int Count9DictionaryIsAlpha_24or32bit(byte[] pixels)
        {
            bool IsAlpha = false;
            for (int i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] != 255)
                {
                    IsAlpha = true;
                    break;
                }
            }

            var table = new Dictionary<uint, int>();//uint
            uint key;
            if (IsAlpha)
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    key = (uint)(pixels[i] + (pixels[i + 1] * 256) + (pixels[i + 2] * 65536) + (pixels[i + 3] * 65536 * 256));
                    if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
                }
            }
            else
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    key = (uint)(pixels[i] + (pixels[i + 1] * 256) + (pixels[i + 2] * 65536));
                    if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
                }
            }
            return table.Count;
        }

        //半透明ピクセルがあった場合だけ32bitでカウント
        private int Count10DictionaryIsAlpha_24or32bitビットシフト(byte[] pixels)
        {
            bool IsAlpha = false;
            for (int i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] != 255)
                {
                    IsAlpha = true;
                    break;
                }
            }

            var table = new Dictionary<uint, int>();//uint
            uint key;
            if (IsAlpha)
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    key = (uint)(pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16) | (pixels[i + 3] << 24));
                    if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
                }
            }
            else
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    key = (uint)(pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16));
                    if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい
                }
            }
            return table.Count;
        }

        private Dictionary<uint, int> CountADictionary_32bitとビットシフト各色カウント(byte[] pixels)
        {
            var table = new Dictionary<uint, int>();//uint
            uint key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = (uint)(pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16) | (pixels[i + 3] << 24));
                if (table.ContainsKey(key) == false) { table.Add(key, 1); }//valueはintならなんでもいい                
                else { table[key] = table[key] + 1; }//value += 1
            }
            return table;
        }

        //無理やり３スレッドにしてみたけどカウント数が合わない
        private int Count11配列とビットシフト_24bitParallel(byte[] pixels)
        {
            int[] colorInt = new int[256 * 256 * 256];
            var a1 = new int[256 * 256 * 256];
            var a2 = new int[256 * 256 * 256];
            var a3 = new int[256 * 256 * 256];
            int v = pixels.Length / 3;
            Parallel.Invoke(() =>
            {
                for (int i = 0; i < v; i += 4)
                {
                    a1[pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16)]++;
                }
            }, () =>
            {
                for (int i = v + 1; i < v * 2; i += 4)
                {
                    a2[pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16)]++;
                }
            }, () =>
            {
                for (int i = v * 2 + 1; i < pixels.Length; i += 4)
                {
                    a3[pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16)]++;
                }
            }
                );


            for (int i = 0; i < a1.Length; i++)
            {
                a1[i] += a2[i] + a3[i];
            }

            int count = 0;
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != 0) { count++; }
            }

            return count;
        }

        //bool型の配列で色の有無だけ確認、速い
        private int Count12bool配列とビットシフト_24bit(byte[] pixels)
        {
            bool[] colorInt = new bool[256 * 256 * 256];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                colorInt[pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16)] = true;//bgr
            }
            int count = 0;
            for (int i = 0; i < colorInt.Length; i++)
            {
                if (colorInt[i] == true) { count++; }
            }
            return count;
        }

        private int Count13boolDictionary_32bitとビットシフト(byte[] pixels)
        {
            var table = new Dictionary<uint, bool>();//uint
            uint key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = (uint)(pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16) | (pixels[i + 3] << 24));
                if (table.ContainsKey(key) == false) { table.Add(key, true); }
            }
            return table.Count;
        }

        //遅いしメモリの大量消費
        private int Count14ConcurrentDictionaryBoolean_32bpp(byte[] pixels)
        {
            var cd = new System.Collections.Concurrent.ConcurrentDictionary<uint, bool>();
            Parallel.For(0, pixels.Length / 4, i =>
            {
                cd.GetOrAdd((uint)(pixels[i * 4] | (pixels[i * 4 + 1] << 8) | (pixels[i * 4 + 2] << 16) | (pixels[i * 4 + 3] << 24)), true);
            });
            return cd.Distinct().ToArray().Length;
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


    public class MyData
    {
        public uint Index { get; set; }
        public int Count { get; set; }
    }
}
