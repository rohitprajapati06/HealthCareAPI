

namespace SmartHealthcare.Application.DTOs.Auth
{
    public class RegisterPatientCommand
    {
        public string FirstName { get; set; }

        public string LastName { get; set; } 
        
        public string Email { get; set; }   
        
        public string Password { get; set; }
        
        public DateTime DateofBirth {  get; set; }

        public string Gender { get; set; }

        public string BloodGroup { get; set; }

    }
}
