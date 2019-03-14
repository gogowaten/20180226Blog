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
using System.Collections.ObjectModel;
using System.Globalization;


//色リストがあって、パレットのBorderとBinding

//動作
//色リストを渡して、パレット作成
//色リストと表示用BorderがBinding、クリックでCyanに
//色リスト変更で表示も一括変更

namespace _20190312
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyImageSource;
        byte[] MyImageByteArray;

        static int ColorCount = 20;
        Palette Palette1;
        PaletteVer2 PaletteV2;

        public MainWindow()
        {
            InitializeComponent();

            MyAdd();
            MyButton1.Click += (s, e) =>
            {
                Palette1.PaletteColors[0] = Colors.White;
            };

            MyButton2.Click += (s, e) =>
            {
                var bitmap = MyImage1.Source;
            };

            MyButton3.Click += (s, e) =>
            {
                Palette1.PaletteColors[0] = Colors.LimeGreen;
            };
            //色リスト変更
            MyButton4.Click += (s, e) =>
            {
                Palette1.ChangeColors(MakeColorList(ColorCount));
                PaletteV2.SetColorList(MakeColorList(ColorCount));

            };
            //色リスト変更
            MyButton5.Click += (s, e) =>
            {
                PaletteV2.SetColorList(MakeColorList(10));
            };
            //色リスト変更
            MyButton6.Click += (s, e) =>
            {
                PaletteV2.SetColorList(MakeColorList(256));
            };
        }

        private void MyAdd()
        {
            //画像ファイル表示
            string filePath = "";
            filePath = @"D:\ブログ用\チェック用2\NEC_6221_2019_02_24_午後わてん_half.jpg";
            (BitmapSource MyImageSource, byte[] MyImageByteArray) =
                GetBitmapSourceWithChangePixelFormat(filePath, PixelFormats.Rgb24, 96, 96);
            MyImage1.Source = MyImageSource;

            //色リスト作成からパレット作成
            List<Color> colors = MakeColorList(ColorCount);
            Palette1 = new Palette(colors);
            MyStackPanel.Children.Add(Palette1);

            PaletteV2 = new PaletteVer2(colors);
            MyStackPanel.Children.Add(PaletteV2);

            MyStackPanel.Children.Add(new PaletteVer2(colors));
            MyStackPanel.Children.Add(new PaletteVer2(colors));
            MyStackPanel.Children.Add(new PaletteVer2(colors));
        }

        #region 画像

        public BitmapSource Reduce(byte[] pixels, List<Color> palette)
        {
            byte[] pixels2 = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i += 3)
            {
                double distance = 0;
                double min = 1000;
                int index = 0;
                for (int j = 0; j < palette.Count; j++)
                {
                    distance = ColorDistance(pixels[i], pixels[i + 1], pixels[i + 2], palette[j]);
                    if (distance < min)
                    {
                        min = distance;
                        index = j;
                    }
                }
                pixels2[i] = palette[index].R;
                pixels2[i + 1] = palette[index].G;
                pixels2[i + 2] = palette[index].B;
            }
            int stride = MyImageSource.Format.BitsPerPixel / 8 * MyImageSource.PixelWidth;
            BitmapSource bitmap = BitmapSource.Create(MyImageSource.PixelWidth,
                MyImageSource.PixelHeight, 96, 96, MyImageSource.Format, null, pixels2, stride);
            return bitmap;
        }

        private double ColorDistance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            return Math.Sqrt(Math.Pow(r1 - r2, 2) + Math.Pow(g1 - g2, 2) + Math.Pow(b1 - b2, 2));
        }
        private double ColorDistance(byte r1, byte g1, byte b1, Color col)
        {
            return ColorDistance(r1, g1, b1, col.R, col.G, col.B);
        }

        /// <summary>
        ///  ファイルパスとPixelFormatを指定してBitmapSourceとそのbyte配列を作成、dpiの変更は任意
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsの中からどれかを指定</param>
        /// <param name="dpiX">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <param name="dpiY">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <returns></returns>
        private (BitmapSource, byte[]) GetBitmapSourceWithChangePixelFormat(
            string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            BitmapSource source = null;
            byte[] pixels = new byte[0];
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

            return (source, pixels);
        }


        #endregion

        #region ランダム色作成
        private List<Color> MakeColorList(int count)
        {
            var colList = new List<Color>();
            var rand = new Random();
            var rgb = new byte[3];
            for (int i = 0; i < count; i++)
            {
                rand.NextBytes(rgb);
                colList.Add(MakeColor(rgb));
            }
            return colList;
        }

        private Color MakeColor(byte[] rgb)
        {
            return Color.FromRgb(rgb[0], rgb[1], rgb[2]);
        }
        #endregion

    }


    //256colors
    public class PaletteVer2 : StackPanel
    {
        //List<Border> Pans;
        private List<Color> AvailablePaletteColors;//実際に使う色リスト、減色処理で使う
        private ObservableCollection<Color> PaletteColors;//表示に使う色リスト(256色以下のとき透明色を含んでいる)
        private List<Border> PalettePans;

        public PaletteVer2(List<Color> colorList)
        {
            this.Orientation = Orientation.Horizontal;
            this.Margin = new Thickness(1);
            PaletteColors = new ObservableCollection<Color>();
            AvailablePaletteColors = new List<Color>();

            AddPans();

            SetColorList(colorList);
            this.DataContext = PaletteColors;
        }

        private void AddPans()
        {
            var btn = new Button() { Content = "実行" };
            btn.Click += Btn_Click;
            this.Children.Add(btn);

            PalettePans = new List<Border>();
            for (int i = 0; i < 256; i++)
            {
                //MakePans
                PaletteColors.Add(Colors.Transparent);
                Border bo = new Border()
                {
                    Width = 18,
                    Height = 18,
                    //Margin = new Thickness(1,0,1,0),
                    BorderBrush = Brushes.AliceBlue,
                    BorderThickness = new Thickness(1),
                };
                bo.MouseLeftButtonDown += Bo_MouseLeftButtonDown;
                PalettePans.Add(bo);
                this.Children.Add(bo);

                //Binding
                var bind = new Binding();
                //bind.Source = PaletteColors;//this.Datacontext = PaletteColorsでも同じ結果
                bind.Path = new PropertyPath("[" + i + "]");//無理やりすぎる
                bind.Converter = new MyConverter();
                bind.Mode = BindingMode.TwoWay;
                bo.SetBinding(Border.BackgroundProperty, bind);

            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //private void ClealPaletteColors()
        //{
        //    for (int i = 0; i < 256; i++) { PaletteColors[i] = Colors.Transparent; }
        //}

        //ColorListの入れ替え
        //相互Bindingしている背景色とPaletteColorは、どちらも指定していないと#00FFFFFFとTransparentになる
        //なので色リストを変更してもBindingは更新しなくていい
        //PaletteColorのすべてをTransparentoにして新しい色を入れていけばいい
        public void SetColorList(List<Color> colors)
        {

            //if (colors.Count < this.AvailablePaletteColors.Count)
            //{
            //    for (int i = colors.Count; i < AvailablePaletteColors.Count; i++)
            //    {
            //        PaletteColors[i] = Colors.Transparent;
            //    }
            //}
            //AvailablePaletteColors = colors;
            ////ClealPaletteColors();//すべてTransparent

            for (int i = 0; i < colors.Count; i++)
            {
                PaletteColors[i] = colors[i];
            }

            int max = (colors.Count > AvailablePaletteColors.Count) ? colors.Count : AvailablePaletteColors.Count;
            for (int i = colors.Count; i < max; i++)
            {
                PaletteColors[i] = Colors.Transparent;
            }

            AvailablePaletteColors = colors.ToList();
        }

        //一つの色を入れ替え、背景色を変える
        private void Bo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border bo = sender as Border;
            SolidColorBrush brush = (SolidColorBrush)bo.Background;
            //int ind = PaletteColors.IndexOf(brush.Color);
            MessageBox.Show($"{brush.Color}をColors.cyanに変更");

            bo.Background = new SolidColorBrush(Colors.Cyan);
        }


    }

    public class Palette : StackPanel
    {
        //List<Border> Pans;
        public ObservableCollection<Color> PaletteColors;

        public Palette(List<Color> colorList)
        {
            this.Orientation = Orientation.Horizontal;
            PaletteColors = new ObservableCollection<Color>();
            //Pans = new List<Border>();

            for (int i = 0; i < colorList.Count; i++)
            {
                PaletteColors.Add(colorList[i]);
                Border bo = new Border()
                {
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(1),
                    BorderBrush = Brushes.AliceBlue,
                    BorderThickness = new Thickness(1)
                };

                var bind = new Binding();
                //bind.Source = PaletteColors;//this.Datacontext = PaletteColorsでも同じ結果
                bind.Path = new PropertyPath("[" + i + "]");//無理やりすぎる
                bind.Converter = new MyConverter();
                bind.Mode = BindingMode.TwoWay;
                bo.SetBinding(Border.BackgroundProperty, bind);
                bo.MouseLeftButtonDown += Bo_MouseLeftButtonDown;
                this.Children.Add(bo);
            }
            this.DataContext = PaletteColors;
        }

        //ColorListの入れ替え
        public void ChangeColors(List<Color> colors)
        {
            //PaletteColors = new ObservableCollection<Color>(colors);
            for (int i = 0; i < colors.Count; i++)
            {
                PaletteColors[i] = colors[i];
            }
        }

        //一つの色を入れ替え、背景色を変える
        private void Bo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border bo = sender as Border;
            bo.Background = new SolidColorBrush(Colors.Cyan);
        }

        //public ObservableCollection<Color> MyColorList
        //{
        //    get { return (ObservableCollection<Color>)GetValue(MyColorListProperty); }
        //    set { SetValue(MyColorListProperty, value); }
        //}

        //public static readonly DependencyProperty MyColorListProperty =
        //    DependencyProperty.Register(nameof(MyColorList), typeof(ObservableCollection<Color>), typeof(Palette));


        //public Color MyColor
        //{
        //    get { return (Color)GetValue(MyColorProperty); }
        //    set { SetValue(MyColorProperty, value); }
        //}

        //public static readonly DependencyProperty MyColorProperty =
        //    DependencyProperty.Register("MyColor", typeof(Color), typeof(Palette));


    }




    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = value as SolidColorBrush;
            return brush.Color;
        }
    }
}
