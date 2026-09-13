using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Prescriptions
{
    public class CreatePrescriptionCommandHandlerTests
    {
        private readonly Mock<ILogger<CreatePrescriptionCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private CreatePrescriptionCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, Appointment Appointment) SeedCompletedAppointment(
            TestApplicationDbContext context, bool withExistingPrescription = false)
        {
            var graph = AppointmentTestData.SeedBookableGraph(context);
            var appointment = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, graph.Slot.Id,
                DateTime.Now.AddHours(-1), AppointmentStatus.Completed);

            context.Appointments.Add(appointment);
            context.SaveChanges();

            if (withExistingPrescription)
            {
                context.Prescriptions.Add(new Prescription
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    DoctorId = graph.Doctor.Id,
                    Medication = "Existing med",
                    Instructions = "Existing instructions",
                    CreatedAt = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            return (graph.Hospital, graph.Doctor, graph.Patient, appointment);
        }

        private static CreatePrescriptionCommand CommandFor(DoctorProfile doctor, Appointment appointment)
            => new()
            {
                AppointmentId = appointment.Id,
                DoctorId = doctor.Id,
                Medication = "Paracetamol 500mg",
                Instructions = "Twice daily after food"
            };

        [Fact]
        public async Task Handle_NonDoctorRole_ValidRequest_CreatesPrescriptionAndReturnsId()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedCompletedAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var prescriptionId = await handler.Handle(CommandFor(seeded.Doctor, seeded.Appointment), CancellationToken.None);

            prescriptionId.Should().NotBeEmpty();
            var prescription = context.Prescriptions.Single(p => p.Id == prescriptionId);
            prescription.AppointmentId.Should().Be(seeded.Appointment.Id);
            prescription.DoctorId.Should().Be(seeded.Doctor.Id);
            prescription.Medication.Should().Be("Paracetamol 500mg");
            prescription.Instructions.Should().Be("Twice daily after food");
        }

        [Fact]
        public async Task Handle_DoctorRoleCreatingForOwnAppointment_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedCompletedAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            var prescriptionId = await handler.Handle(CommandFor(seeded.Doctor, seeded.Appointment), CancellationToken.None);

            prescriptionId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_DoctorRoleImpersonatingAnotherDoctorId_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedCompletedAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(seeded.Doctor, seeded.Appointment), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("Only the assigned doctor can create a prescription.");
        }

        [Fact]
        public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = new CreatePrescriptionCommand
            {
                AppointmentId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                Medication = "Med",
                Instructions = "Instr"
            };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found");
        }

        [Fact]
        public async Task Handle_AppointmentNotCompleted_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);
            var appointment = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, graph.Slot.Id,
                graph.Slot.Date.ToDateTime(graph.Slot.StartTime), AppointmentStatus.Pending);
            context.Appointments.Add(appointment);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph.Doctor, appointment), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Appointment is not completed yet");
        }

        [Fact]
        public async Task Handle_PrescriptionAlreadyExists_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedCompletedAppointment(context, withExistingPrescription: true);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(seeded.Doctor, seeded.Appointment), CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Prescription has been already provided");
        }

        [Fact]
        public async Task Handle_DoctorIdDoesNotMatchAppointmentsDoctor_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedCompletedAppointment(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(seeded.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(otherDoctor, seeded.Appointment), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("Only the assigned doctor can create a prescription.");
        }
    }
}
