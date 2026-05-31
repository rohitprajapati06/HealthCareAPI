

using MediatR;

namespace SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient
{
    public class RegisterPatientCommand : IRequest<Guid>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string PhoneNumber {  get; set; } = string.Empty;

        public DateTime DateofBirth {  get; set; }

        public string Gender { get; set; } = string.Empty;

        public string BloodGroup { get; set; } = string.Empty;

        public string Allergies {  get; set; } = string.Empty;

        public string ExistingConditions {  get; set; }  = string.Empty;
    }
}
