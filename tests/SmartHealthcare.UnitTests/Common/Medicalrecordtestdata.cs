using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.UnitTests.Common
{
    /// <summary>
    /// Helpers for building the graph of entities (ApplicationUser -> PatientProfile,
    /// Hospital, MedicalRecord) that the MedicalRecords query handlers project into
    /// MedicalRecordResponse via navigation properties.
    /// </summary>
    public static class MedicalRecordTestData
    {
        public static Hospital CreateHospital(string name = "City Hospital")
            => new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                RohiniCode = "RH-" + Guid.NewGuid().ToString("N")[..6],
                Address = "1 Health St",
                City = "Mumbai",
                State = "MH",
                Country = "India"
            };

        public static PatientProfile CreatePatient(string firstName = "Asha", string lastName = "Verma", Guid? userId = null)
        {
            var resolvedUserId = userId ?? Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = resolvedUserId,
                UserName = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com",
                Email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com",
                FirstName = firstName,
                LastName = lastName
            };

            return new PatientProfile
            {
                Id = Guid.NewGuid(),
                UserId = resolvedUserId,
                User = user,
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                BloodGroup = "B+",
                Allergies = "None",
                ExistingConditions = "None"
            };
        }

        public static MedicalRecord CreateRecord(PatientProfile patient, Hospital hospital, string recordType = "Radiology")
            => new()
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                Patient = patient,
                HospitalId = hospital.Id,
                Hospital = hospital,
                FileName = "scan.pdf",
                FileUrl = "https://storage/scan.pdf",
                RecordType = recordType,
                CreatedAt = new DateTime(2026, 1, 1)
            };

        /// <summary>
        /// Seeds a fully wired patient/hospital/record graph into the context and saves it.
        /// </summary>
        public static MedicalRecord SeedFullRecord(TestApplicationDbContext context)
        {
            var hospital = CreateHospital();
            var patient = CreatePatient();
            var record = CreateRecord(patient, hospital);

            context.Hospitals.Add(hospital);
            context.PatientProfiles.Add(patient);
            context.MedicalRecords.Add(record);
            context.SaveChanges();

            return record;
        }
    }
}