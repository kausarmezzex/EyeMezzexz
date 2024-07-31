using Microsoft.AspNetCore.Mvc;
using MezzexEye.Models;
using System.Threading.Tasks;
using EyeMezzexz.Models;

namespace MezzexEye.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApiService _apiService;

        public TaskController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Task/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskModelRequest model)
        {
            if (ModelState.IsValid)
            {
                await _apiService.CreateTaskAsync(model);
                return RedirectToAction(nameof(Index)); // Assuming you have an Index action to list tasks
            }
            return View(model);
        }

        // GET: Task/Index
        public async Task<IActionResult> Index()
        {
            var tasks = await _apiService.GetTasksAsync();
            return View(tasks);
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tasks = await _apiService.GetTasksAsync();
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }
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
            return View(model);
        }
    }
}
