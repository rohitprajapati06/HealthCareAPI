using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Prescriptions
{
    public class GetDoctorPrescriptionsQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetDoctorPrescriptionsQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, Prescription Prescription) SeedPrescriptionForDoctor(TestApplicationDbContext context)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);

            var prescription = new Prescription
            {
                Id = Guid.NewGuid(),
                AppointmentId = Guid.NewGuid(),
                DoctorId = doctor.Id,
                DoctorProfile = doctor,
                Medication = "Amoxicillin",
                Instructions = "3 times a day",
                CreatedAt = DateTime.UtcNow
            };

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.Prescriptions.Add(prescription);
            context.SaveChanges();

            return (hospital, doctor, prescription);
        }

        [Fact]
        public async Task Handle_DoctorRoleRequestingOwnPrescriptions_ReturnsMappedList()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescriptionForDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorPrescriptionsQuery(seeded.Doctor.Id), CancellationToken.None);

            results.Should().ContainSingle();
            var response = results.Single();
            response.Id.Should().Be(seeded.Prescription.Id);
            response.DoctorName.Should().Be($"{seeded.Doctor.User.FirstName} {seeded.Doctor.User.LastName}");
            response.Medication.Should().Be("Amoxicillin");
        }

        [Fact]
        public async Task Handle_DoctorRoleRequestingAnotherDoctorsPrescriptions_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescriptionForDoctor(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(seeded.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherDoctor.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetDoctorPrescriptionsQuery(seeded.Doctor.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view these prescriptions.");
        }

        [Fact]
        public async Task Handle_NonDoctorRole_BypassesOwnershipCheckAndReturnsPrescriptions()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedPrescriptionForDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetDoctorPrescriptionsQuery(seeded.Doctor.Id), CancellationToken.None);

            results.Should().ContainSingle();
        }
    }
}
