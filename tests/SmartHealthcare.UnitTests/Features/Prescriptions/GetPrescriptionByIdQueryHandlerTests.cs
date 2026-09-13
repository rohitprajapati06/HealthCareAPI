using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetPrescriptionById;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Prescriptions
{
    public class GetPrescriptionByIdQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetPrescriptionByIdQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, Appointment Appointment, Prescription Prescription) SeedPrescription(
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
        public async Task Handle_NonPatientNonDoctorRole_ReturnsMappedResponse()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetPrescriptionByIdQuery(seeded.Prescription.Id), CancellationToken.None);

            response.Id.Should().Be(seeded.Prescription.Id);
            response.Medication.Should().Be("Amoxicillin");
        }

        [Fact]
        public async Task Handle_PrescriptionNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPrescriptionByIdQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Prescription not found");
        }

        [Fact]
        public async Task Handle_PatientRoleViewingOwnPrescription_ReturnsResponse()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Patient.UserId);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetPrescriptionByIdQuery(seeded.Prescription.Id), CancellationToken.None);

            response.Id.Should().Be(seeded.Prescription.Id);
        }

        [Fact]
        public async Task Handle_PatientRoleViewingAnotherPatientsPrescription_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPrescriptionByIdQuery(seeded.Prescription.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view this prescription.");
        }

        [Fact]
        public async Task Handle_DoctorRoleViewingOwnPrescription_ReturnsResponse()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetPrescriptionByIdQuery(seeded.Prescription.Id), CancellationToken.None);

            response.Id.Should().Be(seeded.Prescription.Id);
        }

        [Fact]
        public async Task Handle_DoctorRoleViewingAnotherDoctorsPrescription_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescription(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(seeded.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherDoctor.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPrescriptionByIdQuery(seeded.Prescription.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view this prescription.");
        }
    }
}
