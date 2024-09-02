using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using MezzexEye.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly WebServiceClient _webServiceClient;
        private readonly UserService _userService;
        private readonly ApplicationDbContext _context;

        public AccountApiController(WebServiceClient webServiceClient, UserService userService, ApplicationDbContext context)
        {
            _webServiceClient = webServiceClient;
            _userService = userService;
            _context = context;
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest1 loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(new { message = "Invalid login request." });
            }

            var response = await Task.Run(() => _webServiceClient.GetLoginDetail(loginRequest.Email, loginRequest.Password));
            if (string.IsNullOrEmpty(response))
            {
                return NotFound(new { message = "Login details not found." });
            }


            var loginDetails = JsonSerializer.Deserialize<List<LoginDetailResult>>(response);
            if (loginDetails == null || !loginDetails.Any())
            {
                return NotFound(new { message = "Login details not found." });
            }

            var firstLoginDetail = loginDetails.First();
            var firstName = firstLoginDetail.FirstName;
            var lastName = firstLoginDetail.LastName;

            // Check if the country name is "UK" and replace it with "United Kingdom"
            var country = firstLoginDetail.CountryName == "UK" ? "United Kingdom" : firstLoginDetail.CountryName;

            var phoneNumber = firstLoginDetail.Phone;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null)
            {
                var registerViewModel = new RegisterViewModel
                {
                    Email = loginRequest.Email,
                    Password = loginRequest.Password,
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = "Male",
                    Active = true,
                    Role = "Registered",
                    CountryName = country, // Save the adjusted country name
                    Phone = phoneNumber,
                };

                var result = await _userService.RegisterUser(registerViewModel);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "User registration failed." });
                }

                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            }
            else
            {
                var today = DateTime.UtcNow.Date;
                if (user.LastLoginTime.HasValue && user.LastLoginTime.Value.Date == today && (!user.LastLogoutTime.HasValue || user.LastLoginTime > user.LastLogoutTime))
                {
                    return BadRequest(new { message = "User already logged in on another device today." });
                }
            }

            user.LastLoginTime = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Login successful", userId = user.Id, username = $"{user.FirstName} {user.LastName}", country = user.CountryName });
        }


        [HttpGet("getAllUsernames")]
        public async Task<IActionResult> GetAllUsernames()
        {
            var users = await _context.Users.Select(u => new { u.FirstName, u.LastName }).ToListAsync();
             
            var usernames = users.Select(u => $"{u.FirstName} {u.LastName}").ToList();

            return Ok(usernames);
        }

        [HttpGet("getUsernames")]
        public async Task<IActionResult> GetUsernames()
        {
            var users = await _context.Users
                                      .Select(u => u.Email)
                                      .ToListAsync();

            return Ok(users);
        }

        [HttpGet("getUserByEmail")]
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
        {
            if (logoutRequest == null || string.IsNullOrEmpty(logoutRequest.Email))
            {
                return BadRequest("Invalid logout request.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == logoutRequest.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.LastLogoutTime = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logout successful", userId = user.Id });
        }

    }
    public class LogoutRequest
    {
        public string Email { get; set; }
    }
    public class LoginRequest1
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginDetailResult
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string CountryName { get; set; }
        public string Phone { get; set; }
    }
}
