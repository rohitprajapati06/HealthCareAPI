using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetPatientAppointments
{
    public class GetPatientAppointmentsQueryHandler : IRequestHandler<GetPatientAppointmentsQuery,List<AppointmentResponse>>
    {
        private readonly IApplicationDbContext dbContext;

        public GetPatientAppointmentsQueryHandler(IApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<AppointmentResponse>> Handle(GetPatientAppointmentsQuery request , CancellationToken cancellationToken)
        {
            return await dbContext.Appointments
                .Include(x => x.Doctor).ThenInclude(u => u.User)
                .Include(x => x.Patient).ThenInclude(u => u.User)
                .Include(x => x.Hospital)
                .Include(x => x.AvailabilitySlot)
                .Where(x => x.PatientId == request.PatientId)
                .Select(x => new AppointmentResponse
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor.User.FirstName +" "+ x.Doctor.User.FirstName,
                    PatientId = x.PatientId,
                    PatientName = x.Patient.User.FirstName +" "+ x.Patient.User.LastName,
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
