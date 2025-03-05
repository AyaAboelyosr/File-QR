using Microsoft.AspNetCore.Mvc;
using FileQR.Application.UseCases;
using FileQR.Domain.Entities;
using FileQR.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json; // Add this for JSON deserialization

namespace FileQR.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly AddQRCodeUseCase _addQRCodeUseCase;

        public FileController(AddQRCodeUseCase addQRCodeUseCase)
        {
            _addQRCodeUseCase = addQRCodeUseCase;
        }

        [HttpPost("AddQRCode")]
        public async Task<IActionResult> AddQRCode([FromForm] IFormFile file, [FromForm] string fileObject)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty or null.");

            if (string.IsNullOrEmpty(fileObject))
                return BadRequest("File object is null or empty.");

            // Deserialize the fileObject JSON string
            Domain.Entities.File fileObj;
            try
            {
                fileObj = JsonConvert.DeserializeObject<Domain.Entities.File>(fileObject);
            }
            catch (JsonException)
            {
                return BadRequest("Invalid JSON format for file object.");
            }

            if (fileObj == null || fileObj.QRSetting == null)
                return BadRequest("File object or QR settings not found.");

            // Step 1: Convert IFormFile to Stream
            using var fileStream = file.OpenReadStream();

            // Step 2: Map Domain.Entities.File and QRSetting to QRContentDTO
            var qrContent = new QRContentDTO
            {
                ShowArabicNames = fileObj.QRSetting.ShowArabicNames,
                QRShowAuthBy = fileObj.QRSetting.QRShowAuthBy,
                QRShowIssueDate = fileObj.QRSetting.QRShowIssueDate,
                QRShowIssuedFor = fileObj.QRSetting.QRShowIssuedFor,
                QRShowLink = fileObj.QRSetting.QRShowLink,
                AuthRequiredFromUser = fileObj.QRSetting.ShowArabicNames
                    ? fileObj.AuthRequiredFromUser?.ArabicDisplayName
                    : fileObj.AuthRequiredFromUser?.DisplayName, // Use ArabicDisplayName if ShowArabicNames is true
                IssuedFor = fileObj.IssuedFor,
                MessageToShowInImage = fileObj.QRSetting.MessageToShowInImage,
                QRLeft = fileObj.QRSetting.QRLeft,
                QRBottom = fileObj.QRSetting.QRBottom,
                QRWidth = fileObj.QRSetting.QRWidth,
                ShowFirstPage = fileObj.ShowFirstPage,
                ShowLastPage = fileObj.ShowLastPage,
                ShowMiddlePages = fileObj.ShowMiddlePages,
                FirstPagePosLeft = fileObj.FirstPagePosLeft,
                FirstPagePosBottom = fileObj.FirstPagePosBottom,
                FirstPagePosWidth = fileObj.FirstPagePosWidth,
                LastPagePosLeft = fileObj.LastPagePosLeft,
                LastPagePosBottom = fileObj.LastPagePosBottom,
                LastPagePosWidth = fileObj.LastPagePosWidth,
                MiddlePagePosLeft = fileObj.MiddlePagePosLeft,
                MiddlePagePosBottom = fileObj.MiddlePagePosBottom,
                MiddlePagePosWidth = fileObj.MiddlePagePosWidth
            };

            
            var result = await _addQRCodeUseCase.Execute(fileStream, file.FileName, qrContent);

            return Ok(result);
        }
    }
}