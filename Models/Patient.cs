// Models/Patient.cs
// FIXED: Removed [Required] from optional fields and proper initialization

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeSmilesDental.Models
{
    [Table("Patients")]
    public class Patient
    {
        [Key]
        [Column("PatientId")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        [Column("BirthDate")]
        public DateTime? BirthDate { get; set; }

        [StringLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<DentalChart> DentalCharts { get; set; } = new List<DentalChart>();
        public virtual ICollection<PatientRecall> PatientRecalls { get; set; } = new List<PatientRecall>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}