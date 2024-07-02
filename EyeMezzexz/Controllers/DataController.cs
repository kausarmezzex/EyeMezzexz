using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using System;
using System.Linq;

namespace EyeMezzexz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public IActionResult UploadData([FromBody] UploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var uploadedData = new UploadedData
            {
                ImageUrl = model.ImageUrl,
                SystemInfo = model.SystemInfo,
                ActivityLog = model.ActivityLog,
                Timestamp = model.Timestamp,  // Use the timestamp from the model
                Username = model.Username,
                SystemName = model.SystemName
            };

            _context.UploadedData.Add(uploadedData);
            _context.SaveChanges();

            return Ok(new { Message = "Data uploaded successfully" });
        }

        [HttpGet("getdata")]
        public IActionResult GetData()
        {
            var data = _context.UploadedData.Select(d => new
            {
                ImageUrl = d.ImageUrl,
                SystemInfo = d.SystemInfo,
                ActivityLog = d.ActivityLog,
                Timestamp = d.Timestamp,
                Username = d.Username,
                SystemName = d.SystemName
            }).ToList();

            return Ok(data);
        }
    }

    public class UploadRequest
    {
        public string ImageUrl { get; set; }
        public string SystemInfo { get; set; }
        public string ActivityLog { get; set; }
        public string Username { get; set; }
        public string SystemName { get; set; }
        public DateTime Timestamp { get; set; }  // Add Timestamp property
    }
}
