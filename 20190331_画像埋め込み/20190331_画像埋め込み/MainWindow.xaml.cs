using System.Windows;
using System.Windows.Media.Imaging;

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
