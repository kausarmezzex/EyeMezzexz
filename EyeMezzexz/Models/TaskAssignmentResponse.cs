namespace EyeMezzexz.Models
{
    public class TaskAssignmentResponse
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public DateTime TaskStartTime { get; set; }
        public DateTime? TaskEndTime { get; set; }
        public TimeSpan AssignedDuration { get; set; }
    }
}
