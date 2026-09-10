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
            // NOTE: the handler Base64Url-decodes request.Token into a `decodedToken`
            // local variable but never actually uses it - it passes the original
            // request.Token straight to userManager.ResetPasswordAsync. This test documents
            // that actual behavior. It happens to line up with ForgotPasswordCommandHandler,
            // which likewise never uses the Base64Url-*encoded* token it builds and instead
            // puts the raw token in the reset link - so end to end this currently works,
            // but only because both dead-code steps cancel out. The Base64UrlDecode call
            // itself still executes eagerly, so it can throw on tokens containing characters
            // outside the base64url alphabet before any password reset actually happens
            // (see the "invalid token characters" test below).
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
        public async Task Handle_TokenContainsCharactersOutsideTheBase64UrlAlphabet_ThrowsFormatExceptionBeforeResettingPassword()
        {
            // A '+' is valid in standard Base64 but not in the URL-safe Base64Url alphabet,
            // so the handler's (unused) decode step throws before it ever calls
            // ResetPasswordAsync - worth checking against what your real reset-link tokens
            // actually look like.
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "patient@example.com" };
            var command = new ResetPasswordCommand
            {
                Email = user.Email,
                Token = "token+with+plus+chars",
                NewPassword = "NewP@ssw0rd1"
            };

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);

            var handler = CreateHandler();

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FormatException>();

            userManagerMock.Verify(
                x => x.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
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
