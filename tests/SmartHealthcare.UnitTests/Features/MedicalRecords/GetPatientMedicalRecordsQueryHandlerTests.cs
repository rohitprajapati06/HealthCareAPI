using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.MedicalRecords.Queries.GetPatientMedicalRecords;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.MedicalRecords
{
    public class GetPatientMedicalRecordsQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetPatientMedicalRecordsQueryCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        [Fact]
        public async Task Handle_NonPatientRole_ReturnsAllRecordsForRequestedPatient()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientMedicalRecordsQuery(record.PatientId), CancellationToken.None);

            results.Should().ContainSingle();
            results[0].Id.Should().Be(record.Id);
            results[0].PatientName.Should().Be($"{record.Patient.User.FirstName} {record.Patient.User.LastName}");
            results[0].HospitalName.Should().Be(record.Hospital.Name);
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingOwnRecords_ReturnsRecords()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(record.Patient.UserId);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientMedicalRecordsQuery(record.PatientId), CancellationToken.None);

            results.Should().ContainSingle(r => r.Id == record.Id);
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingAnotherPatientsRecords_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            var otherPatient = MedicalRecordTestData.CreatePatient("Rohan", "Shah");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPatientMedicalRecordsQuery(record.PatientId), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You are not allowed to view these medical records.");
        }

        [Fact]
        public async Task Handle_PatientRoleWithNoPatientProfile_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPatientMedicalRecordsQuery(record.PatientId), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You are not allowed to view these medical records.");
        }

        [Fact]
        public async Task Handle_NoRecordsForPatient_ReturnsEmptyList()
        {
            // NOTE: the handler has a `medicalRecords == null` check intended to guard the
            // "no records" case, but ToListAsync never returns null - it returns an empty
            // list - so that branch is unreachable. This test documents the actual runtime
            // behavior (an empty list, not a NotFoundException).
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var results = await handler.Handle(new GetPatientMedicalRecordsQuery(Guid.NewGuid()), CancellationToken.None);

            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }
    }
}