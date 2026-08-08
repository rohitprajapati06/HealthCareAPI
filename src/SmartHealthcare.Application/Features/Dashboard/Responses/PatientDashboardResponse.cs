

namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public class PatientDashboardResponse
    {
        public int UpcomingAppointment {  get; set; }

        public int CompletedAppointment {  get; set; }

        public int CancelledAppointment { get; set; }

        public int Prescriptions { get; set; }

        public int MedicalRecords { get; set; }

    }
}
