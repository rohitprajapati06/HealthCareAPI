

namespace SmartHealthcare.Application.Features.Doctors.Responses
{
    public sealed class DoctorResponse
    {
        public Guid Id { get; init; }

        public Guid UserId { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string Specialization { get; init; } = string.Empty;

        public int ExperienceYears { get; init; }

        public decimal ConsultationFee { get; init; }

        public string Qualification { get; init; } = string.Empty;

        public string HospitalName { get; init; } = string.Empty;
    }
}
