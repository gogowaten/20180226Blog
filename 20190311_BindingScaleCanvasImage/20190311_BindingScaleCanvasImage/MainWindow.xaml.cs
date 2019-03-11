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

using System.Globalization;

namespace _20190311_BindingScaleCanvasImage
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += MainWindow_ContentRendered;
            //表示する画像ファイルのパス
            string filePath1 = @"D:\ブログ用\チェック用2\NEC_6221_2019_02_24_午後わてん_half.jpg";

            //Imageに画像表示
            MyImage1.Source = new BitmapImage(new Uri(filePath1));
        }

        //アプリが表示された直後
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            //ソース Slider.Value
            //ターゲット CanvasのWidth
            //Canvas.Width = Slider.Value * Image.ActualWidthをにするためにConverter指定
            //ParameterにImage.Width            
            var b = new Binding();
            b.Source = SliderScale;
            b.Path = new PropertyPath(Slider.ValueProperty);
            b.ConverterParameter = MyImage1.ActualWidth;
            b.Converter = new MyConverter();
            MyCanvas1.SetBinding(WidthProperty, b);

            //↑のHeight版
            b = new Binding();
            b.Source = SliderScale;
            b.Path = new PropertyPath(Slider.ValueProperty);
            b.ConverterParameter = MyImage1.ActualHeight;
            b.Converter = new MyConverter();
            MyCanvas1.SetBinding(HeightProperty, b);
        }

    }

    //Value * parameterを返す
    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * (double)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
