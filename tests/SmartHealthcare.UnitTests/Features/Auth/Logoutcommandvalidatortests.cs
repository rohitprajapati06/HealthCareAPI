using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.Logout;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class LogoutCommandValidatorTests
    {
        private readonly LogoutCommandValidator validator = new();

        [Fact]
        public void Validate_NonEmptyToken_IsValid()
        {
            var result = validator.Validate(new LogoutCommand { RefreshToken = "some-token" });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyToken_HasValidationError()
        {
            var result = validator.Validate(new LogoutCommand { RefreshToken = string.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(LogoutCommand.RefreshToken));
        }
    }
}