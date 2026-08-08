namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public class AdminDashboardResponse
    {
        public int TotalHospitals { get; set; }

        public int TotalDoctors { get; set; }

        public int ApprovedDoctors { get; set; }

        public int PendingDoctors { get; set; }

        public int RejectedDoctors { get; set; }

        public int TotalPatients { get; set; }

        public int TotalAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int PendingAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int TotalPrescriptions { get; set; }

        public int TotalMedicalRecords { get; set; }
    }
}