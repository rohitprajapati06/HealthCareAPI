namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public class DoctorDashboardResponse
    {
        public int TodayAppointments { get; set; }

        public int PendingAppointments { get; set; }

        public int CompletedAppointments { get; set; }

        public int AvailableSlots { get; set; }

        public int PrescriptionsCreated { get; set; }

        public int PatientsTreated { get; set; }
    }
}