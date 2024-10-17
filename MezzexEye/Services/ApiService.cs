using System.Collections.Generic;
using System.Threading.Tasks;
using EyeMezzexz.Controllers; // Assuming this is the namespace of your API controllers
using EyeMezzexz.Models;
using MezzexEye.Models;
using MezzexEye.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class ApiService : IApiService
{
    private readonly DataController _dataController;
    private readonly AccountApiController _accountApiController;
    private readonly TeamAssignmentApiController _teamAssignmentApiController;
    private readonly ILogger<ApiService> _logger;
    private readonly TaskAssignmentController _taskAssignmentController; // Add this line
    public ApiService(DataController dataController, AccountApiController accountApiController, TeamAssignmentApiController teamAssignmentApiController, ILogger<ApiService> logger, TaskAssignmentController taskAssignmentController)
    {
        _dataController = dataController;
        _accountApiController = accountApiController;
        _teamAssignmentApiController = teamAssignmentApiController;
        _taskAssignmentController = taskAssignmentController; // Assign it here
        _logger = logger;
        _taskAssignmentController = taskAssignmentController;
    }

    public async Task<List<ScreenCaptureDataViewModel>> GetScreenCaptureDataAsync(string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetScreenCaptureData(clientTimeZone);
        var value = (result as OkObjectResult)?.Value;

        if (value == null)
        {
            _logger.LogError("Failed to retrieve data from API. Result value is null or invalid.");
            return new List<ScreenCaptureDataViewModel>();
        }

        // Serialize and deserialize the result to ensure proper casting
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<ScreenCaptureDataViewModel>>(jsonString);

        if (data != null)
        {
            // Check if the ImageUrl contains the full URL (starts with http or https)
            foreach (var item in data)
            {
                if (!item.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Log if the URL is malformed
                    _logger.LogError($"Image URL {item.ImageUrl} is not a valid full URL.");
                }
            }
            return data;
        }

        _logger.LogError("Failed to deserialize data from API.");
        return new List<ScreenCaptureDataViewModel>();
    }
    public async Task AssignTasksToUserAsync(int userId, List<TaskAssignmentRequest> taskAssignments, string country)
    {
        // Log data for debugging
        _logger.LogInformation("Assigning tasks for user {UserId} with tasks: {@TaskAssignments}, country: {Country}", userId, taskAssignments, country);

        try
        {
            // Call the API to assign tasks
            await _taskAssignmentController.AssignTasksToUser(userId, taskAssignments, country);
        }
        catch (SqlException ex)
        {
            // Log detailed SQL error
            _logger.LogError("SQL Exception occurred: {Message}", ex.Message);
            throw; // Re-throw to handle it further up the stack
        }
    }


    public async Task<List<TaskAssignmentResponse>> GetAssignedTasksAsync(DateTime? assignedDate)
    {
        // Call the GetAllUserAssignedTasks method to fetch tasks for all users
        var result = await _taskAssignmentController.GetAllUserAssignedTasks(assignedDate);

        // Check if the result is a valid OkObjectResult
        var value = (result as OkObjectResult)?.Value;

        // Convert the result to JSON string for deserialization
        var jsonString = JsonConvert.SerializeObject(value);

        // Deserialize the JSON string to a List of TaskAssignmentResponse objects
        var data = JsonConvert.DeserializeObject<List<TaskAssignmentResponse>>(jsonString);

        // Return the data or an empty list if no data is found
        return data ?? new List<TaskAssignmentResponse>();
    }


    public async Task SaveScreenCaptureDataAsync(UploadRequest model)
    {
        await _dataController.SaveScreenCaptureData(model);
    }

    public async Task<List<TaskTimerResponse>> GetTaskTimersAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetTaskTimerById(userId, clientTimeZone);
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<TaskTimerResponse>>(jsonString);

        return data ?? new List<TaskTimerResponse>();
    }

    public async Task SaveTaskTimerAsync(TaskTimerUploadRequest model)
    {
        await _dataController.SaveTaskTimer(model);
    }

    public async Task<List<TaskTimerResponse>> GetUserCompletedTasksAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetUserCompletedTasks(userId, clientTimeZone);
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<TaskTimerResponse>>(jsonString);

        return data ?? new List<TaskTimerResponse>();
    }

    public async Task<(List<TaskNames> Tasks, int TotalTasks)> GetTasksAsync(int? countryId = null, int page = 1, int pageSize = 10, string search = "")
    {
        var result = await _dataController.GetTasks(countryId, page, pageSize, search);
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var response = JsonConvert.DeserializeObject<ApiResponse>(jsonString);

        var tasks = response?.Tasks ?? new List<TaskNames>();
        var totalTasks = response?.TotalTasks ?? 0;

        return (tasks, totalTasks);
    }

    public async Task<List<TaskNames>> GetTasksListAsync()
    {
        var result = await _dataController.GetTasksList();
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<TaskNames>>(jsonString);

        return data ?? new List<TaskNames>();
    }

    public async Task SaveStaffAsync(StaffInOut model)
    {
        await _dataController.SaveStaff(model);
    }

    public async Task UpdateStaffAsync(StaffInOut model)
    {
        await _dataController.UpdateStaff(model);
    }

    public async Task<List<object>> GetStaffAsync(string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetStaff(clientTimeZone);
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<object>>(jsonString);

        return data ?? new List<object>();
    }

    public async Task<StaffInTimeResponse> GetStaffInTimeAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetStaffInTime(userId, clientTimeZone);
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<StaffInTimeResponse>(jsonString);

        return data;
    }

    public async Task UpdateTaskTimerAsync(UpdateTaskTimerRequest model)
    {
        await _dataController.UpdateTaskTimer(model);
    }

    public async Task CreateTaskAsync(TaskModelRequest model)
    {
        await _dataController.CreateTask(model);
    }

    public async Task<int?> GetTaskTimeIdAsync(int taskId)
    {
        var result = await _dataController.GetTaskTimeId(taskId, "Asia/Kolkata");
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<int?>(jsonString);

        return data;
    }

    public async Task UpdateTaskAsync(TaskNames model)
    {
        await _dataController.UpdateTask(model);
    }

    public async Task<List<string>> GetAllUsernamesAsync()
    {
        var result = await _accountApiController.GetAllUsernames();
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<string>>(jsonString);

        return data ?? new List<string>();
    }


    public async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
        // Assuming _accountApiController.GetUserByEmailAsync returns ApplicationUser directly
        var result = await _accountApiController.GetUserByEmailAsync(email);

        if (result != null)
        {
            return result;
        }

        _logger.LogError($"Failed to retrieve user with email: {email}. Result is null.");
        return null;
    }


    public async Task<List<Country>> GetCountriesAsync()
    {
        var result = await _dataController.GetCountries();
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<Country>>(jsonString);

        return data ?? new List<Country>();
    }

    public async Task<(List<TaskTimerResponse> TaskTimers, int TotalTasks)> GetAllUserRunningTasksAsync(string clientTimeZone)
    {
        var result = await _dataController.GetAllUserRunningTasks(clientTimeZone) as OkObjectResult;
        if (result != null)
        {
            var value = result.Value;
            var jsonString = JsonConvert.SerializeObject(value);
            var response = JsonConvert.DeserializeObject<TaskTimersResponse>(jsonString);

            return (response?.TaskTimers ?? new List<TaskTimerResponse>(), response?.TotalTasks ?? 0);
        }

        _logger.LogError("Failed to retrieve running tasks.");
        return (new List<TaskTimerResponse>(), 0);
    }

    public async Task<List<UserWithoutRunningTasksResponse>> GetIncompleteTasksAsync(string clientTimeZone = "Asia/Kolkata")
    {
        // Call the GetIncompleteTasks method from the DataController
        var result = await _dataController.GetUsersWithoutRunningTasks(clientTimeZone);

        if (result is OkObjectResult okResult)
        {
            // Convert the result to a JSON string
            var jsonString = JsonConvert.SerializeObject(okResult.Value);

            // Deserialize the JSON string into a list of UserWithoutRunningTasksResponse
            var data = JsonConvert.DeserializeObject<List<UserWithoutRunningTasksResponse>>(jsonString);

            if (data != null)
            {
                return data;
            }
        }

        // Log an error if the result is null or invalid
        _logger.LogError("Failed to retrieve incomplete tasks from API. Result value is null or invalid.");
        return new List<UserWithoutRunningTasksResponse>();
    }




    public async Task<List<Computer>> GetComputersAsync()
    {
        var result = await _dataController.GetComputers();
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<Computer>>(jsonString);

        return data ?? new List<Computer>();
    }

    public async Task<int> CreateTeamAsync(Team model)
    {
        var result = await _dataController.CreateTeam(model);
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var response = JsonConvert.DeserializeObject<dynamic>(jsonString); // Assuming the response contains a dynamic object with Message and TeamId

        return response?.TeamId ?? 0;
    }

    public async Task<TeamAssignmentViewModel> GetAssignmentDataAsync()
    {
        var result = await _teamAssignmentApiController.GetAssignmentData();
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<TeamAssignmentViewModel>(jsonString);

        return data;
    }

    public async Task<bool> AssignUserToTeamAsync(TeamAssignmentViewModel model)
    {
        var result = await _teamAssignmentApiController.AssignUserToTeam(model);
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<bool>(jsonString);

        return data;
    }

    public async Task<List<UserWithoutLoginResponse>> GetUsersWithoutLoginAsync(string clientTimeZone = "Asia/Kolkata")
    {
        // Call the GetUsersWithoutLogin method from the DataController
        var result = await _dataController.GetUsersWithoutLogin(clientTimeZone);

        if (result is OkObjectResult okResult)
        {
            // Convert the result to a JSON string
            var jsonString = JsonConvert.SerializeObject(okResult.Value);

            // Deserialize the JSON string into a list of UserWithoutLoginResponse
            var data = JsonConvert.DeserializeObject<List<UserWithoutLoginResponse>>(jsonString);

            if (data != null)
            {
                return data;
            }
        }

        // Log an error if the result is null or invalid
        _logger.LogError("Failed to retrieve users without login from API. Result value is null or invalid.");
        return new List<UserWithoutLoginResponse>();
    }

    public async Task<int> AddComputerAsync(Computer model)
    {
        var result = await _dataController.AddComputer(model);
        var value = (result as OkObjectResult)?.Value;

        if (value == null)
        {
            _logger.LogError("Failed to add computer. Result value is null or invalid.");
            return 0;
        }

        // Assuming the response has the newly added ComputerId
        var jsonString = JsonConvert.SerializeObject(value);
        var response = JsonConvert.DeserializeObject<dynamic>(jsonString); // Deserializing dynamic response
        return response?.ComputerId ?? 0; // Return the ComputerId or 0 if it fails
    }

    public async Task<bool> EditComputerAsync(int id, Computer model)
    {
        var result = await _dataController.EditComputer(id, model);

        if (result is OkObjectResult)
        {
            _logger.LogInformation("Computer successfully updated.");
            return true;
        }

        _logger.LogError("Failed to edit computer.");
        return false;
    }
    public async Task<List<Computer>> GetAllComputersAsync()
    {
        var result = await _dataController.GetAllComputers();
        var value = (result as OkObjectResult)?.Value;
        var jsonString = JsonConvert.SerializeObject(value);
        var data = JsonConvert.DeserializeObject<List<Computer>>(jsonString);

        return data ?? new List<Computer>();
    }
    public async Task<List<object>> GetAllUsersWithAssignedTasksAsync(DateTime? assignedDate)
    {
        // Call the TaskAssignmentController method
        var result = await _taskAssignmentController.GetAllUsersWithAssignedTasks(assignedDate);

        // Check if the result is a valid OkObjectResult
        var value = (result as OkObjectResult)?.Value;

        // Convert the result to JSON string for deserialization
        var jsonString = JsonConvert.SerializeObject(value);

        // Deserialize the JSON string to a List of objects
        var data = JsonConvert.DeserializeObject<List<object>>(jsonString);

        // Return the data or an empty list if no data is found
        return data ?? new List<object>();
    }


    public class ApiResponse
    {
        public List<TaskNames> Tasks { get; set; }
        public int TotalTasks { get; set; }
    }
   
}

