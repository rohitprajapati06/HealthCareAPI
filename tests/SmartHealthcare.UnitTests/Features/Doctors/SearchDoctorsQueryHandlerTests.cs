using FluentAssertions;
using SmartHealthcare.Application.Features.Doctors.Queries.SearchDoctors;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Doctors
{
    public class SearchDoctorsQueryHandlerTests
    {
        private SearchDoctorsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        private static SearchDoctorsQuery EmptyQuery() => new(null, null, null, null, null);

        private static (Hospital HospitalA, Hospital HospitalB, DoctorProfile Cardiologist, DoctorProfile Dermatologist, DoctorProfile Pending) SeedSearchableDoctors(
            TestApplicationDbContext context)
        {
            var hospitalA = AppointmentTestData.CreateHospital("Hospital A");
            var hospitalB = AppointmentTestData.CreateHospital("Hospital B");

            var cardiologist = AppointmentTestData.CreateDoctor(hospitalA.Id, DoctorApprovalStatus.Approved, "Zara", "Ali");
            cardiologist.Specialization = "Cardiology";
            cardiologist.ExperienceYears = 10;
            cardiologist.ConsultationFee = 2000m;

            var dermatologist = AppointmentTestData.CreateDoctor(hospitalB.Id, DoctorApprovalStatus.Approved, "Aman", "Bose");
            dermatologist.Specialization = "Dermatology";
            dermatologist.ExperienceYears = 3;
            dermatologist.ConsultationFee = 800m;

            var pending = AppointmentTestData.CreateDoctor(hospitalA.Id, DoctorApprovalStatus.Pending, "Kiran", "Desai");
            pending.Specialization = "Cardiology";

            context.Hospitals.AddRange(hospitalA, hospitalB);
            context.DoctorProfiles.AddRange(cardiologist, dermatologist, pending);
            context.SaveChanges();

            return (hospitalA, hospitalB, cardiologist, dermatologist, pending);
        }

        [Fact]
        public async Task Handle_NoFilters_ReturnsOnlyApprovedDoctorsOrderedByFirstName()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedSearchableDoctors(context);

            var handler = CreateHandler(context);

            var results = await handler.Handle(EmptyQuery(), CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.Id).Should().NotContain(seeded.Pending.Id);
            results.Select(r => r.FirstName).Should().ContainInOrder("Aman", "Zara"); // alphabetical
        }

        [Fact]
        public async Task Handle_FilterByHospitalId_ReturnsOnlyDoctorsAtThatHospital()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedSearchableDoctors(context);

            var handler = CreateHandler(context);
            var query = EmptyQuery() with { HospitalId = seeded.HospitalB.Id };

            var results = await handler.Handle(query, CancellationToken.None);

            results.Should().ContainSingle(r => r.Id == seeded.Dermatologist.Id);
        }

        [Fact]
        public async Task Handle_FilterBySpecialization_ReturnsMatchingDoctorsOnly()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedSearchableDoctors(context);

            var handler = CreateHandler(context);
            var query = EmptyQuery() with { Specialization = "Cardiology" };

            var results = await handler.Handle(query, CancellationToken.None);

            results.Should().ContainSingle(r => r.Id == seeded.Cardiologist.Id);
        }

        [Fact]
        public async Task Handle_FilterByMinimumExperience_ExcludesLessExperiencedDoctors()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedSearchableDoctors(context);

            var handler = CreateHandler(context);
            var query = EmptyQuery() with { Experience = 5 };

            var results = await handler.Handle(query, CancellationToken.None);

            results.Should().ContainSingle(r => r.Id == seeded.Cardiologist.Id);
        }

        [Fact]
        public async Task Handle_FilterByFeeRange_ReturnsDoctorsWithinRange()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedSearchableDoctors(context);

            var handler = CreateHandler(context);
            var query = EmptyQuery() with { MinFee = 500m, MaxFee = 1000m };

            var results = await handler.Handle(query, CancellationToken.None);

            results.Should().ContainSingle(r => r.Id == seeded.Dermatologist.Id);
        }

        [Fact]
        public async Task Handle_CombinedFiltersMatchingNoOne_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            SeedSearchableDoctors(context);

            var handler = CreateHandler(context);
            var query = EmptyQuery() with { Specialization = "Cardiology", MaxFee = 1000m }; // cardiologist costs 2000

            var results = await handler.Handle(query, CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
