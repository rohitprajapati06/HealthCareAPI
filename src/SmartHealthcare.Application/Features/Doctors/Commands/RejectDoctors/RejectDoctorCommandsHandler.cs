

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Doctors.Commands.RejectDoctors
{
    public class RejectDoctorCommandsHandler : IRequestHandler<RejectDoctorCommands,Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<RejectDoctorCommandsHandler> logger;

        public RejectDoctorCommandsHandler(IApplicationDbContext context , ILogger<RejectDoctorCommandsHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Guid> Handle (RejectDoctorCommands request , CancellationToken cancellationToken)
        {
            var doctor = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId,cancellationToken);

            if(doctor == null)
            {
                throw new NotFoundException("Doctor not found");
            }

            if (doctor.ApprovalStatus == DoctorApprovalStatus.Rejected)
            {
                throw new ConflictException("Doctor is already rejected.");
            }

            doctor.ApprovalStatus = DoctorApprovalStatus.Rejected;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Doctor {DoctorId} rejected successfully.",
                doctor.Id);

            return doctor.Id;
        }
    }
}
