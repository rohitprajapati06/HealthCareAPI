using MediatR;

namespace SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription
{
    public class UpdatePrescriptionCommand : IRequest<Unit>
    {
        public Guid PrescriptionId {  get; set; }

        public string Medication { get; set; }

        public string Instruction { get; set; }
    }

}
