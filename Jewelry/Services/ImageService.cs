using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Jewelry.Services
{
    public class ImageService
    {
        public void SaveImage(string sourcePath, string destinationPath)
        {
            ImageFormat format;
            var extension = Path.GetExtension(sourcePath);
            switch (extension)
            {
                case ".png":
                    format = ImageFormat.Png;
                    break;
                case ".jpg":
                case ".jpeg":
                    format = ImageFormat.Jpeg;
                    break;
                default:
                    format = ImageFormat.Jpeg;
                    break;
            }

            var image = Image.FromFile(sourcePath);
            image.Save(destinationPath, format);
        }

        public OpenFileDialog CreateImageOpenFileDialog()
        {
            var saveFileDialog = new OpenFileDialog
            {
                Filter = "JPEG (.jpeg)|*.jpeg | JPG (.jpg)|*.jpg | PNG (.png)|*.png",
                FilterIndex = 3,
                InitialDirectory = @"C:\"
            };

            return saveFileDialog;
        }

        public BitmapImage GetImageFromPath(string uriPath)
        {
            var uri = new Uri(uriPath);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = uri;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
