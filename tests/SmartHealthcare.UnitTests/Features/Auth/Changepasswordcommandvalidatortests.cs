using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.ChangePassword;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class ChangePasswordCommandValidatorTests
    {
        private readonly ChangePasswordCommandValidator validator = new();

        private static ChangePasswordCommand ValidCommand() => new()
        {
            CurrentPassword = "OldP@ssw0rd",
            NewPassword = "NewP@ssw0rd1"
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyCurrentPassword_HasValidationError()
        {
            var command = ValidCommand();
            command.CurrentPassword = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.CurrentPassword));
        }

        [Theory]
        [InlineData("Sh0rt!")]           // below minimum length
        [InlineData("nouppercase1!")]    // missing uppercase
        [InlineData("NOLOWERCASE1!")]    // missing lowercase
        [InlineData("NoDigitsHere!")]    // missing digit
        [InlineData("NoSpecialChar1")]   // missing special character
        public void Validate_NewPasswordViolatesPolicy_HasValidationError(string newPassword)
        {
            var command = ValidCommand();
            command.NewPassword = newPassword;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
        }

        [Fact]
        public void Validate_NewPasswordExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.NewPassword = "Aa1!" + new string('a', 128);

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
        }
    }
}
