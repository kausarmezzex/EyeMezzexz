using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MezzexEye.Services;
using EyeMezzexz.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MezzexEye.Controllers
{
    public class ManageShiftController : Controller
    {
        private readonly IShiftApiService _shiftService;
        private readonly ILogger<ManageShiftController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageShiftController(IShiftApiService shiftService, ILogger<ManageShiftController> logger, UserManager<ApplicationUser> userManager)
        {
            _shiftService = shiftService;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: /ManageShift
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var shifts = await _shiftService.GetShiftsAsync();
            return View(shifts);
        }

        // GET: /ManageShift/Create (Partial View for Modal)
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateShiftPartial");
        }

        // POST: /ManageShift/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shift shift)
        {
            shift.CreatedBy = User.Identity.Name;
            if (ModelState.IsValid)
            {
                var success = await _shiftService.AddShiftAsync(shift);
                if (success)
                {
                    _logger.LogInformation("Shift successfully created.");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error creating shift.");
            }
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
            return PartialView("_EditShiftPartial", shift);
        }

        // POST: /ManageShift/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shift shift)
        {
            shift.ModifiedBy = User.Identity.Name;
            if (ModelState.IsValid)
            {
                var success = await _shiftService.UpdateShiftAsync(id, shift);
                if (success)
                {
                    _logger.LogInformation("Shift successfully updated.");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error updating shift.");
            }
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
