

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Doctors.Commands.RejectDoctors
{
    public class RejectDoctorCommandsHandler : IRequestHandler<RejectDoctorCommands,Guid>
    {
        private readonly IApplicationDbContext context;

        public RejectDoctorCommandsHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Guid> Handle (RejectDoctorCommands request , CancellationToken cancellationToken)
        {
            var doctors = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId);

            if(doctors == null)
            {
                throw new NotFoundException("Doctor Not Found");
            }

            doctors.ApprovalStatus = DoctorApprovalStatus.Rejected;

            await context.SaveChangesAsync(cancellationToken);

            return doctors.Id;
        }
    }
}
