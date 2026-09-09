using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

namespace SmartHealthcare.UnitTests.Features.MedicalRecords
{
    public class CreateMedicalRecordCommandValidatorTests
    {
        private readonly CreateMedicalRecordCommandValidator validator = new();

        private static Mock<IFormFile> CreateFormFileMock(long length = 1024)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(length);
            return fileMock;
        }

        private static CreateMedicalRecordCommand ValidCommand() => new()
        {
            PatientId = Guid.NewGuid(),
            HospitalId = Guid.NewGuid(),
            File = CreateFormFileMock().Object,
            RecordType = "LabReport"
        };

        [Fact]
        public void Validate_ValidCommand_HasNoValidationErrors()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyHospitalId_HasValidationError()
        {
            var command = ValidCommand();
            command.HospitalId = Guid.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMedicalRecordCommand.HospitalId));
        }

        [Fact]
        public void Validate_NullFile_HasValidationError()
        {
            var command = ValidCommand();
            command.File = null!;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMedicalRecordCommand.File));
        }

        [Fact]
        public void Validate_EmptyFile_HasValidationError()
        {
            var command = ValidCommand();
            command.File = CreateFormFileMock(length: 0).Object;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(CreateMedicalRecordCommand.File) &&
                e.ErrorMessage == "Medical record file cannot be empty.");
        }

        [Fact]
        public void Validate_MissingRecordType_HasValidationError()
        {
            var command = ValidCommand();
            command.RecordType = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMedicalRecordCommand.RecordType));
        }

        [Fact]
        public void Validate_RecordTypeExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.RecordType = new string('a', 101);

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateMedicalRecordCommand.RecordType));
        }
    }
}