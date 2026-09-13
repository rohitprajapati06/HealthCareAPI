using FluentAssertions;
using SmartHealthcare.Application.Features.Doctors.Queries.GetDoctors;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Doctors
{
    public class GetDoctorsQueryHandlerTests
    {
        private GetDoctorsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_ReturnsOnlyApprovedDoctorsMappedCorrectly()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital("City Hospital");
            var approvedDoctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Approved, "Rohan", "Shah");
            var pendingDoctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Pending, "Neha", "Kapoor");
            var rejectedDoctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Rejected, "Amit", "Rao");

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.AddRange(approvedDoctor, pendingDoctor, rejectedDoctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorsQuery(), CancellationToken.None);

            results.Should().ContainSingle();
            var response = results.Single();
            response.Id.Should().Be(approvedDoctor.Id);
            response.FirstName.Should().Be("Rohan");
            response.LastName.Should().Be("Shah");
            response.Specialization.Should().Be(approvedDoctor.Specialization);
            response.HospitalName.Should().Be("City Hospital");
        }

        [Fact]
        public async Task Handle_NoApprovedDoctors_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorsQuery(), CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
