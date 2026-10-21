using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Features.Auth.Commands.ResetPassword;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock = UserManagerMockFactory.Create();
        private readonly Mock<ILogger<ResetPasswordCommandHandler>> loggerMock = new();

        private ResetPasswordCommandHandler CreateHandler()
            => new(userManagerMock.Object, loggerMock.Object);

        [Fact]
        public async Task Handle_ValidRequest_CallsResetPasswordWithTheRawTokenAsReceived()
        {
            // Identity reset tokens are passed through unchanged; URL encoding is handled
            // by the link transport and the ASP.NET query parser.
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "patient@example.com" };
            var command = new ResetPasswordCommand
            {
                Email = user.Email,
                Token = "valid-url-safe-token",
                NewPassword = "NewP@ssw0rd1"
            };

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
            userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var handler = CreateHandler();

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();

            userManagerMock.Verify(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword), Times.Once);
        }

        [Fact]
        public async Task Handle_TokenContainsPlusCharacters_PassesOriginalTokenToIdentity()
        {
            // Identity tokens can contain '+' and should be passed to Identity intact.
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "patient@example.com" };
            var command = new ResetPasswordCommand
            {
                Email = user.Email,
                Token = "token+with+plus+chars",
                NewPassword = "NewP@ssw0rd1"
            };

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
            userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var handler = CreateHandler();

            var act = () => handler.Handle(command, CancellationToken.None);

            (await act()).Should().BeTrue();

            userManagerMock.Verify(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            userManagerMock.Setup(x => x.FindByEmailAsync("missing@example.com")).ReturnsAsync((ApplicationUser?)null);

            var handler = CreateHandler();

            var command = new ResetPasswordCommand { Email = "missing@example.com", Token = "any-token", NewPassword = "NewP@ssw0rd1" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User not found");
        }

        [Fact]
        public async Task Handle_IdentityResetFails_ThrowsBadRequestExceptionWithCombinedErrors()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "patient@example.com" };
            var command = new ResetPasswordCommand { Email = user.Email, Token = "valid-token", NewPassword = "weak" };

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);

            var failure = IdentityResult.Failed(
                new IdentityError { Code = "InvalidToken", Description = "Invalid token." });

            userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
                .ReturnsAsync(failure);

            var handler = CreateHandler();

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Invalid token.");
        }
    }
}
