using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractality
{
    public class MandelbrotRenderer
    {
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public double MultiplyFactor { get; set; } = 1;
        public int MaxIterations { get; set; } = 100;
        
        private int[,] iterationMap;

        public BitmapSource Render(int renderWidth, int renderHeight, BackgroundWorker worker)
        {
            Map(renderWidth, renderHeight, worker);
            
            var bitmap = new WriteableBitmap(renderWidth, renderHeight, 96, 96, 
                PixelFormats.Bgra32, null);

            var pixelWidth = bitmap.PixelWidth;
            var bitsPerPixel = bitmap.Format.BitsPerPixel;
            
            var rect = new Int32Rect(0, 0, renderWidth, renderHeight);
            var pixels = new byte[renderWidth * renderHeight * bitmap.Format.BitsPerPixel / 8];

            Parallel.For(0, renderHeight, j =>
            {
                for (var i = 0; i < renderWidth; i++)
                {
                    var pixel = (i + j * pixelWidth) * bitsPerPixel / 8;
                    var leavingIterations = iterationMap[i, j];
                    if (leavingIterations != -1)
                    {
                        var value = (byte) ((double) leavingIterations / MaxIterations * 255);
                        pixels[pixel] = value;
                        pixels[pixel + 1] = value;
                        pixels[pixel + 2] = value;
                        pixels[pixel + 3] = 255;
                    }
                    else
                    {
                        pixels[pixel] = 0;
                        pixels[pixel + 1] = 0;
                        pixels[pixel + 2] = 0;
                        pixels[pixel + 3] = 255;
                    }
                }
            });

            var stride = pixelWidth * bitsPerPixel / 8;
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }
        
        private void Map(int renderWidth, int renderHeight, BackgroundWorker worker)
        {
            iterationMap = new int[renderWidth, renderHeight];
            
            var ratio = (double) renderWidth / renderHeight;
            var areaHeight = 4d / MultiplyFactor;
            var areaWidth = areaHeight * ratio;
            
            var xStart = OriginX - areaWidth / 2d;
            var yStart = OriginY + areaHeight / 2d;

            var xStep = areaWidth / renderWidth;
            var yStep = areaHeight / renderHeight;
            
            var rowsCalculated = 0;
            
            Parallel.For(0, renderHeight, j =>
            {
                for (var i = 0; i < renderWidth; i++)
                {
                    if (worker.CancellationPending) return;

                    var x = xStart + i * xStep;
                    var y = yStart - j * yStep;
                    
                    iterationMap[i, j] = LeavingIterations(new Complex(x, y));
                }
                rowsCalculated++;
                var progressPercentage = (double) rowsCalculated / renderHeight * 100;
                worker.ReportProgress((int) progressPercentage);
            });
        }

        private int LeavingIterations(Complex c)
        {
            var z = new Complex(0, 0);
            for (var i = 0; i < MaxIterations; i++)
            {
                z = z.Square() + c;
                if (z.Mod() > 2) return i + 1;
            }
            return -1;
        }
    }
}