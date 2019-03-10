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


//全部XAMLで書こうとしたけどムリ、できない
//BindingのparameterにImageのWidthを指定する方法がわからない
//RelativeSourceで渡そうとしたけどImageまででWidthまで届かない感じ
//MultiBindingも試そうとしたけど、そもそもMultiBindingの書き方がわからないし、真似して書いても入力候補が出ないから間違えそう
//結論
//parameterを渡すようなBindingは、XAMLよりC#で書いたほうが早い
//良かったのは、ImageのScaleのBinding、これはXAMLのほうがラク

namespace _20190310_BindingScaleCanvasImageXAML
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            //表示する画像ファイルのパス
            string filePath1 = @"D:\ブログ用\チェック用2\NEC_6221_2019_02_24_午後わてん_half.jpg";

            //Imageに画像表示
            MyImage1.Source = new BitmapImage(new Uri(filePath1));

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

    public class MYmy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyMulti : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {            
            return (double)values[0] * (double)values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
