using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;
using System.Text.RegularExpressions;

namespace LuxeSmilesDental.Controllers
{
    [AuthorizeSession("Dentist", "Dental Assistant", "Secretary")]
    public class DentalChartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int MaxFileSize = 5 * 1024 * 1024;

        public DentalChartController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AuthorizeSession("Dentist", "Dental Assistant")]
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
        public async Task<IActionResult> SaveChart([FromBody] SaveChartRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                if (request.PatientId <= 0 || !await _context.Patients.AnyAsync(p => p.PatientId == request.PatientId))
                {
                    return Json(new { success = false, message = "Invalid patient" });
                }

                if (string.IsNullOrWhiteSpace(request.ImageData) || !request.ImageData.StartsWith("data:image/png;base64,"))
                {
                    return Json(new { success = false, message = "Invalid image format" });
                }

                if (request.VisitDate > DateTime.Now.AddDays(1) || request.VisitDate < DateTime.Now.AddYears(-10))
                {
                    return Json(new { success = false, message = "Invalid visit date" });
                }

                var username = HttpContext.Session.GetString("Username") ?? "Unknown";

                var base64Match = Regex.Match(request.ImageData, @"^data:image/png;base64,([A-Za-z0-9+/=]+)$");
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
                    return Json(new { success = false, message = "Image too large (max 5MB)" });
                }

                if (imageBytes.Length < 8 ||
                    imageBytes[0] != 0x89 || imageBytes[1] != 0x50 ||
                    imageBytes[2] != 0x4E || imageBytes[3] != 0x47)
                {
                    return Json(new { success = false, message = "Invalid PNG file" });
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "dentalcharts");
                Directory.CreateDirectory(uploadsFolder);

                string safeFileName = $"chart_{request.PatientId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.png";
                string filePath = Path.Combine(uploadsFolder, safeFileName);

                if (!filePath.StartsWith(uploadsFolder))
                {
                    return Json(new { success = false, message = "Invalid file path" });
                }

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                var dentalChart = new DentalChart
                {
                    PatientId = request.PatientId,
                    ImagePath = $"/uploads/dentalcharts/{safeFileName}",
                    VisitDate = request.VisitDate,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DentalCharts.Add(dentalChart);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Chart saved successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chart save error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while saving" });
            }
        }

        [HttpGet]
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
        public async Task<IActionResult> ViewChart(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            // Load chart with patient information
            var chart = await _context.DentalCharts
                .Where(d => d.ChartId == id)
                .Select(d => new DentalChart
                {
                    ChartId = d.ChartId,
                    PatientId = d.PatientId,
                    ImagePath = d.ImagePath,
                    VisitDate = d.VisitDate,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    Patient = d.Patient
                })
                .FirstOrDefaultAsync();

            if (chart == null)
            {
                return NotFound();
            }

            return View(chart);
        }
    }

    public class SaveChartRequest
    {
        public int PatientId { get; set; }
        public string ImageData { get; set; }
        public DateTime VisitDate { get; set; }
    }
}