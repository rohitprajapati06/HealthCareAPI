

namespace SmartHealthcare.Application.Features.Dashboard.Responses
{
    public sealed class PatientDashboardResponse
    {
        public int UpcomingAppointment {  get; init; }

        public int CompletedAppointment {  get; init; }

        public int CancelledAppointment { get; init; }

        public int Prescriptions { get; init; }

        public int MedicalRecords { get; init; }

    }
}
