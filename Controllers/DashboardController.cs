using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;

namespace LuxeSmilesDental.Controllers
{
    [AuthorizeSession("Dentist", "Dental Assistant", "Secretary")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(),
                NewRegisteredPatientsToday = await _context.Patients
                    .Where(p => p.CreatedAt.Date == DateTime.Today)
                    .CountAsync(),
                TotalTreatmentDoneToday = await _context.DentalCharts
                    .Where(d => d.VisitDate.Date == DateTime.Today)
                    .CountAsync(),
                TodaysSchedule = await _context.Appointments
                    .Where(a => a.AppointmentDate.Date == DateTime.Today)
                    .Select(a => new Appointment
                    {
                        PatientName = a.PatientName,
                        Notes = a.Notes
                    })
                    .ToListAsync(),
                UpcomingRecalls = await _context.PatientRecalls
                    .Include(r => r.Patient)
                    .Where(r => r.RecallDate >= DateTime.Today && r.Status == "Upcoming Recall")
                    .OrderBy(r => r.RecallDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}