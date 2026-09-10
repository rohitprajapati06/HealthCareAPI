using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterDoctor;
// RegisterDoctorCommand itself lives in the RegisterPatient namespace (copy/paste artifact
// in the source), while RegisterDoctorCommandHandler lives in RegisterDoctor - both usings
// are required.
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class RegisterDoctorCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock = UserManagerMockFactory.Create();
        private readonly Mock<ILogger<RegisterDoctorCommandHandler>> loggerMock = new();

        private RegisterDoctorCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(userManagerMock.Object, context, loggerMock.Object);

        private static RegisterDoctorCommand ValidCommand() => new()
        {
            FirstName = "Rohan",
            LastName = "Shah",
            Email = "rohan.shah@example.com",
            PhoneNumber = "9876543210",
            Password = "Str0ngP@ssw0rd",
            HospitalId = Guid.NewGuid(),
            Specialization = "Cardiology",
            ExperienceYears = 8,
            ConsultationFee = 1500m,
            Qualification = "MBBS, MD"
        };

        [Fact]
        public async Task Handle_ValidRequest_CreatesUserAndDoctorProfilePendingApprovalAndReturnsUserId()
        {
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((ApplicationUser?)null);
            userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = CreateHandler(context);

            var userId = await handler.Handle(command, CancellationToken.None);

            userId.Should().NotBeEmpty();

            var profile = context.DoctorProfiles.Should().ContainSingle().Subject;
            profile.UserId.Should().Be(userId);
            profile.HospitalId.Should().Be(command.HospitalId);
            profile.Specialization.Should().Be("Cardiology");
            profile.ExperienceYears.Should().Be(8);
            profile.ConsultationFee.Should().Be(1500m);
            profile.Qualification.Should().Be("MBBS, MD");
            profile.ApprovalStatus.Should().Be(DoctorApprovalStatus.Pending);

            userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ThrowsConflictExceptionAndDoesNotCreateUser()
        {
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(new ApplicationUser { Email = command.Email });

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Email already exists");

            userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            context.DoctorProfiles.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_IdentityCreateFails_ThrowsBadRequestExceptionWithCombinedErrorsAndDoesNotCreateProfile()
        {
            using var context = TestDbContextFactory.Create();
            var command = ValidCommand();

            userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((ApplicationUser?)null);

            var failure = IdentityResult.Failed(
                new IdentityError { Code = "DuplicateUserName", Description = "Username already taken." });

            userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(failure);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Username already taken.");

            userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            context.DoctorProfiles.Should().BeEmpty();
        }
    }
}
