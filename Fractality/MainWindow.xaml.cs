using System.Windows;

namespace Fractality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SomeButton_OnClick(object sender, RoutedEventArgs e)
        {
            FractalDrawer drawer = new MandelbrotDrawer(800, 600);
            MainImage.Source = drawer.Render();
        }
    }
}