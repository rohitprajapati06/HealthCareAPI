using FluentAssertions;
using SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetDoctorSlots;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.AvailabilitySlots
{
    public class GetDoctorSlotsQueryHandlerTests
    {
        private GetDoctorSlotsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_ReturnsOnlyTodaysSlotsForTheDoctor_RegardlessOfBookedStatus()
        {
            // NOTE: despite the name "GetDoctorSlots", this handler filters to Date ==
            // DateOnly.FromDateTime(DateTime.Today) - it only ever returns *today's* slots
            // for the doctor, booked or not. Slots on any other date (past or future) are
            // excluded. This test documents that actual scoping; worth double-checking this
            // is what the "doctor's slots" screen is actually supposed to show, since it
            // looks like it might be intended to return the doctor's full slot list.
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todaySlotBooked = AppointmentTestData.CreateSlot(doctor.Id, today, new TimeOnly(9, 0), new TimeOnly(9, 30), isBooked: true);
            var todaySlotFree = AppointmentTestData.CreateSlot(doctor.Id, today, new TimeOnly(14, 0), new TimeOnly(14, 30));
            var tomorrowSlot = AppointmentTestData.CreateSlot(doctor.Id, today.AddDays(1), new TimeOnly(9, 0), new TimeOnly(9, 30));
            var yesterdaySlot = AppointmentTestData.CreateSlot(doctor.Id, today.AddDays(-1), new TimeOnly(9, 0), new TimeOnly(9, 30));

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.AvailabilitySlots.AddRange(todaySlotBooked, todaySlotFree, tomorrowSlot, yesterdaySlot);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorSlotsQuery(doctor.Id), CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.Id).Should().BeEquivalentTo(new[] { todaySlotBooked.Id, todaySlotFree.Id });
        }

        [Fact]
        public async Task Handle_DoctorWithNoSlotsToday_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var tomorrowSlot = AppointmentTestData.CreateSlot(
                doctor.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)), new TimeOnly(9, 0), new TimeOnly(9, 30));

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.AvailabilitySlots.Add(tomorrowSlot);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorSlotsQuery(doctor.Id), CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
