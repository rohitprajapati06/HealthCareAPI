using FluentAssertions;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.UpdateAvailabilitySlot;

namespace SmartHealthcare.UnitTests.Features.AvailabilitySlots
{
    public class UpdateAvailabilitySlotValidatorTests
    {
        private readonly UpdateAvailabilitySlotValidator validator = new();

        private static UpdateAvailabilitySlotCommand ValidCommand() => new()
        {
            SlotId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(9, 30)
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptySlotId_HasValidationError()
        {
            var command = ValidCommand();
            command.SlotId = Guid.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAvailabilitySlotCommand.SlotId));
        }

        [Fact]
        public void Validate_EmptyDoctorId_HasValidationError()
        {
            var command = ValidCommand();
            command.DoctorId = Guid.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAvailabilitySlotCommand.DoctorId));
        }

        [Fact]
        public void Validate_EndTimeNotAfterStartTime_HasValidationError()
        {
            var command = ValidCommand();
            command.EndTime = command.StartTime;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "End time must be greater than start time");
        }
    }
}
