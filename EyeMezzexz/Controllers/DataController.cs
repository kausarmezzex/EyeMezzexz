using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EyeMezzexz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DataController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private static TimeSpan GetTimeDifference(string clientTimeZone)
        {
            TimeZoneInfo ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            if (clientTimeZone == "GMT Standard Time")
            {
                return TimeSpan.Zero;
            }

            if (clientTimeZone == "Asia/Kolkata")
            {
                DateTime now = DateTime.UtcNow;
                bool isUKSummerTime = now.Month >= 3 && now.Month <= 10;
                return isUKSummerTime ? TimeSpan.FromHours(4.5) : TimeSpan.FromHours(5.5);
            }

            try
            {
                TimeZoneInfo clientZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientZone) - TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ukTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeSpan.Zero;
            }
        }

        [HttpPost("saveScreenCaptureData")]
        public async Task<IActionResult> SaveScreenCaptureData([FromBody] UploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = await _context.TaskTimers
                .Include(t => t.Task)
                .FirstOrDefaultAsync(t => t.Id == model.TaskTimerId);

            if (taskTimer == null)
            {
                return NotFound("TaskTimer not found");
            }

            var uploadedData = new UploadedData
            {
                ImageUrl = model.ImageUrl,
                CreatedOn = _context.GetDatabaseServerTime(),
                Username = model.Username,
                SystemName = model.SystemName,
                TaskName = taskTimer.Task.Name,
                TaskTimerId = model.TaskTimerId,
                VideoUrl = model.VideoUrl
            };

            _context.UploadedData.Add(uploadedData);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Screen capture data uploaded successfully" });
        }

        [HttpGet("getScreenCaptureData")]
        public async Task<IActionResult> GetScreenCaptureData(string clientTimeZone)
        {
            var timeDifference = GetTimeDifference(clientTimeZone);

            var data = await _context.UploadedData
                .Select(d => new
                {
                    d.ImageUrl,
                    Timestamp = d.CreatedOn.Add(timeDifference),
                    d.Username,
                    d.Id,
                    d.SystemName,
                    d.TaskName,
                    d.VideoUrl
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("saveTaskTimer")]
        public async Task<IActionResult> SaveTaskTimer([FromBody] TaskTimerUploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = new TaskTimer
            {
                UserId = model.UserId,
                TaskId = model.TaskId,
                TaskComment = model.TaskComment,
                TaskStartTime = _context.GetDatabaseServerTime(),
                TaskEndTime = model.TaskEndTime,
                TimeDifference = GetTimeDifference(model.ClientTimeZone),
                ClientTimeZone = model.ClientTimeZone
            };

            _context.TaskTimers.Add(taskTimer);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Task timer data uploaded successfully", TaskTimeId = taskTimer.Id });
        }

        [HttpGet("getTaskTimers")]
        public async Task<IActionResult> GetTaskTimers(int userId, string clientTimeZone)
        {
            var today = DateTime.Today;
            var timeDifference = GetTimeDifference(clientTimeZone);

            var taskTimers = await _context.TaskTimers
                .Include(t => t.Task)
                .Include(t => t.User)
                .Where(t => t.TaskStartTime.Date == today && t.TaskEndTime == null)
                .OrderByDescending(t => t.UserId == userId)
                .ThenBy(t => t.TaskStartTime)
                .Select(t => new TaskTimerResponse
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = $"{t.User.FirstName} {t.User.LastName}",
                    TaskId = t.TaskId,
                    TaskName = t.Task.Name,
                    TaskComment = t.TaskComment,
                    TaskStartTime = t.TaskStartTime.Add(timeDifference),
                    TaskEndTime = t.TaskEndTime.HasValue ? t.TaskEndTime.Value.Add(timeDifference) : (DateTime?)null
                })
                .ToListAsync();

            return Ok(taskTimers);
        }

        [HttpPost("saveStaff")]
        public async Task<IActionResult> SaveStaff([FromBody] StaffInOut model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            model.StaffInTime = _context.GetDatabaseServerTime();
            model.TimeDifference = GetTimeDifference(model.ClientTimeZone);

            _context.StaffInOut.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff data saved successfully", StaffId = model.Id });
        }

        [HttpPost("updateStaff")]
        public async Task<IActionResult> UpdateStaff([FromBody] StaffInOut model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var existingStaff = await _context.StaffInOut.FindAsync(model.Id);
            if (existingStaff == null)
            {
                return NotFound("Staff not found");
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            existingStaff.StaffOutTime = model.StaffOutTime.HasValue ? _context.GetDatabaseServerTime() : (DateTime?)null;
            existingStaff.TimeDifference = GetTimeDifference(model.ClientTimeZone);
            existingStaff.ClientTimeZone = model.ClientTimeZone;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the staff record: {ex.Message}");
            }

            return Ok(new { Message = "Staff data updated successfully", StaffId = existingStaff.Id });
        }

        [HttpGet("getStaff")]
        public async Task<IActionResult> GetStaff(string clientTimeZone)
        {
            var timeDifference = GetTimeDifference(clientTimeZone);

            var staff = await _context.StaffInOut
                .Select(s => new
                {
                    s.Id,
                    StaffInTime = s.StaffInTime.Add(timeDifference),
                    StaffOutTime = s.StaffOutTime.HasValue ? s.StaffOutTime.Value.Add(timeDifference) : (DateTime?)null
                })
                .ToListAsync();

            return Ok(staff);
        }

        [HttpGet("getStaffInTime")]
        public async Task<IActionResult> GetStaffInTime(int userId, string clientTimeZone)
        {
            var today = DateTime.Today;
            var timeDifference = GetTimeDifference(clientTimeZone);

            var staffInOut = await _context.StaffInOut
                .Where(s => s.UserId == userId && s.StaffInTime.Date == today && s.StaffOutTime == null)
                .OrderByDescending(s => s.StaffInTime)
                .FirstOrDefaultAsync();

            if (staffInOut == null)
            {
                return NotFound("Staff in time not found for the given user ID and today's date");
            }

            var response = new
            {
                StaffInTime = staffInOut.StaffInTime.Add(timeDifference),
                StaffId = staffInOut.Id
            };

            return Ok(response);
        }

        [HttpPost("updateTaskTimer")]
        public async Task<IActionResult> UpdateTaskTimer([FromBody] UpdateTaskTimerRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = await _context.TaskTimers.FindAsync(model.Id);
            if (taskTimer == null)
            {
                return NotFound("TaskTimer not found");
            }

            taskTimer.TaskEndTime = _context.GetDatabaseServerTime();
            taskTimer.TimeDifference = GetTimeDifference(model.ClientTimeZone);
            taskTimer.ClientTimeZone = model.ClientTimeZone;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the task timer: {ex.Message}");
            }

            return Ok(new { Message = "Task timer updated successfully", TaskTimeId = taskTimer.Id });
        }

        [HttpGet("getTaskTimeId")]
        public async Task<IActionResult> GetTaskTimeId(int taskId, string clientTimeZone)
        {
            var taskTimer = await _context.TaskTimers
                .Where(t => t.Id == taskId && t.TaskEndTime == null)
                .Select(t => new { t.Id })
                .FirstOrDefaultAsync();

            if (taskTimer == null)
            {
                return NotFound("TaskTimer not found");
            }

            return Ok(new { TaskTimeId = taskTimer.Id });
        }

        [HttpGet("getCountries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await _context.Countries.ToListAsync();
            return Ok(countries);
        }

        [HttpGet("getComputers")]
        public async Task<IActionResult> GetComputers()
        {
            var computers = await _context.Computers.ToListAsync();
            return Ok(computers);
        }

        [HttpPost("createTask")]
        public async Task<IActionResult> CreateTask([FromBody] TaskModelRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var task = new TaskNames
            {
                Name = model.Name,
                TaskCreatedBy = model.TaskCreatedBy,
                TaskCreatedOn = _context.GetDatabaseServerTime(),
                ParentTaskId = model.ParentTaskId,
                CountryId = model.CountryId
            };

            if (model.ParentTaskId.HasValue)
            {
                var parentTask = await _context.TaskNames.FindAsync(model.ParentTaskId.Value);
                if (parentTask == null)
                {
                    ModelState.AddModelError("ParentTaskId", "Invalid parent task.");
                    return BadRequest(ModelState);
                }
                parentTask.SubTasks.Add(task);
                await _context.TaskNames.AddAsync(task);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Sub-task created successfully", TaskId = task.Id });
            }
            else
            {
                await _context.TaskNames.AddAsync(task);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Task created successfully", TaskId = task.Id });
            }
        }

        [HttpPut("updateTask")]
        public async Task<IActionResult> UpdateTask([FromBody] TaskNames model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var existingTask = await _context.TaskNames.FindAsync(model.Id);
            if (existingTask == null)
            {
                return NotFound("Task not found");
            }

            existingTask.Name = model.Name;
            existingTask.TaskModifiedOn = _context.GetDatabaseServerTime();
            existingTask.ParentTaskId = model.ParentTaskId;
            existingTask.CountryId = model.CountryId;
            existingTask.TaskModifiedBy = model.TaskModifiedBy;
            if (model.ParentTaskId.HasValue)
            {
                var parentTask = await _context.TaskNames.FindAsync(model.ParentTaskId.Value);
                if (parentTask == null)
                {
                    ModelState.AddModelError("ParentTaskId", "Invalid parent task.");
                    return BadRequest(ModelState);
                }
                parentTask.SubTasks.Add(existingTask);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the task record: {ex.Message}");
            }

            return Ok(new { Message = "Task updated successfully", TaskId = existingTask.Id });
        }

        private List<SelectListItem> BuildTaskSelectList(IEnumerable<TaskNames> tasks, int? parentId = null, string prefix = "")
        {
            var taskSelectList = new List<SelectListItem>();

            var filteredTasks = tasks.Where(t => t.ParentTaskId == parentId).ToList();
            foreach (var task in filteredTasks)
            {
                taskSelectList.Add(new SelectListItem
                {
                    Value = task.Id.ToString(),
                    Text = $"{prefix}{task.Name}"
                });

                var subTasks = BuildTaskSelectList(tasks, task.Id, $"{prefix}{task.Name} >> ");
                taskSelectList.AddRange(subTasks);
            }

            return taskSelectList;
        }

        [HttpGet("getTasks")]
        public async Task<IActionResult> GetTasks(int? countryId = null, int page = 1, int pageSize = 10, string search = "")
        {
            var query = _context.TaskNames
                .Include(t => t.Country)
                .Include(t => t.SubTasks)
                .Where(t => !countryId.HasValue || t.CountryId == countryId)
                .Where(t => string.IsNullOrEmpty(search) || t.Name.Contains(search))
                .AsQueryable();

            var totalTasks = await query.CountAsync();
            var tasks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.ComputerId,
                    t.ComputerRequired,
                    Country = t.Country != null ? new { t.Country.Id, t.Country.Name } : null,
                    SubTasks = t.SubTasks.Select(st => new
                    {
                        st.Id,
                        st.Name,
                        st.ComputerId,
                        st.ComputerRequired,
                        Country = st.Country != null ? new { st.Country.Id, st.Country.Name } : null
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { tasks, totalTasks });
        }


        [HttpGet("getUserCompletedTasks")]
        public async Task<IActionResult> GetUserCompletedTasks(int userId, string clientTimeZone)
        {
            try
            {
                var today = DateTime.Today;
                var timeDifference = GetTimeDifference(clientTimeZone);

                var completedTaskTimers = await _context.TaskTimers
                    .Include(t => t.Task)
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId && t.TaskStartTime.Date == today && t.TaskEndTime != null)
                    .Select(t => new TaskTimerResponse
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        UserName = t.User.FirstName + " " + t.User.LastName,
                        TaskId = t.TaskId,
                        TaskName = t.Task.Name,
                        TaskComment = t.TaskComment,
                        TaskStartTime = t.TaskStartTime,
                        TaskEndTime = t.TaskEndTime
                    })
                    .ToListAsync();

                var adjustedCompletedTaskTimers = completedTaskTimers.Select(t => new TaskTimerResponse
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = t.UserName,
                    TaskId = t.TaskId,
                    TaskName = t.TaskName,
                    TaskComment = t.TaskComment,
                    TaskStartTime = t.TaskStartTime.Add(timeDifference),
                    TaskEndTime = t.TaskEndTime.HasValue ? t.TaskEndTime.Value.Add(timeDifference) : (DateTime?)null
                }).ToList();

                return Ok(adjustedCompletedTaskTimers);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving completed tasks: {ex.Message}");
            }
        }

    }


}

