using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Jewelry.Services
{
    public class CaptchaService
    {
        public string CaptchaCode { get; private set; }

        public BitmapSource GenerateCaptchaImage(int width = 150, int height = 75)
        {
            var captchaBitmap = GenerateCaptchaBitmap(width, height);
            var captchaImage = Imaging.CreateBitmapSourceFromHBitmap(captchaBitmap.GetHbitmap(),
                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return captchaImage;
        }
        private Bitmap GenerateCaptchaBitmap(int width, int height)
        {
            var random = new Random();
            var captchaImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            var xPos = random.Next(0, width / 4);
            var colors = new Brush[]
            {
                Brushes.Black,
                Brushes.Red,
                Brushes.Magenta,
                Brushes.OrangeRed
            };

            var graphics = Graphics.FromImage(captchaImage);
            graphics.Clear(Color.LightCyan);
            var rect = new Rectangle(0, 0, width, height);
            var graphicPath = new GraphicsPath();

            CaptchaCode = GenerateCaptchaCode();
            for (int i = 0; i < 4; i++)
            {
                graphics.DrawString(CaptchaCode.Substring(i, 1),
                                    new Font("Tahoma", 30),
                                    colors[random.Next(colors.Length)],
                                    new PointF(xPos + i * 25, random.Next(height / 8, height / 2)));
            }

            var hatchBrush = new HatchBrush(HatchStyle.Percent20, Color.Red, Color.CornflowerBlue);
            graphics.FillPath(hatchBrush, graphicPath);

            for (int i = 0; i < (int)(rect.Width * rect.Height / 50F); i++)
            {
                int x = random.Next(width);
                int y = random.Next(height);
                int w = random.Next(10);
                int h = random.Next(10);
                graphics.FillEllipse(hatchBrush, x, y, w, h);
            }

            graphics.DrawLine(Pens.Black,
                              new PointF(0, 0),
                              new PointF(width - 1, height - 1));
            graphics.DrawLine(Pens.Black,
                              new PointF(0, height - 1),
                              new PointF(width - 1, 0));

            hatchBrush.Dispose();
            graphics.Dispose();

            return captchaImage;
        }
        private string GenerateCaptchaCode()
        {
            var random = new Random();
            var charCount = 4;
            var captchaCode = new StringBuilder();

            for (int i = 0; i < charCount; i++)
            {
                int a = random.Next(3);
                char chr;
                switch (a)
                {
                    case 0:
                        chr = (char)random.Next(48, 57);
                        captchaCode.Append(chr.ToString());
                        break;
                    case 1:
                        chr = (char)random.Next(65, 90);
                        captchaCode.Append(chr.ToString());
                        break;
                    case 2:
                        chr = (char)random.Next(97, 122);
                        captchaCode.Append(chr.ToString());
                        break;
                }
            }

            return captchaCode.ToString();
        }
    }
}
