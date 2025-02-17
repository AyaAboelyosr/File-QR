using FileQR.Application.Interfaces;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Infrastructure.Services
{
    public class ImageConversionService : IImageConversion
    {
        public SixLabors.ImageSharp.Image ConvertToImageSharpImage(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return SixLabors.ImageSharp.Image.Load(memoryStream);
            }
        }

        public Bitmap ConvertToBitmap(SixLabors.ImageSharp.Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, new PngEncoder());
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new Bitmap(memoryStream);
            }
        }
    }
}
