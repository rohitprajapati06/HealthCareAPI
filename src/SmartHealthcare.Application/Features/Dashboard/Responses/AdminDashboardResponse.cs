namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public sealed class AdminDashboardResponse
    {
        public int TotalHospitals { get; init; }

        public int TotalDoctors { get; init; }

        public int ApprovedDoctors { get; init; }

        public int PendingDoctors { get; init; }

        public int RejectedDoctors { get; init; }

        public int TotalPatients { get; init; }

        public int TotalAppointments { get; init; }

        public int CompletedAppointments { get; init; }

        public int PendingAppointments { get; init; }

        public int CancelledAppointments { get; init; }

        public int TotalPrescriptions { get; init; }

        public int TotalMedicalRecords { get; init; }
    }
}