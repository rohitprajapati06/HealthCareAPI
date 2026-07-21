

namespace SmartHealthcare.Application.Features.Prescriptions.Responses
{
    public class PrescriptionsResponses
    {
        public Guid Id { get; set; }

        public Guid AppointmentId { get; set; }

        public Guid DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string Medication { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
