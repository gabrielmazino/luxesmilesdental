// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace LuxeSmilesDental.Models
{
    // Models/InventoryItem.cs
    public class InventoryItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }

        public int MinimumStock { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}