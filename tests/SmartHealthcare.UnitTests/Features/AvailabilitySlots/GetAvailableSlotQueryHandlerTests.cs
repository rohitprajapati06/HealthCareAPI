using FluentAssertions;
using SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetAvailableSlots;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.AvailabilitySlots
{
    public class GetAvailableSlotQueryHandlerTests
    {
        private GetAvailableSlotQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_ReturnsOnlyUnbookedSlotsForDoctorOrderedByDateThenStartTime()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);

            var laterSameDay = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(14, 0), new TimeOnly(14, 30));
            var earlierSameDay = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            var nextDay = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(2)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            var bookedSlot = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(8, 0), new TimeOnly(8, 30), isBooked: true);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.AvailabilitySlots.AddRange(laterSameDay, earlierSameDay, nextDay, bookedSlot);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetAvailableSlotQuery(doctor.Id), CancellationToken.None);

            results.Should().HaveCount(3);
            results.Should().OnlyContain(r => !r.IsBooked);
            results.Select(r => r.Id).Should().ContainInOrder(earlierSameDay.Id, laterSameDay.Id, nextDay.Id);
        }

        [Fact]
        public async Task Handle_DoctorWithNoAvailableSlots_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetAvailableSlotQuery(Guid.NewGuid()), CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