public class TaskModelRequest
{
    public string Name { get; set; }
    public int? ParentTaskId { get; set; }
    public int? CountryId { get; set; } // Add CountryId to TaskModelRequest
    public int? ComputerId { get; set; } // Add ComputerId to TaskModelRequest
    public bool? ComputerRequired { get; set; } // Add ComputerRequired to TaskModelRequest
    public string? TaskCreatedBy { get; set; }
    public string? TaskModifiedBy { get; set; }

}
public class UpdateTaskTimerRequest
{
    public int Id { get; set; }
    public DateTime TaskEndTime { get; set; }
    public string? ClientTimeZone { get; set; }
}

public class TaskTimerResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskComment { get; set; }
    public DateTime TaskStartTime { get; set; }
    public DateTime? TaskEndTime { get; set; }
}
public class TaskTimerUploadRequest
{
    public int UserId { get; set; }
    public int TaskId { get; set; }
    public string? TaskComment { get; set; }
    public DateTime TaskStartTime { get; set; }
    public DateTime? TaskEndTime { get; set; }
    public string? ClientTimeZone { get; set; }
}

public class UploadRequest
{
    public string ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string Username { get; set; }
    public string SystemName { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? TaskTimerId { get; set; } // Add TaskTimerId to UploadRequest
}