namespace EyeMezzexz.Models
{
    public class StaffInOut
    {
        public int Id { get; set; }
        public DateTime StaffInTime { get; set; }
        public DateTime? StaffOutTime { get; set; }
        public int UserId { get; set; } // Foreign key property
        public ApplicationUser? User { get; set; }  // Use ApplicationUser instead of User
    }
}
