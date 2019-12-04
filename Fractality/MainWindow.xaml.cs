using System;
using System.ComponentModel;
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
        private MandelbrotRenderer renderer = new MandelbrotRenderer();
        private BackgroundWorker worker;
        private int renderWidth = 0;
        private int renderHeight = 0;
        private Stopwatch watch = new Stopwatch();
        private BitmapSource image;

        public MainWindow()
        {
            InitializeComponent();
            worker = (BackgroundWorker)FindResource("backgroundWorker");
            RenderImage.Source = image;
            Render();
        }
        
        private void RenderButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (worker.IsBusy)
            {
                SaveToFileButton.IsEnabled = true;
                ResetViewButton.IsEnabled = true;
                worker.CancelAsync();
                RenderButton.Content = "Render";
            }
            else
            {
                Render();
            }
        }
        
        private void RenderToFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                }
            }
        }
        
        private void ResetViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            renderer.OriginX = 0;
            renderer.OriginY = 0;
            renderer.MultiplyFactor = 1;
            Render();
        }

        private void RenderImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(RenderImage);
            var pixelWidth = RenderImage.Source.Width;
            var pixelHeight = RenderImage.Source.Height;
            var xClick = pixelWidth * clickPoint.X / RenderImage.ActualWidth;
            var yClick = pixelHeight * clickPoint.Y / RenderImage.ActualHeight;
            if (!worker.IsBusy)
            {
                UpdateOrigin(xClick, yClick);
                renderer.MultiplyFactor *= double.Parse(ZoomFactorBox.Text);
                Render();
            }
        }
        
        private void RenderImage_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(RenderImage);
            var pixelWidth = RenderImage.Source.Width;
            var pixelHeight = RenderImage.Source.Height;
            var xClick = RenderImage.ActualWidth - pixelWidth * clickPoint.X / RenderImage.ActualWidth;
            var yClick = RenderImage.ActualHeight - pixelHeight * clickPoint.Y / RenderImage.ActualHeight;
            UpdateOrigin(xClick, yClick);
            renderer.MultiplyFactor /= double.Parse(ZoomFactorBox.Text);
            Render();
        }

        private void UpdateOrigin(double xClick, double yClick)
        {
            var ratio = (double) renderWidth / renderHeight;
            var areaHeight = 4d / renderer.MultiplyFactor;
            var areaWidth = areaHeight * ratio;
            
            renderer.OriginX = renderer.OriginX - areaWidth / 2d + xClick * (areaWidth / renderWidth);
            renderer.OriginY = renderer.OriginY + areaHeight / 2d - yClick * (areaHeight / renderHeight);
        }

        private void Render()
        {
            SaveToFileButton.IsEnabled = false;
            ResetViewButton.IsEnabled = false;
            RenderButton.Content = "Stop render";
            renderWidth = int.Parse(WidthResBox.Text);
            renderHeight = int.Parse(HeightResBox.Text);
            renderer.MaxIterations = int.Parse(MaxIterationsBox.Text);
            RenderingPanel.Visibility = Visibility.Visible;
            RenderBar.Value = 0;
            watch.Reset();
            watch.Start();
            worker.RunWorkerAsync();
        }

        private void BackgroundWorker_OnDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var bitmap = renderer.Render(renderWidth, renderHeight, worker);
                bitmap.Freeze();
                e.Result = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }
        
        private void BackgroundWorker_OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            watch.Stop();
            RenderingPanel.Visibility = Visibility.Hidden;
            image = (BitmapSource) e.Result;
            RenderImage.Source = image;
            TimeText.Text = watch.ElapsedMilliseconds + "ms.";
            ZoomText.Text = "x" + renderer.MultiplyFactor;
            OriginText.Text = "Origin: x=" + renderer.OriginX + "; y=" + renderer.OriginY;
            SaveToFileButton.IsEnabled = true;
            ResetViewButton.IsEnabled = true;
            RenderButton.Content = "Render";
        }

        private void BackgroundWorker_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RenderBar.Value = e.ProgressPercentage;
        }
    }
}