using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Common.Interface;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.DeleteMedicalRecord;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.MedicalRecords
{
    public class DeleteMedicalRecordCommandHandlerTests
    {
        private readonly Mock<IFileStorageService> fileStorageServiceMock = new();
        private readonly Mock<ILogger<DeleteMedicalRecordCommandHandler>> loggerMock = new();

        private DeleteMedicalRecordCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, fileStorageServiceMock.Object, loggerMock.Object);

        private static MedicalRecord SeedRecord(TestApplicationDbContext context, string fileUrl)
        {
            var record = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                HospitalId = Guid.NewGuid(),
                FileName = "report.pdf",
                FileUrl = fileUrl,
                RecordType = "LabReport"
            };

            context.MedicalRecords.Add(record);
            context.SaveChanges();

            return record;
        }

        [Fact]
        public async Task Handle_ExistingRecordWithFile_DeletesRecordAndPhysicalFile()
        {
            using var context = TestDbContextFactory.Create();
            var record = SeedRecord(context, "https://storage/report.pdf");

            var handler = CreateHandler(context);

            var result = await handler.Handle(new DeleteMedicalRecordCommand(record.Id), CancellationToken.None);

            result.Should().Be(Unit.Value);
            (await context.MedicalRecords.FindAsync(record.Id)).Should().BeNull();

            fileStorageServiceMock.Verify(
                x => x.DeleteAsync("https://storage/report.pdf", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_RecordWithoutFileUrl_DoesNotCallDeleteAsyncButStillRemovesRecord()
        {
            using var context = TestDbContextFactory.Create();
            var record = SeedRecord(context, string.Empty);

            var handler = CreateHandler(context);

            await handler.Handle(new DeleteMedicalRecordCommand(record.Id), CancellationToken.None);

            (await context.MedicalRecords.FindAsync(record.Id)).Should().BeNull();

            fileStorageServiceMock.Verify(
                x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_RecordNotFound_ThrowsNotFoundExceptionAndDoesNotTouchFileStorage()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new DeleteMedicalRecordCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Medical record not found.");

            fileStorageServiceMock.Verify(
                x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}