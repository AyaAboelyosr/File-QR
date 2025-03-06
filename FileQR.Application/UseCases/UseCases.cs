using FileQR.Application.DTOs;
using FileQR.Application.Helpers;
using FileQR.Application.Interfaces;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Navigation;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AddQRCodeUseCase> _logger;

        public AddQRCodeUseCase(IFileManager fileManager, IQRSettingsService qrSettingsService, IQRCodeGeneration qrCodeGeneration, IImageConversion imageConversion, IMeasurementConverter measurementConverter , ILogger<AddQRCodeUseCase> logger)
        {
            _fileManager = fileManager;
            _qrSettingsService = qrSettingsService;
            _qrCodeGeneration = qrCodeGeneration;
            _imageConversion = imageConversion;
            _measurementConverter = measurementConverter;
            _logger = logger;
        }

      public async Task<string> Execute(Stream fileStream, string fileUrl, QRContentDTO content)
{
    if (fileStream == null || fileStream.Length == 0)
        throw new ArgumentException("File stream is empty or null.");

    var tempFilePath = Path.Combine("uploadDir", RandomGenerator.GenerateRandomString(5) + ".pdf");

    try
    {
       
        (string qrContent, string authByLabel) = GenerateQRCodeContent(content, fileUrl);

       
        var qrImageData = GenerateQRCodeImage(qrContent);

       
        AddQRCodeToPdf(fileStream, tempFilePath, qrImageData, content, qrContent, authByLabel, fileUrl);

   
        await UploadFile(tempFilePath, fileUrl);

        return "QR code added successfully.";
    }
    finally
    {
        if (System.IO.File.Exists(tempFilePath))
        {
            try
            {
                System.IO.File.Delete(tempFilePath);
            }
            catch (IOException ex)
            {
              
                _logger.LogError(ex, "Failed to delete temporary file: {FilePath}", tempFilePath);
            }
        }
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
            try
            {
                using (var pdfReader = new PdfReader(fileStream))
                using (var pdfWriter = new PdfWriter(tempFilePath))
                using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
                {
                    iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

               
                    string fontPath = Path.Combine(Environment.CurrentDirectory, "fonts", "TAHOMA.TTF");
                    var textFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

               
                    for (int pageNo = 1; pageNo <= pdfDocument.GetNumberOfPages(); pageNo++)
                    {
                        if (!ShouldAddQRCodeToPage(pageNo, content, pdfDocument))
                            continue;

                        var (left, bottom, width) = GetPagePositions(pageNo, content, pdfDocument);

                      
                        float leftInPoints = _measurementConverter.ConvertToPoints(left);
                        float bottomInPoints = _measurementConverter.ConvertToPoints(bottom);
                        float widthInPoints = _measurementConverter.ConvertToPoints(width);

                       
                        AddTextAndQRCodeToPage(document, pageNo, qrImageData, content, textFont, fileUrl, leftInPoints, bottomInPoints, widthInPoints, authByLabel, pdfDocument);
                    }

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add QR code to PDF: {FilePath}", tempFilePath);
                throw; 
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

            
            iText.Layout.Element.Image qrPdfImage = new iText.Layout.Element.Image(qrImageData)
                .SetFixedPosition(pageNo, leftInPoints, bottomInPoints, widthInPoints);

           
            document.Add(qrPdfImage);

           
            PdfPage page = pdfDocument.GetPage(pageNo);
            PdfExplicitDestination destination = PdfExplicitDestination.CreateFit(page);

            
            PdfLinkAnnotation linkAnnotation = new PdfLinkAnnotation(new iText.Kernel.Geom.Rectangle(leftInPoints, bottomInPoints, widthInPoints, widthInPoints))
                .SetAction(PdfAction.CreateGoTo(destination));
            page.AddAnnotation(linkAnnotation);

            
            var text2Paragraph = new Paragraph($"{authByLabel}: {content.AuthRequiredFromUser}")
                .SetFont(textFont).SetFontSize(8);
            text2Paragraph.SetFixedPosition(pageNo, leftInPoints, bottomInPoints - 10, 200);
            document.Add(text2Paragraph);
        }
        private async Task UploadFile(string tempFilePath, string fileUrl)
        {
            try
            {
              
                using (var authFileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                  
                    await _fileManager.UploadFileAsync(authFileStream, fileUrl, "authContainerName", "application/pdf");
                } 
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "Failed to upload file: {FilePath}", tempFilePath);
                throw;
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