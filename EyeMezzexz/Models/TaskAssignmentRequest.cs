namespace EyeMezzexz.Models
{
    public class TaskAssignmentRequest
    {
        public int TaskId { get; set; }
        public TimeSpan AssignedDuration { get; set; } // Time assigned to this task
        public int TargetQuantity { get; set; } // Target quantity for the task
    }
}
