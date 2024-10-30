using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MezzexEye.Services;
using EyeMezzexz.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using EyeMezzexz.Data;

namespace MezzexEye.Controllers
{
    public class ManageShiftController : Controller
    {
        private readonly IShiftApiService _shiftService;
        private readonly ILogger<ManageShiftController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Include context for accessing country data

        public ManageShiftController(
            IShiftApiService shiftService,
            ILogger<ManageShiftController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _shiftService = shiftService;
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        // Action to get shifts by country for AJAX filtering
        [HttpGet]
        public async Task<IActionResult> GetShiftsByCountry(int? countryId)
        {
            var shifts = countryId.HasValue
                ? await _shiftService.GetShiftsByCountryAsync(countryId.Value)
                : await _shiftService.GetShiftsAsync();

            return Json(shifts);
        }
        // GET: /ManageShift
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Countries"] = _context.Countries.ToList(); // Populate countries for dropdown
            var shifts = await _shiftService.GetShiftsAsync();
            return View(shifts);
        }

        // GET: /ManageShift/Create (Partial View for Modal)
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Countries"] = _context.Countries.ToList(); // Populate countries for dropdown
            return PartialView("_CreateShiftPartial");
        }

        // POST: /ManageShift/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shift shift, int countryId)
        {
            shift.CreatedBy = User.Identity.Name;
            if (!ModelState.IsValid)
            {
                var success = await _shiftService.AddShiftAsync(shift, countryId); // Pass countryId to associate shift with country
                if (success)
                {
                    _logger.LogInformation("Shift successfully created.");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error creating shift.");
            }
            ViewData["Countries"] = _context.Countries.ToList(); // Repopulate countries if model state is invalid
            return PartialView("_CreateShiftPartial", shift);
        }

        // GET: /ManageShift/Edit/{id} (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _shiftService.GetShiftByIdAsync(id);
            if (shift == null)
            {
                return NotFound();
            }
            ViewData["Countries"] = _context.Countries.ToList(); // Populate countries for dropdown
            return PartialView("_EditShiftPartial", shift);
        }

        // POST: /ManageShift/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shift shift, int countryId)
        {
            shift.ModifiedBy = User.Identity.Name;
            if (!ModelState.IsValid)
            {
                shift.CountryId = countryId; // Set the country ID for the shift
                var success = await _shiftService.UpdateShiftAsync(id, shift);
                if (success)
                {
                    _logger.LogInformation("Shift successfully updated.");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error updating shift.");
            }
            ViewData["Countries"] = _context.Countries.ToList(); // Repopulate countries if model state is invalid
            return PartialView("_EditShiftPartial", shift);
        }

        // GET: /ManageShift/Delete/{id} (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _shiftService.GetShiftByIdAsync(id);
            if (shift == null)
            {
                return NotFound();
            }
            return PartialView("_DeleteShiftPartial", shift);
        }

        // POST: /ManageShift/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)    
        {
            var success = await _shiftService.DeleteShiftAsync(id);
            if (success)
            {
                _logger.LogInformation("Shift successfully deleted.");
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
