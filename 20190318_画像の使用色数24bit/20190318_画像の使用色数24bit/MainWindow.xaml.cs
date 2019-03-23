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

namespace _20190318_画像の使用色数24bit
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] MyImageByte;
        BitmapSource MyBitmapSource;

        public MainWindow()
        {
            InitializeComponent();

            this.AllowDrop = true;
            Drop += MainWindow_Drop;

            var bit24 = 256 * 256 * 256;
            uint bit32 = (uint)(bit24 * 256u);//16,777,216
            long bitLong = bit24 * 256u;//4,294,967,296
            long bitLong2 = bit24 * 256;//0
            long bitLong3 = (long)Math.Pow(2, 32);//4,294,967,296
            uint bituint = (uint)Math.Pow(2, 32);//0
            var uintMax = uint.MaxValue;//Uint32.MaxValue, 4,294,967,295
            var uint16Max = ushort.MaxValue;//Uint16.MaxValue

            bool[] b16 = new bool[uint16Max];
            //bool[] ui = new bool[uintMax];//オーバーフロー

            Color c = Colors.AliceBlue;
            int index = RGBtoInt(c.R, c.G, c.B);
            (byte r, byte g, byte b) = IntToRgb(index);
            int aa = 255 & 0x00;//0
            int bb = 255 & 0xFF;//255
            int cc = 0xFDFE;//65022
            int dd = cc & 0x0000;//0
            int ee = cc & 0xFFFF;//65022
            int ff = cc >> 8;//253
            int gg = ff & 0xFF;//253
            int hh = ff & 0x00;//0
            int ii = cc & 0x00FF;//254
            int jj = cc & 0xFF00;//64768
            MyBitTest();

        }

        private void MyBitTest()
        {
            byte b255 = 255;
            byte bMax = byte.MaxValue;

            int ia = b255 & 0x00;
            int ib = b255 & 0b00000000;
            int ic = b255 & 0b11111111;

            //2進数、その桁が1だったときの10進数での値
            //桁    8   7   6   5  4  3  2  1
            //値  128  64  32  16  8  4  2  1


            int a20 = 20;        //  10進数の20
            int b20 = 0b10100;   //  2進数の20
            //一番上の桁の1より上の桁の0は見た目以外の意味はない、桁を揃えたいときに使う
            int c20 = 0b00010100;//  2進数の20、8桁表記、上の桁に0をつけただけ
            int d20 = 0b0001_0100;// 2進数の20、8桁の中間に_で区切り表記

            int e20 = 0x14;      //  16進数の20
            int f20 = 0x0014;    //  16進数の20、これも上の桁に0つけても値は変わらない
            bool vv = (a20 == b20) & (c20 == d20) & (e20 == f20);//true
            int g20 = 0b0001_0100 + 0x14;// 20 + 20 = 40


            //ビットシフト
            int j20 = 20 << 2;//        80、20を左に2シフト
            int k20 = 0x14 << 2;//      80

            int l20 = 0b0001_0100 << 2;//80
            int m20 = 0b0101_0000;//     80

            int n20 = 0b0001_0100 >> 2;// 5
            int o20 = 0b0000_0101;//      5

            int p20 = 0b0001_0100 >> 3;// 2、20を右に3シフト
            int q20 = 0b0000_0010;//      2

            int r20 = 0b0001_0100 >> 6;// 0


            //orはどちらかが1なら1
            int aa = 0b0001_0100 | 0b0000_0000;// =0010_0100 =  20
            int ab = 0b0001_0100 | 0b1111_1111;// =1111_1111 = 255
            int ac = 0b0001_0100 | 0b1110_1011;// =1111_1111 = 255
            int ad = 0b0001_0100 | 0b1000_1101;// =1001_1101 = 157

            //       aa         ab         ac         ad
            //   0001_0100  0001_0100  0001_0100  0001_0100
            //or 0000_0000  1111_1111  1110_1011  1000_1101
            //=  0001_0100  1111_1111  1111_1111  1001_1101


            //andはどちらも1のときだけ1、それ以外は全部0
            int ba = 0b0001_0100 & 0b0000_0000;// =0000 0000 =  0
            int bb = 0b0001_0100 & 0b1111_1111;// =0001 0100 = 20
            int bc = 0b0001_0100 & 0b1110_1011;// =0000 0000 =  0
            int bd = 0b0001_0100 & 0b1000_1101;// =0000_0100 =  4

            //        ba         bb         bc         bd
            //    0001_0100  0001_0100  0001_0100  0001_0100
            //and 0000_0000  1111_1111  1110_1011  1000_1101
            // =  0000_0000  0001_0100  0000_0000  0000_0100


            //キャストでビットマスク？
            //byte型の最大値255を超える値をbyte型にキャストすると
            //下の8bit(1byte)分の値になるみたい
            //逆に言うと9bit以上の値は切り捨て
            int ca = 256;
            byte cb = (byte)ca;//  0
            ca = 257;
            byte cc = (byte)ca;//  1
            ca = 10000;
            byte cd = (byte)ca;// 16
            //                                       下の8bit
            int ce = 0b0001_0000_0000;//        256  0000_0000 = 0
            int cf = 0b0001_0000_0001;//        257  0000_0001 = 1
            int cg = 0b0010_0111_0001_0000;// 10000  0001_0000 = 16



            //ビット演算で
            //ピクセルの色RGBの値をint型に変換する
            //普通のカラー画像はRGBそれぞれ8bitの合計24bit
            Color iro = Colors.AliceBlue;
            byte r, g, b;
            r = iro.R;//                                 240                        1111_0000
            g = iro.G;//                                 248                        1111_1000
            b = iro.B;//                                 255                        1111_1111
            //ビットシフト                      ビットシフト
            int iRed = r;//                              240    0000_0000_0000_0000_1111_0000
            int iGreen = g << 8;//                     63488    0000_0000_1111_1000_0000_0000
            int iBlue = b << 16;//                  16711680    1111_1111_0000_0000_0000_0000
            //or                                          or
            int iColor = iRed | iGreen | iBlue;//   16775408    1111_1111_1111_1000_1111_0000
            //                                                 |    b    |    g    |    r    |
            //1行で
            int iColor2 = r | (g << 8) | (b << 16);


            //int型をbyte型RGBに変換
            //Redは下の8桁の値なので9bit以上を0にして8bit以下をそのままの値にすればいい
            //                          |    r    |
            //       1111_1111_1111_1000_1111_0000  16775408元のint
            //and    0000_0000_0000_0000_1111_1111  ←とandしてbとg部分を取り除く
            //                           1111_0000  240
            byte rr1 = (byte)(iColor & 0b0000_0000_0000_0000_1111_1111);
            byte rr2 = (byte)(iColor & 0b1111_1111);
            byte rr3 = (byte)(iColor & 0xFF);
            byte rr4 = (byte)(iColor & 255);
            byte rr5 = (byte)iColor;//byteキャストで9bit以上を切り捨て

            //Green、中間の9bitから16bitまでの8bitぶんを8bitとして取り出す
            //                |    g    |
            //       1111_1111_1111_1000_1111_0000  16775408元のint
            // >> 8            1111_1111_1111_1000  右に8シフトして
            //and              0000_0000_1111_1111  Redと同じように下の8bitを取り出す
            //                           1111_1000  248
            byte gg1 = (byte)(iColor >> 8 & 0b1111_1111);
            byte gg2 = (byte)(iColor >> 8 & 0xFF);
            byte gg3 = (byte)(iColor >> 8);

            //Blue、一番上の24bitから17bitの8bitぶんなので
            //1~16bitを取り除けばいい、これは右に16シフトでできる
            //      |    b    |
            //       1111_1111_1111_1000_1111_0000  16775408元のint
            // >> 16                     1111_1111  右に16シフト
            byte bb1 = (byte)(iColor >> 16);


        }

        //ウィンドウに画像ファイルドロップで
        //ピクセルフォーマットBgra32で読み込み
        //使用色数表示
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);

            (MyImageByte, MyBitmapSource) = MakeByteArrayAndSourceFromImageFile(filePath[0], PixelFormats.Bgra32, 96, 96);
            if (MyBitmapSource == null)
            {
                MessageBox.Show("画像ファイルじゃないみたい");
            }
            else
            {
                int count = Count24bit(MyImageByte);
                TextBlock1.Text = $"使用色数{count:#,0}(24bit)";
                MyImage.Source = MyBitmapSource;

                var count24 = Count24bit(MyImageByte);
                var count24p = Count24bitColorAndPixels(MyImageByte);
                var count32p = Count32bitColorAndPixels2(MyImageByte);
                //var keys = count32p.Keys.ToArray();
                foreach (var item in count32p)
                {
                    var cc = UintToArgb(item.Key);
                }
            }

        }

        //int型Colorをbyte型に変換
        private (byte r, byte g, byte b) IntToRgb(int value)
        {
            byte r = (byte)value;// & 0xFF;
            byte g = (byte)(value >> 8);// & 0xFF;
            byte b = (byte)(value >> 16);
            return (r, g, b);
        }

        //byte型Colorをintに変換
        private int RGBtoInt(byte r, byte g, byte b)
        {
            return r | (g << 8) | (b << 16);
        }

        //使用色数カウント
        //ピクセルフォーマットBgra32のbyte配列に対応
        //カウントは透明度を無視して24bitだけ対応、bool型の配列で色の有無だけ確認、速い
        private int Count24bit(byte[] pixels)
        {
            //bool[] bColor = new bool[256 * 256 * 256];
            //↑と同じ意味
            bool[] bColor = new bool[0xFFFFFF + 1];
            //RGBをintに変換して、そのインデックスの値をTrueにする
            for (int i = 0; i < pixels.Length; i += 4)
            {
                bColor[pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16)] = true;//bgr
            }
            //Trueの数をカウント
            int count = 0;
            for (int i = 0; i < bColor.Length; i++)
            {
                if (bColor[i] == true) { count++; }
            }

            //LINQでTrueの数をカウント、↑より1～2割遅い
            //int neko = bColor.Where(saru => saru == true).Count();
            //Whereは省略してcountメソッドだけでもカウントできるけど、もっと遅い
            //int inu = bColor.Count(saru => saru);
            return count;
        }

        /// <summary>
        /// 使用色数と色ごとのピクセル数をカウント、24bit
        /// </summary>
        /// <param name="pixels">BGRA順のbyte配列</param>
        /// <returns>Indexが色のint、要素の値がピクセル数</returns>
        private int[] Count24bitColorAndPixels(byte[] pixels)
        {
            int[] iColor = new int[256 * 256 * 256];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                iColor[RGBtoInt(pixels[i], pixels[i + 1], pixels[i + 2])]++;
            }
            return iColor;
        }


        //uint型Colorをbyte型に変換
        private (byte a, byte r, byte g, byte b) UintToArgb(uint value)
        {
            byte a = (byte)value;// (value & 0xFF);
            byte r = (byte)(value >> 8);// ((value >> 8) & 0xFF);
            byte g = (byte)(value >> 16);// ((value >> 16) & 0xFF);
            byte b = (byte)(value >> 24);
            return (a, r, g, b);
        }

        //byte型Colorをintに変換
        private uint ARGBtoUint(byte a, byte r, byte g, byte b)
        {
            return (uint)(a | (r << 8) | (g << 16) | (b << 24));
        }

        //
        /// <summary>
        /// 使用色数と色ごとのピクセル数をカウント、32bit
        /// </summary>
        /// <param name="pixels">BGRA順のbyte配列</param>
        /// <returns>keyが色のuint、Valueはピクセル数</returns>
        private Dictionary<uint, int> Count32bitColorAndPixels2(byte[] pixels)
        {
            Dictionary<uint, int> table = new Dictionary<uint, int>();
            for (int i = 0; i < pixels.Length; i += 4)
            {
                uint key = ARGBtoUint(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                //今の色のuintがなければ要素のValueを1で追加、すでにあればValueに+1
                if (table.ContainsKey(key) == false) table.Add(key, 1);//追加
                else { table[key]++; }//+1
            }
            return table;
        }




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
    }
}
