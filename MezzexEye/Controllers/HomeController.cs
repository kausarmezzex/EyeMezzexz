using EyeMezzexz.Controllers;
using MezzexEye.Models;
using MezzexEye.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MezzexEye.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataForViewController _dataForViewController;
        private readonly AccountController _accountController;
        private readonly ApiService _apiService;
        public HomeController(ILogger<HomeController> logger, DataForViewController dataForViewController, AccountController accountController, ApiService apiService)
        {
            _logger = logger;
            _dataForViewController = dataForViewController;
            _accountController = accountController;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var totalRunningTasks = await _dataForViewController.GetTotalRunningTasks();
            var totalUsers = await _accountController.GetTotalUsers();
            var incompleteTasks = await _apiService.GetIncompleteTasksAsync("Asia/Kolkata");

            // Count the total number of incomplete tasks
            var totalIncompleteTasks = incompleteTasks.Count;

            ViewBag.TotalRunningTasks = totalRunningTasks;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.UsersNotStartTask = totalIncompleteTasks;
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
