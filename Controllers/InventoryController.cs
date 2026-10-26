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
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(ApplicationDbContext context, ILogger<InventoryController> logger)
        {
            _context = context;
            _logger = logger;
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
            try
            {
                _logger.LogInformation("AddItem called with ItemName: {ItemName}", item?.ItemName);

                // Remove validation for auto-generated fields
                ModelState.Remove("ItemId");
                ModelState.Remove("Status");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));

                    // FIXED: Always return proper JSON with status 400 for validation errors
                    Response.StatusCode = 400;
                    return Json(new
                    {
                        success = false,
                        message = "Invalid data: " + string.Join(", ", errors)
                    });
                }

                if (item.Quantity < 0 || item.MinimumStock < 0)
                {
                    Response.StatusCode = 400;
                    return Json(new { success = false, message = "Quantity cannot be negative" });
                }

                item.Status = item.Quantity <= item.MinimumStock ? "Low" : "In Stock";
                item.CreatedAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;

                _context.Inventory.Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Item added successfully: {ItemId}", item.ItemId);

                // FIXED: Return proper JSON response with 200 status code
                Response.StatusCode = 200;
                return Json(new { success = true, message = "Item added successfully", itemId = item.ItemId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item");

                // FIXED: Return proper JSON error response with 500 status
                Response.StatusCode = 500;
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot update items
        public async Task<IActionResult> UpdateItem(InventoryItem item)
        {
            try
            {
                _logger.LogInformation("UpdateItem called for ItemId: {ItemId}", item?.ItemId);

                if (item.ItemId <= 0)
                {
                    Response.StatusCode = 400;
                    return Json(new { success = false, message = "Invalid item ID" });
                }

                // Remove validation for fields that will be set by the server
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");
                ModelState.Remove("Status");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));

                    Response.StatusCode = 400;
                    return Json(new
                    {
                        success = false,
                        message = "Invalid data: " + string.Join(", ", errors)
                    });
                }

                if (item.Quantity < 0 || item.MinimumStock < 0)
                {
                    Response.StatusCode = 400;
                    return Json(new { success = false, message = "Quantity cannot be negative" });
                }

                var existingItem = await _context.Inventory.FindAsync(item.ItemId);
                if (existingItem == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { success = false, message = "Item not found" });
                }

                // Update properties
                existingItem.ItemName = item.ItemName;
                existingItem.Category = item.Category;
                existingItem.Quantity = item.Quantity;
                existingItem.Unit = item.Unit;
                existingItem.MinimumStock = item.MinimumStock;
                existingItem.Supplier = item.Supplier;
                existingItem.Status = item.Quantity <= item.MinimumStock ? "Low" : "In Stock";
                existingItem.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Item updated successfully: {ItemId}", item.ItemId);

                // FIXED: Always return proper JSON with status 200
                Response.StatusCode = 200;
                return Json(new { success = true, message = "Item updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId}", item?.ItemId);

                Response.StatusCode = 500;
                return Json(new { success = false, message = $"Update failed: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeSession("Dentist")] // ONLY Dentist can delete inventory items
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                _logger.LogInformation("DeleteItem called for ItemId: {ItemId}", id);

                if (id <= 0)
                {
                    Response.StatusCode = 400;
                    return Json(new { success = false, message = "Invalid ID" });
                }

                var item = await _context.Inventory.FindAsync(id);
                if (item == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { success = false, message = "Item not found" });
                }

                _context.Inventory.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Item deleted successfully: {ItemId}", id);

                Response.StatusCode = 200;
                return Json(new { success = true, message = "Item deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {ItemId}", id);

                Response.StatusCode = 500;
                return Json(new { success = false, message = $"Delete failed: {ex.Message}" });
            }
        }

        [HttpGet]
        [AuthorizeSession("Dentist", "Dental Assistant")] // Secretary cannot access this
        public async Task<IActionResult> GetItem(int id)
        {
            try
            {
                _logger.LogInformation("GetItem called for ItemId: {ItemId}", id);

                if (id <= 0)
                {
                    Response.StatusCode = 400;
                    return Json(new { success = false, message = "Invalid ID" });
                }

                var item = await _context.Inventory.FindAsync(id);
                if (item == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { success = false, message = "Item not found" });
                }

                Response.StatusCode = 200;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item {ItemId}", id);

                Response.StatusCode = 500;
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}