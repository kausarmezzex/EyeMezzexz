using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using MezzexEye.Services;
using MezzexEye.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EyeMezzexz.Helpers; // Include the namespace for DateTimeExtensions

namespace MezzexEye.Controllers
{
    public class ManageShiftAssignmentController : Controller
    {
        private readonly IShiftAssignmentApiService _shiftAssignmentService;
        private readonly IShiftApiService _shiftService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageShiftAssignmentController> _logger;

        public ManageShiftAssignmentController(
            IShiftAssignmentApiService shiftAssignmentService,
            IShiftApiService shiftService,
            UserManager<ApplicationUser> userManager,
            ILogger<ManageShiftAssignmentController> logger)
        {
            _shiftAssignmentService = shiftAssignmentService;
            _shiftService = shiftService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get the current week starting from Monday
            var startOfWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(6); // Calculate end of the week

            // Fetch data asynchronously
            var users = _userManager.Users.ToList();
            var shifts = await _shiftService.GetShiftsAsync();
            var assignments = await _shiftAssignmentService.GetShiftAssignmentsAsync();

            // Map assignments to DTOs for easy consumption in the view
            var assignmentDtos = assignments.Select(a => new ShiftAssignmentDto
            {
                UserId = a.UserId,
                ShiftId = a.ShiftId,
                DayIndex = (int)(a.AssignedOn - startOfWeek).TotalDays, // Calculate day index within the week
                IsAssigned = true // Since the record exists, it’s considered assigned
            }).ToList();

            // Prepare the view model
            var model = new ShiftAssignmentViewModel
            {
                Users = users,
                Shifts = shifts,
                AvailableHoursPerDay = GetAvailableHoursForWeek(startOfWeek),
                ExistingAssignments = assignmentDtos, // Use pre-mapped assignments
                StartOfWeek = startOfWeek
            };

            return View(model);
        }


        private Dictionary<DateTime, int> GetAvailableHoursForWeek(DateTime startOfWeek)
        {
            var hours = new Dictionary<DateTime, int>();
            for (int i = 0; i < 7; i++)
            {
                hours.Add(startOfWeek.AddDays(i), 125 - (i * 10)); // Example hours logic
            }
            return hours;
        }

        [HttpPost]
        public async Task<IActionResult> SaveShiftAssignments([FromBody] List<ShiftAssignment> assignments)
        {

            try
            {
                foreach (var assignment in assignments)
                {
                    
                    if (assignment.UserId != null && assignment.ShiftId != null)
                    {
                        var existingAssignment = await _shiftAssignmentService.GetShiftAssignmentByIdAsync(assignment.AssignmentId);
                        if (existingAssignment != null)
                        {
                            assignment.ModifiedBy = User.Identity.Name;
                            await _shiftAssignmentService.UpdateShiftAssignmentAsync(assignment.AssignmentId, assignment);
                        }
                        else
                        {
                            assignment.CreatedBy = User.Identity.Name;
                            await _shiftAssignmentService.AddShiftAssignmentAsync(assignment);
                        }
                    }
                    else
                    {
                        await _shiftAssignmentService.DeleteShiftAssignmentAsync(assignment.AssignmentId);
                    }
                }
                return Ok(new { success = true, message = "Shift assignments saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving shift assignments.");
                return StatusCode(500, new { success = false, message = "Error saving shift assignments." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleShiftAssignment(int staffId, int dayIndex, bool isChecked)
        {
            try
            {
                var assignment = await _shiftAssignmentService.GetShiftAssignmentByIdAsync(staffId);

                if (isChecked)
                {
                    if (assignment == null)
                    {
                        assignment = new ShiftAssignment
                        {
                            UserId = staffId,
                            ShiftId = dayIndex, // Assuming dayIndex corresponds to the shift ID; adjust as necessary
                            AssignedOn = DateTime.Now
                        };
                        await _shiftAssignmentService.AddShiftAssignmentAsync(assignment);
                    }
                    else
                    {
                        assignment.ShiftId = dayIndex;
                        await _shiftAssignmentService.UpdateShiftAssignmentAsync(assignment.AssignmentId, assignment);
                    }
                }
                else if (assignment != null)
                {
                    await _shiftAssignmentService.DeleteShiftAssignmentAsync(assignment.AssignmentId);
                }

                return Ok(new { success = true, message = "Shift assignment updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling shift assignment for staffId {staffId} and dayIndex {dayIndex}.");
                return StatusCode(500, new { success = false, message = "Error toggling shift assignment." });
            }
        }
    }
}
