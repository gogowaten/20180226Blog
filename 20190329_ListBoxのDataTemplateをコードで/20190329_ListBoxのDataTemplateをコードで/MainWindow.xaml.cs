using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//ListBoxのItemTemplate(DataTemplate)をC#コードで ( ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15918212.html


//ListBoxの要素全部に同じ背景画像(市松模様)を表示したい
//要素の表示方法変更はDataTemplateを使う
//コードでDataTemplateを作成すれば任意の背景画像を指定できる
namespace _20190329_ListBoxのDataTemplateをコードで
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<int> myData = new List<int>() { 12, 345, 6789, 0 };
            DataContext = myData;

            //コードでDataTemplate設定
            MyListBox.ItemTemplate = CreateDataTemplate();
        }

        //DataTemplate作成
        private DataTemplate CreateDataTemplate()
        {
            //市松模様ブラシ作成
            ImageBrush myPattern = MakeTileBrush(MakeCheckeredPattern(10, Colors.LightGray));
            //市松模様表示用にBorder作成
            var eBorder = new FrameworkElementFactory(typeof(Border));
            eBorder.SetValue(Border.BackgroundProperty, myPattern);//背景市松模様
            eBorder.SetValue(WidthProperty, 100.0);
            eBorder.SetValue(HeightProperty, 20.0);

            //値表示用にTextBlock作成
            var eTextBlock = new FrameworkElementFactory(typeof(TextBlock));
            //表示する値はBindingしたものにする設定
            eTextBlock.SetBinding(TextBlock.TextProperty, new Binding());
            //eTextBlock.SetValue(TextBlock.BackgroundProperty, myPattern);//背景市松模様
            

            //上の2つを入れるStackPanel作成、スタック方向はHorizontal
            var eStackPanel = new FrameworkElementFactory(typeof(StackPanel));
            eStackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            eStackPanel.AppendChild(eBorder);
            eStackPanel.AppendChild(eTextBlock);

            //DataTemplate作成、VisualTreeに上のStackPanelを指定で完成
            var dt = new DataTemplate();
            dt.VisualTree = eStackPanel;
            return dt;
        }



        /// <summary>
        /// 市松模様画像作成
        /// </summary>
        /// <param name="cellSize">タイル1辺のサイズ</param>
        /// <param name="gray">白じゃない方の色指定</param>
        /// <returns></returns>
        private WriteableBitmap MakeCheckeredPattern(int cellSize, Color gray)
        {
            int width = cellSize * 2;
            int height = cellSize * 2;
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.Format.BitsPerPixel / 8 * width;
            byte[] pixels = new byte[stride * height];
            int p = 0;
            Color iro;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((y < cellSize & x < cellSize) | (y >= cellSize & x >= cellSize))
                    {
                        iro = Colors.White;
                    }
                    else { iro = gray; }

                    p = y * stride + x * 3;
                    pixels[p] = iro.R;
                    pixels[p + 1] = iro.G;
                    pixels[p + 2] = iro.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }

        /// <summary>
        /// BitmapからImageBrush作成
        /// 引き伸ばし無しでタイル状に敷き詰め
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private ImageBrush MakeTileBrush(BitmapSource bitmap)
        {
            var imgBrush = new ImageBrush(bitmap);
            imgBrush.Stretch = Stretch.Uniform;//これは必要ないかも
            //タイルモード、タイル
            imgBrush.TileMode = TileMode.Tile;
            //タイルサイズは元画像のサイズ
            imgBrush.Viewport = new Rect(0, 0, bitmap.Width, bitmap.Height);
            //タイルサイズ指定方法は絶対値、これで引き伸ばされない
            imgBrush.ViewportUnits = BrushMappingMode.Absolute;
            return imgBrush;
        }

    }
}
