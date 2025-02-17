using FileQR.Application.Helpers;
using FileQR.Application.Interfaces;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileQR.Application.UseCases
{
    public class AddQRCodeUseCase
    {
        private readonly IFileManager _fileManager;
        private readonly IQRSettingsService _qrSettingsService;
        private readonly IQRCodeGeneration _qrCodeGeneration;
        private readonly IImageConversion _imageConversion;
        private readonly IMeasurementConverter _measurementConverter;

        public AddQRCodeUseCase(IFileManager fileManager, IQRSettingsService qrSettingsService, IQRCodeGeneration qrCodeGeneration, IImageConversion imageConversion, IMeasurementConverter measurementConverter)
        {
            _fileManager = fileManager;
            _qrSettingsService = qrSettingsService;
            _qrCodeGeneration = qrCodeGeneration;
            _imageConversion = imageConversion;
            _measurementConverter = measurementConverter;
        }

        public async Task<string> Execute(Domain.Entities.File fileObject)
        {
            // Step 1: Download the file
            var blob = await _fileManager.DownloadFileAsync(fileObject.Path, "nonauthContainerName");
            if (blob == null)
                throw new FileNotFoundException("File not found.");

            var origFileStream = blob.FileStream;
            var tempFilePath = Path.Combine("uploadDir", RandomGenerator.GenerateRandomString(5) + fileObject.Path);

            // Step 2: Generate QR code content
            var qs = _qrSettingsService.QRSettings.SingleOrDefault(s => s.ID == fileObject.QRSettingId);
            if (qs == null)
                throw new ArgumentException("QR settings not found.");

            string authByLabel = qs.ShowArabicNames ? "معتمد من" : "Authorized By";
            string authDateLabel = qs.ShowArabicNames ? "تاريخ الإصدار" : "Issue Date";
            string issuedForLabel = qs.ShowArabicNames ? "موجه إلى" : "Issued For";

            string QRContent = "";
            if (qs.QRShowAuthBy)
                QRContent += $"{authByLabel} : {fileObject.AuthRequiredFromUser?.DisplayName}\r\n";
            if (qs.QRShowIssueDate)
                QRContent += $"{authDateLabel}: {DateTime.Now:dd/MM/yyyy}\r\n";
            if (qs.QRShowIssuedFor)
                QRContent += $"{issuedForLabel}: {fileObject.IssuedFor}\r\n";
            if (qs.QRShowLink)
                QRContent += $"https://tamr-dms.azurewebsites.net/Home/FileInfo/{fileObject.Id}";

            // Step 3: Generate QR code image
            Bitmap qrBitmap = _qrCodeGeneration.GenerateQRCodeImage(QRContent);
            SixLabors.ImageSharp.Image qrImage = _imageConversion.ConvertToImageSharpImage(qrBitmap);
            ImageData qrImageData = GetImageDataFactory(qrImage);

            // Step 4: Add QR code to the PDF
            using (var pdfReader = new PdfReader(origFileStream))
            using (var pdfWriter = new PdfWriter(tempFilePath))
            using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

                // Load font for text
                string fontPath = Path.Combine(Environment.CurrentDirectory, "fonts", "TAHOMA.TTF");
                var textFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

                // Add QR code and text to each page
                for (int pageNo = 1; pageNo <= pdfDocument.GetNumberOfPages(); pageNo++)
                {
                    float left = 1, bottom = 1, width = 5;

                    if (pageNo == 1 && !fileObject.ShowFirstPage)
                        continue;
                    if (pageNo == pdfDocument.GetNumberOfPages() && !fileObject.ShowLastPage)
                        continue;
                    if (pageNo > 1 && pageNo < pdfDocument.GetNumberOfPages() && !fileObject.ShowMiddlePages)
                        continue;

                    // Adjust positions based on page number
                    if (pageNo == 1)
                    {
                        left = fileObject.FirstPagePosLeft == 0 ? qs.QRLeft : fileObject.FirstPagePosLeft;
                        bottom = fileObject.FirstPagePosBottom == 0 ? qs.QRBottom : fileObject.FirstPagePosBottom;
                        width = fileObject.FirstPagePosWidth == 0 ? qs.QRWidth : fileObject.FirstPagePosWidth;
                    }
                    else if (pageNo == pdfDocument.GetNumberOfPages())
                    {
                        left = fileObject.LastPagePosLeft == 0 ? qs.QRLeft : fileObject.LastPagePosLeft;
                        bottom = fileObject.LastPagePosBottom == 0 ? qs.QRBottom : fileObject.LastPagePosBottom;
                        width = fileObject.LastPagePosWidth == 0 ? qs.QRWidth : fileObject.LastPagePosWidth;
                    }
                    else
                    {
                        left = fileObject.MiddlePagePosLeft == 0 ? qs.QRLeft : fileObject.MiddlePagePosLeft;
                        bottom = fileObject.MiddlePagePosBottom == 0 ? qs.QRBottom : fileObject.MiddlePagePosBottom;
                        width = fileObject.MiddlePagePosWidth == 0 ? qs.QRWidth : fileObject.MiddlePagePosWidth;
                    }

                    // Convert measurements to points
                    float leftInPoints = _measurementConverter.ConvertToPoints(left);
                    float bottomInPoints = _measurementConverter.ConvertToPoints(bottom);
                    float widthInPoints = _measurementConverter.ConvertToPoints(width);

                    // Add text and QR code to the page
                    var text1Paragraph = new Paragraph(qs.MessageToShowInImage).SetFont(textFont).SetFontSize(8);
                    text1Paragraph.SetFixedPosition(pageNo, leftInPoints, bottomInPoints + widthInPoints + 10, 200);
                    document.Add(text1Paragraph);

                    iText.Layout.Element.Image qrPdfImage = new iText.Layout.Element.Image(qrImageData)
                        .SetFixedPosition(pageNo, leftInPoints, bottomInPoints, widthInPoints);
                    document.Add(qrPdfImage);

                    var text2Paragraph = new Paragraph($"{authByLabel}: {fileObject.AuthRequiredFromUser?.DisplayName}")
                        .SetFont(textFont).SetFontSize(8);
                    text2Paragraph.SetFixedPosition(pageNo, leftInPoints, bottomInPoints - 10, 200);
                    document.Add(text2Paragraph);
                }

                document.Close();
            }

            // Step 5: Upload the modified file
            using (var authFileStream = System.IO.File.OpenRead(tempFilePath))
            {
                await _fileManager.UploadFileAsync(authFileStream, fileObject.Path, "authContainerName", "application/pdf");
            }

            // Clean up temporary file
            System.IO.File.Delete(tempFilePath);

            return "QR code added successfully.";
        }

        private ImageData GetImageDataFactory(SixLabors.ImageSharp.Image img)
        {
            var imgStream = new MemoryStream();
            img.Save(imgStream, new PngEncoder());
            return ImageDataFactory.Create(imgStream.ToArray());
        }
    }
}