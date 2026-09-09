using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Common.Interface;
using SmartHealthcare.Application.Common.Models;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.MedicalRecords
{
    public class UpdateMedicalRecordCommandHandlerTests
    {
        private readonly Mock<IFileStorageService> fileStorageServiceMock = new();
        private readonly Mock<ILogger<UpdateMedicalRecordCommandHandler>> loggerMock = new();

        private static MedicalRecord SeedRecord(TestApplicationDbContext context, string fileUrl = "https://storage/old.pdf")
        {
            var record = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                HospitalId = Guid.NewGuid(),
                FileName = "old.pdf",
                FileUrl = fileUrl,
                RecordType = "LabReport"
            };

            context.MedicalRecords.Add(record);
            context.SaveChanges();

            return record;
        }

        private static Mock<IFormFile> CreateFormFileMock(long length = 2048, string fileName = "new.pdf")
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            return fileMock;
        }

        [Fact]
        public async Task Handle_RecordNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = new UpdateMedicalRecordCommandHandler(context, fileStorageServiceMock.Object, loggerMock.Object);

            var command = new UpdateMedicalRecordCommand { Id = Guid.NewGuid(), RecordType = "Updated" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Medical record not found.");
        }

        [Fact]
        public async Task Handle_NoFileProvided_UpdatesRecordTypeOnlyAndSkipsFileStorage()
        {
            using var context = TestDbContextFactory.Create();
            var record = SeedRecord(context);

            var handler = new UpdateMedicalRecordCommandHandler(context, fileStorageServiceMock.Object, loggerMock.Object);
            var command = new UpdateMedicalRecordCommand { Id = record.Id, File = null, RecordType = "UpdatedType" };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);

            var updated = await context.MedicalRecords.FindAsync(record.Id);
            updated!.RecordType.Should().Be("UpdatedType");
            updated.FileUrl.Should().Be("https://storage/old.pdf");
            updated.FileName.Should().Be("old.pdf");

            fileStorageServiceMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            fileStorageServiceMock.Verify(
                x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_FileProvided_UploadsNewFileUpdatesRecordAndDeletesOldFile()
        {
            using var context = TestDbContextFactory.Create();
            var record = SeedRecord(context, "https://storage/old.pdf");

            var newFile = CreateFormFileMock();

            fileStorageServiceMock
                .Setup(x => x.UploadAsync(newFile.Object, "medicalrecords", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUploadResult { FileName = "new.pdf", FileURL = "https://storage/new.pdf" });

            var handler = new UpdateMedicalRecordCommandHandler(context, fileStorageServiceMock.Object, loggerMock.Object);
            var command = new UpdateMedicalRecordCommand { Id = record.Id, File = newFile.Object, RecordType = "UpdatedType" };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);

            var updated = await context.MedicalRecords.FindAsync(record.Id);
            updated!.FileName.Should().Be("new.pdf");
            updated.FileUrl.Should().Be("https://storage/new.pdf");
            updated.RecordType.Should().Be("UpdatedType");

            fileStorageServiceMock.Verify(
                x => x.DeleteAsync("https://storage/old.pdf", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoFileProvided_RecordHadNoOldFile_DoesNotCallDeleteAsync()
        {
            using var context = TestDbContextFactory.Create();
            var record = SeedRecord(context, fileUrl: string.Empty);

            var handler = new UpdateMedicalRecordCommandHandler(context, fileStorageServiceMock.Object, loggerMock.Object);
            var command = new UpdateMedicalRecordCommand { Id = record.Id, File = null, RecordType = "UpdatedType" };

            await handler.Handle(command, CancellationToken.None);

            fileStorageServiceMock.Verify(
                x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_SaveChangesFails_DeletesNewlyUploadedFileButKeepsOldFileAndRethrows()
        {
            var dbName = Guid.NewGuid().ToString();
            var record = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                HospitalId = Guid.NewGuid(),
                FileName = "old.pdf",
                FileUrl = "https://storage/old.pdf",
                RecordType = "LabReport"
            };

            using (var seedContext = new TestApplicationDbContext(TestDbContextFactory.BuildOptions(dbName)))
            {
                seedContext.Database.EnsureCreated();
                seedContext.MedicalRecords.Add(record);
                seedContext.SaveChanges();
            }

            var newFile = CreateFormFileMock();

            fileStorageServiceMock
                .Setup(x => x.UploadAsync(newFile.Object, "medicalrecords", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUploadResult { FileName = "new.pdf", FileURL = "https://storage/new.pdf" });

            using var throwingContext = new ThrowingSaveChangesDbContext(TestDbContextFactory.BuildOptions(dbName));
            var handler = new UpdateMedicalRecordCommandHandler(throwingContext, fileStorageServiceMock.Object, loggerMock.Object);

            var command = new UpdateMedicalRecordCommand { Id = record.Id, File = newFile.Object, RecordType = "UpdatedType" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Simulated save failure");

            // Rollback behavior: the newly uploaded file is cleaned up, the pre-existing
            // file is left untouched since the update never actually committed.
            fileStorageServiceMock.Verify(
                x => x.DeleteAsync("https://storage/new.pdf", It.IsAny<CancellationToken>()),
                Times.Once);
            fileStorageServiceMock.Verify(
                x => x.DeleteAsync("https://storage/old.pdf", It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private sealed class ThrowingSaveChangesDbContext : TestApplicationDbContext
        {
            public ThrowingSaveChangesDbContext(DbContextOptions<TestApplicationDbContext> options)
                : base(options)
            {
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                => throw new InvalidOperationException("Simulated save failure");
        }
    }
}