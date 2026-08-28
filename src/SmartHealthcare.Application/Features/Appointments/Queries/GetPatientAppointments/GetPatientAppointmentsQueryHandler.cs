using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Appointments.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetPatientAppointments
{
    public class GetPatientAppointmentsQueryHandler : IRequestHandler<GetPatientAppointmentsQuery, List<AppointmentResponse>>
    {
        private readonly IApplicationDbContext dbContext;
        private readonly ICurrentUserService currentUserService;

        public GetPatientAppointmentsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
        }

        public async Task<List<AppointmentResponse>> Handle(GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
        {
            if (currentUserService.IsInRole(UserRoles.Patient))
            {
                var ownPatientProfileId = await dbContext.PatientProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserService.UserId)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownPatientProfileId == null || ownPatientProfileId != request.PatientId)
                {
                    throw new ForbiddenException("You are not allowed to view these appointments.");
                }
            }

            return await dbContext.Appointments
                .AsNoTracking()
                .Where(x => x.PatientId == request.PatientId)
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
                    Notes = x.Notes,
                    Status = x.Status.ToString()
                }).ToListAsync(cancellationToken);


        }
    }
}