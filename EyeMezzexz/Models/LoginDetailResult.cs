using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace EyeMezzexz.Models
{
    public class LoginDetailResult
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // New properties
        public string CountryName { get; set; }
        public string Phone { get; set; }
    }


}
