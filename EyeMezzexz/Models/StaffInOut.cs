using System.ComponentModel.DataAnnotations.Schema;

namespace EyeMezzexz.Models
{
    public class StaffInOut
    {
        // Primary Key
        public int Id { get; set; }

        // The time when the staff member checked in
        public DateTime StaffInTime { get; set; }

        // The time when the staff member checked out, can be null if not yet checked out
        public DateTime? StaffOutTime { get; set; }

        // Foreign key property for the user associated with this entry
        [ForeignKey("User")]
        public int UserId { get; set; }

        // Navigation property to the associated user
        public ApplicationUser? User { get; set; }

        // The client's timezone (optional)
        public string? ClientTimeZone { get; set; }

        // The time difference based on the client's timezone (optional)
        public TimeSpan? TimeDifference { get; set; }
    }
}
