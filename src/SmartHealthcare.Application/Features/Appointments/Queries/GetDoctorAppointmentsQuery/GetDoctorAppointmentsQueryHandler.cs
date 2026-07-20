

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetDoctorAppointmentsQuery
{
    public class GetDoctorAppointmentsQueryHandler : IRequestHandler<GetDoctorAppointmentsQuery,List<AppointmentResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetDoctorAppointmentsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<AppointmentResponse>> Handle(GetDoctorAppointmentsQuery request , CancellationToken cancellationToken)
        {
            return await context.Appointments
                .Include(x => x.Doctor).ThenInclude(u => u.User)
                .Include(x => x.Patient).ThenInclude(u => u.User)
                .Include(x => x.Hospital)
                .Include(x => x.AvailabilitySlot)
                .Where(x => x.DoctorId == request.DoctorId)
                .Select(x => new AppointmentResponse
                {
                    Id = x.DoctorId,
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
