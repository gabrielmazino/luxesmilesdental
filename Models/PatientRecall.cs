// Models/PatientRecall.cs

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

        [Required(ErrorMessage = "Please select a patient")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Recall case is required")]
        [StringLength(100, ErrorMessage = "Recall case cannot exceed 100 characters")]
        [Display(Name = "Recall Case")]
        public string RecallCase { get; set; }

        [Display(Name = "Previous Visit Date")]
        [DataType(DataType.Date)]
        public DateTime? PreviousVisitDate { get; set; }

        [Required(ErrorMessage = "Recall date is required")]
        [Display(Name = "Recall Date")]
        [DataType(DataType.Date)]
        public DateTime RecallDate { get; set; }

        // This field is set by UpdateRecallStatus() based on RecallDate
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } // 'Upcoming Recall' or 'Recall Overdue'

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }
    }
}