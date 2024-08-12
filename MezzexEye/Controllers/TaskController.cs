using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EyeMezzexz.Data;
using EyeMezzexz.Controllers;
using Newtonsoft.Json;

namespace MezzexEye.Controllers
{
    public class TaskController : Controller
    {
        private readonly DataController _dataController;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TaskController(DataController dataController, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _dataController = dataController;
            _userManager = userManager;
            _context = context;
        }

        // GET: Task/Create
        public async Task<IActionResult> Create()
        {
            // Call directly from DataController
            var tasksResponse = await _dataController.GetTasksList();
            var tasks = (tasksResponse as OkObjectResult)?.Value as List<TaskNames>;
            ViewBag.Tasks = BuildTaskSelectList(tasks);

            var countriesResponse = await _dataController.GetCountries();
            var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

            return View();
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskModelRequest model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.TaskCreatedBy = user.UserName; // Set the TaskCreatedBy property to the logged-in user's name

            if (!model.CountryId.HasValue)
            {
                ModelState.AddModelError("CountryId", "Please choose a country.");
            }

            if (ModelState.IsValid)
            {
                // Call directly from DataController
                await _dataController.CreateTask(model);
                return RedirectToAction(nameof(Index)); // Assuming you have an Index action to list tasks
            }

            var tasksResponse = await _dataController.GetTasksList();
            var tasks = (tasksResponse as OkObjectResult)?.Value as List<TaskNames>;
            ViewBag.Tasks = BuildTaskSelectList(tasks);

            var countriesResponse = await _dataController.GetCountries();
            var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

            return View(model);
        }

        public async Task<IActionResult> Index(int? countryId = null, int page = 1, int pageSize = 10, string search = "")
        {
            var tasksResponse = await _dataController.GetTasks(countryId, page, pageSize, search);
            var value = (tasksResponse as OkObjectResult)?.Value;

            if (value == null)
            {
                return StatusCode(500, "Failed to retrieve data from the API.");
            }

            // Convert the value to JSON string and deserialize
            var jsonString = JsonConvert.SerializeObject(value);
            var result = JsonConvert.DeserializeObject<TasksResponseDto>(jsonString);

            if (result == null)
            {
                return StatusCode(500, "Failed to deserialize the data from the API.");
            }

            var tasks = result.Tasks;
            var totalTasks = result.TotalTasks;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { tasks, totalTasks });
            }

            var countriesResponse = await _dataController.GetCountries();
            var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;
            ViewBag.Countries = new SelectList(countries, "Id", "Name");
            ViewBag.TotalTasks = totalTasks;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            return View(tasks);
        }


        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.TaskNames
                .Include(t => t.SubTasks)
                .Include(t => t.Country)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            var tasks = await _context.TaskNames
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == task.ParentTaskId
                })
                .ToListAsync();

            var countriesResponse = await _dataController.GetCountries();
            var countries = (countriesResponse as OkObjectResult)?.Value as List<Country>;

            // Set the selected country in the ViewBag
            ViewBag.Countries = new SelectList(countries, "Id", "Name", task.CountryId);
            ViewBag.Tasks = tasks;

            return View(task);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskNames model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.TaskModifiedBy = user.UserName; // Set the TaskModifiedBy property to the logged-in user's name

            if (ModelState.IsValid)
            {
                try
                {
                    // Call directly from DataController
                    await _dataController.UpdateTask(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Handle exceptions if necessary
                    ModelState.AddModelError(string.Empty, $"An error occurred while updating the task: {ex.Message}");
                }
            }

            // If we got this far, something failed; redisplay the form with the validation errors
            var tasks = await _context.TaskNames
                .Select(t => new
                {
                    t.Id,
                    t.Name
                })
                .ToListAsync();

            ViewBag.Tasks = new SelectList(tasks, "Id", "Name");

            return View(model);
        }

        private List<SelectListItem> BuildTaskSelectList(IEnumerable<TaskNames> tasks, int? parentId = null, string prefix = "")
        {
            var taskSelectList = new List<SelectListItem>();

            var filteredTasks = tasks.Where(t => t.ParentTaskId == parentId).ToList();
            foreach (var task in filteredTasks)
            {
                taskSelectList.Add(new SelectListItem
                {
                    Value = task.Id.ToString(),
                    Text = $"{prefix}{task.Name}"
                });

                var subTasks = BuildTaskSelectList(tasks, task.Id, $"{prefix}{task.Name} >> ");
                taskSelectList.AddRange(subTasks);
            }

            return taskSelectList;
        }
    }
}
public class TasksResponseDto
{
    public List<TaskNames> Tasks { get; set; }
    public int TotalTasks { get; set; }
}
