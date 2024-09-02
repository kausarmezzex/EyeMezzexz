using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

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

        private static readonly Dictionary<string, TimeSpan> _timeDifferenceCache = new();

        private TimeSpan GetTimeDifference(string clientTimeZone)
        {
            if (_timeDifferenceCache.TryGetValue(clientTimeZone, out var cachedDifference))
            {
                return cachedDifference;
            }

            TimeZoneInfo ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            TimeSpan timeDifference;

            if (clientTimeZone == "GMT Standard Time")
            {
                timeDifference = TimeSpan.Zero;
            }
            else if (clientTimeZone == "Asia/Kolkata")
            {
                DateTime now = DateTime.UtcNow;
                bool isUKSummerTime = now.Month >= 3 && now.Month <= 10;
                timeDifference = isUKSummerTime ? TimeSpan.FromHours(4.5) : TimeSpan.FromHours(5.5);
            }
            else
            {
                try
                {
                    TimeZoneInfo clientZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
                    timeDifference = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientZone) - TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ukTimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    timeDifference = TimeSpan.Zero;
                }
            }

            _timeDifferenceCache[clientTimeZone] = timeDifference;
            return timeDifference;
        }
        [HttpPost("saveScreenCaptureData")]
        public async Task<IActionResult> SaveScreenCaptureData([FromBody] UploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = model.TaskTimerId.HasValue
                ? await _context.TaskTimers.AsNoTracking()
                    .Where(t => t.Id == model.TaskTimerId.Value)
                    .Include(t => t.Task) // Include the Task to get the Name
                    .FirstOrDefaultAsync()
                : null;

            var uploadedData = new UploadedData
            {
                ImageUrl = model.ImageUrl,
                CreatedOn = _context.GetDatabaseServerTime(),
                Username = model.Username,
                SystemName = model.SystemName,
                TaskName = taskTimer?.Task?.Name, // Safe access using null-conditional operator
                TaskTimerId = taskTimer?.Id
            };

            _context.UploadedData.Add(uploadedData);
            await _context.SaveChangesAsync();

            return Ok("Screen capture data saved successfully.");
        }


        [HttpGet("getScreenCaptureData")]
        public async Task<IActionResult> GetScreenCaptureData(string clientTimeZone)
        {
            var timeDifference = GetTimeDifference(clientTimeZone);

            var data = await _context.UploadedData
                .AsNoTracking()
                .Select(d => new
                {
                    d.ImageUrl,
                    d.VideoUrl,
                    d.CreatedOn,
                    d.Username,
                    d.Id,
                    d.SystemName,
                    d.TaskName,
                    Comment = d.TaskTimer.TaskComment
                })
                .ToListAsync();

            var sortedData = data
                .Select(d => new ScreenCaptureDataViewModel
                {
                    ImageUrl = d.ImageUrl,
                    VideoUrl = d.VideoUrl,
                    Timestamp = d.CreatedOn.Add(timeDifference),
                    Username = d.Username,
                    Id = d.Id,
                    SystemName = d.SystemName,
                    TaskName = d.TaskName,
                    Comment = d.Comment
                })
                .OrderByDescending(d => d.Timestamp)
                .ToList();

            return Ok(sortedData);
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

            var taskTimers = await _context.TaskTimers.AsNoTracking()
                .Where(t => t.UserId == userId && t.TaskStartTime.Date == today && t.TaskEndTime == null)
                .OrderBy(t => t.TaskStartTime)
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

        [HttpGet("getTaskTimerById")]
        public async Task<IActionResult> GetTaskTimerById(int userId, string clientTimeZone)
        {
            var today = DateTime.Today;
            var timeDifference = GetTimeDifference(clientTimeZone);

            var taskTimers = await _context.TaskTimers.AsNoTracking()
                .Where(t => t.UserId == userId && t.TaskStartTime.Date == today && t.TaskEndTime == null)
                .OrderBy(t => t.TaskStartTime)
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

        [HttpGet("getAllUserRunningTasks")]
        public async Task<IActionResult> GetAllUserRunningTasks(string clientTimeZone)
        {
            var today = DateTime.Today;
            var timeDifference = GetTimeDifference(clientTimeZone);

            var taskTimers = await _context.TaskTimers.AsNoTracking()
                .Where(t => t.TaskStartTime.Date == today && t.TaskEndTime == null)
                .OrderByDescending(t => t.UserId)
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

            var response = new TaskTimersResponse
            {
                TaskTimers = taskTimers,
                TotalTasks = taskTimers.Count
            };

            return Ok(response);
        }

        [HttpGet("getUsersWithoutRunningTasks")]
        public async Task<IActionResult> GetUsersWithoutRunningTasks(string clientTimeZone)
        {
            var today = DateTime.Today;
            var timeDifference = GetTimeDifference(clientTimeZone);

            var usersWithoutRunningTasks = await _context.StaffInOut
                .GroupJoin(_context.TaskTimers,
                    staff => staff.UserId,
                    task => task.UserId,
                    (staff, tasks) => new { staff, tasks })
                .SelectMany(
                    staffTasks => staffTasks.tasks.DefaultIfEmpty(),
                    (staffTasks, task) => new { staffTasks.staff, task })
                .Where(x => x.staff.StaffInTime.Date == today)
                .GroupBy(x => new { x.staff.UserId, x.staff.User.FirstName, x.staff.User.LastName, x.staff.StaffInTime })
                .Where(g => g.All(x => x.task == null || (x.task.TaskStartTime != null && x.task.TaskEndTime != null)))
                .Select(g => new UserWithoutRunningTasksResponse
                {
                    UserId = g.Key.UserId,
                    UserName = $"{g.Key.FirstName} {g.Key.LastName}",
                    LastStaffInTime = g.Key.StaffInTime,
                    CompletedTasksCount = g.Count(x => x.task != null && x.task.TaskEndTime.HasValue && x.task.TaskEndTime.Value.Date == today),
                    LastTaskEndTime = g.Where(x => x.task != null && x.task.TaskEndTime.HasValue && x.task.TaskEndTime.Value.Date == today)
                                       .Max(x => (DateTime?)x.task.TaskEndTime)
                })
                .ToListAsync();

            var result = usersWithoutRunningTasks.Select(u => new UserWithoutRunningTasksResponse
            {
                UserId = u.UserId,
                UserName = u.UserName,
                LastStaffInTime = u.LastStaffInTime?.Add(timeDifference),
                CompletedTasksCount = u.CompletedTasksCount,
                LastTaskEndTime = u.LastTaskEndTime?.Add(timeDifference)
            }).ToList();

            if (!result.Any())
            {
                return NotFound("No users found who have checked in today and have no running tasks.");
            }

            return Ok(result);
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

            var staff = await _context.StaffInOut.AsNoTracking()
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

            var staffInOut = await _context.StaffInOut.AsNoTracking()
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
            var taskTimer = await _context.TaskTimers.AsNoTracking()
                .Where(t => t.Id == taskId && t.TaskEndTime == null)
                .Select(t => new { t.Id })
                .FirstOrDefaultAsync();

            if (taskTimer == null)
            {
                return NotFound("TaskTimer not found");
            }

            return Ok(new { TaskTimeId = taskTimer.Id });
        }

        [HttpGet("getTaskTimeIdByUser")]
        public async Task<IActionResult> GetTaskTimeIdByUser(int userId)
        {
            var today = DateTime.Today;

            var taskTimer = await _context.TaskTimers.AsNoTracking()
                .Where(t => t.UserId == userId && t.TaskEndTime == null && t.TaskStartTime.Date == today)
                .Select(t => new { t.Id })
                .FirstOrDefaultAsync();

            if (taskTimer == null)
            {
                return Ok(new { TaskTimeId = -1 });
            }

            return Ok(new { TaskTimeId = taskTimer.Id });
        }

        [HttpGet("getCountries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await _context.Countries.AsNoTracking().ToListAsync();
            return Ok(countries);
        }

        [HttpGet("getComputers")]
        public async Task<IActionResult> GetComputers()
        {
            var computers = await _context.Computers.AsNoTracking().ToListAsync();
            return Ok(computers);
        }

        [HttpPost("createTask")]
        public async Task<IActionResult> CreateTask([FromBody] TaskModelRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            if (!model.CountryId.HasValue)
            {
                ModelState.AddModelError("CountryId", "Please choose a country.");
                return BadRequest(ModelState);
            }

            var isDuplicateTask = await _context.TaskNames.AsNoTracking().AnyAsync(t =>
                t.Name == model.Name && t.ParentTaskId == model.ParentTaskId);

            if (isDuplicateTask)
            {
                ModelState.AddModelError("Name", "A task with the same name already exists.");
                return BadRequest(ModelState);
            }

            var task = new TaskNames
            {
                Name = model.Name,
                TaskCreatedBy = model.TaskCreatedBy,
                TaskCreatedOn = _context.GetDatabaseServerTime(),
                ParentTaskId = model.ParentTaskId,
                CountryId = model.CountryId,
                ComputerRequired = model.ComputerRequired
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
            existingTask.ComputerRequired = model.ComputerRequired;
            existingTask.IsDeleted = model.IsDeleted;

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
            var query = _context.TaskNames.AsNoTracking()
                .Include(t => t.Country)
                .Include(t => t.SubTasks)
                .Where(t => !t.IsDeleted.HasValue || !t.IsDeleted.Value)
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
                    t.ComputerRequired,
                    t.TaskCreatedBy,
                    t.TaskCreatedOn,
                    Country = t.Country != null ? new { t.Country.Id, t.Country.Name } : null,
                    SubTasks = t.SubTasks
                                .Where(st => !st.IsDeleted.HasValue || !st.IsDeleted.Value)
                                .Select(st => new
                                {
                                    st.Id,
                                    st.Name,
                                    st.ComputerRequired,
                                    st.TaskCreatedBy,
                                    st.TaskCreatedOn,
                                    Country = st.Country != null ? new { st.Country.Id, st.Country.Name } : null
                                }).ToList()
                })
                .ToListAsync();

            return Ok(new { tasks, totalTasks });
        }

        [HttpGet("getTasksList")]
        public async Task<IActionResult> GetTasksList()
        {
            var tasks = await _context.TaskNames.AsNoTracking()
                .Where(t => !t.IsDeleted.HasValue || !t.IsDeleted.Value)
                .Select(t => new TaskNames
                {
                    Id = t.Id,
                    Name = t.Name,
                    ComputerRequired = t.ComputerRequired,
                    Country = t.Country,
                    SubTasks = t.SubTasks.Where(st => !st.IsDeleted.HasValue || !st.IsDeleted.Value).ToList(),
                    CountryId = t.CountryId,
                    TaskCreatedBy = t.TaskCreatedBy,
                    TaskCreatedOn = t.TaskCreatedOn,
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("getTasksListWithCountry")]
        public async Task<IActionResult> GetTasksListWithCountry(string country = null)
        {
            var tasksQuery = _context.TaskNames.AsNoTracking()
                .Where(t => !t.IsDeleted.HasValue || !t.IsDeleted.Value);

            if (!string.IsNullOrEmpty(country))
            {
                tasksQuery = tasksQuery.Where(t => t.Country.Name == country);
            }

            var tasks = await tasksQuery
                .Select(t => new TaskNames
                {
                    Id = t.Id,
                    Name = t.Name,
                    ComputerRequired = t.ComputerRequired,
                    Country = t.Country,
                    SubTasks = t.SubTasks.Where(st => !st.IsDeleted.HasValue || !st.IsDeleted.Value).ToList(),
                    CountryId = t.CountryId,
                    TaskCreatedBy = t.TaskCreatedBy,
                    TaskCreatedOn = t.TaskCreatedOn,
                })
                .ToListAsync();

            var defaultTasks = await _context.TaskNames.AsNoTracking()
                .Where(t => (t.Name == "Break" || t.Name == "Lunch" || t.Name == "Other")
                            && (!t.IsDeleted.HasValue || !t.IsDeleted.Value))
                .ToListAsync();

            var taskNames = tasks.Select(t => t.Name).ToHashSet();
            var filteredDefaultTasks = defaultTasks
                .Where(t => !taskNames.Contains(t.Name))
                .ToList();

            tasks.AddRange(filteredDefaultTasks);

            return Ok(tasks);
        }

        [HttpGet("getUserCompletedTasks")]
        public async Task<IActionResult> GetUserCompletedTasks(int userId, string clientTimeZone)
        {
            try
            {
                var today = DateTime.Today;
                var timeDifference = GetTimeDifference(clientTimeZone);

                var completedTaskTimers = await _context.TaskTimers.AsNoTracking()
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving completed tasks: {ex.Message}");
            }
        }

        [HttpPost("createTeam")]
        public async Task<IActionResult> CreateTeam([FromBody] Team model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var isDuplicateTeam = await _context.Teams.AsNoTracking()
                .AnyAsync(t => t.Name == model.Name && !t.IsDeleted);

            if (isDuplicateTeam)
            {
                ModelState.AddModelError("Name", "A team with the same name already exists.");
                return BadRequest(ModelState);
            }

            model.CreatedOn = _context.GetDatabaseServerTime();
            model.IsDeleted = false;

            _context.Teams.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Team created successfully", TeamId = model.Id });
        }

        [HttpGet("getTeams")]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _context.Teams.AsNoTracking()
                .Where(t => !t.IsDeleted)
                .Include(t => t.Country)
                .Select(t => new TeamViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    CountryName = t.Country != null ? t.Country.Name : "N/A",
                    CreatedOn = t.CreatedOn,
                    CreatedBy = t.CreatedBy,
                    ModifyOn = t.ModifyOn,
                    ModifyBy = t.ModifyBy
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpPost("editTeam/{id}")]
        public async Task<IActionResult> EditTeam(int id, [FromBody] TeamViewModel model)
        {
            if (model == null || id != model.Id)
            {
                return BadRequest("Invalid team data.");
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            team.Name = model.Name;
            team.CountryId = model.CountryId;
            team.ModifyBy = model.ModifyBy;
            team.ModifyOn = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating team: {ex.Message}");
            }

            return Ok(new { Message = "Team updated successfully" });
        }

        [HttpGet("getTeam/{id}")]
        public async Task<IActionResult> GetTeam(int id)
        {
            var team = await _context.Teams.AsNoTracking()
                .Include(t => t.Country)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (team == null)
            {
                return NotFound("Team not found.");
            }

            var teamViewModel = new TeamViewModel
            {
                Id = team.Id,
                Name = team.Name,
                CountryId = team.CountryId,
                CountryName = team.Country?.Name,
                CreatedOn = team.CreatedOn,
                CreatedBy = team.CreatedBy,
                ModifyOn = team.ModifyOn,
                ModifyBy = team.ModifyBy
            };

            return Ok(teamViewModel);
        }

        [HttpGet("getTeamsByCountryName")]
        public async Task<IActionResult> GetTeamsByCountry(string countryName)
        {
            var teams = await _context.Teams.AsNoTracking()
                .Where(t => t.Country.Name == countryName)
                .Select(t => new TeamViewModel
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpGet("getUsersByCountryName")]
        public async Task<IActionResult> GetUsersByCountryName(string countryName)
        {
            var users = await _context.Users.AsNoTracking()
                .Where(u => u.CountryName == countryName)
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync();

            return Ok(users);
        }
    }
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