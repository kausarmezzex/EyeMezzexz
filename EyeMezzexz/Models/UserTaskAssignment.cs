namespace EyeMezzexz.Models
{
    public class UserTaskAssignment
    {
        public int UserId { get; set; } // Reference to User
        public List<TaskAssignmentViewModel> TaskAssignments { get; set; } // List of Task Assignments
    }
}
