using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EyeMezzexz.Controllers;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MezzexEye.Services
{
    public class ShiftAssignmentApiService : IShiftAssignmentApiService
    {
        private readonly ShiftAssignmentController _shiftAssignmentController;
        private readonly ILogger<ShiftAssignmentApiService> _logger;

        public ShiftAssignmentApiService(ShiftAssignmentController shiftAssignmentController, ILogger<ShiftAssignmentApiService> logger)
        {
            _shiftAssignmentController = shiftAssignmentController;
            _logger = logger;
        }

        // Fetch all shift assignments
        public async Task<List<ShiftAssignment>> GetShiftAssignmentsAsync()
        {
            var result = await _shiftAssignmentController.GetShiftAssignments();

            if (result.Result is OkObjectResult okResult && okResult.Value is IEnumerable<ShiftAssignment> assignments)
            {
                return new List<ShiftAssignment>(assignments);
            }

            _logger.LogError("Failed to retrieve shift assignments.");
            return new List<ShiftAssignment>();
        }

        // Fetch shift assignment details by ID
        public async Task<ShiftAssignment> GetShiftAssignmentByIdAsync(int assignmentId)
        {
            var result = await _shiftAssignmentController.GetShiftAssignmentById(assignmentId);

            if (result.Result is OkObjectResult okResult && okResult.Value is ShiftAssignment assignment)
            {
                return assignment;
            }

            _logger.LogError($"Failed to retrieve shift assignment with ID {assignmentId}.");
            return null;
        }

        // Add a new shift assignment
        public async Task<bool> AddShiftAssignmentAsync(ShiftAssignment assignment)
        {

            var result = await _shiftAssignmentController.AssignShift(assignment);

            if (result.Result is CreatedAtActionResult)
            {
                _logger.LogInformation("Shift assignment successfully created.");
                return true;
            }

            _logger.LogError("Failed to create shift assignment.");
            return false;
        }

        // Update an existing shift assignment
        public async Task<bool> UpdateShiftAssignmentAsync(int assignmentId, ShiftAssignment updatedAssignment)
        {
            var existingAssignment = await GetShiftAssignmentByIdAsync(assignmentId);
            if (existingAssignment == null)
            {
                _logger.LogError($"Shift assignment with ID {assignmentId} does not exist.");
                return false;
            }

            // Set audit properties
            updatedAssignment.ModifiedBy = "System"; // Replace with actual user if available
            updatedAssignment.ModifiedOn = DateTime.Now;

            var result = await _shiftAssignmentController.UpdateShiftAssignment(assignmentId, updatedAssignment);

            if (result is NoContentResult)
            {
                _logger.LogInformation("Shift assignment successfully updated.");
                return true;
            }

            _logger.LogError("Failed to update shift assignment.");
            return false;
        }

        // Delete a shift assignment by ID
        public async Task<bool> DeleteShiftAssignmentAsync(int assignmentId)
        {
            var existingAssignment = await GetShiftAssignmentByIdAsync(assignmentId);
            if (existingAssignment == null)
            {
                _logger.LogError($"Shift assignment with ID {assignmentId} does not exist.");
                return false;
            }

            var result = await _shiftAssignmentController.DeleteShiftAssignment(assignmentId);

            if (result is NoContentResult)
            {
                _logger.LogInformation("Shift assignment successfully deleted.");
                return true;
            }

            _logger.LogError($"Failed to delete shift assignment with ID {assignmentId}.");
            return false;
        }
    }
}
