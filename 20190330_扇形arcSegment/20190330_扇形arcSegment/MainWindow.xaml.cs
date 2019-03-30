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
//c# - WPF Doughnut ProgressBar - Stack Overflow
//https://stackoverflow.com/questions/36752183/wpf-doughnut-progressbar

namespace _20190330_扇形arcSegment
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyTest();
            MyTest2();

        }

        private void MyTest()
        {

            //var arc = new ArcSegment(new Point(130,50), new Size(80,45), 0.0, true, SweepDirection.Clockwise, true);

            var bind = MyBinding(sliderSX, sliderSY, myPathFigre, PathFigure.StartPointProperty);
            bind.Converter = new ConverterPoint();
            BindingOperations.SetBinding(myPathFigre, PathFigure.StartPointProperty, bind);

            //bind = MyBinding(sliderX, sliderY, mySegment, ArcSegment.PointProperty);
            //bind.Converter = new ConverterPoint();
            //BindingOperations.SetBinding(mySegment, ArcSegment.PointProperty, bind);

            bind = MyBinding(sliderSizeX, sliderSizeY, mySegment, ArcSegment.SizeProperty);
            bind.Converter = new ConverterSize();
            BindingOperations.SetBinding(mySegment, ArcSegment.SizeProperty, bind);
        }
        private void MyTest2()
        {
            var mBind = MyBinding2(sliderMin, sliderMax, sliderValue);
            mBind.Converter = new ConverterValue();
            BindingOperations.SetBinding(mySegment, ArcSegment.PointProperty, mBind);

            mBind = MyBinding2(sliderMin, sliderMax, sliderValue);
            mBind.Converter = new ConverterIsLarge();
            BindingOperations.SetBinding(mySegment, ArcSegment.IsLargeArcProperty, mBind);
        }
        private MultiBinding MyBinding2(Slider min,Slider max,Slider value)
        {
            var bind1 = new Binding();
            bind1.Source = min;
            bind1.Path = new PropertyPath(Slider.ValueProperty);
            var bind2 = new Binding();
            bind2.Source = max;
            bind2.Path = new PropertyPath(Slider.ValueProperty);
            var bind3 = new Binding();
            bind3.Source = value;
            bind3.Path = new PropertyPath(Slider.ValueProperty);

            var mBind = new MultiBinding();
            mBind.Bindings.Add(bind1);
            mBind.Bindings.Add(bind2);
            mBind.Bindings.Add(bind3);
            return mBind;
        }

        private MultiBinding MyBinding(Slider x, Slider y, DependencyObject target, DependencyProperty property)
        {
            var bind1 = new Binding();
            bind1.Source = x;
            bind1.Path = new PropertyPath(Slider.ValueProperty);

            var bind2 = new Binding();
            bind2.Source = y;
            bind2.Path = new PropertyPath(Slider.ValueProperty);

            var mBind = new MultiBinding();
            mBind.Bindings.Add(bind1);
            mBind.Bindings.Add(bind2);
            return mBind;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio.IsChecked == true)
            {
                mySegment.SweepDirection = SweepDirection.Clockwise;
            }
            else
            {
                mySegment.SweepDirection = SweepDirection.Counterclockwise;
            }
        }
    }

    public class ConverterPoint : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new Point((double)values[0], (double)values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ConverterSize : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new Size((double)values[0], (double)values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterValue : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double minimum = (double)values[0];
            double maximum = (double)values[1];
            double value = (double) values[2];
            double current = value / (maximum - minimum) * 360;
            if (current >= 360) { current = 359.99; }
            current -= 90;
            current = current * (Math.PI / 180.0);
            double x = 100 + 100 * Math.Cos(current);
            double y = 100 + 100 * Math.Sin(current);
            return new Point(x, y);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ConverterIsLarge : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double minimum = (double)values[0];
            double maximum = (double)values[1];
            double value = (double)values[2];

            return (value * 2) >= (maximum - minimum);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
