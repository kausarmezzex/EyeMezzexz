using System.Collections.Generic;
using System.Threading.Tasks;
using EyeMezzexz.Models;

namespace MezzexEye.Services
{
    public interface IShiftAssignmentApiService
    {
        Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(); // Fetch all shift assignments
        Task<ShiftAssignment> GetShiftAssignmentByIdAsync(int assignmentId); // Fetch assignment by ID
        Task<bool> AddShiftAssignmentAsync(ShiftAssignment assignment); // Create a new shift assignment
        Task<bool> UpdateShiftAssignmentAsync(int assignmentId, ShiftAssignment updatedAssignment); // Update assignment
        Task<bool> DeleteShiftAssignmentAsync(int assignmentId); // Delete assignment
    }
}
