using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;


namespace SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription
{
    public class UpdatePrescriptionCommandHandler : IRequestHandler<UpdatePrescriptionCommand,Unit>
    {
        private readonly IApplicationDbContext context;

        public UpdatePrescriptionCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Unit> Handle (UpdatePrescriptionCommand request , CancellationToken cancellationToken)
        {
            var prescriptions = await context.Prescriptions.FirstOrDefaultAsync(x => x.Id == request.PrescriptionId,cancellationToken);

            if(prescriptions == null)
            {
                throw new NotFoundException("Prescription not found");
            }

            prescriptions.Instructions = request.Instruction;
            prescriptions.Medication = request.Medication;

            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;

        }
    }
}
