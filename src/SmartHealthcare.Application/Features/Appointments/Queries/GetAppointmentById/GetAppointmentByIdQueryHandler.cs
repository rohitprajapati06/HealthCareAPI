

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

        public GetAppointmentByIdQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
            
        }


        public async Task<AppointmentResponse> Handle(GetAppointmentByIdQuery request , CancellationToken cancellationToken)
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

            if(appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            return appointment;
        } 
    }
}
