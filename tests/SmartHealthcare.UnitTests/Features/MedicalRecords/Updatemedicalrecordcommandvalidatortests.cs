using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord;

namespace SmartHealthcare.UnitTests.Features.MedicalRecords
{
    public class UpdateMedicalRecordCommandValidatorTests
    {
        private readonly UpdateMedicalRecordCommandvalidator validator = new();

        private static Mock<IFormFile> CreateFormFileMock(long length = 1024)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(length);
            return fileMock;
        }

        [Fact]
        public void Validate_NoFileProvided_IsValidSinceFileIsOptional()
        {
            var command = new UpdateMedicalRecordCommand { Id = Guid.NewGuid(), File = null, RecordType = "LabReport" };

            var result = validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyFile_HasValidationError()
        {
            var command = new UpdateMedicalRecordCommand
            {
                Id = Guid.NewGuid(),
                File = CreateFormFileMock(length: 0).Object,
                RecordType = "LabReport"
            };

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateMedicalRecordCommand.File) &&
                e.ErrorMessage == "The uploaded file must not be empty");
        }

        [Fact]
        public void Validate_NonEmptyFile_HasNoFileValidationError()
        {
            var command = new UpdateMedicalRecordCommand
            {
                Id = Guid.NewGuid(),
                File = CreateFormFileMock(length: 2048).Object,
                RecordType = "LabReport"
            };

            var result = validator.Validate(command);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateMedicalRecordCommand.File));
        }

        [Fact]
        public void Validate_MissingRecordType_HasValidationError()
        {
            var command = new UpdateMedicalRecordCommand { Id = Guid.NewGuid(), File = null, RecordType = string.Empty };

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateMedicalRecordCommand.RecordType));
        }
    }
}