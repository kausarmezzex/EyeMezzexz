namespace EyeMezzexz.Models
{
    public class TaskAssignment
    {
        public int Id { get; set; } // Primary Key
        public int UserId { get; set; } // Foreign key reference to ApplicationUser
        public int TaskId { get; set; } // Foreign key reference to TaskNames
        public TimeSpan? AssignedDuration { get; set; }  // Store as TimeSpan
        public int? TargetQuantity { get; set; } // Target Quantity of the task
        
        public string? Country { get; set; } // Country where the task is assigned

        public DateTime AssignedDate { get; set; } // Date of the assignment

        // Navigation properties
        public ApplicationUser User { get; set; } // Reference to the user
        public TaskNames Task { get; set; } // Reference to the task
        public ICollection<TaskAssignmentComputer> TaskAssignmentComputers { get; set; }
    }
}
