using SmartHealthcare.Domain.Common;

namespace SmartHealthcare.Domain.Entities;

public class Hospital : BaseEntity
{
    public string RohiniCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ApplicationUser> Users { get; set; }

    public ICollection<DoctorProfile> Doctors { get; set; }

    public ICollection<Appointment> Appointments { get; set; }
}