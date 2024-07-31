using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EyeMezzexz.Controllers;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

    public async Task<List<TaskNames>> GetTasksAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<TaskNames>>("api/Data/getTasks");
        return response ?? new List<TaskNames>();
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

    public async Task<object> GetStaffInTimeAsync(int userId, string clientTimeZone = "Asia/Kolkata")
    {
        var response = await _httpClient.GetFromJsonAsync<object>($"api/Data/getStaffInTime?userId={userId}&clientTimeZone={clientTimeZone}");
        return response ?? new object();
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

}

