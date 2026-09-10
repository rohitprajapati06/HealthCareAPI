using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.ResetPassword;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class ResetPasswordCommandValidatorTests
    {
        private readonly ResetPasswordCommandValidator validator = new();

        private static ResetPasswordCommand ValidCommand() => new()
        {
            Email = "patient@example.com",
            Token = "some-reset-token",
            NewPassword = "NewP@ssw0rd1"
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyEmail_HasValidationError()
        {
            var command = ValidCommand();
            command.Email = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.Email));
        }

        [Fact]
        public void Validate_EmptyToken_HasValidationError()
        {
            var command = ValidCommand();
            command.Token = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.Token));
        }

        [Fact]
        public void Validate_TokenExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.Token = new string('a', 2049);

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.Token));
        }

        [Fact]
        public void Validate_NewPasswordTooShort_HasValidationError()
        {
            var command = ValidCommand();
            command.NewPassword = "Sh0rt!";

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
        }
    }
}
