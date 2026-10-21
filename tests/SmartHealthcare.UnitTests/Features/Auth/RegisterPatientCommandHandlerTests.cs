using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class RegisterPatientCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock = UserManagerMockFactory.Create();
        private readonly Mock<ILogger<RegisterPatientCommandHandler>> loggerMock = new();

        private RegisterPatientCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(userManagerMock.Object, context, loggerMock.Object);

        private static RegisterPatientCommand ValidCommand() => new()
        {
            FirstName = "Asha",
            LastName = "Verma",
            Email = "asha.verma@example.com",
            Password = "Str0ngP@ssw0rd",
            PhoneNumber = "9876543210",
            DateofBirth = new DateTime(1995, 4, 12),
            Gender = "Female",
            BloodGroup = "B+",
            Allergies = "None",
            ExistingConditions = "None"
        };

        [Fact]
        public async Task Handle_ValidRequest_PersistsUserAndProfileAndReturnsUserId()
        {
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((ApplicationUser?)null);
            userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .Callback<ApplicationUser, string>((user, _) => user.Id = Guid.NewGuid())
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = CreateHandler(context);

            var userId = await handler.Handle(command, CancellationToken.None);
            userId.Should().NotBeEmpty();

            // Despite the exception, the write already happened.
            var savedProfile = context.PatientProfiles.Should().ContainSingle().Subject;
            savedProfile.Gender.Should().Be("Female");
            savedProfile.BloodGroup.Should().Be("B+");

            userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(new ApplicationUser { Email = command.Email });

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Email Already Exists");

            userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            context.PatientProfiles.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_IdentityCreateFails_ThrowsBadRequestExceptionWithCombinedErrors()
        {
            // This path never touches the broken logger field, so it behaves as intended.
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((ApplicationUser?)null);

            var failure = IdentityResult.Failed(
                new IdentityError { Code = "PasswordTooWeak", Description = "Passwords must have at least one non alphanumeric character." });

            userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(failure);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Passwords must have at least one non alphanumeric character.");

            userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            context.PatientProfiles.Should().BeEmpty();
        }
    }
}
