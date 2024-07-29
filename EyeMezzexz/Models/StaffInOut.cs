namespace EyeMezzexz.Models
{
    public class StaffInOut
    {
        public int Id { get; set; }
        public DateTime StaffInTime { get; set; }
        public DateTime? StaffOutTime { get; set; }
        public int UserId { get; set; } // Foreign key property
        public ApplicationUser? User { get; set; }
        public string? ClientTimeZone { get; set; }// Use ApplicationUser instead of User
        public TimeSpan? TimeDifference { get; set; }
    }
}
