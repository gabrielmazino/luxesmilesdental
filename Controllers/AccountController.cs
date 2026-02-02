// Controllers/AccountController.cs
// SECURITY FIXES + Logout fix for GET requests

using Microsoft.AspNetCore.Mvc;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace LuxeSmilesDental.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly ConcurrentDictionary<string, (int attempts, DateTime lockoutTime)> _loginAttempts = new();
        private const int MaxLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (_loginAttempts.TryGetValue(ipAddress, out var lockoutInfo))
            {
                if (lockoutInfo.lockoutTime > DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Too many failed login attempts. Please try again later.");
                    return View(model);
                }

                if (lockoutInfo.attempts >= MaxLoginAttempts && lockoutInfo.lockoutTime <= DateTime.UtcNow)
                {
                    _loginAttempts.TryRemove(ipAddress, out _);
                }
            }

            var user = await _context.Users
                .Where(u => u.Username == model.Username && u.IsActive)
                .Select(u => new { u.UserId, u.Username, u.Password, u.Role, u.EmployeeName })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                bool isValidPassword = false;
                try
                {
                    isValidPassword = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
                }
                catch
                {
                    isValidPassword = false;
                }

                if (isValidPassword)
                {
                    _loginAttempts.TryRemove(ipAddress, out _);

                    HttpContext.Session.Clear();

                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);
                    HttpContext.Session.SetString("EmployeeName", user.EmployeeName);

                    return RedirectToAction("Index", "Dashboard");
                }
            }

            _loginAttempts.AddOrUpdate(
                ipAddress,
                (ip) => (1, DateTime.UtcNow),
                (ip, existing) =>
                {
                    var newAttempts = existing.attempts + 1;
                    var newLockout = newAttempts >= MaxLoginAttempts
                        ? DateTime.UtcNow.Add(LockoutDuration)
                        : existing.lockoutTime;
                    return (newAttempts, newLockout);
                });

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        // FIXED: Accept both GET and POST for logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}