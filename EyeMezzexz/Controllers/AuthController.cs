using EyeMezzexz.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Models;
using System.Linq;

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
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(new { message = "Login successful", userId = user.Id, username = user.Username });
        }

    }
}
