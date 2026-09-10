using FluentAssertions;
using SmartHealthcare.Application.Features.Appointments.Commands.CancelAppointment;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class CancelAppointmentCommandValidatorTests
    {
        private readonly CancelAppointmentCommandValidator validator = new();

        [Fact]
        public void Validate_NonEmptyAppointmentId_IsValid()
        {
            var result = validator.Validate(new CancelAppointmentCommand { AppointmentId = Guid.NewGuid() });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyAppointmentId_HasValidationError()
        {
            var result = validator.Validate(new CancelAppointmentCommand { AppointmentId = Guid.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CancelAppointmentCommand.AppointmentId));
        }
    }
}
