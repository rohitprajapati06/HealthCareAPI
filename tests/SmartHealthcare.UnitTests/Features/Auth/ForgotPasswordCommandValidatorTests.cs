using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.ForgotPassword;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class ForgotPasswordCommandValidatorTests
    {
        private readonly ForgotPasswordCommandValidator validator = new();

        [Fact]
        public void Validate_ValidEmail_IsValid()
        {
            var result = validator.Validate(new ForgotPasswordCommand { Email = "patient@example.com" });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyEmail_HasValidationError()
        {
            var result = validator.Validate(new ForgotPasswordCommand { Email = string.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
        }

        [Fact]
        public void Validate_MalformedEmail_HasValidationError()
        {
            var result = validator.Validate(new ForgotPasswordCommand { Email = "not-an-email" });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
        }
    }
}
