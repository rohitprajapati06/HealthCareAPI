
namespace SmartHealthcare.Application.Contracts.Services
{
    public interface IHospitalImportService
    {
        Task<int> ImportHospitalsAsync(string filepath);
    }
}
