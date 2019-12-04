using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        private readonly MandelbrotRenderer renderer = new MandelbrotRenderer();
        private readonly BackgroundWorker worker;
        
        private int lastRenderWidth;
        private int lastRenderHeight;
        
        private BitmapSource renderedImage;
        private Palette palette;
        
        private readonly Stopwatch watch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            UpdatePalette(new BitmapPalette("C:\\Users\\Borrow\\RiderProjects\\Fractality\\Fractality\\Palettes\\default.png"));
            worker = (BackgroundWorker) FindResource("backgroundWorker");
            RenderImage.Source = renderedImage;
            StartRender();
            ColorStyleComboBox.SelectedIndex = 0;
            KeyDown += OnButtonKeyDown;
        }
        
        private void RenderButtonOnClick(object sender, RoutedEventArgs e)
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
                StartRender();
            }
        }
        
        private void SaveImage(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderedImage));
                    encoder.Save(stream);
                }
            }
        }
        
        private void ResetView(object sender, RoutedEventArgs e)
        {
            renderer.OriginX = 0;
            renderer.OriginY = 0;
            renderer.MultiplyFactor = 1;
            StartRender();
        }

        private void RenderImageOnLeftMouseDown(object sender, MouseButtonEventArgs e)
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
                StartRender();
            }
        }
        
        private void RenderImageOnRightMouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(RenderImage);
            var pixelWidth = RenderImage.Source.Width;
            var pixelHeight = RenderImage.Source.Height;
            var xClick = RenderImage.ActualWidth - pixelWidth * clickPoint.X / RenderImage.ActualWidth;
            var yClick = RenderImage.ActualHeight - pixelHeight * clickPoint.Y / RenderImage.ActualHeight;
            UpdateOrigin(xClick, yClick);
            renderer.MultiplyFactor /= double.Parse(ZoomFactorBox.Text);
            StartRender();
        }

        private void UpdateOrigin(double xClick, double yClick)
        {
            var ratio = (double) lastRenderWidth / lastRenderHeight;
            var areaHeight = 4d / renderer.MultiplyFactor;

            var areaWidth = areaHeight * ratio;
            
            renderer.OriginX = renderer.OriginX - areaWidth / 2d + xClick * (areaWidth / lastRenderWidth);
            renderer.OriginY = renderer.OriginY + areaHeight / 2d - yClick * (areaHeight / lastRenderHeight);
        }

        private void StartRender()
        {
            SaveToFileButton.IsEnabled = false;
            ResetViewButton.IsEnabled = false;
            RenderButton.Content = "Stop render";
            lastRenderWidth = int.Parse(WidthResBox.Text);
            lastRenderHeight = int.Parse(HeightResBox.Text);
            renderer.MaxIterations = int.Parse(MaxIterationsBox.Text);
            renderer.Worker = worker;
            RenderingPanel.Visibility = Visibility.Visible;
            RenderBar.Value = 0;
            watch.Reset();
            watch.Start();
            worker.RunWorkerAsync();
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                
                var bitmap = renderer.Render(lastRenderWidth, lastRenderHeight, palette);
                bitmap.Freeze();
                e.Result = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }
        
        private void RenderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            watch.Stop();
            RenderingPanel.Visibility = Visibility.Collapsed;
            renderedImage = (BitmapSource) e.Result;
            RenderImage.Source = renderedImage;
            TimeText.Text = watch.ElapsedMilliseconds + "ms.";
            ZoomText.Text = "x" + renderer.MultiplyFactor;
            OriginText.Text = "Origin: x=" + renderer.OriginX + "; y=" + renderer.OriginY;
            SaveToFileButton.IsEnabled = true;
            ResetViewButton.IsEnabled = true;
            RenderButton.Content = "Render";
        }

        private void RenderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RenderBar.Value = e.ProgressPercentage;
        }

        private void PaletteStyleSelected(object sender, RoutedEventArgs e)
        {
            PalettePanel.Visibility = Visibility.Visible;
        }
        
        private void MonochromeStyleSelected(object sender, RoutedEventArgs e)
        {
            PalettePanel.Visibility = Visibility.Collapsed;
        }
        
        private void SelectPalette(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                UpdatePalette(new BitmapPalette(openFileDialog.FileName));
                renderedImage = renderer.WriteBitmap(lastRenderWidth, lastRenderHeight, palette);
                RenderImage.Source = renderedImage;
            }
        }

        private void UpdatePalette(Palette newPalette)
        {
            palette = newPalette;
            PaletteImage.Source = ((BitmapPalette) palette).Image;
        }
        
        private void ApplyPaletteButtonPressed(object sender, RoutedEventArgs e)
        {
            renderedImage = renderer.WriteBitmap(lastRenderWidth, lastRenderHeight, palette);
            RenderImage.Source = renderedImage;
        }
        
        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !worker.IsBusy)
            {
                StartRender();
            }
        }
    }
}