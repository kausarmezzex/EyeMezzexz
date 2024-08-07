using Microsoft.AspNetCore.Mvc;
using MezzexEye.Models;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EyeMezzexz.Data;

namespace MezzexEye.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApiService _apiService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TaskController(ApiService apiService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _apiService = apiService;
            _userManager = userManager;
            _context = context;
        }

        // GET: Task/Create
        public async Task<IActionResult> Create()
        {
            var (tasks, _) = await _apiService.GetTasksAsync();
            ViewBag.Tasks = BuildTaskSelectList(tasks);

            var countries = await _apiService.GetCountriesAsync();
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

            if (ModelState.IsValid)
            {
                await _apiService.CreateTaskAsync(model);
                return RedirectToAction(nameof(Index)); // Assuming you have an Index action to list tasks
            }

            var (tasks, _) = await _apiService.GetTasksAsync();
            ViewBag.Tasks = BuildTaskSelectList(tasks);

            var countries = await _apiService.GetCountriesAsync();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

            return View(model);
        }


        public async Task<IActionResult> Index(int? countryId = null, int page = 1, int pageSize = 10, string search = "")
        {
            var (tasks, totalTasks) = await _apiService.GetTasksAsync(countryId, page, pageSize, search);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { tasks, totalTasks });
            }

            var countries = await _apiService.GetCountriesAsync();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");
            ViewBag.TotalTasks = totalTasks;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            return View(tasks);
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var (tasks, _) = await _apiService.GetTasksAsync();
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Tasks = BuildTaskSelectList(tasks);

            var countries = await _apiService.GetCountriesAsync();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

            return View(task);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskNames model)
        {
            if (ModelState.IsValid)
            {
                await _apiService.UpdateTaskAsync(model);
                return RedirectToAction(nameof(Index));
            }

            var (tasks, _) = await _apiService.GetTasksAsync();
            ViewBag.Tasks = BuildTaskSelectList(tasks);

            var countries = await _apiService.GetCountriesAsync();
            ViewBag.Countries = new SelectList(countries, "Id", "Name");

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
