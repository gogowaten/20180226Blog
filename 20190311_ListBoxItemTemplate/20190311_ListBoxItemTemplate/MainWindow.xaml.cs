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


//Listboxに色パレット作成するのは向いていない
//クリックした色を変更する処理の書き方がわからない
//リストItemをBorderで作ったとして、それをクリックしたときに元のデータにたどり着けないから
//どのデータを変更すればいいのかわからない
//parentでたどってもnullになってしまう
//どのListBoxの何番目のItemをクリックしたとか選択したとかが取得できない、方法がわからない
//Window.ResourceでのTemplateを指定しないでListboxの中に直接書いてけばできるかもしれないけど
//それだとコードが長くなる
namespace _20190311_ListBoxItemTemplate
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<MyData> myDatas;
        public MainWindow()
        {
            InitializeComponent();

            myDatas = new ObservableCollection<MyData>() {
                new MyData { Name = "red", ColorBrush = new SolidColorBrush(Colors.Red) },
                new MyData { Name = "cyan", ColorBrush = new SolidColorBrush(Colors.Cyan) } };
            DataContext = myDatas;

            //MyListBox2.SelectionChanged += MyListBox2_SelectionChanged;
        }

        private void MyListBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if( MyListBox2.SelectedIndex > -1)
            //{

            //myDatas[MyListBox2.SelectedIndex] = new MyData() { Name = "ok", ColorBrush = new SolidColorBrush(Colors.Magenta) };
            //}
            
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            StackPanel sp =(StackPanel) tb.Parent;
            ListBox lb = (ListBox)sp.Parent;//null
            var se = sender as TextBlock;
            var se2 = se.Parent as StackPanel;
            var se3 = se2.Parent as ListBox;
            
            MessageBox.Show("");

        }

        private void MyListBox2_Click(object sender, RoutedEventArgs e)
        {
            var button = e.OriginalSource;
            var es = e.Source;
            var rs = e.RoutedEvent;
            var se = sender;
            var pa = sender as StackPanel;
            var neko = pa.Parent;
        }

        private void MyListBox2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var button = e.OriginalSource;
            var es = e.Source;
            var rs = e.RoutedEvent;
            
            var se = sender;
        }
    }

    public class MyData
    {
        public string Name { get; set; }
        public SolidColorBrush ColorBrush { get; set; }

    }
}
