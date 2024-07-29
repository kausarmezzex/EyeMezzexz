using EyeMezzexz.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace EyeMezzexz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerTimeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ServerTimeController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetServerTime()
        {
            var serverTimeUtc = _context.GetDatabaseServerTime();
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            /*var serverTimeIst = TimeZoneInfo.ConvertTimeFromUtc(serverTimeUtc, timeZoneInfo);*/

            return Ok(new
            {
                ServerTimeIst = serverTimeUtc
            });
        }
    }
}
