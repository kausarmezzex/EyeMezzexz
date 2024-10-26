using EyeMezzexz.Models;
using MezzexEye.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace MezzexEye.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;  // Use ApplicationRole
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Active = model.Active,
                    CountryName = model.CountryName,
                    PhoneNumber = model.Phone
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Registered");

                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public async Task<bool> UserExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AllUsers(string searchValue, string sortBy)
        {
            var users = _userManager.Users.AsQueryable();

            // Universal Search logic
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower(); // Optional: make search case-insensitive

                users = users.Where(u =>
                    u.Email.ToLower().Contains(searchValue) ||           // Search by email
                    u.FirstName.ToLower().Contains(searchValue) ||       // Search by first name
                    u.LastName.ToLower().Contains(searchValue) ||        // Search by last name
                    u.PhoneNumber.Contains(searchValue) ||               // Search by phone number
                    u.CountryName.ToLower().Contains(searchValue)        // Search by country
                );
            }

            // Sorting logic (optional)
            users = sortBy switch
            {
                "username" => users.OrderBy(u => u.UserName),
                "email" => users.OrderBy(u => u.Email),
                "firstname" => users.OrderBy(u => u.FirstName),
                "lastname" => users.OrderBy(u => u.LastName),
                _ => users.OrderBy(u => u.UserName) // Default sort by username
            };

            var userList = await users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in userList)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    Active = user.Active,
                    CountryName = user.CountryName,
                    Phone = user.PhoneNumber,
                    Roles = userRoles.ToList()
                });
            }

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserTable", userViewModels);
            }

            // Return full view for non-AJAX requests
            return View(userViewModels);
        }


        [HttpGet]
        public async Task<IActionResult> EditUser(int id)  // id as int
        {
            var user = await _userManager.FindByIdAsync(id.ToString()); // Use ToString() since Identity methods expect string
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = id,  // Directly assign the int Id
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Active = user.Active,
                CountryName = user.CountryName,
                Phone = user.PhoneNumber,
                Roles = roles.ToList()
            };

            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());  // Use ToString() since Identity methods expect string
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Gender = model.Gender;
            user.CountryName = model.CountryName;
            user.PhoneNumber = model.Phone;
            user.Active = model.Active;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(model.Roles).ToList();
                var rolesToAdd = model.Roles.Except(currentRoles).ToList();

                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                await _userManager.AddToRolesAsync(user, rolesToAdd);

                return RedirectToAction("AllUsers");
            }

            AddErrors(result);
            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            // Change the user's password
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("AllUsers");
            }

            // Add errors to the ModelState to display in the view
            AddErrors(result);
            return View(model);
        }

        public async Task<int> GetTotalUsers()
        {
            var users = _userManager.Users.ToList();
            return users.Count;
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context

            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    Active = user.Active,
                    CountryName = user.CountryName,
                    Phone = user.PhoneNumber,
                    Roles = userRoles.ToList()
                });
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Users");

            // Add Headers
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "First Name";
            worksheet.Cells[1, 4].Value = "Last Name";
            worksheet.Cells[1, 5].Value = "Gender";
            worksheet.Cells[1, 6].Value = "Active";
            worksheet.Cells[1, 7].Value = "Country";
            worksheet.Cells[1, 8].Value = "Phone";
            worksheet.Cells[1, 9].Value = "Roles";

            // Add Data
            for (int i = 0; i < userViewModels.Count; i++)
            {
                var user = userViewModels[i];
                worksheet.Cells[i + 2, 2].Value = user.Email;
                worksheet.Cells[i + 2, 3].Value = user.FirstName;
                worksheet.Cells[i + 2, 4].Value = user.LastName;
                worksheet.Cells[i + 2, 5].Value = user.Gender;
                worksheet.Cells[i + 2, 6].Value = user.Active;
                worksheet.Cells[i + 2, 7].Value = user.CountryName;
                worksheet.Cells[i + 2, 8].Value = user.Phone;
                worksheet.Cells[i + 2, 9].Value = string.Join(", ", user.Roles);
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"Users-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
    }
}
