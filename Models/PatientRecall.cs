// Models/PatientRecall.cs
// This model matches your existing PatientRecalls table

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeSmilesDental.Models
{
    [Table("PatientRecalls")]
    public class PatientRecall
    {
        [Key]
        [Column("RecallId")]
        public int RecallId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Recall Case")]
        public string RecallCase { get; set; }

        [Display(Name = "Previous Visit Date")]
        [DataType(DataType.Date)]
        public DateTime? PreviousVisitDate { get; set; }

        [Required]
        [Display(Name = "Recall Date")]
        [DataType(DataType.Date)]
        public DateTime RecallDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // 'Upcoming Recall' or 'Recall Overdue'

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }
    }
}