using System.Windows;
using System.Windows.Media.Imaging;
//WPF、アプリの実行ファイルに画像ファイルを埋め込んで、それを取り出して表示するまでの手順メモ(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15919321.html

namespace _20190331_画像埋め込み
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            System.Reflection.Assembly sra = System.Reflection.Assembly.GetExecutingAssembly();
            var bf = BitmapFrame.Create(sra.GetManifestResourceStream("_20190331_画像埋め込み.HSVRectValue.png"));
            MyImage.Source = bf;
            //1行で
            //MyImage.Source= BitmapFrame.Create(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("_20190331_画像埋め込み.HSVRectValue.png"));
        }

    }
}
