using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using MezzexEye.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ScreenCaptureDataViewModel>> GetScreenCaptureDataAsync(string clientTimeZone = "Asia/Kolkata")
    {
        var response = await _httpClient.GetFromJsonAsync<List<ScreenCaptureDataViewModel>>($"api/Data/getScreenCaptureData?clientTimeZone={clientTimeZone}");
        return response ?? new List<ScreenCaptureDataViewModel>();
    }

    public async Task SaveScreenCaptureDataAsync(UploadRequest model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/saveScreenCaptureData", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TaskTimerResponse>> GetTaskTimersAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var response = await _httpClient.GetFromJsonAsync<List<TaskTimerResponse>>($"api/Data/getTaskTimers?userId={userId}&clientTimeZone={clientTimeZone}");
        return response ?? new List<TaskTimerResponse>();
    }

    public async Task SaveTaskTimerAsync(TaskTimerUploadRequest model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/saveTaskTimer", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TaskTimerResponse>> GetUserCompletedTasksAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var response = await _httpClient.GetFromJsonAsync<List<TaskTimerResponse>>($"api/Data/getUserCompletedTasks?userId={userId}&clientTimeZone={clientTimeZone}");
        return response ?? new List<TaskTimerResponse>();
    }

    public async Task<(List<TaskNames> Tasks, int TotalTasks)> GetTasksAsync(int? countryId = null, int page = 1, int pageSize = 10, string search = "")
    {
        var url = $"api/Data/getTasks?countryId={countryId}&page={page}&pageSize={pageSize}&search={search}";
        var response = await _httpClient.GetFromJsonAsync<ApiResponse>(url);
        return (response.Tasks, response.TotalTasks);
    }

    public async Task<List<TaskNames>> GetTasksListAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<TaskNames>>("api/Data/getTasksList");
        return response ?? new List<TaskNames>();
    }

    public class ApiResponse
    {
        public List<TaskNames> Tasks { get; set; }
        public int TotalTasks { get; set; }
    }

    public async Task SaveStaffAsync(StaffInOut model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/saveStaff", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateStaffAsync(StaffInOut model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/updateStaff", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<object>> GetStaffAsync(string clientTimeZone = "Asia/Kolkata")
    {
        var response = await _httpClient.GetFromJsonAsync<List<object>>($"api/Data/getStaff?clientTimeZone={clientTimeZone}");
        return response ?? new List<object>();
    }

    public async Task<StaffInTimeResponse> GetStaffInTimeAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        try
        {
            var url = $"api/Data/getStaffInTime?userId={userId}&clientTimeZone={clientTimeZone}";
            _logger.LogInformation("Requesting URL: {url}", url);

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Staff in time not found for user ID: {userId}", userId);
                return null;
            }

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            // Read the response content
            var responseData = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response Data: {responseData}", responseData);

            // Deserialize the response data
            var staffInTime = JsonConvert.DeserializeObject<StaffInTimeResponse>(responseData);

            return staffInTime;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "An error occurred while fetching staff in time.");
            throw; // Ensure the exception is propagated
        }
    }

    public async Task UpdateTaskTimerAsync(UpdateTaskTimerRequest model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/updateTaskTimer", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateTaskAsync(TaskModelRequest model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/createTask", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task<int?> GetTaskTimeIdAsync(int taskId)
    {
        var response = await _httpClient.GetFromJsonAsync<int?>($"api/Data/getTaskTimeId?taskId={taskId}");
        return response;
    }

    public async Task UpdateTaskAsync(TaskNames model)
    {
        var response = await _httpClient.PutAsJsonAsync("api/Data/updateTask", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<string>> GetAllUsernamesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<string>>("api/AccountApi/getAllUsernames");
        return response ?? new List<string>();
    }

    public async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
        var response = await _httpClient.GetFromJsonAsync<ApplicationUser>($"api/AccountApi/getUserByEmail?email={email}");
        return response;
    }

    public async Task<List<Country>> GetCountriesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Country>>("api/Data/getCountries");
        return response ?? new List<Country>();
    }
    public async Task<List<Computer>> GetComputersAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Computer>>("api/Data/getComputers");
        return response ?? new List<Computer>();
    }
}