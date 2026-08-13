

namespace SmartHealthcare.Application.Features.Hospitals.Responses
{
    public sealed class HospitalResponse
    {
        public Guid Id { get; init; }

        public string RohiniCode { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Address { get; init; } = string.Empty;

        public string City { get; init; } = string.Empty;

        public string State { get; init; } = string.Empty;

        public string Country { get; init; } = string.Empty;
    }
}
