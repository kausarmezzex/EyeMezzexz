using MezzexEye.Services;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MezzexEye.Controllers
{
    public class TaskManagementController : Controller
    {
        private readonly ApiService _apiService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskManagementController(ApiService apiService, UserManager<ApplicationUser> userManager)
        {
            _apiService = apiService;
            _userManager = userManager;
        }

        // GET: Show all users, tasks, and computers for assignment
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Fetch all users without searching or sorting
            var users = await _userManager.Users.ToListAsync();

            // Fetch tasks from the API
            var tasks = await _apiService.GetTasksListAsync();

            // Fetch computers from the API
            var computers = await _apiService.GetAllComputersAsync();

            // Create the view model with the user, task, and computer information
            var model = new UserTaskAssignmentViewModel
            {
                Users = users,  // Use the ApplicationUser objects directly
                AvailableTasks = tasks,
                Computers = computers // Add computers list
            };

            return View(model); // Pass the model to the view
        }
        [HttpPost]
        public async Task<IActionResult> AssignTasks(UserTaskAssignmentViewModel model)
        {
            if (model == null || model.UserTaskAssignments == null || model.UserTaskAssignments.Count == 0)
            {
                ModelState.AddModelError("", "No task assignments provided.");
                return RedirectToAction("Index");
            }

            foreach (var userTaskAssignment in model.UserTaskAssignments)
            {
                if (userTaskAssignment.TaskAssignments == null || userTaskAssignment.TaskAssignments.Count == 0)
                {
                    continue;
                }

                var userId = userTaskAssignment.UserId;
                var taskAssignments = userTaskAssignment.TaskAssignments;
                var selectedComputerIds = userTaskAssignment.SelectedComputerIds ?? new List<int>();
                var country = model.SelectedCountry;

                // Call the service to save the task assignments
                await _apiService.AssignTasksToUserAsync(userId, taskAssignments, selectedComputerIds, country);
            }

            return RedirectToAction("Index");
        }
    }
}
