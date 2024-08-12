namespace MezzexEye.ViewModel  // Ensure this matches the namespace in your codebase
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }
        public string CountryName { get; set; }
        public string Phone { get; set; }

        public List<string> Roles { get; set; }
    }
}
