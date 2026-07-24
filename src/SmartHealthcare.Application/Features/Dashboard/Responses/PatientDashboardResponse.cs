

namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public class PatientDashboardResponse
    {
        public int upcomingAppointment {  get; set; }

        public int completedAppointment {  get; set; }

        public int cancelledAppointment { get; set; }

        public int Prescriptions { get; set; }

        public int medicalRecords { get; set; }

    }
}
