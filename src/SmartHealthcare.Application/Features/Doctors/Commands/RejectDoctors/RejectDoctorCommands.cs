

using MediatR;

namespace SmartHealthcare.Application.Features.Doctors.Commands.RejectDoctors
{
    public record RejectDoctorCommands(Guid DoctorId) : IRequest<Guid>;
   
}
