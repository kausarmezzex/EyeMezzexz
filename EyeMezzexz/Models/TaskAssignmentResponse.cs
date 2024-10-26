namespace EyeMezzexz.Models
{
    public class TaskAssignmentResponse
    {
        // User Information
        public int UserId { get; set; }
        public string UserName { get; set; }

        // List of Tasks for the User
        public List<TaskDetail> Tasks { get; set; } = new List<TaskDetail>();

        // List of assigned computer names, shown only once per user
        public List<string> Computers { get; set; } = new List<string>();
    }

    public class TaskDetail
    {
        // Task Information
        public int TaskId { get; set; }
        public string TaskName { get; set; }

        // Task Assignment Details
        public TimeSpan AssignedDuration { get; set; } // Duration represented as TimeSpan
        public int TargetQuantity { get; set; }
        public DateTime AssignedDate { get; set; } // When the task was assigned
        public string Country { get; set; } // Country associated with the task

        // List of assigned computer names (only for UK)
        public List<string>? Computers { get; set; } = new List<string>();
    }
}
