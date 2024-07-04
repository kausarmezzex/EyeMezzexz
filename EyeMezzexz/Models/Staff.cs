namespace EyeMezzexz.Models
{
    public class Staff
    {
        public int Id { get; set; }
        public DateTime StaffInTime { get; set; }
        public DateTime? StaffOutTime { get; set; }
        public int UserId { get; set; } // Foreign key property
        public User? User { get; set; }  // Navigation property
    }
}
