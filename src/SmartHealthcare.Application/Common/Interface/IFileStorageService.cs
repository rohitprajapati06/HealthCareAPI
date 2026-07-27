using Microsoft.AspNetCore.Http;
using SmartHealthcare.Application.Common.Models;

namespace SmartHealthcare.Application.Common.Interface
{
    public interface IFileStorageService 
    {
        Task<FileUploadResult> UploadAsync(IFormFile File, string folderName, CancellationToken cancellationToken = default);

        Task DeleteAsync(string filePath , CancellationToken cancellationToken = default);
    }
}
