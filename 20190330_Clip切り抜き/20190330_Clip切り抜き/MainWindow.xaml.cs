using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//第9回 WPFの「グラフィックス」を学ぼう(2/2)：連載：WPF入門 - ＠IT
//https://www.atmarkit.co.jp/ait/articles/1102/02/news100_2.html

//切り抜きClip、GeometryGroupとCombinedGeometry(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15921152.html

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
            //clip用Geometry
            var clip1 = new RectangleGeometry(new Rect(10, 10, 60, 90));
            var clip2 = new EllipseGeometry(new Rect(0, 0, 60, 60));
            var clip3 = new EllipseGeometry(new Rect(50, 0, 60, 60));

            //  1clip
            MyWrapPanel1.Children.Add(CreateBorderWithClip(null));//clipなしの原型
            MyWrapPanel1.Children.Add(CreateBorderWithClip(clip1));
            MyWrapPanel1.Children.Add(CreateBorderWithClip(clip2));
            MyWrapPanel1.Children.Add(CreateBorderWithClip(clip3));



            GeometryGroup geoGroup;
            //  2clip
            geoGroup = new GeometryGroup();
            geoGroup.Children.Add(clip1);
            geoGroup.Children.Add(clip2);
            geoGroup.FillRule = FillRule.EvenOdd;//初期値
            MyWrapPanel2.Children.Add(CreateBorderWithClip(geoGroup));

            geoGroup = new GeometryGroup();
            geoGroup.Children.Add(clip1);
            geoGroup.Children.Add(clip2);
            geoGroup.FillRule = FillRule.Nonzero;
            MyWrapPanel2.Children.Add(CreateBorderWithClip(geoGroup));

            //  3clip
            geoGroup = new GeometryGroup();
            geoGroup.Children.Add(clip1);
            geoGroup.Children.Add(clip2);
            geoGroup.Children.Add(clip3);
            geoGroup.FillRule = FillRule.EvenOdd;
            MyWrapPanel2.Children.Add(CreateBorderWithClip(geoGroup));

            geoGroup = new GeometryGroup();
            geoGroup.Children.Add(clip1);
            geoGroup.Children.Add(clip2);
            geoGroup.Children.Add(clip3);
            geoGroup.FillRule = FillRule.Nonzero;
            MyWrapPanel2.Children.Add(CreateBorderWithClip(geoGroup));



            CombinedGeometry comboGeo;
            //  2clip
            comboGeo = new CombinedGeometry(GeometryCombineMode.Exclude, clip1, clip2);
            MyWrapPanel3.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Intersect, clip1, clip2);
            MyWrapPanel3.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Union, clip1, clip2);
            MyWrapPanel3.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Xor, clip1, clip2);
            MyWrapPanel3.Children.Add(CreateBorderWithClip(comboGeo));


            //  3clip   同じモードを2回
            comboGeo = new CombinedGeometry(GeometryCombineMode.Exclude, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Exclude, comboGeo, clip3);
            MyWrapPanel4.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Intersect, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Intersect, comboGeo, clip3);
            MyWrapPanel4.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Union, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Union, comboGeo, clip3);
            MyWrapPanel4.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Xor, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Xor, comboGeo, clip3);
            MyWrapPanel4.Children.Add(CreateBorderWithClip(comboGeo));


            //  3clip   Excludeと他
            comboGeo = new CombinedGeometry(GeometryCombineMode.Exclude, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Intersect, comboGeo, clip3);
            MyWrapPanel5.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Exclude, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Union, comboGeo, clip3);
            MyWrapPanel5.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Exclude, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Xor, comboGeo, clip3);
            MyWrapPanel5.Children.Add(CreateBorderWithClip(comboGeo));


            //  3clip   Intersectと他
            comboGeo = new CombinedGeometry(GeometryCombineMode.Intersect, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Exclude, comboGeo, clip3);
            MyWrapPanel6.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Intersect, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Union, comboGeo, clip3);
            MyWrapPanel6.Children.Add(CreateBorderWithClip(comboGeo));

            comboGeo = new CombinedGeometry(GeometryCombineMode.Intersect, clip1, clip2);
            comboGeo = new CombinedGeometry(GeometryCombineMode.Xor, comboGeo, clip3);
            MyWrapPanel6.Children.Add(CreateBorderWithClip(comboGeo));

        }

        private Border CreateBorderWithClip(Geometry clip)
        {
            var r = new Border
            {
                Clip = clip,
                Background = Brushes.MediumAquamarine,
                BorderBrush = Brushes.Orchid,
                BorderThickness = new Thickness(4.0),
                Width = 100,
                Height = 100,
                Margin = new Thickness(4.0, 10.0, 4.0, 60.0),
            };
            return r;
        }
    }
}
