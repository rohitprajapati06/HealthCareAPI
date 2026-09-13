using FluentAssertions;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot;

namespace SmartHealthcare.UnitTests.Features.AvailabilitySlots
{
    public class CreateAvailabilitySlotValidatorTests
    {
        private readonly CreateAvailabilitySlotValidator validator = new();

        private static CreateAvailabilitySlot ValidCommand() => new()
        {
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
        public void Validate_EmptyDoctorId_HasValidationError()
        {
            var command = ValidCommand();
            command.DoctorId = Guid.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAvailabilitySlot.DoctorId));
        }

        [Fact]
        public void Validate_EndTimeNotAfterStartTime_HasValidationError()
        {
            var command = ValidCommand();
            command.EndTime = command.StartTime;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAvailabilitySlot.EndTime));
        }
    }
}
