using FluentAssertions;
using SmartHealthcare.Application.Features.Appointments.Commands.RescheduleAppointment;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class RescheduleAppointmentCommandValidatorTests
    {
        private readonly RescheduleAppointmentCommandValidator validator = new();

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(new RescheduleAppointmentCommand
            {
                AppointmentId = Guid.NewGuid(),
                AvailabilitySlotId = Guid.NewGuid()
            });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyAppointmentId_HasValidationError()
        {
            var result = validator.Validate(new RescheduleAppointmentCommand
            {
                AppointmentId = Guid.Empty,
                AvailabilitySlotId = Guid.NewGuid()
            });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RescheduleAppointmentCommand.AppointmentId));
        }

        [Fact]
        public void Validate_EmptyAvailabilitySlotId_HasValidationError()
        {
            var result = validator.Validate(new RescheduleAppointmentCommand
            {
                AppointmentId = Guid.NewGuid(),
                AvailabilitySlotId = Guid.Empty
            });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RescheduleAppointmentCommand.AvailabilitySlotId));
        }
    }
}
