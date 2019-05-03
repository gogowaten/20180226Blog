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

namespace _20190503_エッジ抽出_ラプラシアン
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
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\蓮の花.png";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\SIDBA\Girl.bmp";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\とり.png";
            //ImageFileFullPath = @"D:\ブログ用\テスト用画像\ノイズ除去\風車.png";


            (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Gray8, 96, 96);

            MyImageOrigin.Source = MyBitmapOrigin;
            MyPixelsOrigin = MyPixels;
        }


        /// <summary>
        /// エッジ抽出、注目ピクセル*4-上下左右、PixelFormats.Gray8専用
        /// </summary>
        /// <param name="pixels">画像の輝度値配列</param>
        /// <param name="width">横ピクセル数</param>
        /// <param name="height">縦ピクセル数</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = x + y * stride;//注目ピクセルの位置
                    int total = 0;
                    total += pixels[p - stride];//上のピクセル
                    total += pixels[p - 1];//左
                    total += pixels[p + 1];//右
                    total += pixels[p + stride];//下
                    total = Math.Abs(pixels[p] * 4 - total);//注目ピクセル*4-上下左右
                    //0～255の間に収める
                    total = total < 0 ? 0 : total > 255 ? 255 : total;
                    filtered[p] = (byte)total;
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        /// <summary>
        /// エッジ抽出？、注目ピクセルと上下左右の差の平均、PixelFormats.Gray8専用
        /// </summary>
        /// <param name="pixels">画像の輝度値配列</param>
        /// <param name="width">横ピクセル数</param>
        /// <param name="height">縦ピクセル数</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン差の平均(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int p;
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    p = x + y * stride;//注目ピクセルの位置
                    int pValue = pixels[p];//注目ピクセルの輝度値
                    int total = 0;
                    total += Math.Abs(pValue - pixels[p - stride]);//注目ピクセルと上の差
                    total += Math.Abs(pValue - pixels[p - 1]);//左
                    total += Math.Abs(pValue - pixels[p + 1]);//右
                    total += Math.Abs(pValue - pixels[p + stride]);//下

                    filtered[p] = (byte)(total / 4);//差の平均、四捨五入したほうがいい？
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        //多少高速化？→変化なし
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン2(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int total;
            int start = stride + 1;
            int end = pixels.Length - stride - 1;
            for (int i = start; i < end; i++)
            {
                total = 0;
                total += pixels[i - stride];
                total += pixels[i - 1];
                total += pixels[i + 1];
                total += pixels[i + stride];
                total = Math.Abs(pixels[i] * 4 - total);
                total = total < 0 ? 0 : total > 255 ? 255 : total;
                filtered[i] = (byte)total;
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン8近傍(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int total;
            int start = stride + 1;
            int end = pixels.Length - stride - 1;
            for (int i = start; i < end; i++)
            {
                total = 0;
                total += pixels[i - stride - 1];
                total += pixels[i - stride];
                total += pixels[i - stride + 1];
                total += pixels[i - 1];
                total += pixels[i + 1];
                total += pixels[i + stride - 1];
                total += pixels[i + stride];
                total += pixels[i + stride + 1];
                total = Math.Abs(pixels[i] * 8 - total);
                total = total < 0 ? 0 : total > 255 ? 255 : total;
                filtered[i] = (byte)total;
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン8近傍重みあり(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int total;
            int start = stride + 1;
            int end = pixels.Length - stride - 1;
            for (int i = start; i < end; i++)
            {
                total = 0;
                total += pixels[i - stride - 1];
                total += pixels[i - stride] * 2;
                total += pixels[i - stride + 1];
                total += pixels[i - 1] * 2;
                total += pixels[i + 1] * 2;
                total += pixels[i + stride - 1];
                total += pixels[i + stride] * 2;
                total += pixels[i + stride + 1];
                total = Math.Abs(pixels[i] * 12 - total);
                total = total < 0 ? 0 : total > 255 ? 255 : total;
                filtered[i] = (byte)total;
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン8近傍重みあり差の平均(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int total;
            int start = stride + 1;
            int end = pixels.Length - stride - 1;
            int pValue;
            for (int i = start; i < end; i++)
            {
                pValue = pixels[i];
                total = 0;
                total += Math.Abs(pValue - pixels[i - stride - 1]);
                total += Math.Abs(pValue - pixels[i - stride]) * 2;
                total += Math.Abs(pValue - pixels[i - stride + 1]);
                total += Math.Abs(pValue - pixels[i - 1]) * 2;
                total += Math.Abs(pValue - pixels[i + 1]) * 2;
                total += Math.Abs(pValue - pixels[i + stride - 1]);
                total += Math.Abs(pValue - pixels[i + stride] * 2);
                total += Math.Abs(pValue - pixels[i + stride + 1]);
                filtered[i] = (byte)Math.Round(total / 12.0, MidpointRounding.AwayFromZero);
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
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン差の平均(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }



        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン2(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン8近傍(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン8近傍重みあり(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン8近傍重みあり差の平均(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
