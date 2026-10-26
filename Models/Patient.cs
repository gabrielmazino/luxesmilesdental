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

        // BASIC INFORMATION
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(50)]
        [Display(Name = "Nickname")]
        public string? Nickname { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        [Column("BirthDate")]
        public DateTime? BirthDate { get; set; }

        [StringLength(10)]
        [Display(Name = "Sex")]
        public string? Sex { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string? ContactNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Religion")]
        public string? Religion { get; set; }

        [StringLength(50)]
        [Display(Name = "Nationality")]
        public string? Nationality { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Home Address")]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Health Card Number")]
        public string? HealthCardNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Occupation")]
        public string? Occupation { get; set; }

        [StringLength(100)]
        [Display(Name = "Company")]
        public string? Company { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned Dentist")]
        public string? AssignedDentist { get; set; }

        // FOR MINORS
        [StringLength(150)]
        [Display(Name = "Parent/Guardian Name")]
        public string? ParentGuardianName { get; set; }

        [StringLength(100)]
        [Display(Name = "Parent/Guardian Occupation")]
        public string? ParentGuardianOccupation { get; set; }

        // REFERRAL
        [StringLength(150)]
        [Display(Name = "Referred By")]
        public string? ReferredBy { get; set; }

        [StringLength(500)]
        [Display(Name = "Reason for Consultation")]
        public string? ReasonForConsultation { get; set; }

        // DENTAL HISTORY
        [StringLength(150)]
        [Display(Name = "Previous Dentist")]
        public string? PreviousDentist { get; set; }

        [StringLength(100)]
        [Display(Name = "Previous Dentist Specialty")]
        public string? PreviousDentistSpecialty { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Last Dental Visit")]
        public DateTime? LastDentalVisit { get; set; }

        [StringLength(500)]
        [Display(Name = "Previous Dental Procedure Done")]
        public string? LastDentalProcedure { get; set; }

        // MEDICAL HISTORY
        [StringLength(150)]
        [Display(Name = "Physician Name")]
        public string? PhysicianName { get; set; }

        [StringLength(100)]
        [Display(Name = "Physician Specialty")]
        public string? PhysicianSpecialty { get; set; }

        [StringLength(20)]
        [Display(Name = "Physician Office Number")]
        public string? PhysicianOfficeNumber { get; set; }

        // Medical Questions (Yes/No)
        [Display(Name = "Are you in good health?")]
        public bool? InGoodHealth { get; set; }

        [Display(Name = "Are you under medical treatment now?")]
        public bool? UnderMedicalTreatment { get; set; }

        [StringLength(500)]
        [Display(Name = "Medical Treatment Details")]
        public string? MedicalTreatmentDetails { get; set; }

        [Display(Name = "Have you ever had serious illness or surgical operation?")]
        public bool? HadSeriousIllness { get; set; }

        [StringLength(500)]
        [Display(Name = "Illness/Surgery Details")]
        public string? SeriousIllnessDetails { get; set; }

        [Display(Name = "Have you ever been hospitalized?")]
        public bool? BeenHospitalized { get; set; }

        [StringLength(500)]
        [Display(Name = "Hospitalization Details")]
        public string? HospitalizationDetails { get; set; }

        [Display(Name = "Are you taking any medications?")]
        public bool? TakingMedications { get; set; }

        [StringLength(500)]
        [Display(Name = "Current Medications")]
        public string? CurrentMedications { get; set; }

        [Display(Name = "Smoking History")]
        public bool? SmokingHistory { get; set; }

        [Display(Name = "Alcohol, Cocaine or other dangerous drug use")]
        public bool? AlcoholDrugUse { get; set; }

        [Display(Name = "Previous Allergies (LA or medications)")]
        public bool? HasAllergies { get; set; }

        [StringLength(500)]
        [Display(Name = "Allergy Details")]
        public string? AllergyDetails { get; set; }

        [StringLength(50)]
        [Display(Name = "Bleeding Time")]
        public string? BleedingTime { get; set; }

        // FOR WOMEN ONLY
        [Display(Name = "Pregnant?")]
        public bool? IsPregnant { get; set; }

        [Display(Name = "Are you nursing?")]
        public bool? IsNursing { get; set; }

        [Display(Name = "Are you taking any birth control pills?")]
        public bool? TakingBirthControl { get; set; }

        [StringLength(10)]
        [Display(Name = "Blood Type")]
        public string? BloodType { get; set; }

        [StringLength(20)]
        [Display(Name = "Blood Pressure")]
        public string? BloodPressure { get; set; }

        // MEDICAL CONDITIONS (checkboxes)
        [Display(Name = "High Blood Pressure")]
        public bool HighBloodPressure { get; set; }

        [Display(Name = "Low Blood Pressure")]
        public bool LowBloodPressure { get; set; }

        [Display(Name = "Heart Disease")]
        public bool HeartDisease { get; set; }

        [Display(Name = "Previous Heart Surgery/Attack")]
        public bool HeartSurgeryAttack { get; set; }

        [Display(Name = "Chest Pain")]
        public bool ChestPain { get; set; }

        [Display(Name = "Stroke")]
        public bool Stroke { get; set; }

        [Display(Name = "Liver Disease")]
        public bool LiverDisease { get; set; }

        [Display(Name = "Kidney Disease")]
        public bool KidneyDisease { get; set; }

        [Display(Name = "Thyroid Problems")]
        public bool ThyroidProblems { get; set; }

        [Display(Name = "Lung Disorder")]
        public bool LungDisorder { get; set; }

        [Display(Name = "GIT Disease")]
        public bool GITDisease { get; set; }

        [Display(Name = "Tuberculosis")]
        public bool Tuberculosis { get; set; }

        [Display(Name = "Blood Dyscrasias (Anemia, Leukemia, Thrombocytopenia)")]
        public bool BloodDyscrasias { get; set; }

        [Display(Name = "Bleeding Tendencies")]
        public bool BleedingTendencies { get; set; }

        [Display(Name = "Head Injuries")]
        public bool HeadInjuries { get; set; }

        [Display(Name = "Arthritis / Rheumatism")]
        public bool ArthritisRheumatism { get; set; }

        [Display(Name = "Cancer/Tumors")]
        public bool CancerTumors { get; set; }

        [Display(Name = "Radiation Therapy/Chemotherapy")]
        public bool RadiationChemotherapy { get; set; }

        [Display(Name = "Infective Disease (STDs, AIDS/HIV, Hepatitis)")]
        public bool InfectiveDisease { get; set; }

        [Display(Name = "Extreme Anxiety")]
        public bool ExtremeAnxiety { get; set; }

        // FAMILY HISTORY
        [StringLength(500)]
        [Display(Name = "Paternal Medical History")]
        public string? PaternalHistory { get; set; }

        [StringLength(500)]
        [Display(Name = "Maternal Medical History")]
        public string? MaternalHistory { get; set; }

        // GENERAL NOTES
        [StringLength(1000)]
        [Column("Notes")]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<DentalChart> DentalCharts { get; set; } = new List<DentalChart>();
        public virtual ICollection<PatientRecall> PatientRecalls { get; set; } = new List<PatientRecall>();

        [NotMapped]
        public string FullName => string.IsNullOrEmpty(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        [NotMapped]
        public int? Age
        {
            get
            {
                if (!BirthDate.HasValue) return null;
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}