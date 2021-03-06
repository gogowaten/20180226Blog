﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

//Q092.Binding.RelativeSource の使い方がよくわからない - 周回遅れのブルース
//http://d.hatena.ne.jp/hilapon/20130405/1365143758
//wpf – バインディングConverterParameter - コードログ
//https://codeday.me/jp/qa/20181201/36915.html
//"ConverterParameterプロパティは依存関係プロパティではないため、バインドできません。
//しかし、代替の解決策があります。通常のBindingの代わりにmulti-value converterを使用することができます："

    //blog
//    ListBoxで棒グラフ、MultiBindingを使ってListBox幅と要素幅を連動(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15910034.html


namespace _20190321_ListBoxで棒グラフ_比率で伸縮
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
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
            List<double> myData = new List<double> { 0.5, 0.8, 0.3, 1.0 };
            DataContext = myData;//データバインディング
        }
    }

    //未使用
    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)parameter;
            double rate = (double)value;
            return (width * rate) - 20;//-20はパディングみたいなもの、適当に
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
            //return (width * rate) - 20;//-20はパディングみたいなもの、適当に
            return (width - 16) * rate;//こっちのほうがいい
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
