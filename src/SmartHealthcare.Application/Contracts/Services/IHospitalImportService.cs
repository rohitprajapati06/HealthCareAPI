
namespace SmartHealthcare.Application.Contracts.Services
{
    public interface IHospitalImportService
    {
        Task<int> ImportHospitalsAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default);
    }
}
