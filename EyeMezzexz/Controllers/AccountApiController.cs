using EyeMezzexz.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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

            if (response == null || response.Any == null || response.Any.Length == 0)
            {
                return NotFound("No login details found.");
            }

            var loginDetails = DeserializeLoginDetails(response.Any);

            if (loginDetails == null || loginDetails.Count == 0)
            {
                return NotFound("No login details found.");
            }

            return Ok(loginDetails);
        }

        private List<LoginDetail> DeserializeLoginDetails(XmlElement[] xmlElements)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<LoginDetail>), new XmlRootAttribute("LoginDetails"));

                // Combine the XmlElement contents into a single XML string for deserialization
                var combinedXml = string.Join("", xmlElements.Select(x => x.OuterXml));
                using (var reader = new StringReader(combinedXml))
                {
                    return (List<LoginDetail>)serializer.Deserialize(reader);
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public class LoginRequest1
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginDetail
    {
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        // Add other necessary properties
    }

    public class GetLoginDetailResponseGetLoginDetailResult
    {
        private XmlElement[] anyField;

        [XmlAnyElement(Namespace = "http://www.w3.org/2001/XMLSchema", Order = 0)]
        public XmlElement[] Any
        {
            get { return anyField; }
            set { anyField = value; }
        }
    }
}
