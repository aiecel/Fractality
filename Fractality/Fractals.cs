using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractality
{
    public interface FractalDrawer
    {
        ImageSource Render();
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

    public class MandelbrotDrawer : FractalDrawer
    {
        private int renderWidth;
        private int renderHeight;

        public int RenderWidth
        {
            get => renderWidth;
            set => renderWidth = value;
        }

        public int RenderHeight
        {
            get => renderHeight;
            set => renderHeight = value;
        }

        public MandelbrotDrawer(int renderWidth, int renderHeight)
        {
            this.renderWidth = renderWidth;
            this.renderHeight = renderHeight;
        }

        public ImageSource Render()
        {
            var bitmap = new WriteableBitmap(renderWidth, renderHeight, 96, 96, 
                PixelFormats.Bgra32, null);
            
            byte[] leavingColor = {0, 0, 0, 0};
            byte[] stayingColor = {255, 255, 255, 255};

            for (var j = 0; j < renderHeight; j++)
            {
                for (var i = 0; i < renderWidth; i++)
                {
                    var x = (-renderWidth / 2 + i) / 100d;
                    var y = (renderHeight / 2 - j) / 100d;
                    
                    var rect = new Int32Rect(i, j, 1, 1);

                    bitmap.WritePixels(rect,
                        IsLeaving(new Complex(x, y)) ? leavingColor : stayingColor, 4, 0);
                }
            }

            return bitmap;
        }

        private bool IsLeaving(Complex c)
        {
            var z = new Complex(0, 0);
            for (var i = 0; i < 100; i++)
            {
                z = z.Sq().Add(c);
                if (z.Mod() > 2) return true;
            }
            return false;
        }
    }
}