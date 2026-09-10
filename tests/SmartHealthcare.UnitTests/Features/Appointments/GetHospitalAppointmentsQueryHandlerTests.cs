using FluentAssertions;
using SmartHealthcare.Application.Features.Appointments.Queries.GetHospitalAppointmentsQuery;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class GetHospitalAppointmentsQueryHandlerTests
    {
        private GetHospitalAppointmentsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_HospitalWithAppointments_ReturnsThemOrderedByDateDescending()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            var earlier = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, graph.Slot.Id, DateTime.Now.AddDays(1));

            var laterSlot = AppointmentTestData.CreateSlot(
                graph.Doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(5)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            context.AvailabilitySlots.Add(laterSlot);

            var later = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, laterSlot.Id, DateTime.Now.AddDays(5));

            context.Appointments.AddRange(earlier, later);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetHospitalAppointmentsQuery(graph.Hospital.Id), CancellationToken.None);

            results.Should().HaveCount(2);
            results.Should().BeInDescendingOrder(r => r.AppointmentDate);
            results.Should().OnlyContain(r => r.HospitalId == graph.Hospital.Id);
            results.Should().Contain(r => r.HospitalName == graph.Hospital.Name);
        }

        [Fact]
        public async Task Handle_HospitalWithNoAppointments_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetHospitalAppointmentsQuery(Guid.NewGuid()), CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
