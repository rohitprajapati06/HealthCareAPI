using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.Login;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator validator = new();

        private static LoginCommand ValidCommand() => new()
        {
            Email = "patient@example.com",
            Password = "Str0ngP@ssw0rd"
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
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        }

        [Fact]
        public void Validate_MalformedEmail_HasValidationError()
        {
            var command = ValidCommand();
            command.Email = "not-an-email";

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        }

        [Fact]
        public void Validate_EmailExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.Email = new string('a', 250) + "@example.com";

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        }

        [Fact]
        public void Validate_EmptyPassword_HasValidationError()
        {
            var command = ValidCommand();
            command.Password = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
        }

        [Fact]
        public void Validate_PasswordExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.Password = new string('a', 129);

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
        }
    }
}
