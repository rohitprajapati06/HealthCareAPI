using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Appointments.Queries.GetPatientAppointments;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class GetPatientAppointmentsQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetPatientAppointmentsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, AvailabilitySlot Slot) SeedTwoAppointmentsForPatient(
            TestApplicationDbContext context)
        {
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

            return (graph.Hospital, graph.Doctor, graph.Patient, graph.Slot);
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingOwnAppointments_ReturnsOrderedByDateDescending()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedTwoAppointmentsForPatient(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Patient.UserId);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientAppointmentsQuery(seeded.Patient.Id), CancellationToken.None);

            results.Should().HaveCount(2);
            results.Should().BeInDescendingOrder(r => r.AppointmentDate);
            results.Should().OnlyContain(r => r.PatientId == seeded.Patient.Id);
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingAnotherPatientsAppointments_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedTwoAppointmentsForPatient(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPatientAppointmentsQuery(seeded.Patient.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view these appointments.");
        }

        [Fact]
        public async Task Handle_NonPatientRole_BypassesOwnershipCheckAndReturnsAppointments()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedTwoAppointmentsForPatient(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientAppointmentsQuery(seeded.Patient.Id), CancellationToken.None);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_PatientWithNoAppointments_ReturnsEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientAppointmentsQuery(Guid.NewGuid()), CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
