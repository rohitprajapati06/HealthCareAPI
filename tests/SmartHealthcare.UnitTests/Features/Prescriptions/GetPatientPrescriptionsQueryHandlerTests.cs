using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Prescriptions
{
    public class GetPatientPrescriptionsQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetPatientPrescriptionsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, Appointment Appointment, Prescription Prescription) SeedPrescriptionForPatient(
            TestApplicationDbContext context)
        {
            var graph = AppointmentTestData.SeedBookableGraph(context);
            var appointment = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, graph.Slot.Id,
                DateTime.Now.AddHours(-1), AppointmentStatus.Completed);
            context.Appointments.Add(appointment);

            var prescription = new Prescription
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                Appointment = appointment,
                DoctorId = graph.Doctor.Id,
                DoctorProfile = graph.Doctor,
                Medication = "Amoxicillin",
                Instructions = "3 times a day",
                CreatedAt = DateTime.UtcNow
            };
            context.Prescriptions.Add(prescription);
            context.SaveChanges();

            return (graph.Hospital, graph.Doctor, graph.Patient, appointment, prescription);
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingOwnPrescriptions_ReturnsMappedList()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescriptionForPatient(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Patient.UserId);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientPrescriptionsQuery(seeded.Patient.Id), CancellationToken.None);

            results.Should().ContainSingle();
            results.Single().Id.Should().Be(seeded.Prescription.Id);
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingAnotherPatientsPrescriptions_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescriptionForPatient(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPatientPrescriptionsQuery(seeded.Patient.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view these prescriptions.");
        }

        [Fact]
        public async Task Handle_NonPatientRole_BypassesOwnershipCheckAndReturnsPrescriptions()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescriptionForPatient(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientPrescriptionsQuery(seeded.Patient.Id), CancellationToken.None);

            results.Should().ContainSingle();
        }
    }
}
