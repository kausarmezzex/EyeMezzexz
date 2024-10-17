namespace EyeMezzexz.Models
{
    public class TaskAssignmentComputer
    {
        public int Id { get; set; }  // Primary Key
        public int TaskAssignmentId { get; set; }  // Foreign key to TaskAssignment
        public int ComputerId { get; set; }  // Foreign key to Computer

        // Navigation properties
        public TaskAssignment TaskAssignment { get; set; }
        public Computer Computer { get; set; }
    }
}
