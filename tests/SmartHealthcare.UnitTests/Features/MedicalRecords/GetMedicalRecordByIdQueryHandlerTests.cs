using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.MedicalRecords.Queries.GetMedicalRecordById;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.MedicalRecords
{
    public class GetMedicalRecordByIdQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetMedicalRecordByIdQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        [Fact]
        public async Task Handle_NonPatientRole_ReturnsMappedResponseRegardlessOfOwnership()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetMedicalRecordById(record.Id), CancellationToken.None);

            response.Id.Should().Be(record.Id);
            response.PatientId.Should().Be(record.PatientId);
            response.HospitalId.Should().Be(record.HospitalId);
            response.HospitalName.Should().Be(record.Hospital.Name);
            response.PatientName.Should().Be($"{record.Patient.User.FirstName} {record.Patient.User.LastName}");
            response.FileName.Should().Be(record.FileName);
            response.FileUrl.Should().Be(record.FileUrl);
            response.RecordType.Should().Be(record.RecordType);
        }

        [Fact]
        public async Task Handle_RecordNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetMedicalRecordById(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("No Medical Record found");
        }

        [Fact]
        public async Task Handle_PatientRoleViewingOwnRecord_ReturnsResponse()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(record.Patient.UserId);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetMedicalRecordById(record.Id), CancellationToken.None);

            response.Id.Should().Be(record.Id);
        }

        [Fact]
        public async Task Handle_PatientRoleViewingAnotherPatientsRecord_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            var otherPatient = MedicalRecordTestData.CreatePatient("Rohan", "Shah");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetMedicalRecordById(record.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You are not allowed to view this medical record.");
        }

        [Fact]
        public async Task Handle_PatientRoleWithNoPatientProfile_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var record = MedicalRecordTestData.SeedFullRecord(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetMedicalRecordById(record.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You are not allowed to view this medical record.");
        }
    }
}