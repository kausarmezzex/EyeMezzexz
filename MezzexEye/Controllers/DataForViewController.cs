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

                if (!string.IsNullOrEmpty(username))
                {
                    data = data.Where(d => d.Username == username).ToList();
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
    }
}
