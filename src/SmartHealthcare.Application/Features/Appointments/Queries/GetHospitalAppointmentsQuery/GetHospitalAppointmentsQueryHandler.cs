

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Appointments.Queries.GetPatientAppointments;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetHospitalAppointmentsQuery
{
    public class GetHospitalAppointmentsQueryHandler : IRequestHandler<GetHospitalAppointmentsQuery,List<AppointmentResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetHospitalAppointmentsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<AppointmentResponse>> Handle(GetHospitalAppointmentsQuery request, CancellationToken cancellationToken)
        {
            return await context.Appointments
                .AsNoTracking()
                .Where(x => x.HospitalId == request.HospitalId)
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
