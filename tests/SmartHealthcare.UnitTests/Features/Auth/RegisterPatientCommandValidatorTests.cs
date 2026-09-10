using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class RegisterPatientCommandValidatorTests
    {
        private readonly RegisterPatientCommandValidator validator = new();

        private static RegisterPatientCommand ValidCommand() => new()
        {
            FirstName = "Asha",
            LastName = "Verma",
            Email = "asha.verma@example.com",
            Password = "Str0ngP@ssw0rd",
            PhoneNumber = "9876543210",
            DateofBirth = new DateTime(1995, 4, 12),
            Gender = "Female",
            BloodGroup = "B+",
            Allergies = "None",
            ExistingConditions = "None"
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyFirstName_HasValidationError()
        {
            var command = ValidCommand();
            command.FirstName = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterPatientCommand.FirstName));
        }

        [Fact]
        public void Validate_EmptyLastName_HasValidationError()
        {
            var command = ValidCommand();
            command.LastName = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterPatientCommand.LastName));
        }

        [Fact]
        public void Validate_MalformedEmail_HasValidationError()
        {
            var command = ValidCommand();
            command.Email = "not-an-email";

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterPatientCommand.Email));
        }

        [Fact]
        public void Validate_PasswordTooShort_HasValidationError()
        {
            var command = ValidCommand();
            command.Password = "Sh0rt!";

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterPatientCommand.Password));
        }

        [Fact]
        public void Validate_EmptyGender_HasValidationError()
        {
            var command = ValidCommand();
            command.Gender = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterPatientCommand.Gender));
        }

        [Fact]
        public void Validate_EmptyPhoneNumber_IsStillValid()
        {
            // NOTE: the validator has no rule for PhoneNumber at all, so this documents a gap
            // rather than an assertion about a passing rule - an empty phone number currently
            // sails through validation.
            var command = ValidCommand();
            command.PhoneNumber = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }
    }
}
