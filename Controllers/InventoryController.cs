using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxeSmilesDental.Data;
using LuxeSmilesDental.Models;
using LuxeSmilesDental.Filters;

namespace LuxeSmilesDental.Controllers
{
    // Dentist and Dental Assistant can manage inventory
    // Secretary has read-only access
    [AuthorizeSession("Dentist", "Dental Assistant", "Secretary")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchItem, string category, string supplier, string status)
        {
            var query = _context.Inventory.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchItem))
            {
                searchItem = searchItem.Trim();
                query = query.Where(i => EF.Functions.Like(i.ItemName, $"%{searchItem}%"));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "Select Category")
            {
                query = query.Where(i => i.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(supplier))
            {
                supplier = supplier.Trim();
                query = query.Where(i => EF.Functions.Like(i.Supplier, $"%{supplier}%"));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Select Status")
            {
                query = query.Where(i => i.Status == status);
            }

            var items = await query.ToListAsync();

            // Update stock status
            foreach (var item in items)
            {
                item.Status = item.Quantity <= item.MinimumStock ? "Low" : "In Stock";
            }
            await _context.SaveChangesAsync();

            var userRole = HttpContext.Session.GetString("Role");
            ViewBag.CanModify = userRole == "Dentist" || userRole == "Dental Assistant";

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot add items
        public async Task<IActionResult> AddItem(InventoryItem item)
        {
            ModelState.Remove("ItemId");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                if (item.Quantity < 0 || item.MinimumStock < 0)
                {
                    return Json(new { success = false, message = "Quantity cannot be negative" });
                }

                item.Status = item.Quantity <= item.MinimumStock ? "Low" : "In Stock";
                _context.Inventory.Add(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid data" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot update items
        public async Task<IActionResult> UpdateItem(InventoryItem item)
        {
            if (item.ItemId <= 0)
            {
                return Json(new { success = false, message = "Invalid item" });
            }

            if (ModelState.IsValid)
            {
                if (item.Quantity < 0 || item.MinimumStock < 0)
                {
                    return Json(new { success = false, message = "Quantity cannot be negative" });
                }

                try
                {
                    item.Status = item.Quantity <= item.MinimumStock ? "Low" : "In Stock";
                    _context.Inventory.Update(item);
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
        [AuthorizeSession("Dentist")] // ONLY Dentist can delete inventory items
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            var item = await _context.Inventory.FindAsync(id);
            if (item != null)
            {
                _context.Inventory.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Item not found" });
        }

        [HttpGet]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot access this
        public async Task<IActionResult> GetItem(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false });
            }

            var item = await _context.Inventory.FindAsync(id);
            if (item != null)
            {
                return Json(new
                {
                    success = true,
                    item = new
                    {
                        item.ItemId,
                        item.ItemName,
                        item.Category,
                        item.Quantity,
                        item.Unit,
                        item.MinimumStock,
                        item.Supplier
                    }
                });
            }

            return Json(new { success = false });
        }
    }
}