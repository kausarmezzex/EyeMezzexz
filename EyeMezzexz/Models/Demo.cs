namespace EyeMezzexz.Models
{
    public class Demo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // This should store the hashed password
        public int Salt { get; set; } // This should store the salt value
    }
}
