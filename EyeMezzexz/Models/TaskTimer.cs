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
        public TaskModel? Task { get; set; }  // Navigation property
    }
}
