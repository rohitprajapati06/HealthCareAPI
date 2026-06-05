using System;
using Microsoft.AspNetCore.Identity;

namespace SmartHealthcare.Domain.Entities
{
    public class ApplicationUser:IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public Guid? HospitalId { get; set;}

        public Hospital Hospital { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public DoctorProfile? DoctorProfile { get; set; }

        public PatientProfile? PatientProfile { get; set; }
    }
}
