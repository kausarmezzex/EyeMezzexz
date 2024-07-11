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

    public async Task<List<string>> GetUsernamesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<string>>("api/Auth/usernames");
        return response ?? new List<string>();
    }

    public async Task SaveScreenCaptureDataAsync(UploadRequest model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Data/saveScreenCaptureData", model);
        response.EnsureSuccessStatusCode();
    }

    // Add other methods for TaskTimers, Staff, etc. as needed
}
