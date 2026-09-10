using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.RefreshUserToken;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class RefreshTokenCommandValidatorTests
    {
        private readonly RefreshTokenCommandValidator validator = new();

        [Fact]
        public void Validate_NonEmptyToken_IsValid()
        {
            var result = validator.Validate(new RefreshTokenCommand { RefreshToken = "some-token" });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyToken_HasValidationError()
        {
            var result = validator.Validate(new RefreshTokenCommand { RefreshToken = string.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken));
        }
    }
}
