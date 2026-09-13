using FluentAssertions;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitalById;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Hospitals
{
    public class GetHospitalByIdQueryHandlerTests
    {
        private GetHospitalByIdQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_ActiveHospitalExists_ReturnsMappedResponse()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital("City Hospital");

            context.Hospitals.Add(hospital);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetHospitalByIdQuery(hospital.Id), CancellationToken.None);

            response.Id.Should().Be(hospital.Id);
            response.Name.Should().Be("City Hospital");
            response.RohiniCode.Should().Be(hospital.RohiniCode);
            response.Address.Should().Be(hospital.Address);
        }

        [Fact]
        public async Task Handle_HospitalNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetHospitalByIdQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Hospital not found.");
        }

        [Fact]
        public async Task Handle_InactiveHospital_ThrowsNotFoundExceptionAsIfItDoesNotExist()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital("Closed Hospital");
            hospital.IsActive = false;

            context.Hospitals.Add(hospital);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetHospitalByIdQuery(hospital.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Hospital not found.");
        }
    }
}
