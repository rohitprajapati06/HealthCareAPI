using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Contracts.Services;
using SmartHealthcare.Application.Features.Auth.Commands.ForgotPassword;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class ForgotPasswordCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock = UserManagerMockFactory.Create();
        private readonly Mock<IEmailService> emailServiceMock = new();
        private readonly Mock<ILogger> loggerMock = new();

        private ForgotPasswordCommandHandler CreateHandler()
            => new(userManagerMock.Object, emailServiceMock.Object, loggerMock.Object);

        [Fact]
        public async Task Handle_ValidEmail_SendsResetLinkAsPlainTextBodyAndReturnsTrue()
        {
            // NOTE: the handler builds a nicely formatted HTML `body` string but never uses
            // it - it passes the raw `resetLink` URL as the email body instead. This test
            // documents that actual (probably unintended) behavior.
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "patient@example.com" };

            userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("raw-reset-token");

            string? capturedBody = null;
            string? capturedSubject = null;
            string? capturedTo = null;

            emailServiceMock
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, string, CancellationToken>((to, subject, body, _) =>
                {
                    capturedTo = to;
                    capturedSubject = subject;
                    capturedBody = body;
                })
                .Returns(Task.CompletedTask);

            var handler = CreateHandler();

            var result = await handler.Handle(new ForgotPasswordCommand { Email = user.Email }, CancellationToken.None);

            result.Should().BeTrue();

            capturedTo.Should().Be(user.Email);
            capturedSubject.Should().Be("Password Reset");
            capturedBody.Should().Be($"https://localhost:7183/api/Auth/reset-password?email={user.Email}&token=raw-reset-token");
            capturedBody.Should().NotContain("<h3>", "the HTML body variable the handler builds is currently discarded");
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessExceptionAndDoesNotSendEmail()
        {
            userManagerMock.Setup(x => x.FindByEmailAsync("missing@example.com")).ReturnsAsync((ApplicationUser?)null);

            var handler = CreateHandler();

            var act = () => handler.Handle(new ForgotPasswordCommand { Email = "missing@example.com" }, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User Not Found");

            emailServiceMock.Verify(
                x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
