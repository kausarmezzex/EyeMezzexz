namespace EyeMezzexz.Models
{
    public class TaskTimer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }  // Foreign key reference to TaskModel
        public string TaskComment { get; set; }

        public TaskModel Task { get; set; }  // Navigation property
    }

}
