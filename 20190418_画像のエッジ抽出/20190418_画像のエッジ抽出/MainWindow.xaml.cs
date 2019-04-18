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
using MyNumericUpDownInteger;

namespace _20190418_画像のエッジ抽出
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NumericUDInteger nu;
            for (int i = 0; i < 3; i++)
            {
                var st = new StackPanel();
                st.Orientation = Orientation.Horizontal;
                MyStackPanel.Children.Add(st);
                for (int f = 0; f < 3; f++)
                {
                    nu = new NumericUDInteger();
                    nu.Width = 50;
                    nu.Max = 255;
                    nu.Min = -255;
                    st.Children.Add(nu);
                }
            }
            nu = new NumericUDInteger();
            nu.Max = 255;
            nu.Min = -255;
            
            MyStackPanel.Children.Add(nu);
            
        }






        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int aa = Panel.GetZIndex(MyImage);
            Panel.SetZIndex(MyImageOrigin, aa + 1);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int aa = Panel.GetZIndex(MyImage);
            Panel.SetZIndex(MyImageOrigin, aa - 1);
        }
    }
}
