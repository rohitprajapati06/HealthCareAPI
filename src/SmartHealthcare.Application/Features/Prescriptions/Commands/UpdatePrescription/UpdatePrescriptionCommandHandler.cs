using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;


namespace SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription
{
    public class UpdatePrescriptionCommandHandler : IRequestHandler<UpdatePrescriptionCommand, Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<UpdatePrescriptionCommandHandler> logger;
        private readonly ICurrentUserService currentUserService;

        public UpdatePrescriptionCommandHandler(IApplicationDbContext context, ILogger<UpdatePrescriptionCommandHandler> logger, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.logger = logger;
            this.currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(UpdatePrescriptionCommand request, CancellationToken cancellationToken)
        {
            var prescriptions = await context.Prescriptions.FirstOrDefaultAsync(x => x.Id == request.PrescriptionId, cancellationToken);

            if (prescriptions == null)
            {
                throw new NotFoundException("Prescription not found");
            }

            // Controller already restricts this to Doctor/HospitalAdmin/SuperAdmin.
            // A Doctor caller may only update a prescription they wrote.
            if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var callingDoctor = await context.DoctorProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == prescriptions.DoctorId, cancellationToken);

                if (callingDoctor == null || callingDoctor.UserId != currentUserService.UserId)
                {
                    throw new ForbiddenException("You can only update prescriptions you created.");
                }
            }

            prescriptions.Instructions = request.Instruction;
            prescriptions.Medication = request.Medication;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Prescription Updated {PrescriptionId}", prescriptions.Id);

            return Unit.Value;

        }
    }
}