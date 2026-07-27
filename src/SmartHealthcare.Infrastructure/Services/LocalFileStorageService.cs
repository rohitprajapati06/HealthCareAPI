
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Common.Interface;
using SmartHealthcare.Application.Common.Models;
using SmartHealthcare.Infrastructure.Options;

namespace SmartHealthcare.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment environment;
        private readonly FileStorageOptions options;

        public LocalFileStorageService(IWebHostEnvironment environment , IOptions<FileStorageOptions> options)
        {
            this.environment = environment;
            this.options = options.Value;
        }

        public async Task<FileUploadResult> UploadAsync(IFormFile file ,string folderName , CancellationToken cancellationToken = default)
        {
            if(file == null || file.Length == 0)
            {
                throw new BadRequestException("File is required");
            }

            var extension = Path.GetExtension(file.FileName);
            
            if(options.AllowedExtensions.Contains(extension , StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException("File Type is Not Allowed");
            }

            var maxSize = options.MaxFileSizeinMb * 1024 * 1024;

            if(file.Length > maxSize)
            {
                throw new BadRequestException($"Maximum File Size in {options.MaxFileSizeinMb} Mb");
            }

            var folderPath = Path.Combine(environment.WebRootPath, options.RootFolder, folderName);
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);

            await file.CopyToAsync(stream,cancellationToken);

            return new FileUploadResult
            {
                FileName = fileName,
                FileURL = $"/{options.RootFolder}/{folderName}/{fileName}"
            };
        }

        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var physicalPath = Path.Combine(environment.WebRootPath, filePath.TrimStart('/'));


            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }
    }
}
