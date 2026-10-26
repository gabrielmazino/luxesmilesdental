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
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(ApplicationDbContext context, ILogger<UserManagementController> logger)
        {
            _context = context;
            _logger = logger;
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
        public async Task<IActionResult> AddUser(User user, string password)
        {
            try
            {
                _logger.LogInformation("AddUser called for username: {Username}", user?.Username);

                // Remove fields that are set server-side
                ModelState.Remove("UserId");
                ModelState.Remove("Password");
                ModelState.Remove("CreatedAt");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var errorMessage = errors.Any()
                        ? "Validation failed: " + string.Join(", ", errors)
                        : "Invalid data submitted";

                    _logger.LogWarning("ModelState invalid for AddUser: {Errors}", errorMessage);

                    return Json(new { success = false, message = errorMessage });
                }

                // Validate password provided
                if (string.IsNullOrWhiteSpace(password))
                {
                    return Json(new { success = false, message = "Password is required" });
                }

                // Check for duplicate username
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }

                // Validate role
                var allowedRoles = new[] { "Dentist", "Dental Assistant", "Secretary" };
                if (!allowedRoles.Contains(user.Role))
                {
                    return Json(new { success = false, message = "Invalid role. Allowed roles: Dentist, Dental Assistant, Secretary" });
                }

                // Set server-side fields
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;

                // Handle optional ContactNumber
                if (string.IsNullOrWhiteSpace(user.ContactNumber))
                {
                    user.ContactNumber = null;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User created successfully: {UserId}", user.UserId);

                return Json(new { success = true, message = "User created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Json(new { success = false, message = "Error creating user: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(User user, string? password)
        {
            try
            {
                _logger.LogInformation("UpdateUser called for UserId: {UserId}", user?.UserId);

                if (user.UserId <= 0)
                {
                    return Json(new { success = false, message = "Invalid user ID" });
                }

                var existingUser = await _context.Users.FindAsync(user.UserId);
                if (existingUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Remove fields not sent from client or set server-side
                ModelState.Remove("Password");
                ModelState.Remove("CreatedAt");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var errorMessage = errors.Any()
                        ? "Validation failed: " + string.Join(", ", errors)
                        : "Invalid data submitted";

                    _logger.LogWarning("ModelState invalid for UpdateUser: {Errors}", errorMessage);

                    return Json(new { success = false, message = errorMessage });
                }

                // Check if username is taken by another user
                if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.UserId != user.UserId))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }

                // Validate role
                var allowedRoles = new[] { "Dentist", "Dental Assistant", "Secretary" };
                if (!allowedRoles.Contains(user.Role))
                {
                    return Json(new { success = false, message = "Invalid role. Allowed roles: Dentist, Dental Assistant, Secretary" });
                }

                // Update only allowed fields
                existingUser.Username = user.Username;
                existingUser.EmployeeName = user.EmployeeName;
                existingUser.Role = user.Role;

                // Update password only if a new one was provided
                if (!string.IsNullOrWhiteSpace(password))
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
                }

                // Handle optional ContactNumber
                existingUser.ContactNumber = string.IsNullOrWhiteSpace(user.ContactNumber)
                    ? null
                    : user.ContactNumber;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User updated successfully: {UserId}", user.UserId);

                return Json(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", user?.UserId);
                return Json(new { success = false, message = "Update failed: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {Id}", id);
                return Json(new { success = false, message = "Failed to toggle status: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "Invalid ID" });
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
                            user.ContactNumber,
                            user.IsActive
                        }
                    });
                }

                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {Id}", id);
                return Json(new { success = false, message = "Failed to retrieve user: " + ex.Message });
            }
        }
    }
}
