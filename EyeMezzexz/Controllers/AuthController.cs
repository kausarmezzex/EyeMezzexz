using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using System.Linq;
using System;

namespace EyeMezzexz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == model.Username);

            

            return Ok(new { message = "Login successful", userId = user.Id, username = user.UserName });
        }

        [HttpGet("usernames")]
        public IActionResult GetUsernames()
        {
            var usernames = _context.Users.Select(u => u.UserName).ToList();
            return Ok(usernames);
        }

    }
}
