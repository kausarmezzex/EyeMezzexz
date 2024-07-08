using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using System.Linq;
using AspDotNetStorefrontEncrypt;
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

        [HttpPost("computesaltedhash")]
        public IActionResult ComputeSaltedHash1([FromBody] ComputeSaltedHashRequest model)
        {
            try
            {
                var saltedHash = Encrypt.ComputeSaltedHash(model.Salt, model.ClearPassword);
                return Ok(new { SaltedHash = saltedHash });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Salted hash computation failed", error = ex.Message });
            }
        }

        [HttpPost("adddemo")]
        public IActionResult AddDemo([FromBody] AddDemoRequest model)
        {
            try
            {
                /*// Generate a salt and hash the password
                var salt = new Random().Next(1000, 9999); // Replace with a better random salt generator if needed*/
               // var hashedPassword = Encrypt.ComputeSaltedHash(model.Salt, model.Password);

                var demo = new Demo
                {
                    Username = model.Username,
                    Password = model.Password, // Store the hashed password
                    Salt = model.Salt // Store the salt value
                };

                _context.demos.Add(demo);
                _context.SaveChanges();

                return Ok(new { message = "Demo data added successfully", userId = demo.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to add demo data", error = ex.Message });
            }
        }

        [HttpPost("verifyPassword")]
        public IActionResult VerifyPassword([FromBody] VerifyPasswordRequest model)
        {
            try
            {
                var user = _context.demos.FirstOrDefault(u => u.Username == model.Username);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                int salt = user.Salt;
                string storedSaltedHash = user.Password.ToString();

                string computedSaltedHash = Encrypt.ComputeSaltedHash(salt, model.Password);

                if (computedSaltedHash == storedSaltedHash)
                {
                    return Ok(new { message = "Password verification successful" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Password verification failed", error = ex.Message });
            }
        }
    }

    public class AddDemoRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Salt { get; set; }   
    }

    public class ComputeSaltedHashRequest
    {
        public int Salt { get; set; }
        public string ClearPassword { get; set; }
    }

    public class VerifyPasswordRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
