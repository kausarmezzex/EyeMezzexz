namespace EyeMezzexz.Models
{
    public class IncompleteTasksViewModel
    {
        // Update the type to match the expected type of tasks
        public List<UserWithoutRunningTasksResponse> IncompleteTasks { get; set; }

        // This property holds the total number of incomplete tasks
        public int TotalIncompleteTasks { get; set; }
    }
}
