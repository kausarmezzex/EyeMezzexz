using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using MezzexEye.Services;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountApiController(WebServiceClient webServiceClient, UserService userService, ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
        {
            _webServiceClient = webServiceClient;
            _userService = userService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest1 loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(new { message = "Invalid login request." });
            }

            // Step 1: Check if the user exists locally using UserManager
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user != null)
            {
                // Step 2: Verify the password using ASP.NET Core Identity
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
                if (isPasswordValid)
                {
                    // Step 3: Check if the user is already logged in today
                    var today = DateTime.UtcNow.Date;
                    if (user.LastLoginTime.HasValue && user.LastLoginTime.Value.Date == today &&
                       (!user.LastLogoutTime.HasValue || user.LastLoginTime > user.LastLogoutTime))
                    {
                        return BadRequest(new { message = "User already logged in on another device today." });
                    }

                    // Update user's login time and system name
                    user.LastLoginTime = DateTime.UtcNow;
                    user.SystemName = loginRequest.SystemName;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Login successful", userId = user.Id, username = $"{user.FirstName} {user.LastName}", country = user.CountryName });
                }
            }

            // Step 4: If the user doesn't exist or the password doesn't match, call the external API for login details
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

            // Adjust country name if needed
            var country = firstLoginDetail.CountryName == "UK" ? "United Kingdom" : firstLoginDetail.CountryName;
            var phoneNumber = firstLoginDetail.Phone;

            // Step 5: Register the user if they don't exist locally
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
                    CountryName = country,
                    Phone = phoneNumber,
                    SystemName = loginRequest.SystemName
                };

                var result = await _userService.RegisterUser(registerViewModel);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "User registration failed." });
                }

                user = await _userManager.FindByEmailAsync(loginRequest.Email);
            }
            else
            {
                // Step 6: Update user's details if necessary
                if (string.IsNullOrEmpty(user.CountryName) || string.IsNullOrEmpty(user.PhoneNumber))
                {
                    user.CountryName = string.IsNullOrEmpty(user.CountryName) ? country : user.CountryName;
                    user.PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber) ? phoneNumber : user.PhoneNumber;
                }
            }

            // Step 7: Update login details
            user.LastLoginTime = DateTime.UtcNow;
            user.SystemName = loginRequest.SystemName;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Login successful", userId = user.Id, username = $"{user.FirstName} {user.LastName}", country = user.CountryName });
        }
        [HttpPost("checkLogoutStatus")]
        public async Task<IActionResult> CheckLogoutStatus([FromBody] LogoutRequest logoutRequest)
        {
            if (logoutRequest == null || string.IsNullOrEmpty(logoutRequest.Email))
            {
                return BadRequest(new { message = "Invalid request. Email is required." });
            }

            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == logoutRequest.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check if the user is logged out by comparing LastLoginTime and LastLogoutTime
            if (user.LastLogoutTime.HasValue && user.LastLoginTime.HasValue && user.LastLogoutTime > user.LastLoginTime)
            {
                // User has logged out after logging in
                return Ok(new { isLoggedOut = true, message = "User is logged out." });
            }

            // If LastLogoutTime is null or earlier than LastLoginTime, the user is still logged in
            return Ok(new { isLoggedOut = false, message = "User is still logged in." });
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
        [HttpGet("getSystemNameByEmail")]
        public async Task<IActionResult> GetSystemNameByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email cannot be empty." });
            }

            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // User not found, return appropriate message
                return NotFound(new { message = "User not found." });
            }

            // Return the system name (null if it's not set)
            return Ok(new
            {
                email = user.Email,
                systemName = user.SystemName // This will be null if not set
            });
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
        [HttpPost("checkLoginStatus")]
        public async Task<IActionResult> CheckLoginStatus([FromBody] LoginRequest2 loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.SystemName))
            {
                return BadRequest(new { message = "Invalid request." });
            }

            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check if the user is already logged in today
            var today = DateTime.UtcNow.Date;
            if (user.LastLoginTime.HasValue && user.LastLoginTime.Value.Date == today && (!user.LastLogoutTime.HasValue || user.LastLoginTime > user.LastLogoutTime))
            {
                // User is already logged in, now check if they are logged in from a different system
                if (user.SystemName != loginRequest.SystemName)
                {
                    // If logged in from a different system, send the system name and instruct redirection
                    return Ok(new { isLoggedInOnAnotherDevice = true, systemName = user.SystemName });
                }
                else
                {
                    // User is logged in from the same system
                    return Ok(new { isLoggedInOnAnotherDevice = false, message = "User is already logged in on this system." });
                }
            }

            // User is not logged in today, so allow login
            return Ok(new { isLoggedInOnAnotherDevice = false, message = "User can log in." });
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
        public string SystemName { get; set; }  // New Property for system name
    }

    public class LoginRequest2
    {
        public string Email { get; set; }
        public string SystemName { get; set; }  // New Property for system name
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
