using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost("assignTasksToUser")]
        public async Task<IActionResult> AssignTasksToUser(int userId, [FromBody] List<int> taskIds)
        {
            if (taskIds == null || taskIds.Count == 0)
            {
                return BadRequest("No tasks provided.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var today = DateTime.Today;
            var existingTasks = await _context.TaskTimers
                .Where(t => t.UserId == userId && t.TaskStartTime.Date == today && t.TaskEndTime == null)
                .Select(t => t.TaskId)
                .ToListAsync();

            var newTasks = taskIds.Except(existingTasks).ToList();

            if (!newTasks.Any())
            {
                return BadRequest("All tasks are already assigned to the user for today.");
            }

            foreach (var taskId in newTasks)
            {
                var task = await _context.TaskNames.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound($"Task with ID {taskId} not found.");
                }

                var taskTimer = new TaskTimer
                {
                    UserId = userId,
                    TaskId = taskId,
                    TaskStartTime = DateTime.UtcNow, // Adjust for time zones as necessary
                    TimeDifference = TimeSpan.Zero // Can modify based on user's timezone
                };

                _context.TaskTimers.Add(taskTimer);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Tasks successfully assigned to the user." });
        }

        // GET: api/TaskAssignment/GetAssignedTasks
        [HttpGet("getAssignedTasks")]
        public async Task<IActionResult> GetAssignedTasks(int userId)
        {
            var today = DateTime.Today;

            var tasks = await _context.TaskTimers
                .Include(t => t.Task)
                .Where(t => t.UserId == userId && t.TaskStartTime.Date == today)
                .Select(t => new
                {
                    TaskId = t.TaskId,
                    TaskName = t.Task.Name,
                    StartTime = t.TaskStartTime,
                    EndTime = t.TaskEndTime
                })
                .ToListAsync();

            if (!tasks.Any())
            {
                return NotFound("No tasks found for the user today.");
            }

            return Ok(tasks);
        }
    }
}
