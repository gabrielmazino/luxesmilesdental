using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeSmilesDental.Models
{
    [Table("TreatmentRecords")]
    public class TreatmentRecord
    {
        [Key]
        public int TreatmentRecordId { get; set; }

        [Required]
        public int ChartId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime TreatmentDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Tooth No./s")]
        public string? ToothNumbers { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Procedure")]
        public string Procedure { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Dentist")]
        public string Dentist { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount Charged")]
        public decimal? AmountCharged { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount Paid")]
        public decimal? AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Balance")]
        public decimal? Balance { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Next Appointment")]
        public DateTime? NextAppointment { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("ChartId")]
        public virtual DentalChart DentalChart { get; set; }
    }
}