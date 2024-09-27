namespace EyeMezzexz.Models
{
    public class TaskTimer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }  // Foreign key reference to TaskModel
        public string? TaskComment { get; set; }

        public DateTime TaskStartTime { get; set; }
        public DateTime? TaskEndTime { get; set; }
        public ApplicationUser? User { get; set; } // Use ApplicationUser instead of User
        public TaskNames? Task { get; set; }
        public string? ClientTimeZone { get; set; } // Navigation property
        public TimeSpan? TimeDifference { get; set; }
        public string? ActualAddress { get; set; } // Navigation property
    }
}
