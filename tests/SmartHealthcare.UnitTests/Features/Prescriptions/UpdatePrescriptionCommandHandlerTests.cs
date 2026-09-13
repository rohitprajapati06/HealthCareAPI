using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Prescriptions
{
    public class UpdatePrescriptionCommandHandlerTests
    {
        private readonly Mock<ILogger<UpdatePrescriptionCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private UpdatePrescriptionCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, Prescription Prescription) SeedPrescription(TestApplicationDbContext context)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);

            var prescription = new Prescription
            {
                Id = Guid.NewGuid(),
                AppointmentId = Guid.NewGuid(),
                DoctorId = doctor.Id,
                Medication = "Old med",
                Instructions = "Old instructions"
            };

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.Prescriptions.Add(prescription);
            context.SaveChanges();

            return (hospital, doctor, prescription);
        }

        [Fact]
        public async Task Handle_PrescriptionNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var command = new UpdatePrescriptionCommand { PrescriptionId = Guid.NewGuid(), Medication = "New", Instruction = "New" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Prescription not found");
        }

        [Fact]
        public async Task Handle_NonDoctorRole_UpdatesMedicationAndInstructions()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = new UpdatePrescriptionCommand { PrescriptionId = seeded.Prescription.Id, Medication = "New med", Instruction = "New instructions" };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);

            var updated = context.Prescriptions.Single(p => p.Id == seeded.Prescription.Id);
            updated.Medication.Should().Be("New med");
            updated.Instructions.Should().Be("New instructions");
        }

        [Fact]
        public async Task Handle_DoctorRoleUpdatingOwnPrescription_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);
            var command = new UpdatePrescriptionCommand { PrescriptionId = seeded.Prescription.Id, Medication = "New med", Instruction = "New instructions" };

            await handler.Handle(command, CancellationToken.None);

            context.Prescriptions.Single(p => p.Id == seeded.Prescription.Id).Medication.Should().Be("New med");
        }

        [Fact]
        public async Task Handle_DoctorRoleUpdatingAnotherDoctorsPrescription_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = CreateHandler(context);
            var command = new UpdatePrescriptionCommand { PrescriptionId = seeded.Prescription.Id, Medication = "New med", Instruction = "New instructions" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You can only update prescriptions you created.");

            context.Prescriptions.Single(p => p.Id == seeded.Prescription.Id).Medication.Should().Be("Old med");
        }
    }
}
