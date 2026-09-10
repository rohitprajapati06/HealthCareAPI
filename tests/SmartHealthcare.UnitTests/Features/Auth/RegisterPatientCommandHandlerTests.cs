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
    /// <summary>
    /// IMPORTANT: RegisterPatientCommandHandler has an uninitialized-field bug. It declares
    /// two loggers - `logger1` (ILogger&lt;RegisterPatientCommandHandler&gt;), which the
    /// constructor actually assigns the injected logger to, and `logger`
    /// (ILogger&lt;LoginCommandHandler&gt;), which is never assigned and stays null. Handle()
    /// calls the *unassigned* `logger` field on both the conflict path
    /// (logger.LogWarning before throwing ConflictException) and the success path
    /// (logger.LogInformation after SaveChangesAsync), so both currently throw
    /// NullReferenceException instead of behaving as intended. On the success path this is
    /// worse than it looks: the user and PatientProfile rows are already committed to the
    /// database by the time the NRE is thrown, so the caller sees a 500 error even though
    /// registration actually succeeded. The tests below document this actual behavior;
    /// see the class-level comment in the handler file before "fixing" these expectations -
    /// the real fix is in RegisterPatientCommandHandler (assign the constructor's logger
    /// parameter to the field it actually uses, or better, delete the redundant field and
    /// use one logger).
    /// </summary>
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
        public async Task Handle_ValidRequest_PersistsUserAndProfile_ButThrowsNullReferenceExceptionFromUnassignedLogger()
        {
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((ApplicationUser?)null);
            userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            // Documents the current bug - see class-level comment.
            await act.Should().ThrowAsync<NullReferenceException>();

            // Despite the exception, the write already happened.
            var savedProfile = context.PatientProfiles.Should().ContainSingle().Subject;
            savedProfile.Gender.Should().Be("Female");
            savedProfile.BloodGroup.Should().Be("B+");

            userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ThrowsNullReferenceExceptionFromUnassignedLogger_NotConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(new ApplicationUser { Email = command.Email });

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            // Intended behavior is ConflictException("Email Already Exists"); actual behavior
            // is NullReferenceException because logger.LogWarning runs first against the
            // unassigned field. See class-level comment.
            await act.Should().ThrowAsync<NullReferenceException>();

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
