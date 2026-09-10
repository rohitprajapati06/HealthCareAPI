using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Appointments.Queries.GetDoctorAppointmentsQuery;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class GetDoctorAppointmentsQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetDoctorAppointmentsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, AvailabilitySlot Slot) SeedTwoAppointmentsForDoctor(
            TestApplicationDbContext context)
        {
            var graph = AppointmentTestData.SeedBookableGraph(context);

            var earlier = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, graph.Slot.Id,
                DateTime.Now.AddDays(1));

            var laterSlot = AppointmentTestData.CreateSlot(
                graph.Doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(5)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            context.AvailabilitySlots.Add(laterSlot);

            var later = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, laterSlot.Id,
                DateTime.Now.AddDays(5));

            context.Appointments.AddRange(earlier, later);
            context.SaveChanges();

            return (graph.Hospital, graph.Doctor, graph.Patient, graph.Slot);
        }

        [Fact]
        public async Task Handle_DoctorRoleRequestingOwnAppointments_ReturnsOrderedByDateDescending()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedTwoAppointmentsForDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorAppointmentsQuery(seeded.Doctor.Id), CancellationToken.None);

            results.Should().HaveCount(2);
            results.Should().BeInDescendingOrder(r => r.AppointmentDate);
            results.Should().OnlyContain(r => r.DoctorId == seeded.Doctor.Id);
        }

        [Fact]
        public async Task Handle_DoctorRoleRequestingAnotherDoctorsAppointments_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedTwoAppointmentsForDoctor(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(seeded.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherDoctor.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetDoctorAppointmentsQuery(seeded.Doctor.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view these appointments.");
        }

        [Fact]
        public async Task Handle_NonDoctorRole_BypassesOwnershipCheckAndReturnsAppointments()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedTwoAppointmentsForDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorAppointmentsQuery(seeded.Doctor.Id), CancellationToken.None);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_DoctorWithNoAppointments_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorAppointmentsQuery(Guid.NewGuid()), CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
