using System;
using System.ComponentModel.DataAnnotations;

namespace MezzexEye.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        public bool Active { get; set; }

        public string Role { get; set; }

        // New properties
        [Required]
        public string CountryName { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
    }
}
