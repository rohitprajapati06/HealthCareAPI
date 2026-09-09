using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Commands.RefreshUserToken;
using SmartHealthcare.Application.Features.Auth.Responses;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IJwtTokenService> jwtTokenServiceMock = new();
        private readonly Mock<ILogger<RefreshTokenCommandHandler>> loggerMock = new();

        private RefreshTokenCommandHandler CreateHandler()
            => new(jwtTokenServiceMock.Object, loggerMock.Object);

        [Fact]
        public async Task Handle_ValidRefreshToken_ReturnsAuthResponseFromTokenService()
        {
            var authResponse = new AuthResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Roles = "Patient"
            };

            jwtTokenServiceMock
                .Setup(x => x.RefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(authResponse);

            var handler = CreateHandler();

            var result = await handler.Handle(new RefreshTokenCommand { RefreshToken = "old-refresh-token" }, CancellationToken.None);

            result.Should().Be(authResponse);
            jwtTokenServiceMock.Verify(x => x.RefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_TokenServiceRejectsToken_PropagatesException()
        {
            jwtTokenServiceMock
                .Setup(x => x.RefreshTokenAsync("expired-token", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid refresh token"));

            var handler = CreateHandler();

            var act = () => handler.Handle(new RefreshTokenCommand { RefreshToken = "expired-token" }, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid refresh token");
        }
    }
}