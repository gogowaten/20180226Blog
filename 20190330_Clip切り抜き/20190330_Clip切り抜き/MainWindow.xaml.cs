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
//第9回 WPFの「グラフィックス」を学ぼう(2/2)：連載：WPF入門 - ＠IT
//https://www.atmarkit.co.jp/ait/articles/1102/02/news100_2.html

namespace _20190330_Clip切り抜き
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyClip();
        }
        private void MyClip()
        {
            //MyRectangle.Clip = new EllipseGeometry(new Rect(0, 0, 20, 20));
            //MyRectangle.Clip = new RectangleGeometry(new Rect(10, 10, 20, 20));
            MyWrapPanel.Children.Add(CreateBorder(null));

            var clip1 = new RectangleGeometry(new Rect(10, 10, 60, 90));
            var clip2 = new EllipseGeometry(new Rect(0, 0, 60, 60));
            var gg = new GeometryGroup();
            gg.FillRule = FillRule.EvenOdd;
            gg.Children.Add(clip1);
            gg.Children.Add(clip2);
            MyRectangle.Clip = gg;
            var cg = new CombinedGeometry(GeometryCombineMode.Exclude,clip1,clip2);
            //MyRectangle.Clip = cg;            
            MyWrapPanel.Children.Add(CreateBorder(cg));

            cg = new CombinedGeometry(GeometryCombineMode.Intersect, clip1, clip2);
            MyWrapPanel.Children.Add(CreateBorder(cg));

            cg = new CombinedGeometry(GeometryCombineMode.Union, clip1, clip2);            
            MyWrapPanel.Children.Add(CreateBorder(cg));

            
            cg = new CombinedGeometry(GeometryCombineMode.Xor, clip1, clip2);
            MyWrapPanel.Children.Add(CreateBorder(cg));

            cg = new CombinedGeometry(GeometryCombineMode.Union, clip1, clip2);
            var clip3 = new EllipseGeometry(new Rect(50, 0, 60, 60));
            cg = new CombinedGeometry(GeometryCombineMode.Union, cg, clip3);
            MyWrapPanel.Children.Add(CreateBorder(cg));

            gg = new GeometryGroup();
            gg.Children.Add(clip1);
            gg.Children.Add(clip2);
            gg.Children.Add(clip3);
            MyWrapPanel.Children.Add(CreateBorder(gg));

            gg = new GeometryGroup();
            gg.Children.Add(clip1);
            gg.Children.Add(clip2);
            gg.Children.Add(clip3);
            gg.FillRule = FillRule.Nonzero;
            MyWrapPanel.Children.Add(CreateBorder(gg));

        }

        private Border CreateBorder(Geometry clip)
        {
            var r = new Border
            {
                Clip = clip,
                Background = Brushes.YellowGreen,
                BorderBrush = Brushes.Khaki,
                BorderThickness = new Thickness(4.0),
                Width = 100,
                Height = 100,
                Margin = new Thickness(10.0),
            };
            return r;
        }
    }
}
