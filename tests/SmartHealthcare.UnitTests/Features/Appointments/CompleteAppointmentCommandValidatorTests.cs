using FluentAssertions;
using SmartHealthcare.Application.Features.Appointments.Commands.CompleteAppointment;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class CompleteAppointmentCommandValidatorTests
    {
        private readonly CompleteAppointmentCommandValidator validator = new();

        [Fact]
        public void Validate_NonEmptyAppointmentId_IsValid()
        {
            var result = validator.Validate(new CompleteAppointmentCommand { AppointmentId = Guid.NewGuid() });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyAppointmentId_HasValidationError()
        {
            var result = validator.Validate(new CompleteAppointmentCommand { AppointmentId = Guid.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CompleteAppointmentCommand.AppointmentId));
        }
    }
}
