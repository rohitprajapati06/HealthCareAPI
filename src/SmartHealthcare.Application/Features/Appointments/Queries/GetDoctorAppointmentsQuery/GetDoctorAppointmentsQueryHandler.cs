using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Appointments.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetDoctorAppointmentsQuery
{
    public class GetDoctorAppointmentsQueryHandler : IRequestHandler<GetDoctorAppointmentsQuery, List<AppointmentResponse>>
    {
        private readonly IApplicationDbContext context;
        private readonly ICurrentUserService currentUserService;

        public GetDoctorAppointmentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }

        public async Task<List<AppointmentResponse>> Handle(GetDoctorAppointmentsQuery request, CancellationToken cancellationToken)
        {
            
            if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var ownDoctorProfileId = await context.DoctorProfiles
                    .AsNoTracking()
                    .Where(d => d.UserId == currentUserService.UserId)
                    .Select(d => (Guid?)d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownDoctorProfileId == null || ownDoctorProfileId != request.DoctorId)
                {
                    throw new ForbiddenException("You are not allowed to view these appointments.");
                }
            }

            return await context.Appointments
                .AsNoTracking()
                .Where(x => x.DoctorId == request.DoctorId)
                .OrderByDescending(x => x.AppointmentDate)
                .Select(x => new AppointmentResponse
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor.User.FirstName + " " + x.Doctor.User.LastName,
                    PatientId = x.PatientId,
                    PatientName = x.Patient.User.FirstName + " " + x.Patient.User.LastName,
                    HospitalId = x.HospitalId,
                    HospitalName = x.Hospital.Name,
                    AppointmentDate = x.AppointmentDate,
                    AvailabilitySlotId = x.AvailabilitySlotId,
                    Status = x.Status.ToString(),
                    Notes = x.Notes,
                }).ToListAsync(cancellationToken);
        }
    }
}