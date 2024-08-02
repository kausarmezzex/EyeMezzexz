using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using Microsoft.Extensions.Logging;
using MezzexEye.Models;

namespace EyeMezzexz.Controllers
{
    [Authorize]
    public class DataForViewController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<DataForViewController> _logger;

        public DataForViewController(ApiService apiService, ILogger<DataForViewController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ViewScreenCaptureData(string username = null, DateTime? date = null, int page = 1, string mediaType = "Image")
        {
            try
            {
                int pageSize = 9; // Set pageSize to 9
                var data = await _apiService.GetScreenCaptureDataAsync();
                var usernames = await _apiService.GetAllUsernamesAsync();

                _logger.LogInformation("Retrieved data and usernames from the API.");

                // Get the logged-in user's email
                var email = User.Identity.Name;
                var user = await _apiService.GetUserByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogError("User not found.");
                    return StatusCode(500, "Internal server error");
                }

                // Check the role of the logged-in user
                if (User.IsInRole("Administrator"))
                {
                    _logger.LogInformation("User is an Administrator.");
                    // No additional filtering needed, show all data and usernames
                }
                else if (User.IsInRole("Registered"))
                {
                    _logger.LogInformation("User is a Registered user.");
                    // Filter data to only show the logged-in user's data
                    var loggedInFullName = $"{user.FirstName} {user.LastName}";
                    data = data.Where(d => $"{d.Username}" == loggedInFullName).ToList();
                    usernames = usernames.Where(u => u == loggedInFullName).ToList();
                }

                if (!string.IsNullOrEmpty(username))
                {
                    data = data.Where(d => $"{d.Username}" == username).ToList();
                    _logger.LogInformation($"Filtered data by username: {username}");
                }

                DateTime filterDate = date ?? DateTime.Today;
                data = data.Where(d => d.Timestamp.Date == filterDate.Date).ToList();
                _logger.LogInformation($"Filtered data by date: {filterDate.Date}");

                // Filter by media type
                if (mediaType == "Image")
                {
                    data = data.Where(d => !string.IsNullOrEmpty(d.ImageUrl)).ToList();
                }
                else if (mediaType == "video")
                {
                    data = data.Where(d => !string.IsNullOrEmpty(d.VideoUrl)).ToList();
                }

                var totalItems = data.Count();
                var paginatedData = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var viewModel = new PaginatedScreenCaptureDataViewModel
                {
                    ScreenCaptures = paginatedData,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                };

                ViewBag.Usernames = usernames;
                ViewBag.MediaType = mediaType;
                ViewBag.SelectedUsername = username; // Set the selected username

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    _logger.LogInformation("Returning partial view for AJAX request.");
                    return PartialView("_ScreenCaptureData", viewModel);
                }

                _logger.LogInformation("Returning full view.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the screen capture data.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TaskManagement()
        {
            try
            {
                var email = User.Identity.Name;
                var user = await _apiService.GetUserByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogError("User not found.");
                    return StatusCode(500, "Internal server error");
                }

                // Check if the staff member is clocked in
                var staffInTime = await _apiService.GetStaffInTimeAsync(user.Id);
               
                if (staffInTime == null)
                {
                    _logger.LogInformation("Staff member is not clocked in.");
                    return View("NotClockedIn"); // Return a view indicating the user needs to clock in first
                }

                var taskTypes = await _apiService.GetTasksAsync();
                var activeTasks = await _apiService.GetTaskTimersAsync(user.Id);
                var completedTasks = await _apiService.GetUserCompletedTasksAsync(user.Id);

                var viewModel = new TaskManagementViewModel
                {
                    TaskTypes = taskTypes,
                    ActiveTasks = activeTasks,
                    CompletedTasks = completedTasks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading task management data.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckClockInStatus()
        {
            try
            {
                var email = User.Identity.Name;
                var user = await _apiService.GetUserByEmailAsync(email);

                if (user == null)
                {
                    return Json(new { isClockedIn = false });
                }

                var staffInTime = await _apiService.GetStaffInTimeAsync(user.Id, "Asia/Kolkata");
                return Json(new { isClockedIn = staffInTime != null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking clock-in status.");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> StartTask([FromBody] TaskTimerUploadRequest model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Model is null");
                }
                var email = User.Identity.Name;
                var user = await _apiService.GetUserByEmailAsync(email);
                model.UserId = user.Id;

                await _apiService.SaveTaskTimerAsync(model);
                TaskManagement();
                return Ok(new { Message = "Task started successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting the task.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EndTask([FromBody] UpdateTaskTimerRequest model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Model is null");
                }

                await _apiService.UpdateTaskTimerAsync(model);

                return Ok(new { Message = "Task ended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while ending the task.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> StaffOut([FromBody] StaffInOut model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Model is null");
                }
                var email = User.Identity.Name;
                var user = await _apiService.GetUserByEmailAsync(email);
                var staffInTime = await _apiService.GetStaffInTimeAsync(user.Id);
                var activeTasks = await _apiService.GetTaskTimersAsync(model.UserId);
                foreach (var task in activeTasks)
                {
                    var endTaskRequest = new UpdateTaskTimerRequest
                    {
                        Id = task.Id,
                        TaskEndTime = DateTime.UtcNow,
                        ClientTimeZone = model.ClientTimeZone
                    };
                    await _apiService.UpdateTaskTimerAsync(endTaskRequest);
                }
                model.Id = staffInTime.StaffId;
                model.StaffInTime = staffInTime.StaffInTime;
                model.StaffOutTime = DateTime.UtcNow;
                model.UserId = user.Id;
                await _apiService.UpdateStaffAsync(model);

                return Ok(new { Message = "Staff out successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating staff out.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
