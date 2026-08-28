using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Appointments.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetAppointmentById
{
    public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentResponse>
    {
        private readonly IApplicationDbContext context;
        private readonly ICurrentUserService currentUserService;

        public GetAppointmentByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }


        public async Task<AppointmentResponse> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
        {
            var appointment = await context.Appointments
                .AsNoTracking()
                 .Where(x => x.Id == request.AppointmentId)
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
                 }).FirstOrDefaultAsync(cancellationToken);

            if (appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            if (currentUserService.IsInRole(UserRoles.Patient))
            {
                var ownPatientProfileId = await context.PatientProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserService.UserId)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownPatientProfileId == null || ownPatientProfileId != appointment.PatientId)
                {
                    throw new ForbiddenException("You are not allowed to view this appointment.");
                }
            }
            else if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var ownDoctorProfileId = await context.DoctorProfiles
                    .AsNoTracking()
                    .Where(d => d.UserId == currentUserService.UserId)
                    .Select(d => (Guid?)d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownDoctorProfileId == null || ownDoctorProfileId != appointment.DoctorId)
                {
                    throw new ForbiddenException("You are not allowed to view this appointment.");
                }
            }

            return appointment;
        }
    }
}