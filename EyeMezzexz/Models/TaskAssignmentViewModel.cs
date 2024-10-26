using Microsoft.AspNetCore.Mvc;

namespace EyeMezzexz.Models
{
    public class TaskAssignmentViewModel
    {
        public int TaskId { get; set; }
        public TimeSpan? AssignedDuration { get; set; } // Assigned duration for the task
                                                        // This property will capture the hours from the input field
        [BindProperty(Name = "AssignedDurationHours")]
        public int? AssignedDurationHours { get; set; }
        public int TargetQuantity { get; set; } // Quantity target for the task
        public List<int>? ComputerIds { get; set; } = new List<int>(); // Multiple computers
        public string Country { get; set; } // Country for the task
    }
}
