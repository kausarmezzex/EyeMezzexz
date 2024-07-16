using System.Xml.Serialization;

namespace EyeMezzexz.Models
{
    [XmlRoot(ElementName = "LoginDetailResult", Namespace = "http://tempuri.org/")]
    public class LoginDetailResult
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
