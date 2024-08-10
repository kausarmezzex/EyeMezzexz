using System.Collections.Generic;
using System.Threading.Tasks;
using EyeMezzexz.Controllers; // Assuming this is the namespace of your API controllers
using EyeMezzexz.Models;
using MezzexEye.Models;
using MezzexEye.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class ApiService : IApiService
{
    private readonly DataController _dataController;
    private readonly AccountApiController _accountApiController;
    private readonly TeamAssignmentApiController _teamAssignmentApiController;
    private readonly ILogger<ApiService> _logger;

    public ApiService(DataController dataController, AccountApiController accountApiController, TeamAssignmentApiController teamAssignmentApiController, ILogger<ApiService> logger)
    {
        _dataController = dataController;
        _accountApiController = accountApiController;
        _teamAssignmentApiController = teamAssignmentApiController;
        _logger = logger;
    }
    public async Task<List<ScreenCaptureDataViewModel>> GetScreenCaptureDataAsync(string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetScreenCaptureData(clientTimeZone) as OkObjectResult;
        return result?.Value as List<ScreenCaptureDataViewModel> ?? new List<ScreenCaptureDataViewModel>();
    }

    public async Task SaveScreenCaptureDataAsync(UploadRequest model)
    {
        await _dataController.SaveScreenCaptureData(model);
    }

    public async Task<List<TaskTimerResponse>> GetTaskTimersAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetTaskTimers(userId, clientTimeZone) as OkObjectResult;
        return result?.Value as List<TaskTimerResponse> ?? new List<TaskTimerResponse>();
    }

    public async Task SaveTaskTimerAsync(TaskTimerUploadRequest model)
    {
        await _dataController.SaveTaskTimer(model);
    }

    public async Task<List<TaskTimerResponse>> GetUserCompletedTasksAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetUserCompletedTasks(userId, clientTimeZone) as OkObjectResult;
        return result?.Value as List<TaskTimerResponse> ?? new List<TaskTimerResponse>();
    }

    public async Task<(List<TaskNames> Tasks, int TotalTasks)> GetTasksAsync(int? countryId = null, int page = 1, int pageSize = 10, string search = "")
    {
        var result = await _dataController.GetTasks(countryId, page, pageSize, search) as OkObjectResult;
        var response = result?.Value as ApiResponse;
        return (response?.Tasks ?? new List<TaskNames>(), response?.TotalTasks ?? 0);
    }

    public async Task<List<TaskNames>> GetTasksListAsync()
    {
        var result = await _dataController.GetTasksList() as OkObjectResult;
        return result?.Value as List<TaskNames> ?? new List<TaskNames>();
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
        var result = await _dataController.GetStaff(clientTimeZone) as OkObjectResult;
        return result?.Value as List<object> ?? new List<object>();
    }

    public async Task<StaffInTimeResponse> GetStaffInTimeAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var result = await _dataController.GetStaffInTime(userId, clientTimeZone) as OkObjectResult;
        return result?.Value as StaffInTimeResponse;
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
        var result = await _dataController.GetTaskTimeId(taskId, "Asia/Kolkata") as OkObjectResult;
        return result?.Value as int?;
    }

    public async Task UpdateTaskAsync(TaskNames model)
    {
        await _dataController.UpdateTask(model);
    }

    public async Task<List<string>> GetAllUsernamesAsync()
    {
        var result = await _accountApiController.GetAllUsernames() as OkObjectResult;
        return result?.Value as List<string> ?? new List<string>();
    }

    public async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
        var result = await _accountApiController.GetUserByEmailAsync(email) as ApplicationUser;
        return result;
    }

    public async Task<List<Country>> GetCountriesAsync()
    {
        var result = await _dataController.GetCountries() as OkObjectResult;
        return result?.Value as List<Country> ?? new List<Country>();
    }

    public async Task<List<Computer>> GetComputersAsync()
    {
        var result = await _dataController.GetComputers() as OkObjectResult;
        return result?.Value as List<Computer> ?? new List<Computer>();
    }
    public async Task<int> CreateTeamAsync(Team model)
{
    var result = await _dataController.CreateTeam(model) as OkObjectResult;
    var response = result?.Value as dynamic; // Assuming the response contains dynamic object with Message and TeamId
    return response?.TeamId ?? 0;
}
    public async Task<TeamAssignmentViewModel> GetAssignmentDataAsync()
    {
        var result = await _teamAssignmentApiController.GetAssignmentData() as OkObjectResult;
        return result?.Value as TeamAssignmentViewModel;
    }

    // Method to assign user to a team
    public async Task<bool> AssignUserToTeamAsync(TeamAssignmentViewModel model)
    {
        var result = await _teamAssignmentApiController.AssignUserToTeam(model) as OkObjectResult;
        return result != null;
    }

    public class ApiResponse
    {
        public List<TaskNames> Tasks { get; set; }
        public int TotalTasks { get; set; }
    }
}
