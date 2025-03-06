using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FileQR.Application.Interfaces
{
    public interface IImageConversion
    {
        SixLabors.ImageSharp.Image<Rgba32> ConvertToImageSharpImage(Bitmap bitmap);
        Bitmap ConvertToBitmap(SixLabors.ImageSharp.Image<Rgba32> image);
    }
}