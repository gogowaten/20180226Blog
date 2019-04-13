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

//[WPF.Graphics]:Gushwell's Dev Notes
//http://gushwell.ldblog.jp/tag/WPF.Graphics?p=2

namespace _20190413_文字の描画
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //DrawText("test");
            //DrawText2();
            DrawText3();
        }

        private void DrawText3()
        {
            MyImage.Source = MakeBitmap();
            MyImage.RenderTransform = new ScaleTransform(30, 30);
            RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);

            string str = "R255";

            DrawingGroup drawingGroup = new DrawingGroup();
            RenderOptions.SetEdgeMode(drawingGroup, EdgeMode.Aliased);

            using (var dc = drawingGroup.Open())
            {
                dc.DrawText(MakeFormattedText(str, Brushes.DimGray), new Point());
                dc.DrawText(MakeFormattedText(str, Brushes.WhiteSmoke), new Point(-1, -1));
                dc.DrawText(MakeFormattedText("G255", Brushes.DimGray), new Point(0,10));
                dc.DrawText(MakeFormattedText("G255", Brushes.WhiteSmoke), new Point(-1, 9));
                dc.DrawText(MakeFormattedText("B255", Brushes.DimGray), new Point(0, 20));
                dc.DrawText(MakeFormattedText("B255", Brushes.WhiteSmoke), new Point(-1, 19));

                dc.DrawText(MakeFormattedText("R255", Brushes.DimGray), new Point(40, 10));
                dc.DrawText(MakeFormattedText("R255", Brushes.WhiteSmoke), new Point(39, 9));

                dc.DrawText(MakeFormattedText("R255", Brushes.DimGray), new Point(0, 30));
                dc.DrawText(MakeFormattedText("R255", Brushes.WhiteSmoke), new Point(-1, 29));
                dc.DrawText(MakeFormattedText("G255", Brushes.DimGray), new Point(0, 40));
                dc.DrawText(MakeFormattedText("G255", Brushes.WhiteSmoke), new Point(-1, 39));
                dc.DrawText(MakeFormattedText("B255", Brushes.DimGray), new Point(0, 50));
                dc.DrawText(MakeFormattedText("B255", Brushes.WhiteSmoke), new Point(-1, 49));


            };
            DrawingImage drawingImage = new DrawingImage(drawingGroup);
            RenderOptions.SetEdgeMode(drawingImage, EdgeMode.Aliased);

            MyImage2.Source = drawingImage;
            //RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(MyImage2, EdgeMode.Aliased);

            FormattedText MakeFormattedText(string color, Brush brush)
            {
                //return new FormattedText(color, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("ＭＳ ゴシック"), 13, brush, 96);
                //return new FormattedText(color, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 11.5, brush, 96);
                return new FormattedText(color, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Meiryo UIGothic"), 11, brush, 96);
                //return new FormattedText(color, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Meiryo UI"), 9, brush, 96);
            }
        }


        private void DrawText2()
        {
            MyImage.Source = MakeBitmap();
            MyImage.RenderTransform = new ScaleTransform(30, 30);
            RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);

            string str = "A255";
            Geometry geometry = MakeTextGeometry(str, new Point(), 10);
            DrawingGroup drawingGroup = new DrawingGroup();
            GeometryDrawing geometryDrawing = new GeometryDrawing(Brushes.White, null, geometry);
            //GeometryDrawing geometryDrawing = new GeometryDrawing(Brushes.White, new Pen(Brushes.Black, 0.2), geometry);
            drawingGroup.Children.Add(geometryDrawing);
            drawingGroup.Children.Add(new GeometryDrawing(Brushes.White, null, MakeTextGeometry("R255", new Point(0, 10), 10)));
            drawingGroup.Children.Add(new GeometryDrawing(Brushes.White, null, MakeTextGeometry("G255", new Point(0, 20), 10)));
            DrawingImage drawingImage = new DrawingImage(drawingGroup);
            //RenderOptions.SetEdgeMode(drawingImage, EdgeMode.Aliased);
            MyImage2.Source = drawingImage;
            //RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);
            //RenderOptions.SetEdgeMode(MyImage2, EdgeMode.Aliased);
        }

        private void DrawText(string str)
        {
            var formatted = new FormattedText(str,
                System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface("Meiryo UI"), 100, Brushes.Red, 96);
            Geometry geo = formatted.BuildGeometry(new Point());

            //ImageSourceに表示
            //GeometryからDrawing作成からDrawingImage作成をImageSourceで表示
            //縁取りだけ
            //var drawing = new GeometryDrawing(null, new Pen(Brushes.Lime, 2), geo);
            //文字色と縁取り有り
            //var drawing = new GeometryDrawing(Brushes.Blue, new Pen(Brushes.Lime, 2), geo);
            //文字色だけ
            var drawing = new GeometryDrawing(Brushes.Blue, null, geo);
            var dImage = new DrawingImage(drawing);
            //MyImage.Source = dImage;

            //DrawingBrushを作成、パネルの背景にして表示、パネル全体に拡縮表示される
            //var db = new DrawingBrush(drawing);
            //MyGrid.Background = db;

            //Pathで表示
            var path1 = new Path();
            path1.Data = geo;
            path1.Stroke = Brushes.Aqua;
            path1.StrokeThickness = 3;
            //MyGrid.Children.Add(path1);

            Geometry pGeo = MakeTextGeometry("test" + "\n" + "test2", new Point(), new Typeface("Meiryo UI"), 100);


            //pGeo.AddGeometry((PathGeometry)MakeTextGeometry("test2", new Point(0,30), new Typeface("Meiryo UI")));
            Path path2 = new Path();
            path2.Stroke = Brushes.Aqua;
            path2.StrokeThickness = 3;
            path2.Data = pGeo;
            //MyGrid.Children.Add(path2);

            Rect r = pGeo.Bounds;
            RectangleGeometry rGeo = new RectangleGeometry(r);


            MyImage.Source = MakeBitmap();
            MyImage.RenderTransform = new ScaleTransform(30, 30);
            RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);

            string strKido = "";
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    strKido += "v100 ";
                }
                strKido += "\n";
                strKido += "\n";
            }
            Geometry mGeo = MakeTextGeometry(strKido, new Point(), new Typeface("MS ゴシック"), 12);
            //Geometry mGeo = MakeTextGeometry("v100", new Point(), new Typeface("Meiryo UI"), 0.5);
            MyPath.Data = mGeo;
            //MyPath.Stroke = Brushes.Red;
            //MyPath.StrokeThickness = 1;
            MyPath.Fill = Brushes.Black;

            mGeo = MakeTextGeometry(strKido, new Point(1, 1), new Typeface("MS ゴシック"), 12);
            MyPath2.Fill = Brushes.Red;
            //MyPath2.Data = mGeo;


            //            WPFでGDI風の描画1 DrawingGroup(ソフトウェア ) -Simple is best - Yahoo!ブログ
            //https://blogs.yahoo.co.jp/elku_simple/20807622.html

            //DrawingGroup group = new DrawingGroup();
            ////group.Append().DrawGeometry(Brushes.Red, new Pen(Brushes.Cyan,1), mGeo);
            //using (var context = group.Open())
            //{

            //    Geometry textGeometry = MakeTextGeometry(str, new Point(), new Typeface("Meiryo UI"), 20);
            //    context.DrawGeometry(Brushes.Cyan, null, textGeometry);
            //    context.DrawGeometry(Brushes.MediumAquamarine, null, MakeTextGeometry(str, new Point(1, 1), new Typeface("Meiryo UI"), 20));
            //}

            //var db2 = new DrawingBrush(group);
            //MyGrid.Background = db2;


            DrawingGroup dg2 = new DrawingGroup();
            dg2.Children.Add(drawing);
            dg2.Children.Add(new GeometryDrawing(Brushes.MediumOrchid, null, MakeTextGeometry("test2", new Point(1, 1), new Typeface("Meiryo UI"), 100)));
            //var db3 = new DrawingBrush(dg2);
            MyGrid.Background = new DrawingBrush(dg2);
        }

        private Geometry MakeTextGeometry(string str, Point point, double emSize)
        {
            Typeface typeface;
            typeface = new Typeface(new FontFamily("Meiryo UI"), FontStyles.Italic, FontWeights.Bold, FontStretches.ExtraCondensed);
            typeface = new Typeface(new FontFamily("MS ゴシック"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            //typeface = new Typeface("Meiryo UI", FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            var formatted = new FormattedText(str,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                emSize, new SolidColorBrush(), 96);
            return formatted.BuildGeometry(point);
        }

        private Geometry MakeTextGeometry(string str, Point point, Typeface typeface, double emSize)
        {
            var formatted = new FormattedText(str,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                emSize, new SolidColorBrush(), 96);
            return formatted.BuildGeometry(point);
        }

        private BitmapSource MakeBitmap()
        {
            int w, h, stride;
            w = h = 10;
            stride = w;

            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Gray8, null);
            var pixels = new byte[stride * h];
            wb.CopyPixels(pixels, stride, 0);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    pixels[y * stride + x] = (y % 2 == 0) ? (byte)0 : (byte)255;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }
    }
}
