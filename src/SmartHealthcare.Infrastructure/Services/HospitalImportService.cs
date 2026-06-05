using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Contracts.Services;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Infrastructure.Services
{
    public class HospitalImportService : IHospitalImportService
    {
        private readonly IApplicationDbContext context;

        public HospitalImportService(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<int> ImportHospitalsAsync(string filepath)
        {

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException(
                    $"File not found: {filepath}");
            }

            using var workbook = new XLWorkbook(filepath);

            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RowsUsed().Skip(1);

            var existingCodes = await context.Hospitals.Select(x => x.RohiniCode).ToHashSetAsync();

            var hospitaltoinsert = new List<Hospital>(); 

            foreach (var row in rows) { 
            
                    var RohiniCode = row.Cell(2).GetValue<string>().Trim();

                if (string.IsNullOrWhiteSpace(RohiniCode)) {  continue; }

                if (existingCodes.Contains(RohiniCode)) {  continue; }

                var hospital = new Hospital
                {
                    RohiniCode = RohiniCode,
                    Name = row.Cell(3).GetValue<string>().Trim(),
                    City = row.Cell(4).GetValue<string>().Trim(),
                    Address = row.Cell(5).GetValue<string>().Trim(),
                    State = "Maharashtra",
                    Country = "India",
                    IsActive = true,
                };

                hospitaltoinsert.Add(hospital);

                existingCodes.Add(RohiniCode);

            }

            if (hospitaltoinsert.Count > 0)
            {
                await context.Hospitals.AddRangeAsync(hospitaltoinsert);
                await context.SaveChangesAsync();
            }

            return hospitaltoinsert.Count;

        }
    }
}
