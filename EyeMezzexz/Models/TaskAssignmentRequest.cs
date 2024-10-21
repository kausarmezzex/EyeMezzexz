namespace EyeMezzexz.Models
{
    public class TaskAssignmentRequest
    {
        public int TaskId { get; set; } // ID of the assigned task
        public TimeSpan? AssignedDuration { get; set; }  // Store as TimeSpan
        public int TargetQuantity { get; set; } // Target quantity for the task
        // New properties
        public List<int> ComputerIds { get; set; } = new List<int>(); // List of computer IDs (for UK tasks)
        public string Country { get; set; } // Country for the task (e.g., "India", "UK")
    }
}
