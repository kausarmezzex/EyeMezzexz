using EyeMezzexz.Models;

namespace MezzexEye.ViewModel
{
    public class ShiftAssignmentViewModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public List<Shift> Shifts { get; set; }
        public Dictionary<DateTime, int> AvailableHoursPerDay { get; set; }
        public List<ShiftAssignment> Assignments { get; set; }
        public DateTime StartOfWeek { get; set; } // Start date of the selected week
        public List<ShiftAssignmentDto> ExistingAssignments { get; set; } // New Property
    }
    public class ShiftAssignmentDto
    {
        public int UserId { get; set; }
        public int ShiftId { get; set; }
        public int DayIndex { get; set; }
        public bool IsAssigned { get; set; }
    }
}
