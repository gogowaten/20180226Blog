using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;


//ListBoxで棒グラフ、MultiBindingを使ってListBox幅と要素幅を連動(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15910034.html


namespace _20190321_Listboxで棒グラフ2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            //ContentRenderedイベントでデータバインディングするのは
            //Bindingソースには実際に表示されたListBoxの横幅を使うから
            ContentRendered += MainWindow_ContentRendered;

        }

        //ContentRenderedイベントでデータバインディング
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            List<double> dList = new List<double>() { 1100, 2340, 330, 5328 };
            double max = dList.Max();
            List<MyData> myDatas = new List<MyData>();
            foreach (var item in dList)
            {
                myDatas.Add(new MyData(item, max));
            }
            DataContext = myDatas;//データバインディング
        }
    }

    //使用、2つのソースからターゲットに送る値に変換
    //MultiBindingのConverterで使う
    public class MyMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)values[0];
            double rate = (double)values[1];
            return (width * rate) - 20;//-20はパディングみたいなもの、適当に
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MyData
    {
        public double Value { get; set; }
        public double Rate { get; set; }//割合、比率用

        public MyData(double value, double max)
        {
            Value = value;
            Rate = value / max;
        }
    }
}
