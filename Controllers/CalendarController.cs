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

            // Include Dentist and Patient navigation properties
            ViewBag.Appointments = await _context.Appointments
                .Include(a => a.Dentist)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
                .ToListAsync();

            // Get all dentists for dropdown
            ViewBag.Dentists = await _context.Users
                .Where(u => u.Role == "Dentist" && u.IsActive)
                .OrderBy(u => u.EmployeeName)
                .ToListAsync();

            // Get all patients for dropdown
            ViewBag.Patients = await _context.Patients
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();

            return View();
        }

        // Replace your existing DayDetails action with this one:

        [HttpGet]
        public async Task<IActionResult> DayDetails(string date)
        {
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return RedirectToAction("Index");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Dentist)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate.Date == parsedDate.Date)
                .ToListAsync();

            ViewBag.Date = parsedDate;

            // Pass dentists and patients so the edit modal can populate its dropdowns
            ViewBag.Dentists = await _context.Users
                .Where(u => u.Role == "Dentist" && u.IsActive)
                .OrderBy(u => u.EmployeeName)
                .ToListAsync();

            ViewBag.Patients = await _context.Patients
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();

            return View(appointments);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Dentist)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAppointment(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid appointment ID" });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Dentist)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            return Json(new
            {
                success = true,
                appointment = new
                {
                    appointmentId = appointment.AppointmentId,
                    patientId = appointment.PatientId,
                    patientName = appointment.Patient != null
                        ? $"{appointment.Patient.FirstName} {appointment.Patient.LastName}"
                        : appointment.PatientName,
                    patientPhone = appointment.Patient?.ContactNumber,
                    patientEmail = appointment.Patient?.Email,
                    dentistId = appointment.DentistId,
                    dentistName = appointment.Dentist?.EmployeeName,
                    appointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-ddTHH:mm"),
                    notes = appointment.Notes,
                    status = appointment.Status,
                    createdAt = appointment.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto dto)
        {
            if (dto.AppointmentDate < DateTime.Now.AddDays(-1))
            {
                return Json(new { success = false, message = "Cannot create appointments in the past" });
            }

            // Validate DentistId
            if (dto.DentistId <= 0)
            {
                return Json(new { success = false, message = "Please select a dentist" });
            }

            // Verify dentist exists
            var dentistExists = await _context.Users
                .AnyAsync(u => u.UserId == dto.DentistId && u.Role == "Dentist" && u.IsActive);

            if (!dentistExists)
            {
                return Json(new { success = false, message = "Invalid dentist selected" });
            }

            // Validate patient selection
            if (!dto.PatientId.HasValue || dto.PatientId.Value <= 0)
            {
                return Json(new { success = false, message = "Please select a patient" });
            }

            // Verify patient exists
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == dto.PatientId.Value);
            if (!patientExists)
            {
                return Json(new { success = false, message = "Invalid patient selected" });
            }

            // Get patient name for legacy field
            var patient = await _context.Patients.FindAsync(dto.PatientId.Value);

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                PatientName = $"{patient.FirstName} {patient.LastName}", // Populate legacy field
                DentistId = dto.DentistId,
                AppointmentDate = dto.AppointmentDate,
                Notes = dto.Notes,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointment([FromBody] AppointmentDto dto)
        {
            if (dto.AppointmentId <= 0)
            {
                return Json(new { success = false, message = "Invalid appointment ID" });
            }

            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            // Validate DentistId
            if (dto.DentistId <= 0)
            {
                return Json(new { success = false, message = "Please select a dentist" });
            }

            // Verify dentist exists
            var dentistExists = await _context.Users
                .AnyAsync(u => u.UserId == dto.DentistId && u.Role == "Dentist" && u.IsActive);

            if (!dentistExists)
            {
                return Json(new { success = false, message = "Invalid dentist selected" });
            }

            // Validate patient selection
            if (!dto.PatientId.HasValue || dto.PatientId.Value <= 0)
            {
                return Json(new { success = false, message = "Please select a patient" });
            }

            // Verify patient exists
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == dto.PatientId.Value);
            if (!patientExists)
            {
                return Json(new { success = false, message = "Invalid patient selected" });
            }

            // Get patient name for legacy field
            var patient = await _context.Patients.FindAsync(dto.PatientId.Value);

            appointment.PatientId = dto.PatientId;
            appointment.PatientName = $"{patient.FirstName} {patient.LastName}";
            appointment.DentistId = dto.DentistId;
            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.Notes = dto.Notes;

            try
            {
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Update failed: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment([FromBody] DeleteRequest request)
        {
            if (request.Id <= 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            var appointment = await _context.Appointments.FindAsync(request.Id);
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
                .Include(a => a.Dentist)
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate.Date == parsedDate.Date)
                .Select(a => new
                {
                    appointmentId = a.AppointmentId,
                    patientId = a.PatientId,
                    patientName = a.PatientName,
                    dentistId = a.DentistId,
                    dentistName = a.Dentist.EmployeeName,
                    appointmentDate = a.AppointmentDate,
                    notes = a.Notes
                })
                .ToListAsync();

            return Json(new { success = true, appointments });
        }

        // DTO classes for JSON binding
        public class AppointmentDto
        {
            public int AppointmentId { get; set; }
            public int? PatientId { get; set; }
            public int DentistId { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string Notes { get; set; }
        }

        public class DeleteRequest
        {
            public int Id { get; set; }
        }
    }
}