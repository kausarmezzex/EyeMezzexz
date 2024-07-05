using EyeMezzexz.Data;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Models;
using System.Linq;
using System.Collections.Generic;

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
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(new { message = "Login successful", userId = user.Id, username = user.Username });
        }

        [HttpGet("usernames")]
        public IActionResult GetUsernames()
        {
            var usernames = _context.Users.Select(u => u.Username).ToList();
            return Ok(usernames);
        }
    }
}
