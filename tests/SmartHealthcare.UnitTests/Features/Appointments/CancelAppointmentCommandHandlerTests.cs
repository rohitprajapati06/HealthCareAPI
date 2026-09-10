using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Appointments.Commands.CancelAppointment;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class CancelAppointmentCommandHandlerTests
    {
        private readonly Mock<ILogger<CancelAppointmentCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private CancelAppointmentCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, AvailabilitySlot Slot, Appointment Appointment) SeedPendingAppointment(
            TestApplicationDbContext context, AppointmentStatus status = AppointmentStatus.Pending)
        {
            var graph = AppointmentTestData.SeedBookableGraph(context);
            var appointment = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, graph.Slot.Id,
                graph.Slot.Date.ToDateTime(graph.Slot.StartTime), status);

            graph.Slot.IsBooked = true;
            context.Appointments.Add(appointment);
            context.SaveChanges();

            return (graph.Hospital, graph.Doctor, graph.Patient, graph.Slot, appointment);
        }

        [Fact]
        public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CancelAppointmentCommand { AppointmentId = Guid.NewGuid() }, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found");
        }

        [Fact]
        public async Task Handle_NonPatientNonDoctorRole_CancelsAppointmentAndFreesSlot()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPendingAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            await handler.Handle(new CancelAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            var appointment = context.Appointments.Single(a => a.Id == seeded.Appointment.Id);
            appointment.Status.Should().Be(AppointmentStatus.Cancelled);

            var slot = context.AvailabilitySlots.Single(s => s.Id == seeded.Slot.Id);
            slot.IsBooked.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_PatientRoleCancellingOwnAppointment_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPendingAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Patient.UserId);

            var handler = CreateHandler(context);

            await handler.Handle(new CancelAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            context.Appointments.Single(a => a.Id == seeded.Appointment.Id).Status.Should().Be(AppointmentStatus.Cancelled);
        }

        [Fact]
        public async Task Handle_PatientRoleCancellingAnotherPatientsAppointment_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPendingAppointment(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CancelAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to cancel this appointment.");

            context.Appointments.Single(a => a.Id == seeded.Appointment.Id).Status.Should().Be(AppointmentStatus.Pending);
        }

        [Fact]
        public async Task Handle_DoctorRoleCancellingOwnAppointment_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPendingAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            await handler.Handle(new CancelAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            context.Appointments.Single(a => a.Id == seeded.Appointment.Id).Status.Should().Be(AppointmentStatus.Cancelled);
        }

        [Fact]
        public async Task Handle_DoctorRoleCancellingAnotherDoctorsAppointment_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPendingAppointment(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(seeded.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherDoctor.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CancelAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to cancel this appointment.");
        }

        [Fact]
        public async Task Handle_AlreadyCancelled_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPendingAppointment(context, AppointmentStatus.Cancelled);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CancelAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Appointment is already cancelled");
        }

        [Fact]
        public async Task Handle_CompletedAppointment_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPendingAppointment(context, AppointmentStatus.Completed);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CancelAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Completed appointments cannot be cancelled.");
        }

        [Fact]
        public async Task Handle_AvailabilitySlotMissing_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            // Appointment points at a slot id that doesn't exist in AvailabilitySlots.
            var appointment = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, Guid.NewGuid(),
                graph.Slot.Date.ToDateTime(graph.Slot.StartTime));
            context.Appointments.Add(appointment);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CancelAppointmentCommand { AppointmentId = appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Availability slot not found.");
        }
    }
}
