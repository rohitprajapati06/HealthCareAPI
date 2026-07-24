

namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public class AdminDashboardResponse
    {
        public int totalHospitals { get; set;  }

        public int totalDoctors { get; set; }

        public int approvedDoctors { get; set; }

        public int pendingDoctors { get; set; }

        public int rejectedDoctors { get; set; }

        public int totalPatients    { get; set; }

        public int totalAppointments { get; set; }

        public int completedAppointments { get; set; }

        public int pendingAppoinments { get; set; }

        public int totalPrescriptions { get; set; }

        public int totalMedicalRecords  { get; set; }


    }
}
