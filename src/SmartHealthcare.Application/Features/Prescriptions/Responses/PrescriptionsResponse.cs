

namespace SmartHealthcare.Application.Features.Prescriptions.Responses
{
    public sealed class PrescriptionsResponse
    {
        public Guid Id { get; init; }

        public Guid AppointmentId { get; init; }

        public Guid DoctorId { get; init; }

        public string DoctorName { get; init; } = string.Empty;

        public string Medication { get; init; } = string.Empty;

        public string Instructions { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; }
    }
}
