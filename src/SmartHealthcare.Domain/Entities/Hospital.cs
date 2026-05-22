using SmartHealthcare.Domain.Common;

namespace SmartHealthcare.Domain.Entities;

public class Hospital : BaseEntity
{
    public string Name { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string Country { get; set; }

    public string ContactEmail { get; set; }

    public string ContactPhone { get; set; }

    public ICollection<ApplicationUser> Users { get; set; }

    public ICollection<DoctorProfile> Doctors { get; set; }

    public ICollection<Appointment> Appointments { get; set; }
}