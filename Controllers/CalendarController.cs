using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;
using System.Globalization;

namespace LuxeSmilesDental.Controllers
{
    [AuthorizeSession("Dentist", "Dental Assistant", "Secretary")]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var today = DateTime.Today;
            int displayYear = year ?? today.Year;
            int displayMonth = month ?? today.Month;

            ViewBag.DisplayYear = displayYear;
            ViewBag.DisplayMonth = displayMonth;
            ViewBag.MonthName = new DateTime(displayYear, displayMonth, 1).ToString("MMMM");

            var start = new DateTime(displayYear, displayMonth, 1);
            var end = start.AddMonths(1).AddDays(-1);

            ViewBag.Appointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(Appointment appointment)
        {
            ModelState.Remove("AppointmentId");

            if (ModelState.IsValid)
            {
                if (appointment.AppointmentDate < DateTime.Now.AddDays(-1))
                {
                    return Json(new { success = false, message = "Cannot create appointments in the past" });
                }

                appointment.CreatedAt = DateTime.UtcNow;
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid appointment data" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointment(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch
                {
                    return Json(new { success = false, message = "Update failed" });
                }
            }

            return Json(new { success = false, message = "Invalid data" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Appointment not found" });
        }

        [HttpGet]
        public async Task<IActionResult> GetDayAppointments(string date)
        {
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return Json(new { success = false, message = "Invalid date" });
            }

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == parsedDate.Date)
                .Select(a => new
                {
                    appointmentId = a.AppointmentId,
                    patientName = a.PatientName,
                    appointmentDate = a.AppointmentDate,
                    notes = a.Notes
                })
                .ToListAsync();

            return Json(new { success = true, appointments });
        }
    }
}