using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using Microsoft.Extensions.Logging;
using MezzexEye.Models;

namespace EyeMezzexz.Controllers
{
    [Authorize]
    public class DataForViewController : Controller
    {
        private readonly DataController _dataController;
        private readonly AccountApiController _accountApiController;
        private readonly ILogger<DataForViewController> _logger;

        public DataForViewController(DataController dataController, AccountApiController accountApiController, ILogger<DataForViewController> logger)
        {
            _dataController = dataController ?? throw new ArgumentNullException(nameof(dataController));
            _accountApiController = accountApiController ?? throw new ArgumentNullException(nameof(accountApiController));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpGet]
        public async Task<IActionResult> ViewScreenCaptureData(string username = null, DateTime? date = null, int page = 1, string mediaType = "Image")
        {
            try
            {
                int pageSize = 9;

                // Call directly from DataController
                var dataResponse = await _dataController.GetScreenCaptureData("Asia/Kolkata");
                var data = (dataResponse as OkObjectResult)?.Value as List<ScreenCaptureDataViewModel>;

                var usernamesResponse = await _accountApiController.GetAllUsernames();
                var usernames = (usernamesResponse as OkObjectResult)?.Value as List<string>;

                _logger.LogInformation("Retrieved data and usernames from the DataController and AccountApiController.");

                var email = User.Identity.Name;
                if (email == null)
                {
                    _logger.LogError("User email is null.");
                    return StatusCode(500, "Internal server error");
                }

                var userResponse = await _accountApiController.GetUserByEmailAsync(email);
                var user = userResponse as ApplicationUser;

                if (user == null)
                {
                    _logger.LogError("User not found.");
                    return StatusCode(500, "Internal server error");
                }

                if (User.IsInRole("Administrator"))
                {
                    _logger.LogInformation("User is an Administrator.");
                }
                else if (User.IsInRole("Registered"))
                {
                    _logger.LogInformation("User is a Registered user.");
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

                if (mediaType == "Image")
                {
                    data = data.Where(d => !string.IsNullOrEmpty(d.ImageUrl)).ToList();
                }
                else if (mediaType == "Video")
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
                ViewBag.SelectedUsername = username;

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
                var userResponse = await _accountApiController.GetUserByEmailAsync(email);
                var user = userResponse as ApplicationUser;

                if (user == null)
                {
                    _logger.LogError("User not found.");
                    return StatusCode(500, "Internal server error");
                }

                var staffInTimeResponse = await _dataController.GetStaffInTime(user.Id, "Asia/Kolkata");
                var staffInTime = (staffInTimeResponse as OkObjectResult)?.Value as StaffInTimeResponse;

                if (staffInTime == null)
                {
                    _logger.LogInformation("Staff member is not clocked in.");
                    return View("NotClockedIn");
                }

                var taskTypesResponse = await _dataController.GetTasksList();
                var taskTypes = (taskTypesResponse as OkObjectResult)?.Value as List<TaskNames>;

                var activeTasksResponse = await _dataController.GetTaskTimers(user.Id, "Asia/Kolkata");
                var activeTasks = (activeTasksResponse as OkObjectResult)?.Value as List<TaskTimerResponse>;

                var completedTasksResponse = await _dataController.GetUserCompletedTasks(user.Id, "Asia/Kolkata");
                var completedTasks = (completedTasksResponse as OkObjectResult)?.Value as List<TaskTimerResponse>;

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
                var userResponse = await _accountApiController.GetUserByEmailAsync(email);
                var user = userResponse as ApplicationUser;

                if (user == null)
                {
                    return Json(new { isClockedIn = false });
                }

                var staffInTimeResponse = await _dataController.GetStaffInTime(user.Id, "Asia/Kolkata");
                var staffInTime = (staffInTimeResponse as OkObjectResult)?.Value as StaffInTimeResponse;

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
                var userResponse = await _accountApiController.GetUserByEmailAsync(email);
                var user = userResponse as ApplicationUser;

                if (user == null)
                {
                    return BadRequest("User not found");
                }

                model.UserId = user.Id;
                var result = await _dataController.SaveTaskTimer(model);
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

                await _dataController.UpdateTaskTimer(model);
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
                var userResponse = await _accountApiController.GetUserByEmailAsync(email);
                var user = userResponse as ApplicationUser;

                if (user == null)
                {
                    return BadRequest("User not found");
                }

                var staffInTimeResponse = await _dataController.GetStaffInTime(user.Id, "Asia/Kolkata");
                var staffInTime = (staffInTimeResponse as OkObjectResult)?.Value as StaffInTimeResponse;

                if (staffInTime == null)
                {
                    return BadRequest("Staff in time not found");
                }

                var activeTasksResponse = await _dataController.GetTaskTimers(user.Id, "Asia/Kolkata");
                var activeTasks = (activeTasksResponse as OkObjectResult)?.Value as List<TaskTimerResponse>;

                foreach (var task in activeTasks)
                {
                    var endTaskRequest = new UpdateTaskTimerRequest
                    {
                        Id = task.Id,
                        TaskEndTime = DateTime.UtcNow,
                        ClientTimeZone = model.ClientTimeZone
                    };
                    await _dataController.UpdateTaskTimer(endTaskRequest);
                }

                model.Id = staffInTime.StaffId;
                model.StaffInTime = staffInTime.StaffInTime;
                model.StaffOutTime = DateTime.UtcNow;
                model.UserId = user.Id;

                await _dataController.UpdateStaff(model);

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
