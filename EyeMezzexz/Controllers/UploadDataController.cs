using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadDataController : ControllerBase
    {
        private readonly string screenshotPath = @"C:\ScreenShot";

        [HttpPost("upload-Images")]
        public async Task<IActionResult> UploadImages(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not provided.");
            }

            var originalFileName = Path.GetFileName(file.FileName);
            var fileExtension = Path.GetExtension(originalFileName);
            var uniqueFileName = Path.GetFileNameWithoutExtension(originalFileName) +
                                 DateTime.Now.ToString("_ddMMyyhhmmss") + fileExtension;
            var destinationFilePath = Path.Combine(screenshotPath, uniqueFileName);

            try
            {
                // Save the file to C:\ScreenShot
                using (var stream = new FileStream(destinationFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Construct the URL to access the file
                var fileUrl = $"{Request.Scheme}://{Request.Host}/images/{uniqueFileName}";
                return Ok(new UploadResponse { FileName = uniqueFileName, FileUrl = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class UploadResponse
        {
            public string FileName { get; set; }
            public string FileUrl { get; set; }
        }
    }
}
