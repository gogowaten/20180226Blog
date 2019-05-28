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

namespace _20190528_エッジとノイズ
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


            (MyPixels, MyBitmapOrigin) = MakeBitmapSourceAndByteArray(ImageFileFullPath, PixelFormats.Gray8, 96, 96);

            MyImageOrigin.Source = MyBitmapOrigin;
            MyPixelsOrigin = MyPixels;
        }

        //ラプラシアンでエッジ判定、しきい値以下をメディアンフィルタ
        private (byte[] pixels, BitmapSource bitmap) Filterメディアンしきい値(byte[] pixels, int width, int height, int threshold, bool absolute = false)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;
            int begin = stride + 1;
            int end = pixels.Length - stride - 1;
            for (int i = begin; i < end; i++)
            {
                byte edge = MakeLaplacian8Near(pixels, i, stride, absolute);
                filtered[i] = pixels[i];
                if (edge <= threshold)
                {
                    //メディアンフィルタ
                    var v = new byte[9];
                    v[0] = pixels[i - stride - 1];//注目ピクセルの左上
                    v[1] = pixels[i - stride];    //上
                    v[2] = pixels[i - stride + 1];//右上
                    v[3] = pixels[i - 1];         //左
                    v[4] = pixels[i];
                    v[5] = pixels[i + 1];         //右
                    v[6] = pixels[i + stride - 1];//左下
                    v[7] = pixels[i + stride];    //した
                    v[8] = pixels[i + stride + 1];//右下

                    //var neko = v.OrderBy(x => x).ToList()[4];
                    filtered[i] = v.OrderBy(x => x).ToList()[4];
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        /// <summary>
        /// エッジを残してぼかし処理、ラプラシアンフィルタでエッジ取得、しきい値以下ならぼかし処理、注目ピクセル*4-上下左右、PixelFormats.Gray8専用
        /// </summary>
        /// <param name="pixels">画像の輝度値配列</param>
        /// <param name="width">横ピクセル数</param>
        /// <param name="height">縦ピクセル数</param>
        /// <param name="threshold">エッジ判定用のしきい値、-1~255で指定、以下でぼかし処理</param>
        /// <param name="absolute">trueなら絶対値で計算</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアンしきい値(byte[] pixels, int width, int height, int threshold, bool absolute = false)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる

            //int count = 0;
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
                    int laplacian = total - pixels[p] * 4;//上下左右 - 注目ピクセル*4
                    if (absolute) laplacian = Math.Abs(laplacian);//絶対値で計算

                    //0～255の間に収める
                    laplacian = laplacian < 0 ? 0 : laplacian > 255 ? 255 : laplacian;
                    //しきい値以下ならぼかし処理
                    if (laplacian <= threshold)
                    {
                        //count++;
                        //double average = (total + pixels[p]) / 5.0;
                        //byte neko = (byte)average;
                        //byte neko2 = (byte)((total + pixels[p]) / 5.0);
                        //if (neko != neko2) { var uma = 0; }
                        filtered[p] = (byte)((total + pixels[p]) / 5.0);
                    }
                    else
                    {
                        filtered[p] = pixels[p];
                    }

                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }




        //近傍の値を1固定にして、中心だけ1～10にしてぼかし具合を変更する場合
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン割合(byte[] pixels, int width, int height, bool absolute = false)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる

            //int count = 0;
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
                    int laplacian = total - pixels[p] * 4;//上下左右 - 注目ピクセル*4
                    if (absolute) laplacian = Math.Abs(laplacian);//絶対値で計算
                    //0～255の間に収める
                    laplacian = laplacian < 0 ? 0 : laplacian > 255 ? 255 : laplacian;

                    //倍率決定、エッジが強いほど中心倍率を10に近づける
                    double rate = (laplacian / 255.0) * (10 - 1) + 1;
                    //rate = 100;
                    //平均値を新しい値にする
                    filtered[p] = (byte)((total + pixels[p] * rate) / (4 + rate));

                }
            }
            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }



        //ノイズ表示
        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン8近傍(byte[] pixels, int width, int height, int threshold, bool absolute = false)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int total;
            int begin = stride + 1;
            int end = pixels.Length - stride - 1;
            for (int i = begin; i < end; i++)
            {
                if (i % stride == 0 | i % stride == stride - 1) { continue; }
                total = 0;
                total += pixels[i - stride - 1];//注目ピクセルの左上
                total += pixels[i - stride];    //上
                total += pixels[i - stride + 1];//右上
                total += pixels[i - 1];         //左
                total += pixels[i + 1];         //右
                total += pixels[i + stride - 1];//左下
                total += pixels[i + stride];    //した
                total += pixels[i + stride + 1];//右下
                total -= pixels[i] * 8;
                if (absolute) total = Math.Abs(total);
                if (total > threshold)
                {
                    total = total < 0 ? 0 : total > 255 ? 255 : total;
                    filtered[i] = (byte)total;
                }
                else
                {
                    filtered[i] = 0;
                }
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }

        private byte MakeLaplacian8Near(byte[] pixels, int i, int stride, bool absolute)
        {
            if (i % stride == 0 | i % stride == stride - 1) { return 0; }
            int total = 0;
            total += pixels[i - stride - 1];//注目ピクセルの左上
            total += pixels[i - stride];    //上
            total += pixels[i - stride + 1];//右上
            total += pixels[i - 1];         //左
            total += pixels[i + 1];         //右
            total += pixels[i + stride - 1];//左下
            total += pixels[i + stride];    //した
            total += pixels[i + stride + 1];//右下
            total -= pixels[i] * 8;
            if (absolute) total = Math.Abs(total);
            total = total < 0 ? 0 : total > 255 ? 255 : total;
            return (byte)total;
        }

        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン8近傍2(byte[] pixels, int width, int height, int threshold, bool absolute = false)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;
            int begin = stride + 1;
            int end = pixels.Length - stride - 1;
            for (int i = begin; i < end; i++)
            {
                var v = MakeLaplacian8Near(pixels, i, stride, absolute);
                if (v > threshold) v = 0;
                filtered[i] = v;
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }



        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン8近傍重みあり(byte[] pixels, int width, int height, bool absolute = false)
        {
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int total;
            int begin = stride + 1;
            int end = pixels.Length - stride - 1;
            for (int i = begin; i < end; i++)
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
                total -= pixels[i] * 12;
                if (absolute) total = Math.Abs(total);

                total = total < 0 ? 0 : total > 255 ? 255 : total;
                filtered[i] = (byte)total;
            }

            return (filtered, BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Gray8, null, filtered, width));
        }


        private (byte[] pixels, BitmapSource bitmap) Filterラプラシアン5x5近傍(byte[] pixels, int width, int height, bool absolute = false)
        {
            //1, 1,  1, 1, 1
            //1, 1,  1, 1, 1
            //1, 1,-24, 1, 1
            //1, 1,  1, 1, 1
            //1, 1,  1, 1, 1
            byte[] filtered = new byte[pixels.Length];//処理後の輝度値用
            int stride = width;//一行のbyte数、Gray8は1ピクセルあたりのbyte数は1byteなのでwidthとおなじになる
            int total;
            int diff1 = -stride * 2 - 2;
            int diff2 = -stride - 2;
            int diff3 = -2;
            int diff4 = stride - 2;
            int diff5 = stride * 2 - 2;

            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    int p = y * stride + x;
                    total = 0;
                    for (int z = 0; z < 5; z++)
                    {
                        total += pixels[p + diff1 + z];
                        total += pixels[p + diff2 + z];
                        total += pixels[p + diff3 + z];
                        total += pixels[p + diff4 + z];
                        total += pixels[p + diff5 + z];
                    }
                    total -= pixels[p];
                    total -= pixels[p] * 24;
                    if (absolute) total = Math.Abs(total);

                    total = total < 0 ? 0 : total > 255 ? 255 : total;
                    filtered[p] = (byte)total;
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


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアンしきい値(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderThreshold.Value,
                (bool)CheckBoxAbsolute.IsChecked);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }




        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン割合(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (bool)CheckBoxAbsolute.IsChecked);
            MyImage.Source = bitmap;
            MyPixels = pixels;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン8近傍(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderThreshold2.Value,
                (bool)CheckBoxAbsolute.IsChecked);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン8近傍重みあり(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (bool)CheckBoxAbsolute.IsChecked);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン5x5近傍(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (bool)CheckBoxAbsolute.IsChecked);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterラプラシアン8近傍2(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderThreshold.Value,
                (bool)CheckBoxAbsolute.IsChecked);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (MyPixels == null) { return; }
            (byte[] pixels, BitmapSource bitmap) = Filterメディアンしきい値(
                MyPixels,
                MyBitmapOrigin.PixelWidth,
                MyBitmapOrigin.PixelHeight,
                (int)SliderThreshold.Value,
                (bool)CheckBoxAbsolute.IsChecked);
            MyImage.Source = bitmap;
            MyPixels = pixels;

        }
    }
}
