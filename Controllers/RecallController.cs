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

        public RecallController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string status = "All")
        {
            var recalls = _context.PatientRecalls
                .Include(p => p.Patient)
                .AsQueryable();

            var allowedStatuses = new[] { "All", "Upcoming Recall", "Recall Overdue" };
            if (!allowedStatuses.Contains(status))
            {
                status = "All";
            }

            if (status != "All")
            {
                recalls = recalls.Where(r => r.Status == status);
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
        // All staff can create recalls
        public IActionResult Create(int? patientId)
        {
            ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", patientId);

            var recall = new PatientRecall();
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
            ModelState.Remove("Patient");

            if (ModelState.IsValid)
            {
                if (recall.RecallDate > DateTime.Now.AddYears(5))
                {
                    ModelState.AddModelError("RecallDate", "Recall date too far in future");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                if (!await _context.Patients.AnyAsync(p => p.PatientId == recall.PatientId))
                {
                    ModelState.AddModelError("PatientId", "Invalid patient");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName");
                    return View(recall);
                }

                recall.CreatedAt = DateTime.UtcNow;
                UpdateRecallStatus(recall);

                _context.Add(recall);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
            return View(recall);
        }

        [HttpGet]
        // All staff can edit recalls
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

            ModelState.Remove("Patient");

            if (ModelState.IsValid)
            {
                if (recall.RecallDate > DateTime.Now.AddYears(5))
                {
                    ModelState.AddModelError("RecallDate", "Recall date too far in future");
                    ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
                    return View(recall);
                }

                try
                {
                    UpdateRecallStatus(recall);
                    _context.Update(recall);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecallExists(recall.RecallId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Patients = new SelectList(_context.Patients.OrderBy(p => p.LastName), "PatientId", "FullName", recall.PatientId);
            return View(recall);
        }

        [HttpGet]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot delete recalls
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
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot delete recalls
        public async Task<IActionResult> DeleteConfirmed(int id)
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
            }

            return RedirectToAction(nameof(Index));
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

            ViewBag.PatientName = patient.FullName;
            ViewBag.PatientId = id;
            return View(recalls);
        }

        private bool RecallExists(int id)
        {
            return _context.PatientRecalls.Any(e => e.RecallId == id);
        }

        private void UpdateRecallStatus(PatientRecall recall)
        {
            recall.Status = recall.RecallDate < DateTime.Today ? "Recall Overdue" : "Upcoming Recall";
        }
    }
}