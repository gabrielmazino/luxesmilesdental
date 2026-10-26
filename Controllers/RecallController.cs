using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;

namespace LuxeSmilesDental.Controllers
{
    [AuthorizeSession("Dentist", "Dental Assistant", "Secretary")]
    public class RecallController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecallController> _logger;

        public RecallController(ApplicationDbContext context, ILogger<RecallController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string status = "All")
        {
            var recalls = _context.PatientRecalls
                .Include(p => p.Patient)
                .AsQueryable();

            // Added "Soon" to allowed statuses
            var allowedStatuses = new[] { "All", "Upcoming Recall", "Recall Overdue", "Soon" };
            if (!allowedStatuses.Contains(status))
            {
                status = "All";
            }

            if (status != "All")
            {
                if (status == "Soon")
                {
                    // Logic for Soon: RecallDate is between today and 7 days from now
                    var today = DateTime.Today;
                    var sevenDaysFromNow = today.AddDays(7);
                    recalls = recalls.Where(r => r.RecallDate >= today && r.RecallDate <= sevenDaysFromNow);
                }
                else
                {
                    recalls = recalls.Where(r => r.Status == status);
                }
            }

            var recallsList = await recalls.OrderBy(r => r.RecallDate).ToListAsync();

            foreach (var recall in recallsList)
            {
                UpdateRecallStatus(recall);
            }
            await _context.SaveChangesAsync();

            ViewBag.CurrentStatus = status;
            return View(recallsList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var recall = await _context.PatientRecalls
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.RecallId == id);

            if (recall == null)
            {
                return NotFound();
            }

            return View(recall);
        }

        [HttpGet]
        public IActionResult Create(int? patientId)
        {
            ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", patientId);

            var recall = new PatientRecall
            {
                RecallDate = DateTime.Today  // ? ADD THIS
            };

            if (patientId.HasValue && patientId.Value > 0)
            {
                recall.PatientId = patientId.Value;
            }

            return View(recall);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientRecall recall)
        {
            try
            {
                _logger.LogInformation("Creating recall for PatientId: {PatientId}, RecallCase: {RecallCase}",
                    recall.PatientId, recall.RecallCase);

                // FIXED: Remove Status from ModelState validation since it's computed
                ModelState.Remove("Status");
                ModelState.Remove("Patient");
                ModelState.Remove("CreatedAt");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));

                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                // Validate recall date is not too far in the future
                if (recall.RecallDate > DateTime.Today.AddYears(5))
                {
                    ModelState.AddModelError("RecallDate", "Recall date cannot be more than 5 years in the future");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                // Validate patient exists
                if (!await _context.Patients.AnyAsync(p => p.PatientId == recall.PatientId))
                {
                    ModelState.AddModelError("PatientId", "Selected patient does not exist");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName");
                    return View(recall);
                }

                // Validate PreviousVisitDate is not in the future
                if (recall.PreviousVisitDate.HasValue && recall.PreviousVisitDate.Value > DateTime.Today)
                {
                    ModelState.AddModelError("PreviousVisitDate", "Previous visit date cannot be in the future");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                // Set audit fields
                recall.CreatedAt = DateTime.UtcNow;

                // CRITICAL FIX: Automatically compute Status based on RecallDate
                UpdateRecallStatus(recall);

                _logger.LogInformation("Status automatically set to: {Status}", recall.Status);

                _context.Add(recall);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Recall created successfully with RecallId: {RecallId}", recall.RecallId);

                TempData["SuccessMessage"] = "Patient recall created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recall");
                ModelState.AddModelError("", "An error occurred while creating the recall. Please try again.");
                ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                return View(recall);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var recall = await _context.PatientRecalls.FindAsync(id);
            if (recall == null)
            {
                return NotFound();
            }

            ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
            return View(recall);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientRecall recall)
        {
            if (id != recall.RecallId || id <= 0)
            {
                return NotFound();
            }

            try
            {
                _logger.LogInformation("Editing recall RecallId: {RecallId}", id);

                // FIXED: Remove Status from ModelState validation
                ModelState.Remove("Status");
                ModelState.Remove("Patient");
                ModelState.Remove("CreatedAt");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));

                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                // Validate recall date
                if (recall.RecallDate > DateTime.Today.AddYears(5))
                {
                    ModelState.AddModelError("RecallDate", "Recall date cannot be more than 5 years in the future");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                // Validate previous visit date
                if (recall.PreviousVisitDate.HasValue && recall.PreviousVisitDate.Value > DateTime.Today)
                {
                    ModelState.AddModelError("PreviousVisitDate", "Previous visit date cannot be in the future");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                // Automatically update Status based on RecallDate
                UpdateRecallStatus(recall);

                _logger.LogInformation("Status automatically updated to: {Status}", recall.Status);

                _context.Update(recall);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Recall updated successfully");

                TempData["SuccessMessage"] = "Patient recall updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RecallExists(recall.RecallId))
                {
                    return NotFound();
                }

                _logger.LogError(ex, "Concurrency error updating recall {RecallId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recall {RecallId}", id);
                ModelState.AddModelError("", "An error occurred while updating the recall. Please try again.");
                ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                return View(recall);
            }
        }

        [HttpGet]
        [AuthorizeSession("Dentist", "Dental Assistant")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var recall = await _context.PatientRecalls
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.RecallId == id);

            if (recall == null)
            {
                return NotFound();
            }

            return View(recall);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("Dentist", "Dental Assistant")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound();
                }

                var recall = await _context.PatientRecalls.FindAsync(id);
                if (recall != null)
                {
                    _context.PatientRecalls.Remove(recall);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Recall deleted successfully: RecallId {RecallId}", id);
                    TempData["SuccessMessage"] = "Patient recall deleted successfully!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recall {RecallId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the recall.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ByPatient(int? id)
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

            var recalls = await _context.PatientRecalls
                .Where(r => r.PatientId == id)
                .OrderByDescending(r => r.RecallDate)
                .ToListAsync();

            // Update statuses for all recalls
            foreach (var recall in recalls)
            {
                UpdateRecallStatus(recall);
            }
            await _context.SaveChangesAsync();

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientId = id;
            return View(recalls);
        }

        private bool RecallExists(int id)
        {
            return _context.PatientRecalls.Any(e => e.RecallId == id);
        }

        /// <summary>
        /// Automatically sets the Status field based on the RecallDate
        /// This is the business logic that computes the status
        /// </summary>
        /// <param name="recall">The recall to update</param>
        private void UpdateRecallStatus(PatientRecall recall)
        {
            if (recall == null) throw new ArgumentNullException(nameof(recall));

            var daysUntil = (recall.RecallDate - DateTime.Today).Days;

            if (daysUntil < 0)
                recall.Status = "Recall Overdue";
            else if (daysUntil <= 7)
                recall.Status = "Soon";          // ? NEW
            else
                recall.Status = "Upcoming Recall";
        }
    }
}