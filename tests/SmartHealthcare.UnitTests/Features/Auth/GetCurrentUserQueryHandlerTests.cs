using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Queries.GetCurrentUser;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class GetCurrentUserQueryHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock = UserManagerMockFactory.Create();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetCurrentUserQueryHandler CreateHandler()
            => new(userManagerMock.Object, currentUserServiceMock.Object);

        [Fact]
        public async Task Handle_AuthenticatedUser_ReturnsCurrentUserResponseWithRoles()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                FirstName = "Asha",
                LastName = "Verma",
                Email = "asha.verma@example.com"
            };

            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
            userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Patient" });

            var handler = CreateHandler();

            var response = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

            response.Id.Should().Be(userId);
            response.FirstName.Should().Be("Asha");
            response.LastName.Should().Be("Verma");
            response.Email.Should().Be("asha.verma@example.com");
            response.Roles.Should().ContainSingle().Which.Should().Be("Patient");
        }

        [Fact]
        public async Task Handle_NoAuthenticatedUser_ThrowsUnauthorizedAccessException()
        {
            currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

            var handler = CreateHandler();

            var act = () => handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User is not Authenticated");

            userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserIdNotFoundInStore_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
            userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

            var handler = CreateHandler();

            var act = () => handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found");

            userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}
