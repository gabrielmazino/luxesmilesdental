using System.Collections.Generic;

namespace LuxeSmilesDental.Models
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int NewRegisteredPatientsToday { get; set; }
        public int TotalTreatmentDoneToday { get; set; }

        public List<Appointment> TodaysSchedule { get; set; }
        public List<PatientRecall> UpcomingRecalls { get; set; }
    }
}
