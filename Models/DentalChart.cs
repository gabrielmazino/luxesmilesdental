using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeSmilesDental.Models
{
    [Table("DentalCharts")]
    public class DentalChart
    {
        [Key]
        [Column("ChartId")]
        public int ChartId { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Image Path")]
        public string ImagePath { get; set; }

        [Required]
        [Display(Name = "Visit Date")]
        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // INTRAORAL EXAMINATION FIELDS
        // Periodontal Screening
        [Display(Name = "Gingivitis")]
        public bool HasGingivitis { get; set; }

        [Display(Name = "Periodontitis")]
        public bool HasPeriodontitis { get; set; }

        // Appliances
        [Display(Name = "Orthodontic Appliance")]
        public bool HasOrthodonticAppliance { get; set; }

        // Occlusion
        [StringLength(50)]
        [Display(Name = "Class (Molar)")]
        public string? OcclusionClassMolar { get; set; }

        [StringLength(50)]
        [Display(Name = "Overjet")]
        public string? Overjet { get; set; }

        [StringLength(50)]
        [Display(Name = "Overbite")]
        public string? Overbite { get; set; }

        [StringLength(50)]
        [Display(Name = "Midline Deviation")]
        public string? MidlineDeviation { get; set; }

        [Display(Name = "Crossbite")]
        public bool HasCrossbite { get; set; }

        // TMD (Temporomandibular Disorder)
        [Display(Name = "Clenching")]
        public bool HasClenching { get; set; }

        [Display(Name = "Clicking")]
        public bool HasClicking { get; set; }

        [Display(Name = "Trismus")]
        public bool HasTrismus { get; set; }

        [Display(Name = "Muscle Spasm")]
        public bool HasMuscleSpasm { get; set; }

        // Notes
        [StringLength(1000)]
        [Display(Name = "Examination Notes")]
        public string? ExaminationNotes { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; } = new List<TreatmentRecord>();
    }
}