using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace _20190310_BindingScaleCanvasImage
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

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            MyBinding();
        }

        private void MyBinding()
        {
            //Image拡大表示の補間法指定、今回はニアレストネイバー法
            RenderOptions.SetBitmapScalingMode(MyImage1, BitmapScalingMode.NearestNeighbor);

            //ScaleTransform作成してImageのRenderTransformに指定
            //これをしないと拡大できない
            var st = new ScaleTransform();
            MyImage1.RenderTransform = st;


            //ここからBinding
            //ソースはSliderのValue
            //ターゲットはImageのScaleTransformのXとY
            var b = new Binding();
            b.Source = SliderScale;
            b.Path = new PropertyPath(Slider.ValueProperty);
            BindingOperations.SetBinding(st, ScaleTransform.ScaleXProperty, b);
            BindingOperations.SetBinding(st, ScaleTransform.ScaleYProperty, b);

            //ソース Slider.Value
            //ターゲット CanvasのWidth
            //Canvas.Width = Slider.Value * Image.ActualWidthをにするためにConverter指定
            //ParameterにImage.Width
            b = new Binding();
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
