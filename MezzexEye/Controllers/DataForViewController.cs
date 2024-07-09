using EyeMezzexz.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using EyeMezzexz.Models;

namespace EyeMezzexz.Controllers
{
    public class DataForViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataForViewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ViewScreenCaptureData()
        {
            var data = _context.UploadedData.Select(d => new ScreenCaptureDataViewModel
            {
                ImageUrl = d.ImageUrl,
                SystemInfo = d.SystemInfo,
                ActivityLog = d.ActivityLog,
                Timestamp = d.Timestamp,
                Username = d.Username,
                Id = d.Id,
                SystemName = d.SystemName,
                TaskName = d.TaskName
            }).ToList();

            return View(data);
        }
    }
}
