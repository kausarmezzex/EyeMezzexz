using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EyeMezzexz.Controllers;
using EyeMezzexz.Models;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ScreenCaptureDataViewModel>> GetScreenCaptureDataAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ScreenCaptureDataViewModel>>("api/Data/getScreenCaptureData");
        return response ?? new List<ScreenCaptureDataViewModel>();
    }

    public async Task SaveScreenCaptureDataAsync(UploadRequest model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/saveScreenCaptureData", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TaskTimerResponse>> GetTaskTimersAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<TaskTimerResponse>>("api/Data/getTaskTimers");
        return response ?? new List<TaskTimerResponse>();
    }

    public async Task SaveTaskTimerAsync(TaskTimer model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/saveTaskTimer", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TaskTimerResponse>> GetUserCompletedTasksAsync(int userId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<TaskTimerResponse>>($"api/Data/getUserCompletedTasks?userId={userId}");
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

    public async Task<List<object>> GetStaffAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<object>>("api/Data/getStaff");
        return response ?? new List<object>();
    }

    public async Task<object> GetStaffInTimeAsync(int userId)
    {
        var response = await _httpClient.GetFromJsonAsync<object>($"api/Data/getStaffInTime?userId={userId}");
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

}
