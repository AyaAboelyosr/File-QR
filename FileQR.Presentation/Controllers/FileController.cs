using Microsoft.AspNetCore.Mvc;
using FileQR.Application.UseCases;
using FileQR.Domain.Entities;


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
        public async Task<IActionResult> AddQRCode([FromBody]  Domain.Entities.File fileObject)
        {
            var result = await _addQRCodeUseCase.Execute(fileObject);
            return Ok(result);
        }


    }
}
