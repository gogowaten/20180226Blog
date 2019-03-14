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

            };

            MyButton3.Click += (s, e) =>
            {
                Palette1.PaletteColors[0] = Colors.LimeGreen;
            };

            MyButton4.Click += (s, e) =>
            {
                Palette1.ChangeColors(MakeColorList(ColorCount));
                PaletteV2.SetColorList(MakeColorList(ColorCount));

            };
            MyButton5.Click += (s, e) =>
            {
                PaletteV2.SetColorList(MakeColorList(10));
            };
            MyButton6.Click += (s, e) =>
            {
                PaletteV2.SetColorList(MakeColorList(256));
            };
        }

        private void MyAdd()
        {

            List<Color> colors = MakeColorList(ColorCount);

            Palette1 = new Palette(colors);
            MyStackPanel.Children.Add(Palette1);

            PaletteV2 = new PaletteVer2(colors);
            MyStackPanel.Children.Add(PaletteV2);

            MyStackPanel.Children.Add(new PaletteVer2(colors));

        }

        private void Tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            tb.Background = new SolidColorBrush(Colors.Cyan);
        }

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
            MessageBox.Show("");
        }

        private void ClealPaletteColors()
        {
            for (int i = 0; i < 256; i++) { PaletteColors[i] = Colors.Transparent; }
        }

        //ColorListの入れ替え
        //相互Bindingしている背景色とPaletteColorは、どちらも指定していないと#00FFFFFFとTransparentになる
        //なので色リストを変更してもBindingは更新しなくていい
        //PaletteColorのすべてをTransparentoにして新しい色を入れていけばいい
        public void SetColorList(List<Color> colors)
        {
            AvailablePaletteColors = colors;
            ClealPaletteColors();//すべてTransparent

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
