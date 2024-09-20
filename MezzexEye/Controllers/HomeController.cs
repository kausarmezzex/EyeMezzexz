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
            // Get the total running tasks
            var totalRunningTasks = await _dataForViewController.GetTotalRunningTasks();

            // Get the total users
            var totalUsers = await _accountController.GetTotalUsers();

            // Get the incomplete tasks
            var incompleteTasks = await _apiService.GetIncompleteTasksAsync("Asia/Kolkata");

            // Get the total number of incomplete tasks
            var totalIncompleteTasks = incompleteTasks.Count;

            // Get the users who haven't logged in today (the new method)
            var usersWithoutLogin = await _apiService.GetUsersWithoutLoginAsync("Asia/Kolkata");

            // Count the total number of users without login today
            var totalUsersWithoutLogin = usersWithoutLogin.Count;

            // Set the data to ViewBag to pass it to the view
            ViewBag.TotalRunningTasks = totalRunningTasks;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.UsersNotStartTask = totalIncompleteTasks;
            ViewBag.UsersWithoutLogin = totalUsersWithoutLogin;

            // Render the view
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
