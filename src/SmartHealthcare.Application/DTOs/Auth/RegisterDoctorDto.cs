

namespace SmartHealthcare.Application.DTOs.Auth
{
    public class RegisterDoctorDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }    

        public string Email { get; set; }   

        public string Password { get; set; }    

        public string Specailization { get; set; }

        public int ExperienceYears { get; set; }

        public decimal ConsultantionFee { get; set; }

        public string Qualification { get; set; }

        public Guid HospitalId { get; set; }

    }
}
