using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Doctors.Commands.ApproveDoctors
{
    public class ApproveDoctorCommandHandler : IRequestHandler<ApproveDoctorCommand,Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<ApproveDoctorCommandHandler> logger;

        public ApproveDoctorCommandHandler(IApplicationDbContext context , ILogger<ApproveDoctorCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Guid> Handle(ApproveDoctorCommand request ,CancellationToken cancellationToken)
        {
            var doctor = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId, cancellationToken);

            if(doctor == null)
            {
                throw new NotFoundException("Doctor Not Found");
            }

            if (doctor.ApprovalStatus == DoctorApprovalStatus.Approved)
            {
                throw new ConflictException("Doctor is already approved.");
            }

            doctor.ApprovalStatus = DoctorApprovalStatus.Approved;

             await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Doctor {DoctorId} approved successfully.",
                doctor.Id);

            return doctor.Id;
        }
    }
}
