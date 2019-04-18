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
using MyNumericUpDownInteger;

namespace _20190418_画像のエッジ抽出
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        List<NumericUDInteger> MyNumericMatrix;
        NumericUDInteger MyNumericDiv;

        string ImageFileFullPath;
        BitmapSource MyBitmapOrigin;
        byte[] MyPixelsOrigin;//元画像、リセット用
        byte[] MyPixels;//一時保存用、エッジを元にぼかすときに使う
        //byte[] MyEdge;//エッジ抽出した画像、フィルタ用
        //byte[] MyPixels膨張収縮;//
        //byte[] MyFiltered;//フィルタを掛けた画像＆フィルタを掛ける用の画像

        public MainWindow()
        {
            InitializeComponent();

            this.Drop += MainWindow_Drop;
            AddNumericUD();
            MyNumericDiv.Value = 1;
            MyNumericMatrix[4].Value = 1;
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


            (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Gray8, 96, 96);

            MyImageOrigin.Source = MyBitmapOrigin;
            MyPixelsOrigin = MyPixels;
            //MyPixels膨張収縮 = MyPixels;
        }

        private (byte[] pixels, BitmapSource bitmap) Filter(byte[] pixels, int width, int height, int[][] weight, int div, int offset, bool absolute = false)
        {
            //int[][] weight = new int[][] {
            //    new int[] { 0, 1, 0 },
            //    new int[] { 1, 1, 1 },
            //    new int[] { 0, 1, 0 } };
            //int offset = 0;
            //int div = 5;
            if (div == 0) { return (pixels, MyBitmapOrigin); }
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
                    if (absolute) { v = Math.Abs(v); }//絶対値で取る
                    v /= div;
                    v += offset;
                    v = (v > 255) ? 255 : (v < 0) ? 0 : v;
                    filtered[p] = (byte)v;
                }
            }

            return (filtered, BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
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
                //MyEdge = MyPixels;
                //MyPixels膨張収縮 = MyPixels;
            }
        }

        private void AddNumericUD()
        {
            MyNumericMatrix = new List<NumericUDInteger>();
            NumericUDInteger nu;
            for (int i = 0; i < 3; i++)
            {
                var st = new StackPanel();
                st.Orientation = Orientation.Horizontal;
                MyStackPanel.Children.Add(st);
                for (int f = 0; f < 3; f++)
                {
                    nu = new NumericUDInteger();
                    nu.Width = 50;
                    nu.Max = 255;
                    nu.Min = -255;
                    nu.LargeChange = 10;
                    st.Children.Add(nu);
                    MyNumericMatrix.Add(nu);
                }
            }
            nu = new NumericUDInteger();
            nu.Max = 255;
            nu.Min = -255;
            nu.LargeChange = 10;
            MyStackPanel.Children.Add(nu);
            MyNumericDiv = nu;
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

        //ヌメリック群から配列作成
        private int[][] GetWeight()
        {
            int[][] weight = new int[3][] { new int[3], new int[3], new int[3] };

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    weight[y][x] = MyNumericMatrix[y * 3 + x].Value;
                }
            }
            return weight;
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


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int[][] weight = GetWeight();
            (byte[] pixels, BitmapSource bitmap) = Filter(MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight, weight, MyNumericDiv.Value, 0);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int[][] weight = GetWeight();
            (byte[] pixels, BitmapSource bitmap) = Filter(MyPixels, MyBitmapOrigin.PixelWidth, MyBitmapOrigin.PixelHeight, weight, MyNumericDiv.Value, 0, true);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

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


        private void SetNumericValue(int[] vs)
        {
            for (int i = 0; i < MyNumericMatrix.Count; i++)
            {
                MyNumericMatrix[i].Value = vs[i];
            }
        }

        private void Button_Click_ぼかし1(object sender, RoutedEventArgs e)
        {
            int[] vs = new int[] { 0, 1, 0, 1, 1, 1, 0, 1, 0 };
            SetNumericValue(vs);
            MyNumericDiv.Value = 5;
        }

        private void Button_Click_ぼかし2(object sender, RoutedEventArgs e)
        {
            //MyNumericMatrix.Select(x => x.Value = 1);
            for (int i = 0; i < MyNumericMatrix.Count; i++)
            {
                MyNumericMatrix[i].Value = 1;
            }
            MyNumericDiv.Value = 9;
        }

        private void Button_Click_シャープネス(object sender, RoutedEventArgs e)
        {
            int[] vs = new int[] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
            SetNumericValue(vs);
            MyNumericDiv.Value = 1;
        }

        private void Button_Click_ラプラシアン(object sender, RoutedEventArgs e)
        {
            int[] vs = new int[] { 0, -1, 0, -1, 4, -1, 0, -1, 0 };
            SetNumericValue(vs);
            MyNumericDiv.Value = 1;
        }

        private void Button_Click_ガウシアン(object sender, RoutedEventArgs e)
        {
            int[] vs = new int[] { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            SetNumericValue(vs);
            MyNumericDiv.Value = 16;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {

        }
    }
}
