using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestImageController : ControllerBase
    {
        private readonly IFileService _photoService;

        public TestImageController(IFileService photoService)
        {
            _photoService = photoService;
        }
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var result = await _photoService.UploadPictureAsync(file);
            if (result != null)
                return Ok(result);

            return BadRequest("Upload failed");
        }
    }
}
