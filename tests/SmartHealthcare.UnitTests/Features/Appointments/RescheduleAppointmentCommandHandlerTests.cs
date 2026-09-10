using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Appointments.Commands.RescheduleAppointment;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class RescheduleAppointmentCommandHandlerTests
    {
        private readonly Mock<ILogger<RescheduleAppointmentCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private RescheduleAppointmentCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private sealed record Graph(
            Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient,
            AvailabilitySlot OldSlot, AvailabilitySlot NewSlot, Appointment Appointment);

        private static Graph SeedReschedulableGraph(TestApplicationDbContext context, AppointmentStatus status = AppointmentStatus.Pending)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var patient = AppointmentTestData.CreatePatient();

            var oldSlot = AppointmentTestData.CreateSlot(
                doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(10, 30), isBooked: true);

            var newSlot = AppointmentTestData.CreateSlot(
                doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(2)), new TimeOnly(11, 0), new TimeOnly(11, 30), isBooked: false);

            var appointment = AppointmentTestData.CreateAppointment(
                doctor.Id, patient.Id, hospital.Id, oldSlot.Id, oldSlot.Date.ToDateTime(oldSlot.StartTime), status);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.Add(patient);
            context.AvailabilitySlots.AddRange(oldSlot, newSlot);
            context.Appointments.Add(appointment);
            context.SaveChanges();

            return new Graph(hospital, doctor, patient, oldSlot, newSlot, appointment);
        }

        private static RescheduleAppointmentCommand CommandFor(Graph graph)
            => new() { AppointmentId = graph.Appointment.Id, AvailabilitySlotId = graph.NewSlot.Id };

        [Fact]
        public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var command = new RescheduleAppointmentCommand { AppointmentId = Guid.NewGuid(), AvailabilitySlotId = Guid.NewGuid() };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found");
        }

        [Fact]
        public async Task Handle_PatientRoleReschedulingOwnAppointment_FreesOldSlotBooksNewSlotAndUpdatesAppointment()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(graph.Patient.UserId);

            var handler = CreateHandler(context);

            await handler.Handle(CommandFor(graph), CancellationToken.None);

            var appointment = context.Appointments.Single(a => a.Id == graph.Appointment.Id);
            appointment.AvailabilitySlotId.Should().Be(graph.NewSlot.Id);
            appointment.AppointmentDate.Should().Be(graph.NewSlot.Date.ToDateTime(graph.NewSlot.StartTime));

            context.AvailabilitySlots.Single(s => s.Id == graph.OldSlot.Id).IsBooked.Should().BeFalse();
            context.AvailabilitySlots.Single(s => s.Id == graph.NewSlot.Id).IsBooked.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PatientRoleReschedulingAnotherPatientsAppointment_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to reschedule this appointment.");
        }

        [Fact]
        public async Task Handle_DoctorRoleReschedulingAnotherDoctorsAppointment_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(graph.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherDoctor.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to reschedule this appointment.");
        }

        [Theory]
        [InlineData(AppointmentStatus.Completed)]
        [InlineData(AppointmentStatus.Cancelled)]
        public async Task Handle_NonPendingAppointment_ThrowsBadRequestExceptionWithGenericMessage(AppointmentStatus status)
        {
            // NOTE: the handler checks `status != Pending` first and throws a generic message,
            // so the more specific "has been compeleted"/"has been cancelled" checks further
            // down are unreachable dead code. This test documents the message that actually
            // comes back for both statuses.
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context, status);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Only pending appointments can be rescheduled.");
        }

        [Fact]
        public async Task Handle_NewSlotNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = new RescheduleAppointmentCommand { AppointmentId = graph.Appointment.Id, AvailabilitySlotId = Guid.NewGuid() };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Slot not found");
        }

        [Fact]
        public async Task Handle_NewSlotAlreadyBooked_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);
            graph.NewSlot.IsBooked = true;
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Slot is already book");
        }

        [Fact]
        public async Task Handle_NewSlotBelongsToAnotherDoctor_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(graph.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            var foreignSlot = AppointmentTestData.CreateSlot(
                otherDoctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(2)), new TimeOnly(11, 0), new TimeOnly(11, 30));
            context.DoctorProfiles.Add(otherDoctor);
            context.AvailabilitySlots.Add(foreignSlot);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = new RescheduleAppointmentCommand { AppointmentId = graph.Appointment.Id, AvailabilitySlotId = foreignSlot.Id };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("The slot does not belong to the doctor");
        }

        [Fact]
        public async Task Handle_NewSlotInThePast_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            var pastSlot = AppointmentTestData.CreateSlot(
                graph.Doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            context.AvailabilitySlots.Add(pastSlot);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = new RescheduleAppointmentCommand { AppointmentId = graph.Appointment.Id, AvailabilitySlotId = pastSlot.Id };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Cannot reschedule to a past slot.");
        }

        [Fact]
        public async Task Handle_PatientBusyAtNewSlotTime_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            context.Appointments.Add(AppointmentTestData.CreateAppointment(
                Guid.NewGuid(), graph.Patient.Id, graph.Hospital.Id, Guid.NewGuid(),
                graph.NewSlot.Date.ToDateTime(graph.NewSlot.StartTime)));
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Patient already has another appointment at this time.");
        }

        [Fact]
        public async Task Handle_DoctorBusyAtNewSlotTime_ThrowsConflictException()
        {
            // Unlike BookAppointmentCommandHandler, this doctorBusy check is actually enforced.
            using var context = TestDbContextFactory.Create();
            var graph = SeedReschedulableGraph(context);

            context.Appointments.Add(AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, Guid.NewGuid(), graph.Hospital.Id, Guid.NewGuid(),
                graph.NewSlot.Date.ToDateTime(graph.NewSlot.StartTime)));
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Doctor already has another appointment at this time.");
        }
    }
}
