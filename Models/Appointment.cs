using System;
using System.ComponentModel.DataAnnotations;

namespace LuxeSmilesDental.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [Required]
        public string PatientName { get; set; }

        public int DentistId { get; set; }
        public User Dentist { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public TimeSpan? AppointmentTime { get; set; }

        public string Status { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
