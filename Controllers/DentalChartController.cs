using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;
using System.Text.RegularExpressions;

namespace LuxeSmilesDental.Controllers
{
    // Dentist and Dental Assistant can create/edit charts
    // Secretary can only view
    [AuthorizeSession("Dentist", "Dental Assistant", "Secretary")]
    public class DentalChartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int MaxFileSize = 5 * 1024 * 1024;
        private static readonly string[] AllowedMimeTypes = { "image/png" };

        public DentalChartController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot create charts
        public async Task<IActionResult> Chart(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            ViewBag.PatientId = id;
            ViewBag.PatientName = patient.FullName;

            var previousCharts = await _context.DentalCharts
                .Where(d => d.PatientId == id)
                .OrderByDescending(d => d.VisitDate)
                .Select(d => new { d.ChartId, d.VisitDate, d.CreatedBy })
                .ToListAsync();

            ViewBag.PreviousCharts = previousCharts;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot save charts
        public async Task<IActionResult> SaveChart(int patientId, string imageData, DateTime visitDate)
        {
            try
            {
                if (patientId <= 0 || !await _context.Patients.AnyAsync(p => p.PatientId == patientId))
                {
                    return Json(new { success = false, message = "Invalid patient" });
                }

                if (string.IsNullOrWhiteSpace(imageData) || !imageData.StartsWith("data:image/png;base64,"))
                {
                    return Json(new { success = false, message = "Invalid image format" });
                }

                if (visitDate > DateTime.Now.AddDays(1) || visitDate < DateTime.Now.AddYears(-10))
                {
                    return Json(new { success = false, message = "Invalid visit date" });
                }

                var username = HttpContext.Session.GetString("Username") ?? "Unknown";

                var base64Match = Regex.Match(imageData, @"^data:image/png;base64,([A-Za-z0-9+/=]+)$");
                if (!base64Match.Success)
                {
                    return Json(new { success = false, message = "Invalid image data" });
                }

                string base64Data = base64Match.Groups[1].Value;
                byte[] imageBytes;

                try
                {
                    imageBytes = Convert.FromBase64String(base64Data);
                }
                catch
                {
                    return Json(new { success = false, message = "Invalid image encoding" });
                }

                if (imageBytes.Length > MaxFileSize)
                {
                    return Json(new { success = false, message = "Image too large" });
                }

                if (imageBytes.Length < 8 ||
                    imageBytes[0] != 0x89 || imageBytes[1] != 0x50 ||
                    imageBytes[2] != 0x4E || imageBytes[3] != 0x47)
                {
                    return Json(new { success = false, message = "Invalid PNG file" });
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "dentalcharts");
                Directory.CreateDirectory(uploadsFolder);

                string safeFileName = $"chart_{patientId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.png";
                string filePath = Path.Combine(uploadsFolder, safeFileName);

                if (!filePath.StartsWith(uploadsFolder))
                {
                    return Json(new { success = false, message = "Invalid file path" });
                }

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                var dentalChart = new DentalChart
                {
                    PatientId = patientId,
                    ImagePath = $"/uploads/dentalcharts/{safeFileName}",
                    VisitDate = visitDate,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DentalCharts.Add(dentalChart);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Chart saved successfully" });
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        [HttpGet]
        // All roles can view history
        public async Task<IActionResult> History(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            var charts = await _context.DentalCharts
                .Where(d => d.PatientId == id)
                .OrderByDescending(d => d.VisitDate)
                .ToListAsync();

            ViewBag.PatientName = patient.FullName;
            return View(charts);
        }

        [HttpGet]
        // All roles can view individual charts
        public async Task<IActionResult> ViewChart(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var chart = await _context.DentalCharts
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.ChartId == id);

            if (chart == null)
            {
                return NotFound();
            }

            var webRoot = _webHostEnvironment.WebRootPath;
            var fullPath = Path.Combine(webRoot, chart.ImagePath.TrimStart('/'));

            if (!fullPath.StartsWith(Path.Combine(webRoot, "uploads", "dentalcharts")))
            {
                return NotFound();
            }

            return View(chart);
        }
    }
}