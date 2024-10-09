namespace EyeMezzexz.Models
{
    public class UserTaskAssignmentViewModel
    {
        public List<ApplicationUser> Users { get; set; } // List of users
        public List<TaskNames>? AvailableTasks { get; set; } // Available tasks to assign
        public List<Computer>? Computers { get; set; } // Available computers for selection
        public string SelectedCountry { get; set; } // Country selected for all users
        public List<UserTaskAssignment> UserTaskAssignments { get; set; } // List of assignments for each user
    }

}
