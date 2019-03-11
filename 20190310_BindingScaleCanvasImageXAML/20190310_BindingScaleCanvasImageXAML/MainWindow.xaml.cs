using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;



//BindingのほとんどをXAMLで書いてみた
//最初はできないかと思った
//BindingのparameterにImageのWidthを指定する方法がわからない
//RelativeSourceで渡そうとしたけどImageまででWidthまで届かない感じ
//だったけどMultiBindingでなんとかできた

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
