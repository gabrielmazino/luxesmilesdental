using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;

namespace LuxeSmilesDental.Controllers
{
    // ONLY Dentist can manage users
    [AuthorizeSession("Dentist")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.IsActive)
                .ThenBy(u => u.EmployeeName)
                .ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(User user)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("Password"); // Will be set separately

            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }

                var allowedRoles = new[] { "Dentist", "Dental Assistant", "Secretary" };
                if (!allowedRoles.Contains(user.Role))
                {
                    return Json(new { success = false, message = "Invalid role" });
                }

                // Hash default password
                user.Password = BCrypt.Net.BCrypt.HashPassword("Password123!");
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid data" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(User user)
        {
            if (user.UserId <= 0)
            {
                return Json(new { success = false, message = "Invalid user" });
            }

            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Check if username is taken by another user
            if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.UserId != user.UserId))
            {
                return Json(new { success = false, message = "Username already exists" });
            }

            var allowedRoles = new[] { "Dentist", "Dental Assistant", "Secretary" };
            if (!allowedRoles.Contains(user.Role))
            {
                return Json(new { success = false, message = "Invalid role" });
            }

            // Prevent last Dentist from being deactivated
            if (existingUser.Role == "Dentist" && !user.IsActive)
            {
                var dentistCount = await _context.Users.CountAsync(u => u.Role == "Dentist" && u.IsActive);
                if (dentistCount <= 1)
                {
                    return Json(new { success = false, message = "Cannot deactivate the last Dentist" });
                }
            }

            existingUser.Username = user.Username;
            existingUser.EmployeeName = user.EmployeeName;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            try
            {
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Update failed" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Prevent deactivating the last active Dentist
            if (user.Role == "Dentist" && user.IsActive)
            {
                var activeDentistCount = await _context.Users.CountAsync(u => u.Role == "Dentist" && u.IsActive);
                if (activeDentistCount <= 1)
                {
                    return Json(new { success = false, message = "Cannot deactivate the last Dentist" });
                }
            }

            user.IsActive = !user.IsActive;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = user.IsActive });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword("Password123!");
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Password reset to: Password123!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false });
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                return Json(new
                {
                    success = true,
                    user = new
                    {
                        user.UserId,
                        user.Username,
                        user.EmployeeName,
                        user.Role,
                        user.IsActive
                    }
                });
            }

            return Json(new { success = false });
        }
    }
}