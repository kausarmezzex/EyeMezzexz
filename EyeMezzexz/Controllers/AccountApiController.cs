using EyeMezzexz.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly WebServiceClient _webServiceClient;

        public AccountApiController(WebServiceClient webServiceClient)
        {
            _webServiceClient = webServiceClient;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest1 loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Invalid login request.");
            }

            bool isValid = await _webServiceClient.CheckLoginDetailAsync(loginRequest.Email, loginRequest.Password);
            if (isValid)
            {
                // Handle successful login
                return Ok(new { Message = "Login successful." });
            }
            else
            {
                // Handle login failure
                return Unauthorized(new { Message = "Invalid email or password." });
            }
        }
    }
}
public class LoginRequest1
{
    public string Email { get; set; }
    public string Password { get; set; }
}

