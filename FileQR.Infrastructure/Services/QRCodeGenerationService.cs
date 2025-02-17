using FileQR.Application.Interfaces;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Infrastructure.Services
{
    public class QRCodeGenerationService : IQRCodeGeneration
    {
        private readonly IImageConversion _imageConversion;

        public QRCodeGenerationService(IImageConversion imageConversion)
        {
            _imageConversion = imageConversion;
        }

        public Bitmap GenerateQRCodeImage(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

           
            SixLabors.ImageSharp.Image icon = SixLabors.ImageSharp.Image.Load("assets/icon.png");

            
            Bitmap iconBitmap = _imageConversion.ConvertToBitmap(icon);

         
            System.Drawing.Color blackColor = System.Drawing.Color.Black;
            System.Drawing.Color whiteColor = System.Drawing.Color.White;

           
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);

            
            return qrCode.GetGraphic(
                pixelsPerModule: 4,
                darkColor: blackColor,
                lightColor: whiteColor,
                icon: iconBitmap,
                iconSizePercent: 20,
                iconBorderWidth: 2,
                drawQuietZones: false
            );
        }

    }
}
