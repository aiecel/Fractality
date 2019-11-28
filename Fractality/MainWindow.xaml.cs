using System.Windows;
using System.Windows.Input;

namespace Fractality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MandelbrotDrawer drawer = new MandelbrotDrawer();
        private int renderWidth = 0;
        private int renderHeight = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void RenderButton_OnClick(object sender, RoutedEventArgs e)
        {
            Render();
        }

        private void RenderImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(RenderImage);
            var pixelWidth = RenderImage.Source.Width;
            var pixelHeight = RenderImage.Source.Height;
            var xClick = pixelWidth * clickPoint.X / RenderImage.ActualWidth;
            var yClick = pixelHeight * clickPoint.Y / RenderImage.ActualHeight;
            UpdateOrigin(xClick, yClick);
            drawer.MultiplyFactor *= 2;
            Render();
        }
        
        private void RenderImage_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(RenderImage);
            var pixelWidth = RenderImage.Source.Width;
            var pixelHeight = RenderImage.Source.Height;
            var xClick = RenderImage.ActualWidth - pixelWidth * clickPoint.X / RenderImage.ActualWidth;
            var yClick = RenderImage.ActualHeight - pixelHeight * clickPoint.Y / RenderImage.ActualHeight;
            UpdateOrigin(xClick, yClick);
            drawer.MultiplyFactor /= 2;
            Render();
        }

        private void UpdateOrigin(double xClick, double yClick)
        {
            var ratio = (double) renderWidth / renderHeight;
            var areaHeight = 4d / drawer.MultiplyFactor;
            var areaWidth = areaHeight * ratio;
            
            drawer.OriginX = drawer.OriginX - areaWidth / 2d + xClick * (areaWidth / renderWidth);
            drawer.OriginY = drawer.OriginY + areaHeight / 2d - yClick * (areaHeight / renderHeight);
        }

        private void Render()
        {
            renderWidth = int.Parse(WidthResBox.Text);
            renderHeight = int.Parse(HeightResBox.Text);
            RenderImage.Source = 
                drawer.Render(renderWidth, renderHeight);
        }
    }
}