using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractality
{
    public interface Palette
    {
        Color GetColor(double percentage);
    }
    
    public class BitmapPalette : Palette
    {
        private byte[] pixels;
        private int width;

        public BitmapSource Image { get; private set; }

        public BitmapPalette(string path)
        {
            getFromImage(new BitmapImage(new Uri(path)));
        }

        public BitmapPalette(BitmapSource image)
        {
            getFromImage(image);
        }

        public Color GetColor(double percentage)
        {
            var index = 4 * (int) (percentage * width / 100);
            return new Color
            {
                B = pixels[index], 
                G = pixels[index + 1], 
                R = pixels[index + 2], 
                A = pixels[index + 3]
            };
        }

        private void getFromImage(BitmapSource bitmap)
        {
            var stride = bitmap.PixelWidth * 4;
            var size = bitmap.PixelWidth * stride;
            pixels = new byte[size];
            bitmap.CopyPixels(pixels, stride, 0);

            var paletteImage = new WriteableBitmap(bitmap.PixelWidth, 1, 96, 96, 
                PixelFormats.Bgra32, null);
            
            paletteImage.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, 1), pixels, stride, 0);
            Image = paletteImage;
            Image.Freeze();

            width = bitmap.PixelWidth;
        }
    }
}