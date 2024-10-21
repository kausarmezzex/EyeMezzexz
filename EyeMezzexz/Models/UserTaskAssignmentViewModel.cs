namespace EyeMezzexz.Models
{
    public class UserTaskAssignmentViewModel
    {
        public List<ApplicationUser>? Users { get; set; } // List of users
        public List<TaskNames>? AvailableTasks { get; set; } // List of tasks
        public List<Computer>? Computers { get; set; } // List of computers
        public string CurrentCountry { get; set; } // New property to track the selected country
                                                   // This captures the task assignments submitted from the form
        public DateTime? SelectedDate { get; set; }
        public List<UserTaskAssignment>? UserTaskAssignments { get; set; }
    }
}
