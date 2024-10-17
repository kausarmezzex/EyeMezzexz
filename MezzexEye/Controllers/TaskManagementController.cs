﻿using MezzexEye.Services;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace MezzexEye.Controllers
{
    public class TaskManagementController : Controller
    {
        private readonly ApiService _apiService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TaskManagementController> _logger;

        public TaskManagementController(ApiService apiService, UserManager<ApplicationUser> userManager, ILogger<TaskManagementController> logger)
        {
            _apiService = apiService;
            _userManager = userManager;
            _logger = logger;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTasks(UserTaskAssignmentViewModel model)
        {
            // Log the incoming model for debugging
            _logger.LogInformation("UserTaskAssignments: {@UserTaskAssignments}", model.UserTaskAssignments);

            // Only process assignments where TaskAssignments has data
            var validUserTaskAssignments = model.UserTaskAssignments
                .Where(userTaskAssignment => userTaskAssignment.TaskAssignments != null && userTaskAssignment.TaskAssignments.Count > 0)
                .ToList();

            foreach (var userTaskAssignment in validUserTaskAssignments)
            {
                // Log user and task count
                _logger.LogInformation("Processing User: {UserId}, Task Count: {TaskCount}", userTaskAssignment.UserId, userTaskAssignment.TaskAssignments.Count);

                var taskAssignments = new List<TaskAssignmentRequest>();
                string userCountry = null;

                foreach (var taskAssignment in userTaskAssignment.TaskAssignments)
                {
                    // Set the country once per user
                    if (userCountry == null)
                    {
                        userCountry = taskAssignment.Country;
                    }

                    // Create a new TaskAssignmentRequest
                    var taskAssignmentRequest = new TaskAssignmentRequest
                    {
                        TaskId = taskAssignment.TaskId,
                        AssignedDuration = TimeSpan.FromHours(taskAssignment.AssignedDurationHours.Value),
                        TargetQuantity = taskAssignment.TargetQuantity,
                        Country = taskAssignment.Country,
                        ComputerIds = taskAssignment.ComputerIds // Include ComputerIds directly
                    };

                    taskAssignments.Add(taskAssignmentRequest);

                    // Log each task assignment
                    _logger.LogInformation("Assigned Task: {TaskId} to User: {UserId} for {Country} with {ComputerCount} Computers",
                        taskAssignment.TaskId, userTaskAssignment.UserId, taskAssignment.Country, taskAssignment.ComputerIds?.Count ?? 0);
                }

                // Assign tasks to the user
                try
                {
                    await _apiService.AssignTasksToUserAsync(
                        userTaskAssignment.UserId,
                        taskAssignments,
                        userCountry
                    );
                }
                catch (SqlException ex)
                {
                    // Log SQL exception
                    _logger.LogError("SQL Exception during task assignment for User: {UserId} - {Message}", userTaskAssignment.UserId, ex.Message);
                    throw;
                }
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> ViewAllUserTaskAssignments(DateTime? assignedDate)
        {
            try
            {
                // Call the API service and pass the assignedDate filter
                var allUserTaskAssignments = await _apiService.GetAssignedTasksAsync(assignedDate);

                // Log the result
                _logger.LogInformation("Retrieved {TaskCount} task assignments for all users on {Date}.", allUserTaskAssignments.Count, assignedDate?.ToString("yyyy-MM-dd"));

                // Pass the assignedDate to the view (for persistence in the search field)
                ViewData["AssignedDate"] = assignedDate?.ToString("yyyy-MM-dd");

                // Return the data to the view
                return View(allUserTaskAssignments);
            }
            catch (Exception ex)
            {
                // Log any errors
                _logger.LogError("Error retrieving user task assignments: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving task assignments.");
            }
        }

    }
}