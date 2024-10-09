using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EyeMezzexz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskAssignmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskAssignmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/TaskAssignment/AssignTasksToUser
        // Assign multiple tasks with specified duration, target quantity, multiple computers, and country
        [HttpPost("assignTasksToUser")]
        public async Task<IActionResult> AssignTasksToUser(
    int userId,
    [FromBody] List<TaskAssignmentRequest> taskAssignments,
    [FromQuery] List<int> computerIds,
    [FromQuery] string country)
        {
            if (taskAssignments == null || !taskAssignments.Any())
            {
                return BadRequest("No tasks provided.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var computers = await _context.Computers
                .Where(c => computerIds.Contains(c.Id))
                .ToListAsync();

            if (computers.Count != computerIds.Count)
            {
                var missingIds = computerIds.Except(computers.Select(c => c.Id)).ToList();
                return NotFound($"Computers with IDs {string.Join(", ", missingIds)} not found.");
            }

            foreach (var computer in computers)
            {
                foreach (var taskAssignment in taskAssignments)
                {
                    var task = await _context.TaskNames.FindAsync(taskAssignment.TaskId);
                    if (task == null)
                    {
                        return NotFound($"Task with ID {taskAssignment.TaskId} not found.");
                    }

                    var newTaskAssignment = new TaskAssignment
                    {
                        UserId = userId,
                        TaskId = taskAssignment.TaskId,
                        AssignedDuration = taskAssignment.AssignedDuration,
                        TargetQuantity = taskAssignment.TargetQuantity,
                        AssignedDate = DateTime.UtcNow,
                        ComputerId = computer.Id,
                        Country = country
                    };

                    _context.TaskAssignments.Add(newTaskAssignment);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Tasks successfully assigned to the specified computers with duration, target quantity, and country." });
        }

        // GET: api/TaskAssignment/GetAssignedTasks
        // Retrieve tasks assigned to the user for today
        [HttpGet("getAssignedTasks")]
        public async Task<IActionResult> GetAssignedTasks(int userId)
        {
            var today = DateTime.Today;

            var tasks = await _context.TaskAssignments
                .Include(t => t.Task)
                .Include(t => t.Computer) // Include computer information
                .Where(t => t.UserId == userId && t.AssignedDate.Date == today)
                .Select(t => new
                {
                    TaskId = t.TaskId,
                    TaskName = t.Task.Name,
                    AssignedDuration = t.AssignedDuration, // Show the duration assigned to the task
                    TargetQuantity = t.TargetQuantity, // Show the target quantity
                    ComputerName = t.Computer.Name, // Include computer name
                    Country = t.Country // Include country
                })
                .ToListAsync();

            if (!tasks.Any())
            {
                return NotFound("No tasks assigned to the user today.");
            }

            return Ok(tasks);
        }

        // GET: api/TaskAssignment/GetActiveTasks
        // Retrieve only tasks that are currently in progress (tracked by TaskTimer elsewhere)
        [HttpGet("getActiveTasks")]
        public async Task<IActionResult> GetActiveTasks(int userId)
        {
            // Placeholder implementation if active tasks are tracked separately, as by TaskTimer.
            return BadRequest("Active tasks are tracked using a different mechanism (TaskTimer).");
        }
    }
}
