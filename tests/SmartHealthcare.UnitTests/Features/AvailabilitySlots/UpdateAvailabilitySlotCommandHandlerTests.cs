using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.UpdateAvailabilitySlot;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.AvailabilitySlots
{
    public class UpdateAvailabilitySlotCommandHandlerTests
    {
        private readonly Mock<ILogger<UpdateAvailabilitySlotCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private UpdateAvailabilitySlotCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, AvailabilitySlot Slot) SeedDoctorWithSlot(
            TestApplicationDbContext context, DateOnly? date = null, bool isBooked = false)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var slot = AppointmentTestData.CreateSlot(
                doctor.Id, date ?? DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(9, 0), new TimeOnly(9, 30), isBooked);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.AvailabilitySlots.Add(slot);
            context.SaveChanges();

            return (hospital, doctor, slot);
        }

        private static UpdateAvailabilitySlotCommand CommandFor(DoctorProfile doctor, AvailabilitySlot slot, DateOnly? date = null)
            => new()
            {
                SlotId = slot.Id,
                DoctorId = doctor.Id,
                Date = date ?? slot.Date,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(11, 30)
            };

        [Fact]
        public async Task Handle_SlotNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var command = new UpdateAvailabilitySlotCommand
            {
                SlotId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(9, 30)
            };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("There is no slots booked");
        }

        [Fact]
        public async Task Handle_SlotBelongsToDifferentDoctorThanRoute_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            var handler = CreateHandler(context);
            var command = CommandFor(seeded.Doctor, seeded.Slot);
            command.DoctorId = Guid.NewGuid();

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You cannot update the another doctors slots ");
        }

        [Fact]
        public async Task Handle_DoctorRoleImpersonatingAnotherDoctorsRouteId_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = CreateHandler(context);
            var command = CommandFor(seeded.Doctor, seeded.Slot);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You cannot update the another doctors slots ");
        }

        [Fact]
        public async Task Handle_BookedSlot_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context, isBooked: true);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor(seeded.Doctor, seeded.Slot);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Booked availability slots cannot be updated.");
        }

        [Fact]
        public async Task Handle_PastSlot_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context, date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor(seeded.Doctor, seeded.Slot, date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Past availability slots cannot be updated.");
        }

        [Fact]
        public async Task Handle_EndTimeNotAfterStartTime_ThrowsConflictException()
        {
            // NOTE: unlike CreateAvailabilitySlotCommandHandler (which throws
            // BadRequestException for the same invalid input), this handler throws
            // ConflictException for a non-positive time range. Documents the actual,
            // inconsistent behavior.
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor(seeded.Doctor, seeded.Slot);
            command.EndTime = command.StartTime;

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("End time must be greater than the start time");
        }

        [Fact]
        public async Task Handle_OverlapsWithAnotherSlot_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            var otherSlot = AppointmentTestData.CreateSlot(seeded.Doctor.Id, seeded.Slot.Date, new TimeOnly(11, 0), new TimeOnly(12, 0));
            context.AvailabilitySlots.Add(otherSlot);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor(seeded.Doctor, seeded.Slot); // new times 11:00-11:30 overlap otherSlot's 11:00-12:00

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("This slot is overlap with another slot");
        }

        [Fact]
        public async Task Handle_ValidUpdate_UpdatesSlotFieldsAndReturnsUnit()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var newDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
            var command = CommandFor(seeded.Doctor, seeded.Slot, date: newDate);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);

            var updated = context.AvailabilitySlots.Single(s => s.Id == seeded.Slot.Id);
            updated.Date.Should().Be(newDate);
            updated.StartTime.Should().Be(new TimeOnly(11, 0));
            updated.EndTime.Should().Be(new TimeOnly(11, 30));
        }
    }
}
