
using MediatR;

namespace SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription
{
    public class CreatePrescriptionCommand : IRequest<Guid>
    {
        public Guid AppointmentId { get; set; }

        public Guid DoctorId { get; set; }

        public string Medication { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;
    }
}
