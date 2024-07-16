using EyeMezzexz.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

            var response = await _webServiceClient.GetLoginDetailAsync(loginRequest.Email, loginRequest.Password);
            if (string.IsNullOrEmpty(response))
            {
                return NotFound("Login details not found.");
            }

            return Ok(response);
        }

        [HttpPost("loginSync")]
        public IActionResult LoginSync([FromBody] LoginRequest1 loginRequest)
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

            return Ok(response);
        }
    }

    public class LoginRequest1
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
