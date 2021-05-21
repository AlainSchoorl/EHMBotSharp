using EHMBot.Core;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NLog;
using OpenCvSharp;

namespace EHMBot.Utilities
{
    public static class Utilities
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public const int LEFT_DOWN = 0x02;
        public const int LEFT_UP = 0x04;
        public const int RIGHTDOWN = 0x08;
        public const int RIGHTUP = 0x10;


        //This simulates a left mouse click
        public static async Task Click(int xPos, int yPos)
        {
            SetCursorPos(xPos, yPos);
            await Task.Delay(100);
            mouse_event(LEFT_DOWN, xPos, yPos, 0, 0);
            await Task.Delay(100);
            mouse_event(LEFT_UP, xPos, yPos, 0, 0);
        }

        public static async Task Click(Coordinate coordinate)
        {
            await Click(coordinate.X, coordinate.Y);
        }


        //This simulates a left mouse click
        public static async Task RightClick(int xPos, int yPos)
        {
            SetCursorPos(xPos, yPos);
            await Task.Delay(100);
            mouse_event(RIGHTDOWN, xPos, yPos, 0, 0);
            await Task.Delay(100);
            mouse_event(RIGHTUP, xPos, yPos, 0, 0);
        }

        public static async Task RightClick(Coordinate coordinate)
        {
            await RightClick(coordinate.X, coordinate.Y);
        }

        public static Color GetColorAt(int x, int y)
        {
            var bitmap = new Bitmap(2, 2);
            var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(2, 2));
            return bitmap.GetPixel(1, 1);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        public static Bitmap MatToBitmap(Mat image)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
        }

        public static async Task ActivateApp()
        {
            logger.Debug("Activating app");
            var processes = Process.GetProcessesByName("ehm");
            if (processes.Any()) SetForegroundWindow(processes[0].MainWindowHandle);
            await Task.Delay(250);
        }

        public static async Task<bool> KillAppIfRunning()
        {
            logger.Debug("Killing app if running");
            var processes = Process.GetProcessesByName("ehm");
            if (processes.Any()) processes[0].Kill();
            await Task.Delay(500);
            processes = Process.GetProcessesByName("ehm");
            return !processes.Any();
        }
    }
}