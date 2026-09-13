using FluentAssertions;
using SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription;

namespace SmartHealthcare.UnitTests.Features.Prescriptions
{
    public class CreatePrescriptionCommandValidatorTests
    {
        private readonly CreatePrescriptionCommandValidator validator = new();

        private static CreatePrescriptionCommand ValidCommand() => new()
        {
            AppointmentId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            Medication = "Paracetamol 500mg",
            Instructions = "Twice daily after food"
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyAppointmentId_HasValidationError()
        {
            var command = ValidCommand();
            command.AppointmentId = Guid.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePrescriptionCommand.AppointmentId));
        }

        [Fact]
        public void Validate_EmptyMedication_HasValidationError()
        {
            var command = ValidCommand();
            command.Medication = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePrescriptionCommand.Medication));
        }

        [Fact]
        public void Validate_InstructionsExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.Instructions = new string('a', 1001);

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePrescriptionCommand.Instructions));
        }
    }
}
