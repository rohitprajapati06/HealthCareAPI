using FluentAssertions;
using SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitals;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Hospitals
{
    public class GetHospitalsQueryHandlerTests
    {
        private GetHospitalsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_ReturnsOnlyActiveHospitalsMappedCorrectly()
        {
            using var context = TestDbContextFactory.Create();
            var activeHospital = AppointmentTestData.CreateHospital("City Hospital");
            var inactiveHospital = AppointmentTestData.CreateHospital("Closed Hospital");
            inactiveHospital.IsActive = false;

            context.Hospitals.AddRange(activeHospital, inactiveHospital);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetHospitalsQuery(), CancellationToken.None);

            results.Should().ContainSingle();
            var response = results.Single();
            response.Id.Should().Be(activeHospital.Id);
            response.Name.Should().Be("City Hospital");
            response.RohiniCode.Should().Be(activeHospital.RohiniCode);
            response.City.Should().Be(activeHospital.City);
        }

        [Fact]
        public async Task Handle_NoActiveHospitals_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetHospitalsQuery(), CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
