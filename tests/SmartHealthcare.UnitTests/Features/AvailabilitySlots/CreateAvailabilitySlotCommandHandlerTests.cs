using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using Microsoft.Extensions.Logging;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.AvailabilitySlots
{
    public class CreateAvailabilitySlotCommandHandlerTests
    {
        private readonly Mock<ILogger<CreateAvailabilitySlotCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private CreateAvailabilitySlotCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor) SeedApprovedDoctor(
            TestApplicationDbContext context, DoctorApprovalStatus status = DoctorApprovalStatus.Approved)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, status);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            return (hospital, doctor);
        }

        private static CreateAvailabilitySlot CommandFor(DoctorProfile doctor, DateOnly? date = null)
            => new()
            {
                DoctorId = doctor.Id,
                Date = date ?? DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(9, 30)
            };

        [Fact]
        public async Task Handle_NonDoctorRole_ValidRequest_CreatesSlotAndReturnsId()
        {
            using var context = TestDbContextFactory.Create();
            var (_, doctor) = SeedApprovedDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var slotId = await handler.Handle(CommandFor(doctor), CancellationToken.None);

            slotId.Should().NotBeEmpty();
            var slot = context.AvailabilitySlots.Single(s => s.Id == slotId);
            slot.DoctorId.Should().Be(doctor.Id);
            slot.IsBooked.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_DoctorRoleCreatingOwnSlot_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var (_, doctor) = SeedApprovedDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(doctor.UserId);

            var handler = CreateHandler(context);

            var slotId = await handler.Handle(CommandFor(doctor), CancellationToken.None);

            slotId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_DoctorRoleCreatingForAnotherDoctor_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var (_, doctor) = SeedApprovedDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid()); // not this doctor's user id

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(doctor), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You cannot create availability slots for another doctor.");

            context.AvailabilitySlots.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_DoctorNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var command = new CreateAvailabilitySlot
            {
                DoctorId = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(9, 30)
            };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found");
        }

        [Fact]
        public async Task Handle_DoctorNotApproved_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var (_, doctor) = SeedApprovedDoctor(context, DoctorApprovalStatus.Pending);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(doctor), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("Doctor is Not Approved");
        }

        [Fact]
        public async Task Handle_PastDate_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var (_, doctor) = SeedApprovedDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor(doctor, DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Cannot create slots for past dates.");
        }

        [Fact]
        public async Task Handle_EndTimeNotAfterStartTime_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var (_, doctor) = SeedApprovedDoctor(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor(doctor);
            command.EndTime = command.StartTime;

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("End time must be greater than start time");
        }

        [Fact]
        public async Task Handle_OverlapsWithExistingSlot_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var (_, doctor) = SeedApprovedDoctor(context);

            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            var existing = AppointmentTestData.CreateSlot(doctor.Id, date, new TimeOnly(9, 0), new TimeOnly(10, 0));
            context.AvailabilitySlots.Add(existing);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = new CreateAvailabilitySlot
            {
                DoctorId = doctor.Id,
                Date = date,
                StartTime = new TimeOnly(9, 30), // overlaps [9:00-10:00)
                EndTime = new TimeOnly(10, 30)
            };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Overlapping exists with the existing slot ");
        }
    }
}
