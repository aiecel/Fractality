using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractality
{
    public interface FractalRenderer
    {
        BitmapSource Render(int renderWidth, int renderHeight, BackgroundWorker worker);
    }

    public class Complex
    {
        private double r;
        private double i;

        public double Real
        {
            get => r;
            set => r = value;
        }

        public double Im
        {
            get => i;
            set => i = value;
        }

        public Complex(double r, double i)
        {
            this.r = r;
            this.i = i;
        }
        
        public Complex Sq()
        {
            return new Complex(r * r - i * i, 2 * i * r);
        }

        public Complex Add(Complex c)
        {
            return new Complex(this.r + c.r, this.i + c.i);
        }

        public double Mod()
        {
            return Math.Sqrt(r * r + i * i);
        }
    }

    public class MandelbrotRenderer : FractalRenderer
    {
        private double multiplyFactor = 1;
        private double originX = 0;
        private double originY = 0;
        private int maxIterations = 100;

        public double MultiplyFactor
        {
            get => multiplyFactor;
            set => multiplyFactor = value;
        }

        public double OriginX
        {
            get => originX;
            set => originX = value;
        }

        public double OriginY
        {
            get => originY;
            set => originY = value;
        }

        public int MaxIterations
        {
            get => maxIterations;
            set => maxIterations = value;
        }

        public BitmapSource Render(int renderWidth, int renderHeight, BackgroundWorker worker)
        {
            var pixelsTotal = renderWidth * renderHeight;
            var pixelCount = 0;
            
            var bitmap = new WriteableBitmap(renderWidth, renderHeight, 96, 96, 
                PixelFormats.Bgra32, null);

            var pixelWidth = bitmap.PixelWidth;
            var bitsPerPixel = bitmap.Format.BitsPerPixel;
            
            var rect = new Int32Rect(0, 0, renderWidth, renderHeight);
            var pixels = new byte[pixelsTotal * bitmap.Format.BitsPerPixel / 8];

            var ratio = (double) renderWidth / renderHeight;
            var areaHeight = 4d / multiplyFactor;
            var areaWidth = areaHeight * ratio;
            
            Parallel.For(0, renderHeight, j =>
            {
                for (var i = 0; i < renderWidth; i++)
                {
                    var x = originX - areaWidth / 2d + i * (areaWidth / renderWidth);
                    var y = originY + areaHeight / 2d - j * (areaHeight / renderHeight);

                    var pixel = (i + j * pixelWidth) * bitsPerPixel / 8;

                    var leavingIterations = LeavingIterations(new Complex(x, y));
                    if (leavingIterations != -1)
                    {
                        var value = (byte) ((double) leavingIterations / maxIterations * 255);
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

                    pixelCount++;
                    var progressPercentage = (double) pixelCount / pixelsTotal * 100;
                    if (Math.Abs(progressPercentage % 1) < 0.001d)
                    {
                        worker.ReportProgress((int) ((double) pixelCount / pixelsTotal * 100));
                    }

                    if (worker.CancellationPending)
                    {
                        return;
                    }
                }
            });

            var stride = pixelWidth * bitsPerPixel / 8;
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }

        private int LeavingIterations(Complex c)
        {
            var z = new Complex(0, 0);
            for (var i = 0; i < maxIterations; i++)
            {
                z = z.Sq().Add(c);
                if (z.Mod() > 2) return i + 1;
            }
            return -1;
        }
    }
}