namespace MezzexEye.Models
{
    public class TaskModelRequest
    {
        public string Name { get; set; }
        public int? ParentTaskId { get; set; } // Add ParentTaskId to TaskModelRequest
        public string? TaskCreatedBy { get; set; }
        public DateTime? TaskCreatedOn { get; set; }
    }
}
