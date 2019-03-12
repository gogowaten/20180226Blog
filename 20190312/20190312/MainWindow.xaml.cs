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
        List<MyData> myData = new List<MyData>();
        MyDatas myDataList;
        Palette Palette1;

        public MainWindow()
        {
            InitializeComponent();

            MyAdd();
            MyButton1.Click += (s, e) =>
            {
                myData[0].Color = Colors.White;//おk
                myDataList.MyDatasList[0].Color = Colors.White;//おk
                Palette1.PaletteColors[0] = Colors.White;
            };

            MyButton2.Click += (s, e) =>
            {
                myData[0] = new MyData() { Color = Colors.Red };//Bindingが外れる
                myDataList.MyDatasList[0] = new MyData() { Color = Colors.Red };//Bindingが外れる
            };

            MyButton3.Click += (s, e) =>
            {
                myData[0].Color = Colors.Lime;//おk
                myDataList.MyDatasList[0].Color = Colors.LightBlue;//おk
                Palette1.PaletteColors[0] = Colors.LimeGreen;
            };

            MyButton4.Click += (s, e) =>
            {
                Palette1.ChangeColors(MakeColorList(ColorCount));
            };
        }

        private void MyAdd()
        {
            var rand = new Random();
            var rgb = new byte[3];


            Binding bind;
            TextBlock tb;
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
            MyStackPanel.Children.Add(sp);
            List<Color> colors = MakeColorList(ColorCount);
            for (int i = 0; i < ColorCount; i++)
            {
                rand.NextBytes(rgb);
                myData.Add(new MyData() { Color = colors[i] });
                //sp.Children.Add(new Border() { Width = 16, Height = 16 });

                tb = new TextBlock() { Margin = new Thickness(1) };
                tb.MouseLeftButtonDown += Tb_MouseLeftButtonDown;
                sp.Children.Add(tb);

                //bind = new Binding("Color");
                bind = new Binding(nameof(MyData.Color));
                bind.Source = myData[i];
                tb.SetBinding(TextBlock.TextProperty, bind);

                bind = new Binding(nameof(MyData.Color));
                bind.Source = myData[i];
                bind.Mode = BindingMode.TwoWay;
                bind.Converter = new MyConverter();
                tb.SetBinding(TextBlock.BackgroundProperty, bind);

            }



            colors = MakeColorList(ColorCount);
            sp = new StackPanel() { Orientation = Orientation.Horizontal };
            MyStackPanel.Children.Add(sp);
            myDataList = new MyDatas(colors);
            for (int i = 0; i < ColorCount; i++)
            {
                tb = new TextBlock() { Margin = new Thickness(1) };
                tb.MouseLeftButtonDown += Tb_MouseLeftButtonDown;
                bind = new Binding();
                bind.Source = myDataList.MyDatasList[i];
                bind.Path = new PropertyPath(MyData.ColorProperty);
                tb.SetBinding(TextBlock.TextProperty, bind);
                sp.Children.Add(tb);

                bind = new Binding();
                bind.Source = myDataList.MyDatasList[i];
                bind.Path = new PropertyPath(MyData.ColorProperty);
                bind.Mode = BindingMode.TwoWay;
                bind.Converter = new MyConverter();
                tb.SetBinding(TextBlock.BackgroundProperty, bind);

            }

            Palette1 = new Palette(colors);
            MyStackPanel.Children.Add(Palette1);
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

    public class MyData : DependencyObject
    {
        //public Color Color { get; set; }
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(MyData));

    }

    public class MyDatas
    {
        public List<MyData> MyDatasList { get; set; }
        public MyDatas(List<Color> colors)
        {
            MyDatasList = new List<MyData>();
            for (int i = 0; i < colors.Count; i++)
            {
                MyDatasList.Add(new MyData() { Color = colors[i] });
            }
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
                    Margin = new Thickness(2),
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

        public void ChangeColors(List<Color> colors)
        {
            //PaletteColors = new ObservableCollection<Color>(colors);
            for (int i = 0; i < colors.Count; i++)
            {
                PaletteColors[i] = colors[i];
            }
        }

        private void Bo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border bo = sender as Border;
            bo.Background = new SolidColorBrush(Colors.Cyan);
            bo.Focus();
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
