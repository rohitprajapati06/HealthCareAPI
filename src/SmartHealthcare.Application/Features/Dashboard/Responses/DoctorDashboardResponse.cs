namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public sealed class DoctorDashboardResponse
    {
        public int TodayAppointments { get; init; }

        public int PendingAppointments { get; init; }

        public int CompletedAppointments { get; init; }

        public int AvailableSlots { get; init; }

        public int PrescriptionsCreated { get; init; }

        public int PatientsTreated { get; init; }
    }
}