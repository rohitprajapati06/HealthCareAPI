using MediatR;


namespace SmartHealthcare.Application.Features.Doctors.Commands.ApproveDoctors
{
    public record ApproveDoctorCommand(Guid DoctorId) : IRequest<Guid>;
    
    
}
