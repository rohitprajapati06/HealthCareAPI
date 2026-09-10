using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Appointments.Commands.CompleteAppointment;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class CompleteAppointmentCommandHandlerTests
    {
        private readonly Mock<ILogger<CompleteAppointmentCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private CompleteAppointmentCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, AvailabilitySlot Slot, Appointment Appointment) SeedAppointment(
            TestApplicationDbContext context, DateTime appointmentDate, AppointmentStatus status = AppointmentStatus.Pending)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var patient = AppointmentTestData.CreatePatient();
            var slot = AppointmentTestData.CreateSlot(
                doctor.Id, DateOnly.FromDateTime(appointmentDate), TimeOnly.FromDateTime(appointmentDate), TimeOnly.FromDateTime(appointmentDate).AddMinutes(30), isBooked: true);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.Add(patient);
            context.AvailabilitySlots.Add(slot);

            var appointment = AppointmentTestData.CreateAppointment(doctor.Id, patient.Id, hospital.Id, slot.Id, appointmentDate, status);
            context.Appointments.Add(appointment);
            context.SaveChanges();

            return (hospital, doctor, patient, slot, appointment);
        }

        [Fact]
        public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CompleteAppointmentCommand { AppointmentId = Guid.NewGuid() }, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found");
        }

        [Fact]
        public async Task Handle_DoctorRoleCompletingOwnPastAppointment_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context, DateTime.Now.AddHours(-1));

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            await handler.Handle(new CompleteAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            context.Appointments.Single(a => a.Id == seeded.Appointment.Id).Status.Should().Be(AppointmentStatus.Completed);
        }

        [Fact]
        public async Task Handle_DoctorRoleCompletingAnotherDoctorsAppointment_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context, DateTime.Now.AddHours(-1));

            var otherDoctor = AppointmentTestData.CreateDoctor(seeded.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherDoctor.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CompleteAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to complete this appointment.");
        }

        [Fact]
        public async Task Handle_NonDoctorRole_BypassesOwnershipCheckAndSucceeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context, DateTime.Now.AddHours(-1));

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            await handler.Handle(new CompleteAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            context.Appointments.Single(a => a.Id == seeded.Appointment.Id).Status.Should().Be(AppointmentStatus.Completed);
        }

        [Fact]
        public async Task Handle_AlreadyCancelled_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context, DateTime.Now.AddHours(-1), AppointmentStatus.Cancelled);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CompleteAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Appointment is already cancelled");
        }

        [Fact]
        public async Task Handle_AlreadyCompleted_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context, DateTime.Now.AddHours(-1), AppointmentStatus.Completed);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CompleteAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Appointment is already completed");
        }

        [Fact]
        public async Task Handle_FutureAppointment_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context, DateTime.Now.AddDays(1));

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new CompleteAppointmentCommand { AppointmentId = seeded.Appointment.Id }, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Future appointments cannot be completed.");
        }
    }
}
