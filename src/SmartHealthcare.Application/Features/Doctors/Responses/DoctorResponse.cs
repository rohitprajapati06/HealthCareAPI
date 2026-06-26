

namespace SmartHealthcare.Application.Features.Doctors.Responses
{
    public class DoctorResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }

        public decimal ConsultationFee { get; set; }

        public string Qualification { get; set; } = string.Empty;

        public string HospitalName { get; set; } = string.Empty;
    }
}
