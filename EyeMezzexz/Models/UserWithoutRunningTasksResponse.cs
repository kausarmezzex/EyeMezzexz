namespace EyeMezzexz.Models
{
    public class UserWithoutRunningTasksResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CompletedTasksCount { get; set; }
        public DateTime? LastTaskEndTime { get; set; }
        public DateTime? LastStaffInTime { get; set; } // Add StaffInTime property
    }
}
