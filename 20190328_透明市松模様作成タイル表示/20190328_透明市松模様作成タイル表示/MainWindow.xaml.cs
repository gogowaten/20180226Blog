using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//無限の透明市松模様をWriteableBitmapとImageBrushのタイル表示で作成(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15917385.html

namespace _20190328_透明市松模様作成タイル表示
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WriteableBitmap bitmap = MakeCheckeredPattern(10, Colors.LightGray);
            MyGrid.Background = MakeTileBrush(bitmap);
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
