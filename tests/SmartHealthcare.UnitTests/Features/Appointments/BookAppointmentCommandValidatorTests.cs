using FluentAssertions;
using SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class BookAppointmentCommandValidatorTests
    {
        private readonly BookAppointmentCommandValidator validator = new();

        private static BookAppointmentCommand ValidCommand() => new()
        {
            DoctorId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            HospitalId = Guid.NewGuid(),
            AvailabilitySlotId = Guid.NewGuid(),
            Notes = "First visit"
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(nameof(BookAppointmentCommand.DoctorId))]
        [InlineData(nameof(BookAppointmentCommand.PatientId))]
        [InlineData(nameof(BookAppointmentCommand.HospitalId))]
        [InlineData(nameof(BookAppointmentCommand.AvailabilitySlotId))]
        public void Validate_EmptyRequiredGuid_HasValidationError(string propertyName)
        {
            var command = ValidCommand();

            switch (propertyName)
            {
                case nameof(BookAppointmentCommand.DoctorId): command.DoctorId = Guid.Empty; break;
                case nameof(BookAppointmentCommand.PatientId): command.PatientId = Guid.Empty; break;
                case nameof(BookAppointmentCommand.HospitalId): command.HospitalId = Guid.Empty; break;
                case nameof(BookAppointmentCommand.AvailabilitySlotId): command.AvailabilitySlotId = Guid.Empty; break;
            }

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == propertyName);
        }

        [Fact]
        public void Validate_NotesExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.Notes = new string('a', 501);

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(BookAppointmentCommand.Notes));
        }

        [Fact]
        public void Validate_NullNotes_IsValidSinceNotesIsOptional()
        {
            var command = ValidCommand();
            command.Notes = null;

            var result = validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }
    }
}
