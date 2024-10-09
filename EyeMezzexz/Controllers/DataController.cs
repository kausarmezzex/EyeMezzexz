    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using EyeMezzexz.Data;
    using EyeMezzexz.Models;
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
            private readonly IConfiguration _configuration;
            private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
            public DataController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
            {
                _context = context;
                _userManager = userManager;
                _configuration = configuration;
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
                try
                {
                    TaskTimer taskTimer = null;

                    // Fetch TaskTimer only if TaskTimerId is provided
                    if (model.TaskTimerId.HasValue)
                    {
                        taskTimer = await _context.TaskTimers
                            .Include(t => t.Task)
                            .FirstOrDefaultAsync(t => t.Id == model.TaskTimerId.Value);
                    }

                    var uploadedData = new UploadedData
                    {
                        ImageUrl = model.ImageUrl,
                        CreatedOn = _context.GetDatabaseServerTime(),
                        Username = model.Username,
                        SystemName = model.SystemName,
                        TaskName = taskTimer?.Task?.Name, // Set to null if TaskTimer or Task is null
                        TaskTimerId = taskTimer?.Id // This will be null if TaskTimer is not found
                    };

                    // Add the new record and save changes
                    _context.UploadedData.Add(uploadedData);
                    await _context.SaveChangesAsync();

                    return Ok("Screen capture data saved successfully.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
            [HttpGet("getScreenCaptureData")]
            public async Task<IActionResult> GetScreenCaptureData(string clientTimeZone)
            {
                try
                {
                    var timeDifference = GetTimeDifference(clientTimeZone);

                    // Accessing BaseUrl and UploadFolder from appsettings.json with fallback values
                    string baseUrl = _configuration["EnvironmentSettings:BaseUrl"] ?? "https://sm.mezzex.com";
                    string uploadFolder = _configuration["EnvironmentSettings:UploadFolder"] ?? "ScreenShot";

                    var data = await _context.UploadedData
                        .AsNoTracking()
                        .Include(d => d.TaskTimer) // Explicitly include TaskTimer
                        .Select(d => new
                        {
                            d.ImageUrl,
                            d.VideoUrl,
                            d.CreatedOn,
                            d.Username,
                            d.Id,
                            d.SystemName,
                            d.TaskName,
                            actualAddress = d.TaskTimer.ActualAddress, // Ensure ActualAddress is selected
                            Comment = d.TaskTimer.TaskComment
                        })
                        .ToListAsync();

                    // Ensure data is successfully retrieved
                    if (data == null || !data.Any())
                    {
                        return NotFound("No screen capture data available.");
                    }

                    // Processing the data
                    var sortedData = data
                        .Select(d => new ScreenCaptureDataViewModel
                        {
                            ImageUrl = $"{baseUrl}/{uploadFolder}/{d.ImageUrl}",
                            VideoUrl = d.VideoUrl,
                            Timestamp = d.CreatedOn.Add(timeDifference),
                            Username = d.Username,
                            Id = d.Id,
                            SystemName = d.SystemName,
                            TaskName = d.TaskName,
                            ActualAddress = d.actualAddress, // Include ActualAddress in the view model
                            Comment = d.Comment
                        })
                        .OrderByDescending(d => d.Timestamp)
                        .ToList();

                    // Return the sorted data as a response
                    return Ok(sortedData);
                }
                catch (Exception ex)
                {
                    // Log the error (consider logging framework)
                    return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
                }
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
                    ClientTimeZone = model.ClientTimeZone,
                    ActualAddress = model.ActualAddress
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
                        TaskEndTime = t.TaskEndTime.HasValue ? t.TaskEndTime.Value.Add(timeDifference) : (DateTime?)null,
                        TimeDifference = t.TimeDifference,
                        ActualAddress = t.ActualAddress
                    })
                    .ToListAsync();

                return Ok(taskTimers);
            }

            [HttpGet("getTaskTimerById")]
            public async Task<IActionResult> GetTaskTimerById(int userId, string clientTimeZone)
            {
                var today = DateTime.Today;
                var timeDifference = GetTimeDifference(clientTimeZone);

                var taskTimers = await _context.TaskTimers
                    .Include(t => t.Task)   
                    .Include(t => t.User)
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
                        TaskEndTime = t.TaskEndTime.HasValue ? t.TaskEndTime.Value.Add(timeDifference) : (DateTime?)null,
                        TimeDifference = t.TimeDifference
                    })
                    .ToListAsync();

                return Ok(taskTimers);
            }

            [HttpGet("getAllUserRunningTasks")]
            public async Task<IActionResult> GetAllUserRunningTasks(string clientTimeZone)
            {
                var today = DateTime.Today;
                var timeDifference = GetTimeDifference(clientTimeZone);

                var taskTimers = await _context.TaskTimers
                    .Include(t => t.Task)
                    .Include(t => t.User)
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

                // Fetch users who logged in today and their task timers
                var usersWithoutRunningTasks = await _context.Users
                    .Include(u => u.TaskTimers)
                    .Include(u => u.StaffInOuts) // Include StaffInOuts
                    .Where(u => u.LastLoginTime.HasValue && u.LastLoginTime.Value.Date == today) // Filter users who logged in today
                    .Where(u => !u.TaskTimers.Any() ||  // Users with no tasks
                                !u.TaskTimers.Any(t => t.TaskStartTime.Date == today && t.TaskEndTime == null)) // No running tasks today
                    .Select(u => new
                    {
                        u.Id,
                        UserName = $"{u.FirstName} {u.LastName}",
                        TaskTimers = u.TaskTimers
                                         .Where(t => t.TaskEndTime.HasValue && t.TaskEndTime.Value.Date == today) // Get only today's completed tasks
                                         .ToList(),
                        LastStaffInOut = u.StaffInOuts
                                            .Where(s => s.StaffInTime.Date == today) // Filter only today's StaffInOuts
                                            .OrderByDescending(s => s.StaffInTime)
                                            .FirstOrDefault() // Get the most recent "StaffIn" of the day
                    })
                    .ToListAsync();

                // Map the result and handle null checks
                var response = usersWithoutRunningTasks.Select(u => new UserWithoutRunningTasksResponse
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    CompletedTasksCount = u.TaskTimers.Count, // This now counts only today's completed tasks
                    LastTaskEndTime = u.TaskTimers
                                        .OrderByDescending(t => t.TaskEndTime)
                                        .FirstOrDefault()?.TaskEndTime?.Add(timeDifference), // Latest task's end time, if any
                    LastStaffInTime = u.LastStaffInOut != null ? u.LastStaffInOut.StaffInTime.Add(timeDifference) : (DateTime?)null
                }).ToList();

                return Ok(response);
            }


            [HttpGet("getUsersWithoutLogin")]
            public async Task<IActionResult> GetUsersWithoutLogin(string clientTimeZone)
            {
                var today = DateTime.Today;
                var timeDifference = GetTimeDifference(clientTimeZone);

                // Fetch users who have not logged in today
                var usersWithoutLogin = await _context.Users
                    .Include(u => u.TaskTimers)
                    .Include(u => u.StaffInOuts)
                    .Where(u => !u.LastLoginTime.HasValue || u.LastLoginTime.Value.Date != today) // Filter users who haven't logged in today
                    .Select(u => new
                    {
                        u.Id,
                        UserName = $"{u.FirstName} {u.LastName}",
                        LastLoginTime = u.LastLoginTime,
                        TaskTimers = u.TaskTimers.ToList(), // Fetch all task timers
                        LastStaffInOut = u.StaffInOuts
                                            .OrderByDescending(s => s.StaffInTime)
                                            .FirstOrDefault() // Get the most recent "StaffIn" regardless of the date
                    })
                    .ToListAsync();

                // Map the result and handle null checks
                var response = usersWithoutLogin.Select(u => new UserWithoutLoginResponse
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    LastLoginTime = u.LastLoginTime.HasValue ? u.LastLoginTime.Value.Add(timeDifference) : (DateTime?)null,
                    CompletedTasksCount = u.TaskTimers.Count, // Count all tasks (you can modify this logic if needed)
                    LastTaskEndTime = u.TaskTimers
                                        .OrderByDescending(t => t.TaskEndTime)
                                        .FirstOrDefault()?.TaskEndTime?.Add(timeDifference), // Latest task's end time, if any
                    LastStaffInTime = u.LastStaffInOut != null ? u.LastStaffInOut.StaffInTime.Add(timeDifference) : (DateTime?)null
                }).ToList();

                return Ok(response);
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

            // Method to get staffId by email
            [HttpGet("getStaffIdByEmail")]
            public async Task<IActionResult> GetStaffIdByEmail(string email)
            {
                // Get the ApplicationUser (staff) based on the email
                var user = await _context.Users
                    .Where(u => u.Email == email)
                    .FirstOrDefaultAsync();

                // If user is not found, return NotFound
                if (user == null)
                {
                    return NotFound("No user found with the given email.");
                }

                // Find the latest staff in/out record for that user
                var staffInOut = await _context.StaffInOut
                    .Where(s => s.UserId == user.Id)
                    .OrderByDescending(s => s.StaffInTime)
                    .FirstOrDefaultAsync();

                // If no staffInOut record is found, return NotFound
                if (staffInOut == null)
                {
                    return NotFound("No staff in/out record found for the user.");
                }

                // Return the staffId
                var response = new
                {
                    StaffId = staffInOut.Id,
                    UserId = user.Id,
                    Email = user.Email
                };

                return Ok(response);
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

            [HttpPost("updateTaskAddress")]
            public async Task<IActionResult> UpdateTaskAddress([FromBody] UpdateTaskAddressRequest model)
            {
                if (model == null)
                {
                    return BadRequest("Model is null");
                }

                // Find the task timer by ID
                var taskTimer = await _context.TaskTimers.FindAsync(model.TaskTimeId);
                if (taskTimer == null)
                {
                    return NotFound("TaskTimer not found");
                }

                // Update the actual address
                taskTimer.ActualAddress = model.ActualAddress;

                try
                {
                    // Save the changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the task address: {ex.Message}");
                }

                // Return a success message
                return Ok(new { Message = "Task address updated successfully", TaskTimeId = taskTimer.Id });
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

            [HttpGet("getTaskTimeIdByUser")]
            public async Task<IActionResult> GetTaskTimeIdByUser(int userId)
            {
                var today = DateTime.Today;

                var taskTimer = await _context.TaskTimers
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

                if (!model.CountryId.HasValue)
                {
                    ModelState.AddModelError("CountryId", "Please choose a country.");
                    return BadRequest(ModelState);
                }

                // Check for duplicate task
                bool isDuplicateTask;
                if (model.ParentTaskId.HasValue)
                {
                    isDuplicateTask = await _context.TaskNames.AnyAsync(t => t.Name == model.Name && t.ParentTaskId == model.ParentTaskId);
                }
                else
                {
                    isDuplicateTask = await _context.TaskNames.AnyAsync(t => t.Name == model.Name && t.ParentTaskId == null);
                }

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
                    ComputerRequired = model.ComputerRequired,

                    // Set TargetQuantity
                    TargetQuantity = model.TargetQuantity
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

                // Update all relevant fields
                existingTask.Name = model.Name;
                existingTask.TaskModifiedOn = _context.GetDatabaseServerTime();
                existingTask.ParentTaskId = model.ParentTaskId;
                existingTask.CountryId = model.CountryId;
                existingTask.TaskModifiedBy = model.TaskModifiedBy;
                existingTask.ComputerRequired = model.ComputerRequired;
                existingTask.IsDeleted = model.IsDeleted;
                existingTask.TargetQuantity = model.TargetQuantity; // Add TargetQuantity update

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
                    .Where(t => !t.IsDeleted.HasValue || !t.IsDeleted.Value)  // Exclude deleted tasks
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
                        t.TargetQuantity,
                        Country = t.Country != null ? new { t.Country.Id, t.Country.Name } : null,
                        SubTasks = t.SubTasks
                                    .Where(st => !st.IsDeleted.HasValue || !st.IsDeleted.Value)  // Exclude deleted subtasks
                                    .Select(st => new
                                    {
                                        st.Id,
                                        st.Name,
                                        st.ComputerRequired,
                                        st.TaskCreatedBy,
                                        st.TaskCreatedOn,
                                        st.TargetQuantity,
                                        Country = st.Country != null ? new { st.Country.Id, st.Country.Name } : null
                                    }).ToList()
                    })
                    .ToListAsync();

                return Ok(new { tasks, totalTasks });
            }


            [HttpGet("getTasksList")]
            public async Task<IActionResult> GetTasksList()
            {
                var tasks = await _context.TaskNames
                    .Where(t => !t.IsDeleted.HasValue || !t.IsDeleted.Value)  // Exclude deleted tasks
                    .Select(t => new TaskNames
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ComputerRequired = t.ComputerRequired,
                        Country = t.Country,
                        SubTasks = t.SubTasks.Where(st => !st.IsDeleted.HasValue || !st.IsDeleted.Value).ToList(),  // Exclude deleted subtasks
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
                var tasksQuery = _context.TaskNames
                    .Where(t => !t.IsDeleted.HasValue || !t.IsDeleted.Value);  // Exclude deleted tasks

                // If a country is provided, filter the tasks by the given country
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
                        SubTasks = t.SubTasks.Where(st => !st.IsDeleted.HasValue || !st.IsDeleted.Value).ToList(),  // Exclude deleted subtasks
                        CountryId = t.CountryId,
                        TaskCreatedBy = t.TaskCreatedBy,
                        TaskCreatedOn = t.TaskCreatedOn,
                    })
                    .ToListAsync();

                // Fetch the default tasks from the database
                var defaultTasks = await _context.TaskNames
                    .Where(t => (t.Name == "Break" || t.Name == "Lunch" || t.Name == "Other")
                                && (!t.IsDeleted.HasValue || !t.IsDeleted.Value))
                    .ToListAsync();

                // Remove default tasks from the list if they already exist
                var taskNames = tasks.Select(t => t.Name).ToHashSet();
                var filteredDefaultTasks = defaultTasks
                    .Where(t => !taskNames.Contains(t.Name))
                    .ToList();

                // Add the filtered default tasks to the list
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
                            TaskEndTime = t.TaskEndTime,
                            TimeDifference = t.TimeDifference
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
                        TaskEndTime = t.TaskEndTime.HasValue ? t.TaskEndTime.Value.Add(timeDifference) : (DateTime?)null,
                        TimeDifference = t.TimeDifference

                    }).ToList();

                    return Ok(adjustedCompletedTaskTimers);
                }
                catch (Exception ex)
                {
                    // Log the exception as needed
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

                // Check if a team with the same name already exists
                bool isDuplicateTeam = await _context.Teams.AnyAsync(t => t.Name == model.Name && !t.IsDeleted);
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
                var teams = await _context.Teams
                    .Where(t => !t.IsDeleted)
                    .Include(t => t.Country)  // Include the Country data
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

                // Update team properties
                team.Name = model.Name;
                team.CountryId = model.CountryId;
                team.ModifyBy = model.ModifyBy;
                team.ModifyOn = DateTime.UtcNow; // Assuming user identity is available

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
                var team = await _context.Teams
                    .Include(t => t.Country)  // Include the related Country data
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);  // Ensure the team is not deleted

                if (team == null)
                {
                    return NotFound("Team not found.");
                }

                // Map the team entity to TeamViewModel
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
                var teams = await _context.Teams
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
                var users = await _context.Users
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

        [HttpPost("addComputer")]
        public async Task<IActionResult> AddComputer([FromBody] Computer model)
        {
            if (model == null)
            {
                return BadRequest("Invalid data.");
            }

            // Set the created timestamp and user
            model.CreatedOn = DateTime.UtcNow;

            // Add the new computer record
            _context.Computers.Add(model);

            try
            {
                // Save the changes to the database
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Computer added successfully", ComputerId = model.Id });
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during database save operation
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("editComputer/{id}")]
        public async Task<IActionResult> EditComputer(int id, [FromBody] Computer model)
        {
            if (model == null || id != model.Id)
            {
                return BadRequest("Invalid data.");
            }

            // Find the existing computer record by ID
            var existingComputer = await _context.Computers.FindAsync(id);
            if (existingComputer == null)
            {
                return NotFound("Computer not found.");
            }

            // Update the fields
            existingComputer.Name = model.Name;
            existingComputer.ModifyOn = DateTime.UtcNow;
            existingComputer.IsDeleted = model.IsDeleted;
            existingComputer.TargetQuantity = model.TargetQuantity; // Ensure TargetQuantity is updated
            try
            {
                // Save the updated record to the database
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Computer updated successfully" });
            }
            catch (Exception ex)
            {
                // Handle any errors during update
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("getAllComputers")]
        public async Task<IActionResult> GetAllComputers()
        {
            var computers = await _context.Computers.ToListAsync();

            if (computers == null || !computers.Any())
            {
                return NotFound("No computers found.");
            }

            return Ok(computers);
        }

    }
}


    public class UpdateTaskTimerRequest
    {
        public int Id { get; set; }
        public DateTime? TaskEndTime { get; set; }
        public string? ClientTimeZone { get; set; }

    }

    public class UpdateTaskAddressRequest
    {
        public int TaskTimeId { get; set; }
        public string ActualAddress { get; set; }
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
        public TimeSpan? TimeDifference { get; set; }
        public string? ActualAddress { get; set; }
   
    }
    public class TaskTimerUploadRequest
    {
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public string? TaskComment { get; set; }
        public DateTime TaskStartTime { get; set; }
        public DateTime? TaskEndTime { get; set; }
        public string? ClientTimeZone { get; set; }
        public string? ActualAddress { get; set; } // Navigation property
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