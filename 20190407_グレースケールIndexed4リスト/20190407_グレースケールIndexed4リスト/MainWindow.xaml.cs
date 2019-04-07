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

namespace _20190407_グレースケールIndexed4リスト
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ImageFileFullPath;
        private byte[] MyPixels;
        private BitmapSource MyBitmap;
        private int ColorBit;
        private List<string> FilePathList;

        public MainWindow()
        {
            InitializeComponent();

            AllowDrop = true;
            Drop += MainWindow_Drop;
            ColorBit = 3;

        }


        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton r = sender as RadioButton;
            ColorBit = int.Parse((string)r.Content);
            MyImage.Source = ToReduceColor(MyPixels, ColorBit, MyBitmap);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var ne = MakeSaveFilePath(FilePathList[0]);
            BitmapSource source = (BitmapSource)MyImage.Source;
            SaveImage(new FormatConvertedBitmap(source, PixelFormats.Indexed4, null, 0));
        }

        private void ButtonSaves_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < FilePathList.Count; i++)
            {

                SaveToPngImage(FilePathList[i]);
            }
        }
        private void SaveToPngImage(string originPath)
        {
            var (pixels, bitmap) = MakeBitmapSourceAndByteArray(originPath, PixelFormats.Gray8, 96, 96);
            if (bitmap == null)
            {

            }
            else
            {
                BitmapSource source = ToReduceColor(pixels, ColorBit, bitmap);
                string filePath = MakeSaveFilePath(originPath);
                SaveImage(new FormatConvertedBitmap(source, PixelFormats.Indexed4, null, 0), filePath);
            }
        }

        private string MakeSaveFilePath(string originPath)
        {
            string aa = System.IO.Path.GetFileNameWithoutExtension(originPath);
            string dd = System.IO.Path.GetDirectoryName(originPath);
            return System.IO.Path.Combine(dd, aa) + "_.png";
        }
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            FilePathList = filePath.ToList();
            DataContext = FilePathList;

            DisplayImage(filePath[0]);

        }

        private void DisplayImage(string filePath)
        {
            var (pixels, bitmap) = MakeBitmapSourceAndByteArray(filePath, PixelFormats.Gray8, 96, 96);

            if (bitmap == null)
            {
                ImageFileFullPath = "";
                MyPixels = null;
                MyBitmap = null;
                MyImage.Source = null;
                MessageBox.Show("画像ファイルじゃないみたい");
            }
            else
            {
                ImageFileFullPath = filePath;
                MyPixels = pixels;
                MyBitmap = bitmap;
                MyImageOrigin.Source = bitmap;
                MyImage.Source = ToReduceColor(pixels, ColorBit, bitmap);
            }
        }

        private BitmapSource ToReduceColor(byte[] pixels, int bit, BitmapSource bitmap)
        {
            var pixelsNew = new byte[pixels.Length];
            var table = new byte[256];
            double step = 255.0 / (Math.Pow(2, bit) - 1);//1階調ぶんの値
            int shift = 8 - bit;
            for (int i = 0; i < 256; i++)
            {
                table[i] = (byte)((i >> shift) * step);
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                pixelsNew[i] = table[pixels[i]];
            }

            var source = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, 96, 96, PixelFormats.Gray8, null, pixelsNew, bitmap.PixelWidth);
            return source;

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

        private void SaveImage(BitmapSource source, string filePath)
        {
            while (System.IO.File.Exists(filePath))
            {
                filePath = MakeSaveFilePath(filePath);
            }

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
            {
                encoder.Save(fs);
            }
        }


        private void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            int select = lb.SelectedIndex;
            if(select > 0) { DisplayImage(FilePathList[select]); }            
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            int reduce = Panel.GetZIndex(MyImage);
            Panel.SetZIndex(MyImageOrigin, reduce + 1);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int reduce = Panel.GetZIndex(MyImage);
            Panel.SetZIndex(MyImageOrigin, reduce);
        }
    }
}
