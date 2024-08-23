using EyeMezzexz.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadDataController : ControllerBase
    {
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

            var tempFilePath = Path.Combine(Path.GetTempPath(), uniqueFileName);
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                var uploadedFileName = Uploadnvoice(tempFilePath);
                System.IO.File.Delete(tempFilePath);

                // Using a named class to avoid the anonymous type error
                return Ok(new UploadResponse { FileName = uploadedFileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string Uploadnvoice(string filePath)
        {
            FileInfo fileInf = new FileInfo(filePath);
            string fname = fileInf.Name;  // Use the file's name with the correct extension
            string ftpFilePath = $"ftp://sm.mezzex.com/wwwroot/ScreenShot/{fname}";
            FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpFilePath));

            reqFTP.Credentials = new NetworkCredential("SM", "Direct@143#");
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
            reqFTP.ContentLength = fileInf.Length;

            byte[] buff = new byte[2048];
            int contentLen;

            using (FileStream fs = fileInf.OpenRead())
            using (Stream strm = reqFTP.GetRequestStream())
            {
                contentLen = fs.Read(buff, 0, buff.Length);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buff.Length);
                }
            }

            // Prefix the returned file name with the base URL
            string baseUrl = "https://sm.mezzex.com/ScreenShot/";
            return $"{baseUrl}{fname}";
        }
    }
}
