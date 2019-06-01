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

namespace _20190530_メディアンフィルタ2
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

        byte[][] MyUnits;

        public MainWindow()
        {
            InitializeComponent();

            this.Drop += MainWindow_Drop;
            this.AllowDrop = true;


            //            参照渡し - C# によるプログラミング入門 | ++C++; // 未確認飛行 C
            //https://ufcpp.net/study/csharp/sp_ref.html



            byte[] A = new byte[] { 8, 2, 5 };
            byte[] B = new byte[] { 6, 9, 3 };
            byte[] C = new byte[] { 1, 7, 4 };

            var AA = Median2(A);
            var BB = Median2(B);
            var CC = Median2(C);
            var unit = new byte[3][];
            unit[0] = AA;
            unit[1] = BB;
            unit[2] = CC;

            var u2 = new byte[3][] { A, B, C };
            MedianSortByte(ref u2[0]);
            MedianSortByte(ref u2[1]);
            MedianSortByte(ref u2[2]);
            //ソートの結果を値コピー
            var bb = new byte[3];//ここはvar bb=u2[1];だと参照になるのでおかしくなる
            bb[0] = u2[1][0];
            bb[1] = u2[1][1];
            bb[2] = u2[1][2];
            var cc = new byte[3];//ここはvar cc=u2[2];だと参照になるのでおかしくなる
            cc[0] = u2[2][0];
            cc[1] = u2[2][1];
            cc[2] = u2[2][2];
            MedianSortUnit(ref u2);


            u2[0] = bb;
            u2[1] = cc;
            u2[2][0] = 5;
            u2[2][1] = 5;
            u2[2][2] = 5;
            MedianSortByte(ref u2[2]);
            cc = u2[2];
            MedianSortUnit(ref u2);


            var UU = MedianSortUnit(unit);
            var med = MedianFind(UU);

            byte[] unit2 = unit[1];
            byte[] unit3 = unit[2];
            MedianSortUnit(ref unit);

        }



        private void MedianSortByte(ref byte[] pixels)
        {
            byte temp;
            if (pixels[0] >= pixels[1])
            {
                // 0 1                
                if (pixels[1] >= pixels[2])
                {
                    // 0 1 2
                    //そのまま
                }
                else if (pixels[0] >= pixels[2])// 0 >= 1 and 2 >= 1
                {
                    // 0 2 1
                    temp = pixels[1];
                    pixels[1] = pixels[2];
                    pixels[2] = temp;
                }
                else
                {
                    // 2 0 1
                    temp = pixels[0];
                    pixels[0] = pixels[2];
                    pixels[2] = pixels[1];
                    pixels[1] = temp;
                }
            }
            else
            {
                // 1 0
                if (pixels[2] >= pixels[1])
                {
                    // 2 1 0
                    temp = pixels[0];
                    pixels[0] = pixels[2];
                    pixels[2] = temp;
                }
                else if (pixels[0] >= pixels[2])// 1 0 and 1 2
                {
                    // 1 0 2
                    temp = pixels[0];
                    pixels[0] = pixels[1];
                    pixels[1] = temp;
                }
                else
                {
                    // 1 2 0
                    temp = pixels[0];
                    pixels[0] = pixels[1];
                    pixels[1] = pixels[2];
                    pixels[2] = temp;
                }
            }

        }
        private byte[] MedianSortByte(byte[] pixels)
        {
            byte[] v = new byte[3];
            if (pixels[0] >= pixels[1])
            {
                // 0 1                
                if (pixels[1] >= pixels[2])
                {
                    // 0 1 2
                    //そのまま
                    v[0] = pixels[0];
                    v[1] = pixels[1];
                    v[2] = pixels[2];
                }
                else if (pixels[0] >= pixels[2])// 0 >= 1 and 2 >= 1
                {
                    // 0 2 1
                    v[0] = pixels[0];
                    v[1] = pixels[2];
                    v[2] = pixels[1];
                }
                else
                {
                    // 2 0 1
                    v[0] = pixels[2];
                    v[1] = pixels[0];
                    v[2] = pixels[1];
                }
            }
            else
            {
                // 1 0
                if (pixels[2] >= pixels[1])
                {
                    // 2 1 0
                    v[0] = pixels[2];
                    v[1] = pixels[1];
                    v[2] = pixels[0];
                }
                else if (pixels[0] >= pixels[2])// 1 0 and 1 2
                {
                    // 1 0 2
                    v[0] = pixels[1];
                    v[1] = pixels[0];
                    v[2] = pixels[2];
                }
                else
                {
                    // 1 2 0
                    v[0] = pixels[1];
                    v[1] = pixels[2];
                    v[2] = pixels[0];
                }
            }
            return v;
        }
        private byte[] Median2(byte[] pixels)
        {
            var neko = pixels.OrderByDescending(x => x).ToArray();
            return neko;
        }

        private byte[][] MedianSortUnit(byte[][] pixels)
        {
            var v = new byte[3][];
            if (pixels[0][1] >= pixels[1][1])
            {
                if (pixels[1][1] >= pixels[2][1])
                {
                    v[0] = pixels[0];
                    v[1] = pixels[1];
                    v[2] = pixels[2];
                }
                else if (pixels[0][1] >= pixels[2][1])
                {
                    v[0] = pixels[0];
                    v[1] = pixels[2];
                    v[2] = pixels[1];
                }
                else
                {
                    v[0] = pixels[2];
                    v[1] = pixels[0];
                    v[2] = pixels[1];
                }
            }
            else
            {
                if (pixels[2][1] >= pixels[1][1])
                {
                    v[0] = pixels[2];
                    v[1] = pixels[1];
                    v[2] = pixels[0];
                }
                else if (pixels[0][1] >= pixels[2][1])
                {
                    v[0] = pixels[1];
                    v[1] = pixels[0];
                    v[2] = pixels[2];
                }
                else
                {
                    v[0] = pixels[1];
                    v[1] = pixels[2];
                    v[2] = pixels[0];
                }
            }
            return v;
        }

        private byte MedianFind(byte[][] pixels)
        {
            byte median;
            if ((pixels[0][2] >= pixels[1][1] & pixels[1][1] >= pixels[2][0]) || (pixels[2][0] >= pixels[1][1] & pixels[1][1] >= pixels[0][2]))
            {
                median = pixels[1][1];
            }
            else if (pixels[0][2] > pixels[1][1] && pixels[2][0] > pixels[1][1])
            {
                median = Math.Min(pixels[0][2], Math.Min(pixels[1][0], pixels[2][0]));
            }
            else
            {
                median = Math.Max(pixels[0][2], Math.Max(pixels[1][2], pixels[2][0]));
            }
            return median;
        }

        private byte MedianFindVerticalUnit(byte[][] pixels)
        {
            byte median;
            if ((pixels[0][2] >= pixels[1][1] & pixels[1][1] >= pixels[2][0]) || (pixels[2][0] >= pixels[1][1] & pixels[1][1] >= pixels[0][2]))
            {
                median = pixels[1][1];
            }
            else if (pixels[0][2] > pixels[1][1] && pixels[2][0] > pixels[1][1])
            {
                median = Math.Min(pixels[0][2], Math.Min(pixels[2][0], pixels[1][0]));
            }
            else
            {
                median = Math.Max(pixels[0][2], Math.Max(pixels[2][0], pixels[1][2]));
            }
            return median;
        }
        private byte MedianFindVerticalUnit(byte[] v)
        {
            if ((v[2] >= v[4] && v[4] >= v[6]) || (v[6] >= v[4] && v[4] >= v[2]))
            {
                return v[4];
            }
            else if(v[2]>=v[4] && v[6] >= v[4])
            {
                return Math.Min(v[2], Math.Min(v[6], v[3]));
            }
            else
            {
                return Math.Max(v[2], Math.Max(v[6], v[5]));
            }
        }

        //sort1
        private void MedianSort1(ref byte[] pixels)
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

        private void MedianSort2(ref byte[] pixels)
        {
            byte[] temp = new byte[3];
            if (pixels[1] >= pixels[4])
            {
                if (pixels[4] >= pixels[7])
                {
                    //そのまま
                }
                else if (pixels[1] >= pixels[7])
                {
                    // 0 2 1
                    temp[0] = pixels[3];
                    temp[1] = pixels[4];
                    temp[2] = pixels[5];
                    pixels[3] = pixels[6];
                    pixels[4] = pixels[7];
                    pixels[5] = pixels[8];
                    pixels[6] = temp[0];
                    pixels[7] = temp[1];
                    pixels[8] = temp[2];
                }
                else
                {
                    // 2 0 1
                    temp[0] = pixels[6];
                    temp[1] = pixels[7];
                    temp[2] = pixels[8];
                    pixels[6] = pixels[3];
                    pixels[7] = pixels[4];
                    pixels[8] = pixels[5];
                    pixels[3] = pixels[0];
                    pixels[4] = pixels[1];
                    pixels[5] = pixels[2];
                    pixels[0] = temp[0];
                    pixels[1] = temp[1];
                    pixels[2] = temp[2];
                }
            }
            else
            {
                if (pixels[7] >= pixels[4])
                {
                    // 2 1 0
                    temp[0] = pixels[0];
                    temp[1] = pixels[1];
                    temp[2] = pixels[2];
                    pixels[0] = pixels[6];
                    pixels[1] = pixels[7];
                    pixels[2] = pixels[8];
                    pixels[6] = temp[0];
                    pixels[7] = temp[1];
                    pixels[8] = temp[2];
                }
                else if (pixels[1] >= pixels[7])
                {
                    // 1 0 2
                    temp[0] = pixels[0];
                    temp[1] = pixels[1];
                    temp[2] = pixels[2];
                    pixels[0] = pixels[3];
                    pixels[1] = pixels[4];
                    pixels[2] = pixels[5];
                    pixels[3] = temp[0];
                    pixels[4] = temp[1];
                    pixels[5] = temp[2];
                }
                else
                {
                    // 1 2 0
                    temp[0] = pixels[0];
                    temp[1] = pixels[1];
                    temp[2] = pixels[2];
                    pixels[0] = pixels[3];
                    pixels[1] = pixels[4];
                    pixels[2] = pixels[5];
                    pixels[3] = pixels[6];
                    pixels[4] = pixels[7];
                    pixels[5] = pixels[8];
                    pixels[6] = temp[0];
                    pixels[7] = temp[1];
                    pixels[8] = temp[2];
                }
            }
        }

        private byte MedianFind(byte[] vs)
        {
            if ((vs[2] >= vs[4] && vs[4] >= vs[6]) || (vs[6] >= vs[4] && vs[4] >= vs[2]))
            {
                return vs[4];
            }
            else if (vs[2] >= vs[4] && vs[6] >= vs[4])
            {
                return Math.Min(vs[2], Math.Min(vs[3], vs[6]));
            }
            else
            {
                return Math.Max(vs[2], Math.Max(vs[5], vs[6]));
            }
        }


        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化(byte[] pixels, int width, int height)
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
                    MedianSort1(ref v);
                    MedianSort2(ref v);
                    filtered[p] = MedianFind(v);
                    //filtered[p] = v.OrderBy(z => z).ToList()[4];
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化1_1(byte[] pixels, int width, int height)
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
                    v[1] = pixels[p - 1];//左
                    v[2] = pixels[p + stride - 1];//左下
                    v[3] = pixels[p - stride];//上
                    v[4] = pixels[p];
                    v[5] = pixels[p + stride];//下
                    v[6] = pixels[p - stride + 1];//右上
                    v[7] = pixels[p + 1];//右
                    v[8] = pixels[p + stride + 1];//右下
                    //ソートして中央の値(5番目)を新しい値にする
                    MedianSort1(ref v);
                    MedianSort2(ref v);
                    filtered[p] = MedianFind(v);
                    //filtered[p] = v.OrderBy(z => z).ToList()[4];
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        //ようやくできた！！！！！！
        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化5(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数
            byte[] v = new byte[9];
            //中央列
            v[3] = pixels[0];
            v[4] = pixels[stride];
            v[5] = pixels[stride * 2];
            MedianSortByte(ref v, 3);//sort
            //右列
            v[6] = pixels[1];
            v[7] = pixels[stride + 1];
            v[8] = pixels[stride * 2 + 1];
            MedianSortByte(ref v, 6);

            //ソートの結果を値コピー
            byte v3 = v[3];
            byte v4 = v[4];
            byte v5 = v[5];
            byte v6 = v[6];
            byte v7 = v[7];
            byte v8 = v[8];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    //中央列と右列を左側にシフト
                    v[0] = v3;
                    v[1] = v4;
                    v[2] = v5;
                    v[3] = v6;
                    v[4] = v7;
                    v[5] = v8;
                    //中央列は次回に使うので記録
                    v3 = v[3];
                    v4 = v[4];
                    v5 = v[5];

                    int p = x + y * stride;//注目ピクセルの位置
                    //右列
                    v[6] = pixels[p - stride + 1];
                    v[7] = pixels[p + 1];
                    v[8] = pixels[p + stride + 1];
                    MedianSortByte(ref v, 6);//byte sort
                    //一時記憶
                    v6 = v[6];
                    v7 = v[7];
                    v8 = v[8];

                    MedianSortVerticalUnit(ref v);//unit sort

                    filtered[p] = MedianFindVerticalUnit(v);
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }




        private void MedianSortVerticalUnit(ref byte[] v)
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

        //unit0 > unit1が確定でunit2が新規のとき
     



        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化4(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            //列unit
            byte[] unit1 = new byte[3] { pixels[0], pixels[stride], pixels[stride * 2] };
            byte[] unit2 = new byte[3] { pixels[0], pixels[stride], pixels[stride * 2] };
            byte[] unit3 = new byte[3] { pixels[0], pixels[stride + 1], pixels[stride * 2 + 1] };

            byte[][] units = new byte[3][] { unit1, unit2, unit3 };

            MedianSortByte(ref unit2);//byte sort
            MedianSortByte(ref unit3);


            //ソートの結果を値コピー
            var bb = new byte[3];//ここはvar bb=u2[1];だと参照になるのでおかしくなる
            bb[0] = units[1][0];
            bb[1] = units[1][1];
            bb[2] = units[1][2];
            var cc = new byte[3];//ここはvar cc=u2[2];だと参照になるのでおかしくなる
            cc[0] = units[2][0];
            cc[1] = units[2][1];
            cc[2] = units[2][2];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    units[0] = bb;
                    units[1] = cc;
                    int p = x + y * stride;//注目ピクセルの位置
                    units[2][0] = pixels[p - stride + 1];//右上
                    units[2][1] = pixels[p + 1];//右
                    units[2][2] = pixels[p + stride + 1];//右下
                    MedianSortByte(ref units[2]);//byte sort
                    //一時記憶
                    //bb[0] = units[1][0];//これも参照になるから不適当
                    //bb = units[1].ToArray();//これなら適当だけど遅い
                    //cc = units[2].ToArray();
                    //units[1].CopyTo(bb, 0);//これも参照になるから不適当
                    //units[2].CopyTo(cc, 0);
                    //Array.Copy(units[1], bb, 3);//これも参照になるから不適当
                    //Array.Copy(units[2], cc, 3);
                    bb = new byte[] { units[1][0], units[1][2], units[1][1] };//LINQよりは速いけど遅い
                    cc = new byte[] { units[2][0], units[2][2], units[2][1] };

                    MedianSortUnit(ref units);
                    //MedianSortUnit(ref MyUnits);//unit sort
                    var ore = MedianSortUnit(units);

                    filtered[p] = MedianFind(units);
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }




        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化2_2(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            byte[] unit2;// = new byte[3];
            byte[] unit3;// = new byte[3];
            //上中下段unit
            MyUnits = new byte[3][] {
                new byte[3],
                new byte[] { pixels[0], pixels[stride], pixels[stride * 2] },
                new byte[] { pixels[1], pixels[stride + 1], pixels[stride * 2 + 1] } };
            MedianSortByte(ref MyUnits[1]);//byte sort
            MedianSortByte(ref MyUnits[2]);
            unit2 = MedianSortByte(MyUnits[1]);
            unit3 = MedianSortByte(MyUnits[2]);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    MyUnits[0] = unit2;
                    MyUnits[1] = unit3;
                    //MyUnits[0][0] = unit2[0];
                    //MyUnits[0][1] = unit2[1];
                    //MyUnits[0][2] = unit2[2];
                    //MyUnits[1][0] = unit3[0];
                    //MyUnits[1][1] = unit3[1];
                    //MyUnits[1][2] = unit3[2];
                    int p = x + y * stride;//注目ピクセルの位置
                    MyUnits[2][0] = pixels[p - stride + 1];//右上
                    MyUnits[2][1] = pixels[p + 1];//右
                    MyUnits[2][2] = pixels[p + stride + 1];//右下

                    MedianSortByte(ref MyUnits[2]);//byte sort
                    unit2 = MyUnits[1];//一時記憶
                    unit3 = MyUnits[2];

                    var neko = MyUnits;
                    //MedianSortUnit(ref MyUnits);//unit sort
                    var ore = MedianSortUnit(MyUnits);

                    filtered[p] = MedianFind(ore);
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }



        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化3(byte[] pixels, int width, int height)
        {
            //列unit
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            byte[] v = new byte[9];
            byte[] unit2 = new byte[3];
            byte[] unit3 = new byte[3];

            v[3] = pixels[0];
            v[4] = pixels[stride];
            v[5] = pixels[stride * 2];
            MedianSortByte(ref v, 3);
            unit2[0] = v[3];
            unit2[1] = v[4];
            unit2[2] = v[5];

            v[6] = pixels[1];
            v[7] = pixels[stride + 1];
            v[8] = pixels[stride * 2 + 1];
            MedianSortByte(ref v, 6);
            unit3[0] = v[6];
            unit3[1] = v[7];
            unit3[2] = v[8];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    v[0] = unit2[0];
                    v[1] = unit2[1];
                    v[2] = unit2[2];
                    v[3] = unit3[0];
                    v[4] = unit3[1];
                    v[5] = unit3[2];
                    int p = x + y * stride;//注目ピクセルの位置
                    v[6] = pixels[p + stride - 1];//左下
                    v[7] = pixels[p + stride];//下
                    v[8] = pixels[p + stride + 1];//右下
                    MedianSortByte(ref v, 6);

                    unit2[0] = v[3];
                    unit2[1] = v[4];
                    unit2[2] = v[5];
                    unit3[0] = v[6];
                    unit3[1] = v[7];
                    unit3[2] = v[8];

                    //ソートして中央の値(5番目)を新しい値にする
                    //MedianSort1(ref v);
                    MedianSort2(ref v);
                    filtered[p] = MedianFind2(v);
                    //filtered[p] = v.OrderBy(z => z).ToList()[4];
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

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

        private byte MedianFind2(byte[] vs)
        {
            if ((vs[2] >= vs[4] && vs[4] >= vs[6]) || (vs[6] >= vs[4] && vs[4] >= vs[2]))
            {
                return vs[4];
            }
            else if (vs[2] >= vs[4] && vs[6] >= vs[4])
            {
                return Math.Min(vs[2], Math.Min(vs[3], vs[6]));
            }
            else
            {
                return Math.Max(vs[2], Math.Max(vs[5], vs[6]));
            }
        }

        private (byte[] pixels, BitmapSource bitmap) Filterメディアン高速化2(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            //上中下段
            MyUnits = new byte[3][] {
                new byte[3],
                new byte[] { pixels[0], pixels[stride], pixels[stride * 2] },
                new byte[] { pixels[1], pixels[stride + 1], pixels[stride * 2 + 1] } };
            MedianSortByte(ref MyUnits[1]);//byte sort
            MedianSortByte(ref MyUnits[2]);
            List<byte> neko = new List<byte>();

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    MyUnits[0] = MyUnits[1];
                    MyUnits[1] = MyUnits[2];
                    //MyUnits[0][0] = MyUnits[1][0];
                    //MyUnits[0][1] = MyUnits[1][1];
                    //MyUnits[0][2] = MyUnits[1][2];
                    //MyUnits[1][0] = MyUnits[2][0];
                    //MyUnits[1][1] = MyUnits[2][1];
                    //MyUnits[1][2] = MyUnits[2][2];
                    int p = x + y * stride;//注目ピクセルの位置
                    MyUnits[2][0] = pixels[p - stride + 1];//右上
                    MyUnits[2][1] = pixels[p + 1];//右
                    MyUnits[2][2] = pixels[p + stride + 1];//右下
                    //ソートして中央の値(5番目)を新しい値にする
                    MedianSortByte(ref MyUnits[2]);//byte sort

                    MedianSortUnit(ref MyUnits);//unit sort
                    neko.Add(MedianFind(MyUnits));
                    filtered[p] = MedianFind(MyUnits);
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private void MedianSortUnit(ref byte[][] units)
        {
            //unitを中央値でソート
            byte[] temp;
            if (units[0][1] >= units[1][1])
            {
                if (units[1][1] >= units[2][1])
                {
                    // 0 1 2 そのまま
                }
                else if (units[0][1] >= units[2][1])
                {
                    // 0 2 1
                    temp = units[1];
                    units[1] = units[2];
                    units[2] = temp;
                }
                else
                {
                    // 2 0 1
                    temp = units[0];
                    units[0] = units[2];
                    units[2] = units[1];
                    units[1] = temp;
                }
            }
            else
            {
                // 1 > 0
                if (units[2][1] >= units[1][1])
                {
                    // 2 1 0
                    temp = units[0];
                    units[0] = units[2];
                    units[2] = temp;
                }
                else if (units[0][1] >= units[2][1])
                {
                    // 1 0 2
                    temp = units[0];
                    units[0] = units[1];
                    units[1] = temp;
                }
                else
                {
                    // 1 2 0
                    temp = units[0];
                    units[0] = units[1];
                    units[1] = units[2];
                    units[2] = temp;
                }
            }
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

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            var rand = new Random();
            var vv = new byte[9];
            rand.NextBytes(vv);
            MedianSort1(ref vv);
            for (int i = 0; i < vv.Length; i++)
            {
                vv[i] = (byte)rand.Next(9);
            }
            MedianSort1(ref vv);
            MedianSort2(ref vv);
            var neko = MedianFind(vv);
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            var sw = new Stopwatch();
            sw.Start();
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン高速化1_1(
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
            MyTextBlock2.Text = $"処理時間 = {sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
