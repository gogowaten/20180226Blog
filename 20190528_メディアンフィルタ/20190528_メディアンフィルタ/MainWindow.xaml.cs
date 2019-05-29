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

//メディアンフィルタで画像のノイズ除去試してみた、WPF、C# ( ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15965377.html

namespace _20190528_メディアンフィルタ
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

        private (byte[] pixels, BitmapSource bitmap) Filterメディアン4近傍(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;
            byte[] v = new byte[5];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = x + y * stride;//注目ピクセルの位置
                    v[0] = pixels[p - stride];//注目ピクセルの上
                    v[1] = pixels[p - 1];//左
                    v[2] = pixels[p];
                    v[3] = pixels[p + 1];//右
                    v[4] = pixels[p + stride];//下

                    //ソートして中央の値(3番目)を新しい値にする
                    filtered[p] = v.OrderBy(z => z).ToList()[2];
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixels, BitmapSource bitmap) Filterメディアン4近傍斜め(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            byte[] v = new byte[5];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = x + y * stride;//注目ピクセルの位置
                    v[0] = pixels[p - stride - 1];//注目ピクセルの左上
                    v[1] = pixels[p - stride + 1];//右上
                    v[2] = pixels[p];
                    v[3] = pixels[p + stride - 1];//左下
                    v[4] = pixels[p + stride + 1];//右下

                    //ソートして中央の値(3番目)を新しい値にする
                    filtered[p] = v.OrderBy(z => z).ToList()[2];
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        //自身を除く8近傍のメディアン、要素数が偶数のメディアンなので中央の2つの値の平均値
        private (byte[] pixels, BitmapSource bitmap) Filterメディアン自身を除く(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;
            byte[] v = new byte[8];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = x + y * stride;//注目ピクセルの位置
                    v[0] = pixels[p - stride - 1];//注目ピクセルの左上
                    v[1] = pixels[p - stride];//上
                    v[2] = pixels[p - stride + 1];//右上
                    v[3] = pixels[p - 1];//左
                    //v[4] = pixels[p];
                    v[4] = pixels[p + 1];//右
                    v[5] = pixels[p + stride - 1];//左下
                    v[6] = pixels[p + stride];//下
                    v[7] = pixels[p + stride + 1];//右下
                    //ソートして4,5番目の平均値を四捨五入を新しい値にする
                    var temp = v.OrderBy(z => z).ToList();
                    //byte neko = (byte)Math.Round((temp[3] + temp[4]) / 2.0, MidpointRounding.AwayFromZero);
                    filtered[p] = (byte)Math.Round((temp[3] + temp[4]) / 2.0, MidpointRounding.AwayFromZero);
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private (byte[] pixels, BitmapSource bitmap) Filterメディアン24近傍(byte[] pixels, int width, int height)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;
            byte[] v = new byte[25];//注目ピクセルとその近傍24ピクセルの輝度値用

            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    //24近傍と注目ピクセルの輝度値収集
                    for (int i = 0; i < 5; i++)
                    {
                        int pp = (y + i - 2) * stride + x - 2;
                        for (int j = 0; j < 5; j++)
                        {
                            v[i * 5 + j] = pixels[pp + j];
                        }
                    }
                    var neko = v.OrderBy(z => z).ToList()[12];
                    //輝度値をソートして中央値(13番目)を新しい値にする
                    filtered[y * stride + x] = v.OrderBy(z => z).ToList()[12];
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
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン4近傍(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン4近傍斜め(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン自身を除く(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterメディアン24近傍(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
