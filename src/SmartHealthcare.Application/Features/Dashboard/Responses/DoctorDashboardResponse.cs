

namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public class DoctorDashboardResponse
    {
        public int todayAppointments { get; set; }

        public int pendingAppointments { get; set; }

        public int completedAppointment { get; set; }

        public int availableSlots { get; set; }

        public int prescriptionsCreated { get; set; }

        public int PatientTreated { get; set; }

    }
}
