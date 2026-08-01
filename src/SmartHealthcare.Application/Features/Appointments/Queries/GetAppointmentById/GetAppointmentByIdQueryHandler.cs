

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetAppointmentById
{
    public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery,AppointmentResponse>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger logger;

        public GetAppointmentByIdQueryHandler(IApplicationDbContext context , ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }


        public async Task<AppointmentResponse> Handle(GetAppointmentByIdQuery request , CancellationToken cancellationToken)
        {
            var appointments = await context.Appointments
                .Include(x => x.Doctor).ThenInclude(u => u.User)
                .Include(x => x.Patient).ThenInclude(u => u.User)
                .Include(x => x.Hospital)
                .Include(x => x.AvailabilitySlot)
                .FirstOrDefaultAsync(x => x.Id == request.AppointmentId , cancellationToken);

            if(appointments == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            return new AppointmentResponse
            {
                Id = appointments.Id,
                DoctorId = appointments.DoctorId,
                DoctorName = $"{appointments.Doctor.User.FirstName} {appointments.Doctor.User.LastName}",
                PatientId = appointments.PatientId,
                PatientName = $"{appointments.Patient.User.FirstName} {appointments.Patient.User.LastName}",
                HospitalId = appointments.HospitalId,
                HospitalName = appointments.Hospital.Name,
                AppointmentDate = appointments.AppointmentDate,
                AvailabilitySlotId = appointments.AvailabilitySlotId,
                Notes = appointments.Notes,
                Status = appointments.Status.ToString(),
                
            };
        } 
    }
}
