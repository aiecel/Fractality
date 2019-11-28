using System;
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
        
        private void RenderButton_OnClick(object sender, RoutedEventArgs e)
        {
            FractalDrawer drawer = 
                new MandelbrotDrawer(int.Parse(WidthResBox.Text), int.Parse(HeightResBox.Text));
            RenderImage.Source = drawer.Render();
        }
    }
}