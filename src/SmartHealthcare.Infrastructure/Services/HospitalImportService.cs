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

        public async Task<int> ImportHospitalsAsync(
            Stream fileStream,
            CancellationToken cancellationToken = default)
        {
            using var workbook = new XLWorkbook(fileStream);

            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RowsUsed().Skip(1);

            var existingCodes = await context.Hospitals
                .Select(x => x.RohiniCode)
                .ToHashSetAsync(cancellationToken);

            var hospitalsToInsert = new List<Hospital>();

            foreach (var row in rows)
            {
                var rohiniCode = row.Cell(2)
                    .GetValue<string>()
                    .Trim();

                if (string.IsNullOrWhiteSpace(rohiniCode))
                {
                    continue;
                }

                if (existingCodes.Contains(rohiniCode))
                {
                    continue;
                }

                var hospital = new Hospital
                {
                    RohiniCode = rohiniCode,
                    Name = row.Cell(3).GetValue<string>().Trim(),
                    City = row.Cell(4).GetValue<string>().Trim(),
                    Address = row.Cell(5).GetValue<string>().Trim(),
                    State = "Maharashtra",
                    Country = "India",
                    IsActive = true
                };

                hospitalsToInsert.Add(hospital);
                existingCodes.Add(rohiniCode);
            }

            if (hospitalsToInsert.Count > 0)
            {
                await context.Hospitals.AddRangeAsync(
                    hospitalsToInsert,
                    cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }

            return hospitalsToInsert.Count;
        }
    }
}