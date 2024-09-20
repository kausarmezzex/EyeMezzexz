using EyeMezzexz.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadDataController : ControllerBase
    {
        private readonly string _uploadPhysicalFolder;
        private readonly string _uploadFolder;
        private readonly string _baseUrl;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public UploadDataController(IConfiguration configuration)
        {
            _uploadPhysicalFolder = configuration["EnvironmentSettings:UploadPhysicalFolder"];
            _uploadFolder = configuration["EnvironmentSettings:UploadFolder"];
            _baseUrl = configuration["EnvironmentSettings:BaseUrl"];

            // Ensure _uploadPhysicalFolder is a valid path
            if (string.IsNullOrEmpty(_uploadPhysicalFolder) || _uploadPhysicalFolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException("Invalid upload physical folder path.");
            }
        }

        [HttpPost("upload-Images")]
        public async Task<IActionResult> UploadImages(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not provided.");
            }

            // Create a unique file name with timestamp and GUID
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid()}{fileExtension}";

            // Construct the physical path to save the file
            var physicalFilePath = Path.Combine(_uploadPhysicalFolder, uniqueFileName);

            // Ensure the directory exists
            if (!Directory.Exists(_uploadPhysicalFolder))
            {
                Directory.CreateDirectory(_uploadPhysicalFolder);
            }

            // Use SemaphoreSlim for asynchronous synchronization
            await _semaphoreSlim.WaitAsync(); // Wait asynchronously
            try
            {
                // Save the file to the physical path
                using (var stream = new FileStream(physicalFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Construct the URL to access the file using _baseUrl and _uploadFolder
                var fileAccessUrl = uniqueFileName;

                // Return the URL as the response
                return Ok(new UploadResponse { FileName = fileAccessUrl });
            }
            catch (Exception ex)
            {
                // Log the exception (Optional: Use a logging framework here)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                // Always release the semaphore to avoid deadlocks
                _semaphoreSlim.Release();
            }
        }
    }

    // Define the UploadResponse class
    public class UploadResponse
    {
        public string FileName { get; set; }  // File access URL
    }
}
