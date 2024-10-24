using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

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
        [HttpPost("assignTasksToUser")]
        public async Task<IActionResult> AssignTasksToUser(int userId, [FromBody] List<TaskAssignmentRequest> taskAssignments, [FromQuery] string country, [FromQuery] DateTime? selectedDate)
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

            // Ensure selectedDate is set
            if (selectedDate == null)
            {
                return BadRequest("Selected date must be provided.");
            }

            // Remove any existing tasks for the user on the selected date
            var existingTaskAssignments = await _context.TaskAssignments
                .Where(ta => ta.UserId == userId && ta.AssignedDate.Date == selectedDate.Value.Date)
                .ToListAsync();

            if (existingTaskAssignments.Any())
            {
                // Remove the existing task assignments
                _context.TaskAssignments.RemoveRange(existingTaskAssignments);

                // Only remove related TaskAssignmentComputer entries if the country is "UK"
                if (country == "United Kingdom")
                {
                    var existingTaskAssignmentIds = existingTaskAssignments.Select(ta => ta.Id).ToList();

                    // Remove associated TaskAssignmentComputer records
                    var existingTaskAssignmentComputers = await _context.TaskAssignmentComputers
                        .Where(tac => existingTaskAssignmentIds.Contains(tac.TaskAssignmentId))
                        .ToListAsync();

                    _context.TaskAssignmentComputers.RemoveRange(existingTaskAssignmentComputers);
                }
            }

            // Add the new task assignments
            foreach (var taskAssignment in taskAssignments)
            {
                var task = await _context.TaskNames.FindAsync(taskAssignment.TaskId);
                if (task == null)
                {
                    return NotFound($"Task with ID {taskAssignment.TaskId} not found.");
                }

                // Create the new TaskAssignment entry
                var newTaskAssignment = new TaskAssignment
                {
                    UserId = userId,
                    TaskId = taskAssignment.TaskId,
                    AssignedDuration = taskAssignment.AssignedDuration,
                    TargetQuantity = taskAssignment.TargetQuantity,
                    AssignedDate = selectedDate.Value,  // Use the provided selected date
                    Country = taskAssignment.Country
                };

                _context.TaskAssignments.Add(newTaskAssignment);

                // If the task is for the UK, handle the computer mappings
                if (taskAssignment.Country == "United Kingdom")
                {
                    if (!taskAssignment.ComputerIds.Any())
                    {
                        return BadRequest("For UK tasks, at least one Computer ID must be provided.");
                    }

                    foreach (var computerId in taskAssignment.ComputerIds)
                    {
                        var computer = await _context.Computers.FindAsync(computerId);
                        if (computer == null)
                        {
                            return NotFound($"Computer with ID {computerId} not found.");
                        }

                        var taskAssignmentComputer = new TaskAssignmentComputer
                        {
                            TaskAssignment = newTaskAssignment,
                            ComputerId = computerId
                        };

                        _context.TaskAssignmentComputers.Add(taskAssignmentComputer);
                    }
                }
            }

            // Save changes to the database
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"A database error occurred: {ex.Message}");
            }

            return Ok(new { Message = "Tasks successfully reassigned with the specified computers and details." });
        }

        // GET: api/TaskAssignment/getAssignedTasks
        // Retrieve tasks assigned to the user for today
        [HttpGet("getAssignedTasks")]
        public async Task<IActionResult> GetAssignedTasks(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var tasks = await _context.TaskAssignments
                .Include(t => t.Task)
                .Include(t => t.TaskAssignmentComputers) // Include related computer mappings
                .ThenInclude(tac => tac.Computer) // Include computer details
                .Where(t => t.UserId == userId && t.AssignedDate.Date == today)
                .Select(t => new
                {
                    TaskId = t.TaskId,
                    TaskName = t.Task.Name,
                    AssignedDuration = t.AssignedDuration,
                    TargetQuantity = t.TargetQuantity,
                    Computers = t.TaskAssignmentComputers
                                 .Select(tac => tac.Computer.Name)
                                 .ToList(), // Retrieve a list of computer names
                    Country = t.Country
                })
                .ToListAsync();

            if (!tasks.Any())
            {
                return NotFound("No tasks assigned to the user today.");
            }

            return Ok(tasks);
        }


        [HttpGet("getAllUsersWithAssignedTasks")]
        public async Task<IActionResult> GetAllUsersWithAssignedTasks(DateTime? assignedDate)
        {
            // Pehle saare users ko fetch karenge
            var allUsers = await _context.Users.ToListAsync();

            // Start a query to fetch Task Assignments
            var taskAssignmentsQuery = _context.TaskAssignments
                .Include(t => t.Task)
                .Include(t => t.TaskAssignmentComputers)
                .ThenInclude(tac => tac.Computer)
                .AsQueryable();

            // Filter by assignedDate agar diya gaya ho
            if (assignedDate.HasValue)
            {
                taskAssignmentsQuery = taskAssignmentsQuery.Where(t => t.AssignedDate.Date == assignedDate.Value.Date);
            }

            // Task assignments ko group karenge user ke basis pe
            var taskAssignments = await taskAssignmentsQuery
                .GroupBy(t => new { t.User.Id, FullName = t.User.FirstName + " " + t.User.LastName })
                .Select(g => new
                {
                    UserId = g.Key.Id,
                    UserName = g.Key.FullName,
                    Computers = g.SelectMany(t => t.TaskAssignmentComputers)
                                 .Select(tac => tac.Computer.Name)
                                 .Distinct()
                                 .ToList(),
                    Tasks = g.Select(t => new TaskDetail
                    {
                        TaskId = t.TaskId,
                        TaskName = t.Task.Name,
                        AssignedDuration = t.AssignedDuration ?? TimeSpan.Zero,
                        TargetQuantity = t.TargetQuantity ?? 0,
                        AssignedDate = t.AssignedDate,
                        Country = t.Country
                    }).ToList()
                })
                .ToListAsync();

            // User ka ek list banaate hain jo final output mein hoga
            var response = new List<object>();

            // Pehle users jinke paas tasks hain, unko add karenge response mein
            foreach (var assignment in taskAssignments)
            {
                response.Add(new
                {
                    UserId = assignment.UserId,
                    UserName = assignment.UserName,
                    HasTasks = true,
                    Computers = assignment.Computers,
                    Tasks = assignment.Tasks
                });
            }

            // Ab un users ko handle karenge jinke paas koi task assigned nahi hai
            var usersWithNoTasks = allUsers
                .Where(u => !taskAssignments.Any(ta => ta.UserId == u.Id))
                .Select(u => new
                {
                    UserId = u.Id,
                    UserName = u.FirstName + " " + u.LastName,
                    HasTasks = false,
                    Computers = new List<string>(),
                    Tasks = new List<TaskDetail>() // Empty task list
                })
                .ToList();

            // No task users ko response mein add karenge
            response.AddRange(usersWithNoTasks);

            // Final response wapas karenge
            return Ok(response);
        }



        [HttpGet("getAllUserAssignedTasks")]
        public async Task<IActionResult> GetAllUserAssignedTasks(DateTime? assignedDate)
        {
            // Start the query
            var query = _context.TaskAssignments
                .Include(t => t.Task)
                .Include(t => t.User)
                .Include(t => t.TaskAssignmentComputers)
                .ThenInclude(tac => tac.Computer)
                .AsQueryable();

            // Filter by assignedDate if provided
            if (assignedDate.HasValue)
            {
                query = query.Where(t => t.AssignedDate.Date == assignedDate.Value.Date); // Only tasks on the specific date
            }

            var tasks = await query
    .GroupBy(t => new { t.User.Id, FullName = t.User.FirstName + " " + t.User.LastName }) // Merge first and last name
    .Select(g => new TaskAssignmentResponse
    {
        UserId = g.Key.Id,
        UserName = g.Key.FullName, // Use the merged FullName
        Computers = g.SelectMany(t => t.TaskAssignmentComputers)
                     .Select(tac => tac.Computer.Name)
                     .Distinct()
                     .ToList(),
        Tasks = g.Select(t => new TaskDetail
        {
            TaskId = t.TaskId,
            TaskName = t.Task.Name,
            AssignedDuration = (TimeSpan)t.AssignedDuration,
            TargetQuantity = (int)t.TargetQuantity,
            AssignedDate = t.AssignedDate,
            Country = t.Country
        }).ToList()
    })
    .ToListAsync();


            // If no tasks found
            if (!tasks.Any())
            {
                return NotFound("No task assignments found.");
            }

            return Ok(tasks);
        }

		// GET: api/TaskAssignment/getTasksForUserWithCountry
		[HttpGet("getTasksForUserWithCountry")]
		public async Task<IActionResult> GetTasksForUserWithCountry(int userId, string country)
		{
			var today = DateTime.UtcNow.Date;

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				return NotFound("User not found.");
			}

			var query = _context.TaskAssignments
				.Include(t => t.Task)
				.Where(t => t.UserId == userId && t.AssignedDate.Date == today)
				.AsQueryable();

			// Filter based on country
			if (country == "United Kingdom")
			{
				query = query.Include(t => t.TaskAssignmentComputers)
							 .ThenInclude(tac => tac.Computer); // Include computer details for UK
			}

			var tasks = await query.Select(t => new TaskDetail
			{
				TaskId = t.TaskId,
				TaskName = t.Task.Name,
				AssignedDuration = t.AssignedDuration ?? TimeSpan.Zero,
				TargetQuantity = t.TargetQuantity ?? 0,
				AssignedDate = t.AssignedDate,
				Country = t.Country,
				Computers = country == "United Kingdom" ? t.TaskAssignmentComputers.Select(tac => tac.Computer.Name).ToList() : null
			}).ToListAsync();

			if (!tasks.Any())
			{
				return NotFound("No tasks assigned to the user today.");
			}

			var response = new TaskAssignmentResponse
			{
				UserId = userId,
				UserName = $"{user.FirstName} {user.LastName}",
				Tasks = tasks,
				Computers = tasks.SelectMany(t => t.Computers ?? new List<string>()).Distinct().ToList() // Only include if UK
			};

			return Ok(response);
		}

	}
}
