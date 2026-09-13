using FluentAssertions;
using SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription;

namespace SmartHealthcare.UnitTests.Features.Prescriptions
{
    public class UpdatePrescriptionCommandValidatorTests
    {
        private readonly UpdatePrescriptionCommandValidator validator = new();

        private static UpdatePrescriptionCommand ValidCommand() => new()
        {
            PrescriptionId = Guid.NewGuid(),
            Medication = "Paracetamol 500mg",
            Instruction = "Twice daily after food"
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyPrescriptionId_HasValidationError()
        {
            var command = ValidCommand();
            command.PrescriptionId = Guid.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePrescriptionCommand.PrescriptionId));
        }

        [Fact]
        public void Validate_EmptyInstruction_HasValidationError()
        {
            var command = ValidCommand();
            command.Instruction = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePrescriptionCommand.Instruction));
        }

        [Fact]
        public void Validate_MedicationExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.Medication = new string('a', 1001);

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePrescriptionCommand.Medication));
        }
    }
}
