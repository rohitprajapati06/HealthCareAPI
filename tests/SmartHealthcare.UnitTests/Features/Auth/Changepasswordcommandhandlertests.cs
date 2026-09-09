using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Commands.ChangePassword;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class ChangePasswordCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock = UserManagerMockFactory.Create();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();
        private readonly Mock<ILogger<ChangePasswordCommandHandler>> loggerMock = new();

        private ChangePasswordCommandHandler CreateHandler()
            => new(userManagerMock.Object, currentUserServiceMock.Object, loggerMock.Object);

        private static ApplicationUser CreateUser(Guid id) => new()
        {
            Id = id,
            UserName = "patient@example.com",
            Email = "patient@example.com",
            FirstName = "Asha",
            LastName = "Verma"
        };

        [Fact]
        public async Task Handle_ValidRequest_ChangesPasswordAndReturnsTrue()
        {
            var userId = Guid.NewGuid();
            var user = CreateUser(userId);

            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
            userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, "OldP@ssw0rd", "NewP@ssw0rd1"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = CreateHandler();

            var command = new ChangePasswordCommand { CurrentPassword = "OldP@ssw0rd", NewPassword = "NewP@ssw0rd1" };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NoCurrentUser_ThrowsUnauthorizedAccessException()
        {
            currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

            var handler = CreateHandler();

            var command = new ChangePasswordCommand { CurrentPassword = "OldP@ssw0rd", NewPassword = "NewP@ssw0rd1" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User Not Authorize");

            userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserIdNotFoundInStore_ThrowsUnauthorizedAccessException()
        {
            var userId = Guid.NewGuid();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
            userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

            var handler = CreateHandler();

            var command = new ChangePasswordCommand { CurrentPassword = "OldP@ssw0rd", NewPassword = "NewP@ssw0rd1" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User Not Found");

            userManagerMock.Verify(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_IdentityRejectsPasswordChange_ThrowsForbiddenExceptionWithCombinedErrors()
        {
            var userId = Guid.NewGuid();
            var user = CreateUser(userId);

            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
            userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var failure = IdentityResult.Failed(
                new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." });

            userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, "WrongCurrent", "NewP@ssw0rd1"))
                .ReturnsAsync(failure);

            var handler = CreateHandler();

            var command = new ChangePasswordCommand { CurrentPassword = "WrongCurrent", NewPassword = "NewP@ssw0rd1" };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("Incorrect password.");
        }
    }
}