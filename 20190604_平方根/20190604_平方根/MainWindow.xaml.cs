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
using System.Diagnostics;


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
            MyLabelMySqrt.Content = x.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var x1 = MySqrt(MySlider.Value);
            var x2= Math.Sqrt(MySlider.Value);
            MyLabelMySqrt.Content = x1;
            MyLabelSqrt.Content = x2;
            MyLabelDiff.Content = Math.Abs(x2 - x1);
        }

        /// <summary>
        /// 平方根の近似値を求める
        /// </summary>
        /// <param name="v">平方根を求める対象の数値</param>
        /// <param name="acceptable">0より大きい数値で指定、誤差の許容値、これ以下になったら返す</param>
        /// <returns></returns>
        private double MySqrt(double v, double acceptable = 0.0001)
        {
            if (v < 0) { return double.NaN; }
            double x = 1.0;//初期値
            double x2;
            while (true)
            {
                x2 = (x + (v / x)) / 2.0;
                if (Math.Abs(x2 - x) < acceptable) break;
                x = x2;
            }
            return x2;
        }

        /// <summary>
        /// 整数だけで計算する平方根
        /// </summary>
        /// <param name="v">平方根を求める対象の数値</param>
        /// <param name="acceptable">1以上で指定、誤差の許容値、これ以下になったら返す</param>
        /// <returns></returns>
        private int MySqrtInt(int v, int acceptable = 1)
        {
            if (v < 0) v = -v;
            int x = 1;
            int x2;
            while (true)
            {
                x2 = (x + (v / x)) >> 1;//>> 1はビットシフト、意味は/2
                if (Math.Abs(x2 - x) <= acceptable) break;
                x = x2;
            }
            return x2;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int x = int.Parse(MyTextBox.Text);
            MyLabelMySqrt.Content = MySqrtInt(x);
            MyLabelSqrt.Content = Math.Sqrt(x);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var v = -20;
            for (int i = 0; i < 1000000; i++)
            {
                MySqrtInt(v);
            }
            sw.Stop();
            MyLabelMySqrtTime.Content = $"time = {sw.Elapsed.TotalSeconds.ToString("00.0000")}秒";
        }

        //普通のMathのSqrtのほうが速い、100倍以上速い！！！！！！！！！！
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var v = 20;
            for (int i = 0; i < 1000000; i++)
            {
                Math.Sqrt(v);
            }
            sw.Stop();
            MyLabelSqrtTime.Content = $"time = {sw.Elapsed.TotalSeconds.ToString("00.0000")}秒";
        }
    }
}
