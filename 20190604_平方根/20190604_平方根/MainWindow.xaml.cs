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

namespace _20190604_平方根
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var v = MySlider.Value;
            var x = 5.0;
            while (true)
            {
                var x2 = x - (x * x - v) / (x * v);
                if (Math.Abs(x2 - x) < 0.0001) { break; }
                x = x2;
            }
            MyLabel.Content = x.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var v = MySlider.Value;
            var x = 5.0;            
            while (true)
            {
                var x2 = (x + v / x) / v;
                if (Math.Abs(x2 - x) < 0.0001) break;
                x = x2;
            }
        }
    }
}
