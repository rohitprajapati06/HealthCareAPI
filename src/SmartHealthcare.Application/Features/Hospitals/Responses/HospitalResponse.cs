

namespace SmartHealthcare.Application.Features.Hospitals.Responses
{
    public class HospitalResponse
    {
        public Guid Id { get; set; }

        public string RohiniCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
    }
}
