using FileQR.Application.DTOs;
using FileQR.Application.Helpers;
using FileQR.Application.Interfaces;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Annot;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Drawing;
using System.IO;
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

        public async Task<string> Execute(Stream fileStream, string fileUrl, QRContentDTO content)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("File stream is empty or null.");

            var tempFilePath = Path.Combine("uploadDir", RandomGenerator.GenerateRandomString(5) + ".pdf");

            try
            {
                // Step 1: Generate QR code content
                (string qrContent, string authByLabel) = GenerateQRCodeContent(content, fileUrl);

                // Step 2: Generate QR code image
                var qrImageData = GenerateQRCodeImage(qrContent);

                // Step 3: Add QR code to the PDF
                AddQRCodeToPdf(fileStream, tempFilePath, qrImageData, content, qrContent, authByLabel, fileUrl);

                // Step 4: Upload the modified file
                await UploadFile(tempFilePath, fileUrl);

                return "QR code added successfully.";
            }
            finally
            {
                // Clean up temporary file
                if (System.IO.File.Exists(tempFilePath))
                    System.IO.File.Delete(tempFilePath);
            }
        }

        private (string qrContent, string authByLabel) GenerateQRCodeContent(QRContentDTO content, string fileUrl)
        {
            string authByLabel = content.ShowArabicNames ? "معتمد من" : "Authorized By";
            string authDateLabel = content.ShowArabicNames ? "تاريخ الإصدار" : "Issue Date";
            string issuedForLabel = content.ShowArabicNames ? "موجه إلى" : "Issued For";

            string qrContent = "";
            if (content.QRShowAuthBy)
                qrContent += $"{authByLabel} : {content.AuthRequiredFromUser}\r\n";
            if (content.QRShowIssueDate)
                qrContent += $"{authDateLabel}: {DateTime.Now:dd/MM/yyyy}\r\n";
            if (content.QRShowIssuedFor)
                qrContent += $"{issuedForLabel}: {content.IssuedFor}\r\n";
            if (content.QRShowLink)
                qrContent += fileUrl;

            return (qrContent, authByLabel);
        }

        private ImageData GenerateQRCodeImage(string qrContent)
        {
            Bitmap qrBitmap = _qrCodeGeneration.GenerateQRCodeImage(qrContent);
            SixLabors.ImageSharp.Image qrImage = _imageConversion.ConvertToImageSharpImage(qrBitmap);
            return GetImageDataFactory(qrImage);
        }

        private void AddQRCodeToPdf(Stream fileStream, string tempFilePath, ImageData qrImageData, QRContentDTO content, string qrContent, string authByLabel, string fileUrl)
        {
            using (var pdfReader = new PdfReader(fileStream))
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
                    if (!ShouldAddQRCodeToPage(pageNo, content, pdfDocument))
                        continue;

                    var (left, bottom, width) = GetPagePositions(pageNo, content, pdfDocument);

                    // Convert measurements to points
                    float leftInPoints = _measurementConverter.ConvertToPoints(left);
                    float bottomInPoints = _measurementConverter.ConvertToPoints(bottom);
                    float widthInPoints = _measurementConverter.ConvertToPoints(width);

                    // Add text and QR code to the page
                    AddTextAndQRCodeToPage(document, pageNo, qrImageData, content, textFont, fileUrl, leftInPoints, bottomInPoints, widthInPoints, authByLabel, pdfDocument);
                }

                document.Close();
            }
        }

        private bool ShouldAddQRCodeToPage(int pageNo, QRContentDTO content, PdfDocument pdfDocument)
        {
            if (pageNo == 1 && !content.ShowFirstPage)
                return false;
            if (pageNo == pdfDocument.GetNumberOfPages() && !content.ShowLastPage)
                return false;
            if (pageNo > 1 && pageNo < pdfDocument.GetNumberOfPages() && !content.ShowMiddlePages)
                return false;

            return true;
        }

        private (float left, float bottom, float width) GetPagePositions(int pageNo, QRContentDTO content, PdfDocument pdfDocument)
        {
            float left = 1, bottom = 1, width = 5;

            if (pageNo == 1)
            {
                left = content.FirstPagePosLeft == 0 ? content.QRLeft : content.FirstPagePosLeft;
                bottom = content.FirstPagePosBottom == 0 ? content.QRBottom : content.FirstPagePosBottom;
                width = content.FirstPagePosWidth == 0 ? content.QRWidth : content.FirstPagePosWidth;
            }
            else if (pageNo == pdfDocument.GetNumberOfPages())
            {
                left = content.LastPagePosLeft == 0 ? content.QRLeft : content.LastPagePosLeft;
                bottom = content.LastPagePosBottom == 0 ? content.QRBottom : content.LastPagePosBottom;
                width = content.LastPagePosWidth == 0 ? content.QRWidth : content.LastPagePosWidth;
            }
            else
            {
                left = content.MiddlePagePosLeft == 0 ? content.QRLeft : content.MiddlePagePosLeft;
                bottom = content.MiddlePagePosBottom == 0 ? content.QRBottom : content.MiddlePagePosBottom;
                width = content.MiddlePagePosWidth == 0 ? content.QRWidth : content.MiddlePagePosWidth;
            }

            return (left, bottom, width);
        }

        private void AddTextAndQRCodeToPage(iText.Layout.Document document, int pageNo, ImageData qrImageData, QRContentDTO content, PdfFont textFont, string fileUrl, float leftInPoints, float bottomInPoints, float widthInPoints, string authByLabel, PdfDocument pdfDocument)
        {
            var text1Paragraph = new Paragraph(content.MessageToShowInImage).SetFont(textFont).SetFontSize(8);
            text1Paragraph.SetFixedPosition(pageNo, leftInPoints, bottomInPoints + widthInPoints + 10, 200);
            document.Add(text1Paragraph);

            // Create the QR code image
            iText.Layout.Element.Image qrPdfImage = new iText.Layout.Element.Image(qrImageData)
                .SetFixedPosition(pageNo, leftInPoints, bottomInPoints, widthInPoints);

            // Add the QR code image to the document
            document.Add(qrPdfImage);

            // Add a clickable link annotation to the QR code
            if (!string.IsNullOrEmpty(fileUrl))
            {
                PdfPage page = pdfDocument.GetPage(pageNo);
                PdfLinkAnnotation linkAnnotation = new PdfLinkAnnotation(new iText.Kernel.Geom.Rectangle(leftInPoints, bottomInPoints, widthInPoints, widthInPoints))
                    .SetAction(PdfAction.CreateURI(fileUrl));
                page.AddAnnotation(linkAnnotation);
            }

            var text2Paragraph = new Paragraph($"{authByLabel}: {content.AuthRequiredFromUser}")
                .SetFont(textFont).SetFontSize(8);
            text2Paragraph.SetFixedPosition(pageNo, leftInPoints, bottomInPoints - 10, 200);
            document.Add(text2Paragraph);
        }

        private async Task UploadFile(string tempFilePath, string fileUrl)
        {
            using (var authFileStream = System.IO.File.OpenRead(tempFilePath))
            {
                await _fileManager.UploadFileAsync(authFileStream, fileUrl, "authContainerName", "application/pdf");
            }
        }

        private ImageData GetImageDataFactory(SixLabors.ImageSharp.Image img)
        {
            var imgStream = new MemoryStream();
            img.Save(imgStream, new PngEncoder());
            return ImageDataFactory.Create(imgStream.ToArray());
        }
    }
}