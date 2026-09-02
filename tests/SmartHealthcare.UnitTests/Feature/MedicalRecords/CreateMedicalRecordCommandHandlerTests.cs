using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Common.Interface;
using SmartHealthcare.Application.Common.Models;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.MedicalRecords
{
    public class CreateMedicalRecordCommandHandlerTests
    {
        private readonly Mock<IFileStorageService> fileStorageServiceMock = new();
        private readonly Mock<ILogger<CreateMedicalRecordCommandHandler>> loggerMock = new();

        private static (Guid PatientId, Guid HospitalId) SeedPatientAndHospital(TestApplicationDbContext context)
        {
            var patientId = Guid.NewGuid();
            var hospitalId = Guid.NewGuid();

            context.PatientProfiles.Add(new PatientProfile
            {
                Id = patientId,
                UserId = Guid.NewGuid(),
                DateOfBirth = new DateTime(1995, 5, 20),
                Gender = "Male",
                BloodGroup = "O+",
                Allergies = "None",
                ExistingConditions = "None"
            });

            context.Hospitals.Add(new Hospital
            {
                Id = hospitalId,
                Name = "General Hospital",
                RohiniCode = "RH-001",
                Address = "123 Main St",
                City = "Mumbai",
                State = "MH",
                Country = "India"
            });

            context.SaveChanges();

            return (patientId, hospitalId);
        }

        private static Mock<IFormFile> CreateFormFileMock(long length = 1024, string fileName = "report.pdf")
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            return fileMock;
        }

        private CreateMedicalRecordCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, fileStorageServiceMock.Object);

        [Fact]
        public async Task Handle_ValidRequest_CreatesMedicalRecordAndReturnsId()
        {
            using var context = TestDbContextFactory.Create();
            var (patientId, hospitalId) = SeedPatientAndHospital(context);

            fileStorageServiceMock
                .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), "medicalrecords", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUploadResult { FileName = "report.pdf", FileURL = "https://storage/report.pdf" });

            var handler = CreateHandler(context);

            var command = new CreateMedicalRecordCommand
            {
                PatientId = patientId,
                HospitalId = hospitalId,
                File = CreateFormFileMock().Object,
                RecordType = "LabReport"
            };

            var resultId = await handler.Handle(command, CancellationToken.None);

            resultId.Should().NotBeEmpty();

            var saved = await context.MedicalRecords.FindAsync(resultId);
            saved.Should().NotBeNull();
            saved!.PatientId.Should().Be(patientId);
            saved.HospitalId.Should().Be(hospitalId);
            saved.FileName.Should().Be("report.pdf");
            saved.FileUrl.Should().Be("https://storage/report.pdf");
            saved.RecordType.Should().Be("LabReport");

            fileStorageServiceMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>(), "medicalrecords", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PatientNotFound_ThrowsNotFoundExceptionAndDoesNotUploadFile()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var command = new CreateMedicalRecordCommand
            {
                PatientId = Guid.NewGuid(),
                HospitalId = Guid.NewGuid(),
                File = CreateFormFileMock().Object,
                RecordType = "LabReport"
            };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found");

            fileStorageServiceMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_HospitalNotFound_ThrowsNotFoundExceptionAndDoesNotUploadFile()
        {
            using var context = TestDbContextFactory.Create();

            var patientId = Guid.NewGuid();
            context.PatientProfiles.Add(new PatientProfile
            {
                Id = patientId,
                UserId = Guid.NewGuid(),
                DateOfBirth = new DateTime(1988, 3, 10),
                Gender = "Female",
                BloodGroup = "A+",
                Allergies = "None",
                ExistingConditions = "None"
            });
            context.SaveChanges();

            var handler = CreateHandler(context);

            var command = new CreateMedicalRecordCommand
            {
                PatientId = patientId,
                HospitalId = Guid.NewGuid(),
                File = CreateFormFileMock().Object,
                RecordType = "LabReport"
            };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Hospitals not found");

            fileStorageServiceMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_UploadFails_PropagatesExceptionAndDoesNotCreateRecord()
        {
            using var context = TestDbContextFactory.Create();
            var (patientId, hospitalId) = SeedPatientAndHospital(context);

            fileStorageServiceMock
                .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), "medicalrecords", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Storage unavailable"));

            var handler = CreateHandler(context);

            var command = new CreateMedicalRecordCommand
            {
                PatientId = patientId,
                HospitalId = hospitalId,
                File = CreateFormFileMock().Object,
                RecordType = "LabReport"
            };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<IOException>();

            context.MedicalRecords.Should().BeEmpty();
        }
    }
}