using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace Fractality
{
    public class MandelbrotRenderer
    {
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public double MultiplyFactor { get; set; } = 1;
        public int MaxIterations { get; set; } = 100;
        public BackgroundWorker Worker { get; set; }
        
        private int[,] iterationMap;
        private GPGPU gpu;

        public MandelbrotRenderer()
        {
            CudafyModes.Target = eGPUType.OpenCL;
            CudafyTranslator.Language = eLanguage.OpenCL;
            
            gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            var module = CudafyTranslator.Cudafy();
            gpu.LoadModule(module);
        }

        public BitmapSource Render(int renderWidth, int renderHeight, Palette palette)
        {
            iterationMap = new int[renderWidth, renderHeight];
            
            var deviceIterationMap = gpu.CopyToDevice(iterationMap);

            var gridX = (int) Math.Ceiling(renderWidth / 1024d);
            
            gpu.Launch(new dim3(gridX, renderHeight), 1024).Map(deviceIterationMap, MaxIterations, OriginX, OriginY, MultiplyFactor);
            gpu.Synchronize();
            
            gpu.CopyFromDevice(deviceIterationMap, iterationMap);
            gpu.FreeAll();

            //Map(deviceIterationMap);
            return WriteBitmap(renderWidth, renderHeight, palette);
        }

        public WriteableBitmap WriteBitmap(int renderWidth, int renderHeight, Palette palette)
        {
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
                        var color = palette.GetColor(((double) leavingIterations / MaxIterations) * 100);
                        pixels[pixel] = color.B;
                        pixels[pixel + 1] = color.G;
                        pixels[pixel + 2] = color.R;
                        pixels[pixel + 3] = color.A;
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
        
        [Cudafy]
        private static void Map(GThread thread, int[,] iterationMap, int maxIterations, double originX, double originY, double multiplyFactor)
        {
            var renderWidth = iterationMap.GetLength(0);
            var renderHeight = iterationMap.GetLength(1);

            var threadIndex = thread.threadIdx.x;
            var blockIndexX = thread.blockIdx.x;
            var blockIndexY = thread.blockIdx.y;
            if (threadIndex + 1024 * blockIndexX > renderWidth) return;

            var ratio = (double) renderWidth / renderHeight;
            var areaHeight = 4d / multiplyFactor;
            var areaWidth = areaHeight * ratio;

            var stepX = areaWidth / renderWidth;
            var stepY = areaHeight / renderHeight;
            
            var x = originX - areaWidth / 2d + 1024 * blockIndexX * stepX + stepX * threadIndex;
            var y = originY + areaHeight / 2d - blockIndexY * stepY;

            iterationMap[threadIndex + 1024 * blockIndexX, blockIndexY] = LeavingIterations(x, y, maxIterations);
            //rowsCalculated++;
            //var progressPercentage = (double) rowsCalculated / renderHeight * 100;
            //Worker.ReportProgress((int) progressPercentage);
        }
        
        [Cudafy]
        private static int LeavingIterations(double real, double im, int maxIterations)
        {
            double zReal = 0;
            double zIm = 0;
            for (var i = 0; i < maxIterations; i++)
            {
                var zRealNew = zReal * zReal - zIm * zIm + real;
                var zImNew = 2 * zReal * zIm + im;
                if (Math.Sqrt(zRealNew * zRealNew + zImNew * zImNew) > 2)
                {
                    return i + 1;
                }
                zReal = zRealNew;
                zIm = zImNew;
            }
            return -1;
        }
    }
}