using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Commands.Logout;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class LogoutCommandHandlerTests
    {
        private readonly Mock<IJwtTokenService> jwtTokenServiceMock = new();
        private readonly Mock<ILogger<LogoutCommandHandler>> loggerMock = new();

        private LogoutCommandHandler CreateHandler()
            => new(jwtTokenServiceMock.Object, loggerMock.Object);

        [Fact]
        public async Task Handle_ValidRefreshToken_RevokesTokenAndReturnsTrue()
        {
            jwtTokenServiceMock
                .Setup(x => x.LogoutAsync("some-refresh-token", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = CreateHandler();

            var result = await handler.Handle(new LogoutCommand { RefreshToken = "some-refresh-token" }, CancellationToken.None);

            result.Should().BeTrue();
            jwtTokenServiceMock.Verify(x => x.LogoutAsync("some-refresh-token", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_TokenServiceThrows_PropagatesExceptionAndDoesNotSwallowIt()
        {
            jwtTokenServiceMock
                .Setup(x => x.LogoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Token store unavailable"));

            var handler = CreateHandler();

            var act = () => handler.Handle(new LogoutCommand { RefreshToken = "some-refresh-token" }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Token store unavailable");
        }
    }
}
