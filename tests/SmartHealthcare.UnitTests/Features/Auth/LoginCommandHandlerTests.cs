using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Commands.Login;
using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock = UserManagerMockFactory.Create();
        private readonly Mock<IJwtTokenService> jwtTokenServiceMock = new();
        private readonly Mock<ILogger<LoginCommandHandler>> loggerMock = new();

        private LoginCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(userManagerMock.Object, jwtTokenServiceMock.Object, context, loggerMock.Object);

        private static ApplicationUser CreateUser(string email = "patient@example.com") => new()
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "Asha",
            LastName = "Verma",
            IsActive = true
        };

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsAuthResponseAndPersistsRefreshToken()
        {
            using var context = TestDbContextFactory.Create();

            var user = CreateUser();
            var command = new LoginCommand { Email = user.Email!, Password = "Str0ngP@ssw0rd" };

            var authResponse = new AuthResponse
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Roles = "Patient"
            };

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password)).ReturnsAsync(true);
            jwtTokenServiceMock
                .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(authResponse);

            var handler = CreateHandler(context);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().Be(authResponse);

            var storedToken = context.RefreshTokens.Single();
            storedToken.UserId.Should().Be(user.Id);
            storedToken.Token.Should().Be("refresh-token");
            storedToken.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessExceptionAndSkipsPasswordCheck()
        {
            using var context = TestDbContextFactory.Create();

            var command = new LoginCommand { Email = "missing@example.com", Password = "whatever" };

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((ApplicationUser?)null);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("No User Found");

            userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
            context.RefreshTokens.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessExceptionAndDoesNotIssueToken()
        {
            using var context = TestDbContextFactory.Create();

            var user = CreateUser();
            var command = new LoginCommand { Email = user.Email!, Password = "WrongPassword" };

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password)).ReturnsAsync(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid Credentails");

            jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
            context.RefreshTokens.Should().BeEmpty();
        }
    }
}
