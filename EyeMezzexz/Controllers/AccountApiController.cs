using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using MezzexEye.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
                return BadRequest("Invalid login request.");
            }

            var response = _webServiceClient.GetLoginDetail(loginRequest.Email, loginRequest.Password);
            if (string.IsNullOrEmpty(response))
            {
                return NotFound("Login details not found.");
            }

            // Deserialize the JSON response
            var loginDetails = JsonSerializer.Deserialize<List<LoginDetailResult>>(response);

            if (loginDetails == null || !loginDetails.Any())
            {
                return NotFound("Login details not found.");
            }

            var firstLoginDetail = loginDetails.First();
            var firstName = firstLoginDetail.FirstName;
            var lastName = firstLoginDetail.LastName;

            // Check if the user already exists in the local database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null)
            {
                // User does not exist, register the user
                var registerViewModel = new RegisterViewModel
                {
                    Email = loginRequest.Email,
                    Password = loginRequest.Password,
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = "Male", // Adjust as necessary
                    Active = true,
                    Role = "Registered" // Adjust the role if necessary
                };

                // Register the user
                var result = await _userService.RegisterUser(registerViewModel);
                if (!result.Succeeded)
                {
                    return BadRequest("User registration failed.");
                }

     
            }
            // Fetch the newly created user details
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            // Return the simplified response
            return Ok(new { message = "Login successful", userId = user.Id, username = user.FirstName+" "+user.LastName });
        }

        [HttpGet("getAllUsernames")]
        public async Task<IActionResult> GetAllUsernames()
        {
            var users = await _context.Users.Select(u => new { u.FirstName, u.LastName }).ToListAsync();

            var usernames = users.Select(u => $"{u.FirstName} {u.LastName}").ToList();

            return Ok(usernames);
        }

        [HttpGet("getUsernames")]
        public async Task<IActionResult> GetUsernames(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 3)
            {
                return BadRequest("Query must be at least 3 characters long.");
            }

            var users = await _context.Users
                                      .Where(u => u.Email.StartsWith(query))
                                      .Select(u => u.Email)
                                      .ToListAsync();

            return Ok(users);
        }


        [HttpGet("getUserByEmail")]
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

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
    }
}
