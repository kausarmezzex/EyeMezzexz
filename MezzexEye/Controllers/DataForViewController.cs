using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using Microsoft.Extensions.Logging;
using MezzexEye.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using MezzexEye.ViewModel;

namespace EyeMezzexz.Controllers
{
    [Authorize]
    public class DataForViewController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<DataForViewController> _logger;
        private readonly DataController _dataController;
        private readonly TeamAssignmentApiController _teamAssignmentApiController;
        public DataController DataController { get; }

        public DataForViewController(DataController dataController, ApiService apiService, ILogger<DataForViewController> logger, TeamAssignmentApiController teamAssignmentApiController)
        {
            _dataController = dataController;
            _apiService = apiService;
            _logger = logger;
            _teamAssignmentApiController = teamAssignmentApiController;
        }

        [HttpGet]
        public async Task<IActionResult> ViewScreenCaptureData(string username = null, DateTime? date = null, string taskName = null, int page = 1, string mediaType = "Image")
        {
            try
            {
                var data = await _apiService.GetScreenCaptureDataAsync();
                var usernames = await _apiService.GetAllUsernamesAsync();

                _logger.LogInformation("Retrieved data and usernames from the API.");

                // Get the logged-in user's email
                var email = User.Identity.Name;
                if (email == null)
                {
                    _logger.LogError("User email is null.");
                    return StatusCode(500, "Internal server error");
                }

                var user = await _apiService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogError("User not found.");
                    return StatusCode(500, "Internal server error");
                }

                // Filter data based on user roles
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

                // Apply filters
                if (!string.IsNullOrEmpty(username))
                {
                    data = data.Where(d => $"{d.Username}" == username).ToList();
                    _logger.LogInformation($"Filtered data by username: {username}");
                }

                DateTime filterDate = date ?? DateTime.Today;
                data = data.Where(d => d.Timestamp.Date == filterDate.Date).ToList();
                _logger.LogInformation($"Filtered data by date: {filterDate.Date}");

                if (taskName != null)
                {
                    if (taskName == "No Task")
                    {
                        data = data.Where(d => string.IsNullOrEmpty(d.TaskName)).ToList();
                        _logger.LogInformation("Filtered data for records with no task name.");
                    }
                    else
                    {
                        data = data.Where(d => d.TaskName == taskName).ToList();
                        _logger.LogInformation($"Filtered data by task name: {taskName}");
                    }
                }

                if (mediaType == "Image")
                {
                    data = data.Where(d => !string.IsNullOrEmpty(d.ImageUrl)).ToList();
                }
                else if (mediaType == "Video")
                {
                    data = data.Where(d => !string.IsNullOrEmpty(d.VideoUrl)).ToList();
                }

                // Implement pagination with 12 items per page
                int pageSize = 12; // Show 12 items per page
                int totalRecords = data.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                data = data.Skip((page - 1) * pageSize).Take(pageSize).ToList(); // Take only the records for the current page

                // Calculate page numbers to display (limit visible pages)
                int maxVisiblePages = 5; // Show 5 pages around the current page
                int startPage = Math.Max(1, page - (maxVisiblePages / 2));
                int endPage = Math.Min(totalPages, startPage + maxVisiblePages - 1);

                if (endPage - startPage + 1 < maxVisiblePages)
                {
                    startPage = Math.Max(1, endPage - maxVisiblePages + 1);
                }

                var pageNumbers = Enumerable.Range(startPage, endPage - startPage + 1).ToList();

                var viewModel = new PaginatedScreenCaptureDataViewModel
                {
                    ScreenCaptures = data,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageNumbers = pageNumbers
                };

                var taskTypes= await _apiService.GetTasksListAsync();
                var taskNames = taskTypes.Select(t => t.Name).Distinct().ToList();
                taskNames.Add("No Task");

                ViewBag.Usernames = usernames;
                ViewBag.TaskNames = taskNames;
                ViewBag.MediaType = mediaType;
                ViewBag.SelectedUsername = username;
                ViewBag.SelectedTaskName = taskName;
                ViewBag.SelectedDate = filterDate.ToString("yyyy-MM-dd");

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

                var taskTypes = await _apiService.GetTasksListAsync();
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
        public async Task<IActionResult> ViewAllRunningTasks()
        {
            try
            {
                // Call the API service to get all running tasks and their total count
                var (allRunningTasks, totalRunningTasks) = await _apiService.GetAllUserRunningTasksAsync("Asia/Kolkata");
                var incompleteTasks = await _apiService.GetIncompleteTasksAsync("Asia/Kolkata");

                // Count the total number of incomplete tasks
                var totalIncompleteTasks = incompleteTasks.Count;
                ViewBag.UsersNotStartTask = totalIncompleteTasks;
                // Create a view model to pass data to the view
                var viewModel = new RunningTasksViewModel
                {
                    AllRunningTasks = allRunningTasks,
                    TotalRunningTasks = totalRunningTasks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving running tasks.");
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
        // GET: /DataForView/CreateTeam
        [HttpGet]
        public async Task<IActionResult> CreateTeam()
        {
            var countriesResponse = await _dataController.GetCountries();
            var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;
            ViewBag.Countries = new SelectList(countries, "Id", "Name");
            return View();
        }

        public async Task<IActionResult> TeamList()
        {
            try
            {
                // Call the GetTeams method from the DataController
                var responseTeam = await _dataController.GetTeams();

                // Extract the data from the action result and cast it to List<TeamViewModel>
                var teams = (responseTeam as OkObjectResult)?.Value as List<TeamViewModel>;

                if (teams == null)
                {
                    _logger.LogError("Failed to retrieve teams.");
                    return StatusCode(500, "Failed to retrieve teams.");
                }

                // Pass the List<TeamViewModel> to the view
                return View(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the team list.");
                return StatusCode(500, "Internal server error");
            }
        }



        // POST: /DataForView/CreateTeam
        [HttpPost]
        public async Task<IActionResult> CreateTeam(TeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var email = User.Identity.Name;
                    var user = await _apiService.GetUserByEmailAsync(email);

                    var team = new Team
                    {
                        Name = model.Name,
                        CreatedBy = user.Email,
                        CountryId = model.CountryId,
                    };

                    int teamId = await _apiService.CreateTeamAsync(team);

                    if (teamId > 0)
                    {
                        _logger.LogInformation("Team created successfully with ID: {TeamId}", teamId);
                        return RedirectToAction("TeamList");
                    }
                    else
                    {
                        var countriesResponse = await _dataController.GetCountries();
                        var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;
                        ViewBag.Countries = new SelectList(countries, "Id", "Name");
                        
                        ModelState.AddModelError("", "Failed to create team. Please try again.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating the team.");
                    ModelState.AddModelError("", "An error occurred while creating the team.");
                }
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditTeam(int id)
        {
            try
            {
                var countriesResponse = await _dataController.GetCountries();
                var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;

                // Fetch the team details
                var responseTeam = await _dataController.GetTeam(id);
                var team = (responseTeam as OkObjectResult)?.Value as TeamViewModel;

                if (team == null)
                {
                    _logger.LogError("Failed to retrieve team details.");
                    return NotFound("Team not found.");
                }

                // Populate ViewBag with country list
                ViewBag.Countries = new SelectList(countries, "Id", "Name", team.CountryId);

                return View(team);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the team data.");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditTeam(TeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.ModifyBy = User.Identity.Name;
                    // Directly call the API method to update the team
                    var result = await _dataController.EditTeam(model.Id, model);

                    if ((result as OkObjectResult) != null)
                    {
                        _logger.LogInformation("Team updated successfully.");
                        return RedirectToAction("TeamList");
                    }
                    else
                    {
                        _logger.LogError("Failed to update team.");
                        ModelState.AddModelError("", "Failed to update team.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the team.");
                    ModelState.AddModelError("", "An error occurred while updating the team.");
                }
            }

            // Re-populate countries list if the model state is invalid
            var countriesResponse = await _dataController.GetCountries();
            var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AssignUserToTeam()
        {
            try
            {
                var model = await _apiService.GetAssignmentDataAsync();
                if (model == null)
                {
                    _logger.LogError("Failed to load assignment data.");
                    return StatusCode(500, "Failed to load assignment data.");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the assignment data.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: /DataForView/AssignUserToTeam
        [HttpPost]
        public async Task<IActionResult> AssignUserToTeam(TeamAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool success = await _apiService.AssignUserToTeamAsync(model);
                    if (success)
                    {
                        _logger.LogInformation("User assigned to team successfully.");
                        return RedirectToAction("TeamList"); // Create a success page or redirect as needed
                    }
                    else
                    {
                        _logger.LogError("Failed to assign user to team.");
                        ModelState.AddModelError("", "Failed to assign user to team. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while assigning the user to the team.");
                    ModelState.AddModelError("", "An error occurred while assigning the user to the team.");
                }
            }

            // Reload the assignment data if the model state is invalid
            var assignmentData = await _apiService.GetAssignmentDataAsync();
            if (assignmentData == null)
            {
                _logger.LogError("Failed to reload assignment data.");
                return StatusCode(500, "Failed to reload assignment data.");
            }
            model.Teams = assignmentData.Teams;
            model.Users = assignmentData.Users;
            model.Countries = assignmentData.Countries;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamsAndUsersByCountry(string countryName)
        {
            try
            {
                // Fetch teams and users based on the selected country
                var responseTeams = await _dataController.GetTeamsByCountry(countryName);
                var responseUsers = await _dataController.GetUsersByCountryName(countryName);

                var teamsJson = JsonConvert.SerializeObject((responseTeams as OkObjectResult)?.Value);
                var usersJson = JsonConvert.SerializeObject((responseUsers as OkObjectResult)?.Value);

                var teams = JsonConvert.DeserializeObject<List<TeamViewModel>>(teamsJson);
                var users = JsonConvert.DeserializeObject<List<UserModel>>(usersJson);

                if (teams == null || users == null)
                {
                    return StatusCode(500, "Failed to retrieve teams or users.");
                }

                return Json(new
                {
                    teams = teams.Select(t => new { id = t.Id, name = t.Name }),
                    users = users.Select(u => new { id = u.Id, name = $"{u.FirstName} {u.LastName}" })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching teams and users.");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ViewAllTeamAssignments()
        {
            try
            {
                var response = await _teamAssignmentApiController.GetAllTeamAssignments();
                var assignments = (response as OkObjectResult)?.Value as List<TeamAssignmentViewModel1>;

                if (assignments == null)
                {
                    _logger.LogError("Failed to retrieve team assignments.");
                    return StatusCode(500, "Failed to retrieve team assignments.");
                }

                return View(assignments); // Pass the strongly typed model to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving team assignments.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewIncompleteTasks()
        {
            try
            {
                // Fetch incomplete tasks using the ApiService
                var userTasks = await _apiService.GetIncompleteTasksAsync("Asia/Kolkata");

                // Return the view with the list of UserWithoutRunningTasksResponse
                return View(userTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving incomplete tasks.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> UsersWithoutLogin()
        {
            try
            {
                // Call the API service to get users who haven't logged in today
                var usersWithoutLogin = await _apiService.GetUsersWithoutLoginAsync("Asia/Kolkata");

                // Return the view with the list of users
                return View(usersWithoutLogin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users without login.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        public async Task<int> GetTotalRunningTasks()
        {
            var (_, totalRunningTasks) = await _apiService.GetAllUserRunningTasksAsync("Asia/Kolkata");
            return totalRunningTasks;
        }


    }
}
