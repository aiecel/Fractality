using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Fractality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MandelbrotRenderer _renderer = new MandelbrotRenderer();
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
        
        private void RenderToFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                var a = _renderer.Render(renderWidth, renderHeight);
                using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(a));
                    encoder.Save(stream);
                }
            }
        }
        
        private void ResetViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            _renderer.OriginX = 0;
            _renderer.OriginY = 0;
            _renderer.MultiplyFactor = 1;
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
            _renderer.MultiplyFactor *= double.Parse(ZoomFactorBox.Text);
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
            _renderer.MultiplyFactor /= double.Parse(ZoomFactorBox.Text);
            Render();
        }

        private void UpdateOrigin(double xClick, double yClick)
        {
            var ratio = (double) renderWidth / renderHeight;
            var areaHeight = 4d / _renderer.MultiplyFactor;
            var areaWidth = areaHeight * ratio;
            
            _renderer.OriginX = _renderer.OriginX - areaWidth / 2d + xClick * (areaWidth / renderWidth);
            _renderer.OriginY = _renderer.OriginY + areaHeight / 2d - yClick * (areaHeight / renderHeight);
        }

        private void Render()
        {
            renderWidth = int.Parse(WidthResBox.Text);
            renderHeight = int.Parse(HeightResBox.Text);
            _renderer.MaxIterations = int.Parse(MaxIterationsBox.Text);
            var watch = Stopwatch.StartNew();
            RenderImage.Source = _renderer.Render(renderWidth, renderHeight);
            watch.Stop();
            TimeText.Text = watch.ElapsedMilliseconds + "ms.";
            ZoomText.Text = "x" + _renderer.MultiplyFactor;
            OriginText.Text = "Origin: x=" + _renderer.OriginX + "; y=" + _renderer.OriginY;
        }
    }
}