using Microsoft.AspNetCore.Mvc;
using System;

namespace EyeMezzexz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerTimeController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetServerTime()
        {
            var serverTimeUtc = DateTime.UtcNow;
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var serverTimeIst = TimeZoneInfo.ConvertTimeFromUtc(serverTimeUtc, timeZoneInfo);

            return Ok(new
            {
                ServerTimeIst = serverTimeIst
            });
        }
    }
}
