namespace EyeMezzexz.Models
{
    public class UserTaskAssignment
    {
        public int UserId { get; set; } // User ID
        public List<TaskAssignmentRequest> TaskAssignments { get; set; } // Tasks assigned to the user
        public List<int>? SelectedComputerIds { get; set; } // Selected computer IDs
    }
}
