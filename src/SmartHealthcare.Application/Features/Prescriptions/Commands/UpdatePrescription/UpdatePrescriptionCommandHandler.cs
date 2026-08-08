using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;


namespace SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription
{
    public class UpdatePrescriptionCommandHandler : IRequestHandler<UpdatePrescriptionCommand,Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<UpdatePrescriptionCommandHandler> logger;

        public UpdatePrescriptionCommandHandler(IApplicationDbContext context , ILogger<UpdatePrescriptionCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
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

            logger.LogInformation("Prescription Updated {PrescriptionId}",prescriptions.Id);

            return Unit.Value;

        }
    }
}
