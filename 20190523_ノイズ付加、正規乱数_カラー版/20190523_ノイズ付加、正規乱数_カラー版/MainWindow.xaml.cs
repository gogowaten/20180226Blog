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
//画像にノイズ付加するアプリ、カラー版(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15962326.html

namespace _20190523_ノイズ付加_正規乱数_カラー版
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
            //MyTest();

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


            (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Rgb24, 96, 96);

            MyImageOrigin.Source = MyBitmapOrigin;
            MyPixelsOrigin = MyPixels;
        }

        /// <summary>
        /// 画像にノイズ付加、RGBの3要素に別々のノイズ付加
        /// </summary>
        /// <param name="pixels">輝度値の配列、Rgb24専用</param>
        /// <param name="width">横ピクセル数</param>
        /// <param name="height">縦ピクセル数</param>
        /// <param name="sigma">標準偏差、輝度の変化幅になるので、それはノイズの大きさになる</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Addノイズ12(byte[] pixels, int width, int height, double sigma)
        {
            //疑似正規乱数生成
            var random = new Random();
            double MyRandom()
            {
                double temp = 0;
                for (int i = 0; i < 12; i++)
                {
                    temp += random.NextDouble();
                }
                return temp - 6.0;
            }

            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            //1ピクセル行のbyte数
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x++)
                {
                    int p = y * stride + x;
                    double v = (sigma * MyRandom()) + pixels[p];
                    v = v < 0 ? 0 : v > 255 ? 255 : v;
                    filtered[p] = (byte)v;

                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }


        //RGBの3要素に同じ値のノイズ付加
        private (byte[] pixels, BitmapSource bitmap) Addノイズ12_1(byte[] pixels, int width, int height, double sigma)
        {
            //疑似正規乱数生成
            var random = new Random();
            int p;
            double MyRandom()
            {
                double temp = 0;
                for (int i = 0; i < 12; i++)
                {
                    temp += random.NextDouble();
                }
                return temp - 6.0;
            }

            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            //1ピクセル行のbyte数
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x += 3)
                {
                    p = y * stride + x;
                    double z = sigma * MyRandom();//ノイズ
                    filtered[p] = RoundByte0to255(pixels[p] + z);//red
                    filtered[p + 1] = RoundByte0to255(pixels[p + 1] + z);//green
                    filtered[p + 2] = RoundByte0to255(pixels[p + 2] + z);//blue
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }

        //数値を0～255の間に丸めてbyte型にキャスト
        private byte RoundByte0to255(double value)
        {
            var b = value < 0 ? 0 : value > 255 ? 255 : value;
            return (byte)b;
        }




        //ボックス=ミュラー法
        private double RandomBoxMuller(double x, double y)
        {
            return Math.Sqrt(-2 * Math.Log(x)) * Math.Cos(2 * Math.PI * y);
            //return Math.Sqrt(-2 * Math.Log(x)) * Math.Sin(2 * Math.PI * y);
        }

        // 画像にノイズ付加、RGBの3要素に別々のノイズ付加
        //sigmaは標準偏差、輝度の変化幅になるので、それはノイズの大きさになる
        private (byte[] pixels, BitmapSource bitmap) Addボックスミュラー法cos(byte[] pixels, int width, int height, double sigma)
        {
            //乱数生成、ボックス=ミュラー法のCosを使う方だけ使用
            var random = new Random();
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            //1ピクセル行のbyte数
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x++)
                {
                    int p = y * stride + x;
                    //ノイズ付加
                    filtered[p] = RoundByte0to255(sigma * RandomBoxMuller(random.NextDouble(), random.NextDouble()) + pixels[p]);
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }

        //RGBの3要素に同じ値のノイズ付加
        private (byte[] pixels, BitmapSource bitmap) Addボックスミュラー法cos_2(byte[] pixels, int width, int height, double sigma)
        {
            //乱数生成、ボックス=ミュラー法のCosを使う方だけ使用
            var random = new Random();

            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            //1ピクセル行のbyte数
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x += 3)
                {
                    int p = y * stride + x;
                    //ノイズ付加
                    double z = sigma * RandomBoxMuller(random.NextDouble(), random.NextDouble());
                    filtered[p] = RoundByte0to255(pixels[p] + z);//red
                    filtered[p + 1] = RoundByte0to255(pixels[p + 1] + z);//green
                    filtered[p + 2] = RoundByte0to255(pixels[p + 2] + z);//blue
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }




        // 画像にノイズ付加、RGBの3要素に別々のノイズ付加
        //一様分布乱数(普通の乱数)、noiseは1～255で指定
        private (byte[] pixels, BitmapSource bitmap) AddNoise(byte[] pixels, int width, int height, int noise)
        {
            var random = new Random();
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x++)
                {
                    int p = y * stride + x;
                    double v = pixels[p] + random.Next(-noise, noise);
                    v = v < 0 ? 0 : v > 255 ? 255 : v;
                    filtered[p] = (byte)v;

                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }

        //RGBの3要素に同じ値のノイズ付加
        //一様分布乱数(普通の乱数)、noiseは1～255で指定
        private (byte[] pixels, BitmapSource bitmap) AddNoise2(byte[] pixels, int width, int height, int noise)
        {
            var random = new Random();
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x += 3)
                {
                    int p = y * stride + x;
                    double z = random.Next(-noise, noise);//ノイズ作成
                    filtered[p] = RoundByte0to255(pixels[p] + z);//red
                    filtered[p + 1] = RoundByte0to255(pixels[p + 1] + z);//green
                    filtered[p + 2] = RoundByte0to255(pixels[p + 2] + z);//blue
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }




        /// <summary>
        /// 画像にノイズ付加、RGBの3要素に別々のごま塩ノイズ付加
        /// </summary>
        /// <param name="pixels">ピクセルフォーマットRgb24画像のbyte配列専用</param>
        /// <param name="width">画像の横ピクセル数</param>
        /// <param name="height">縦</param>
        /// <param name="noise">ノイズの割合、0.0～1.0で指定</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Addカラーごま塩ノイズ(byte[] pixels, int width, int height, double noise)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            var random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x++)
                {
                    int p = y * stride + x;
                    filtered[p] = pixels[p];
                    //しきい値以下ならごま塩にする
                    if (random.NextDouble() <= noise)
                    {
                        filtered[p] = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;
                    }
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }

        //RGBの3要素に同じ値のごま塩ノイズ付加
        private (byte[] pixels, BitmapSource bitmap) Addごま塩ノイズ(byte[] pixels, int width, int height, double noise)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            var random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x += 3)
                {
                    int p = y * stride + x;
                    filtered[p] = pixels[p];
                    filtered[p + 1] = pixels[p + 1];
                    filtered[p + 2] = pixels[p + 2];
                    if (random.NextDouble() <= noise)
                    {
                        double z = random.NextDouble();
                        byte zz = z < 0.5 ? (byte)0 : (byte)255;
                        filtered[p] = zz;
                        filtered[p + 1] = zz;
                        filtered[p + 2] = zz;
                    }
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }

        private (byte[] pixels, BitmapSource bitmap) Addカラーごま塩ノイズ2(byte[] pixels, int width, int height, double noise)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width * 3;//Rgb24は1ピクセルあたり3byteなので横ピクセル数 * 3

            var random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x += 3)
                {
                    int p = y * stride + x;
                    filtered[p] = pixels[p];
                    filtered[p + 1] = pixels[p + 1];
                    filtered[p + 2] = pixels[p + 2];
                    if (random.NextDouble() <= noise)
                    {
                        filtered[p] = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;
                        filtered[p + 1] = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;
                        filtered[p + 2] = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;                        
                    }
                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Rgb24, null, filtered, stride));
        }


        private void Kakunin(byte[] pixels)
        {
            double average;//平均
            double total = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                total += pixels[i];
            }
            average = total / pixels.Length;

            //分散
            total = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                total += Math.Pow(pixels[i] - average, 2.0);
            }
            double variance = total / pixels.Length;
            //標準偏差
            double stdev = Math.Sqrt(variance);
            //表示
            MessageBox.Show($"平均＝{ average}\n分散＝{variance}\n標準偏差＝{ stdev}");
        }

        #region その他

        //画像ファイルドロップ時の処理
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            var (pixels, bitmap) = MakeBitmapSourceAndByteArray(filePath[0], PixelFormats.Rgb24, 96, 96);

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

        //クリップボードに画像をコピー
        private void BitmapToClipboard(BitmapSource source)
        {
            Clipboard.SetImage(source);
        }
        #endregion



        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addノイズ12(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            Kakunin(MyPixels);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addボックスミュラー法cos(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addカラーごま塩ノイズ(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value / SliderSTDEV.Maximum);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = AddNoise(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addノイズ12_1(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addボックスミュラー法cos_2(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = AddNoise2(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderSTDEV.Value);

            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addごま塩ノイズ(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value / SliderSTDEV.Maximum);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }


        //表示画像をクリップボードへコピー
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            Clipboard.SetImage((BitmapSource)MyImage.Source);

        }

        //クリップボードの画像を表示
        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            BitmapSource source = Clipboard.GetImage();
            if (source == null)
            {
                MessageBox.Show("クリップボードに画像はありませんでした");
                return;
            }
            else
            {
                //ピクセルフォーマットをRgb24に変換
                var rgb24 = new FormatConvertedBitmap(source, PixelFormats.Rgb24, null, 0);
                int w = rgb24.PixelWidth;
                int h = rgb24.PixelHeight;
                int stride = w * 3;
                byte[] pixels = new byte[h * stride];
                rgb24.CopyPixels(pixels, stride, 0);
                MyPixels = pixels;
                MyPixelsOrigin = pixels;
                MyBitmapOrigin = rgb24;
                MyImage.Source = rgb24;
                MyImageOrigin.Source = rgb24;
            }
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Addカラーごま塩ノイズ2(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                SliderSTDEV.Value / SliderSTDEV.Maximum);

            MyImage.Source = bitmap;
            MyPixels = pixels;
        }
    }
}
