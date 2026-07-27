
namespace SmartHealthcare.Infrastructure.Options
{
    public class FileStorageOptions
    {
        public const string SectionName = "FileStorage";

        public string RootFolder { get; set; } = "uploads";

        public int MaxFileSizeinMb { get; set; } = 10;

        public List<string> AllowedExtensions { get; set; } = new() ;
        


    }
}
