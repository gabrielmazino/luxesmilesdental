using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;

namespace LuxeSmilesDental.Controllers
{
    [AuthorizeSession("Dentist", "Dental Assistant", "Secretary")]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchName, string contactNumber)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                searchName = searchName.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.FirstName, $"%{searchName}%") ||
                    EF.Functions.Like(p.LastName, $"%{searchName}%"));
            }

            if (!string.IsNullOrWhiteSpace(contactNumber))
            {
                contactNumber = contactNumber.Trim();
                query = query.Where(p => EF.Functions.Like(p.ContactNumber, $"%{contactNumber}%"));
            }

            var patients = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(patients);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.DentalCharts)
                .Include(p => p.PatientRecalls)
                .FirstOrDefaultAsync(m => m.PatientId == id);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpGet]
        // All roles can create patients
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,ContactNumber,Email,Address,BirthDate,Notes")] Patient patient)
        {
            ModelState.Remove("DentalCharts");
            ModelState.Remove("PatientRecalls");
            ModelState.Remove("FullName");

            if (ModelState.IsValid)
            {
                if (patient.BirthDate.HasValue && patient.BirthDate.Value > DateTime.Now)
                {
                    ModelState.AddModelError("BirthDate", "Birth date cannot be in the future");
                    return View(patient);
                }

                patient.CreatedAt = DateTime.UtcNow;
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        [HttpGet]
        // All roles can edit patients
        public async Task<IActionResult> Edit(int? id)
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
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientId,FirstName,LastName,ContactNumber,Email,Address,BirthDate,Notes,CreatedAt")] Patient patient)
        {
            if (id != patient.PatientId || id <= 0)
            {
                return NotFound();
            }

            ModelState.Remove("DentalCharts");
            ModelState.Remove("PatientRecalls");
            ModelState.Remove("FullName");

            if (ModelState.IsValid)
            {
                if (patient.BirthDate.HasValue && patient.BirthDate.Value > DateTime.Now)
                {
                    ModelState.AddModelError("BirthDate", "Birth date cannot be in the future");
                    return View(patient);
                }

                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.PatientId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        [HttpGet]
        [AuthorizeSession("Dentist")] // ONLY Dentist can delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.PatientId == id);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("Dentist")] // ONLY Dentist can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}