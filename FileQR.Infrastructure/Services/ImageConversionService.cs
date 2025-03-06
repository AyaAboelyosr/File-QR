using FileQR.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.IO;

namespace FileQR.Infrastructure.Services
{
    public class ImageConversionService : IImageConversion
    {
        public SixLabors.ImageSharp.Image<Rgba32> ConvertToImageSharpImage(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return SixLabors.ImageSharp.Image.Load<Rgba32>(memoryStream);
            }
        }

        public Bitmap ConvertToBitmap(SixLabors.ImageSharp.Image<Rgba32> image)
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