using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.DeleteAvailabilitySlot;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.AvailabilitySlots
{
    public class DeleteAvailabilitySlotCommandHandlerTests
    {
        private readonly Mock<ILogger<DeleteAvailabilitySlotCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private DeleteAvailabilitySlotCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, AvailabilitySlot Slot) SeedDoctorWithSlot(
            TestApplicationDbContext context, bool isBooked = false)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var slot = AppointmentTestData.CreateSlot(
                doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(9, 0), new TimeOnly(9, 30), isBooked);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.AvailabilitySlots.Add(slot);
            context.SaveChanges();

            return (hospital, doctor, slot);
        }

        [Fact]
        public async Task Handle_SlotNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new DeleteAvailabilitySlotCommand { SlotId = Guid.NewGuid(), DoctorId = Guid.NewGuid() }, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Availability slot not found.");
        }

        [Fact]
        public async Task Handle_SlotBelongsToDifferentDoctorThanRoute_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            var handler = CreateHandler(context);
            var command = new DeleteAvailabilitySlotCommand { SlotId = seeded.Slot.Id, DoctorId = Guid.NewGuid() };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to delete another doctor's availability slot.");

            context.AvailabilitySlots.Should().ContainSingle();
        }

        [Fact]
        public async Task Handle_DoctorRoleDeletingOwnSlot_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);
            var result = await handler.Handle(new DeleteAvailabilitySlotCommand { SlotId = seeded.Slot.Id, DoctorId = seeded.Doctor.Id }, CancellationToken.None);

            result.Should().Be(Unit.Value);
            context.AvailabilitySlots.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_DoctorRoleImpersonatingAnotherDoctorsRouteId_ThrowsForbiddenException()
        {
            // The route DoctorId matches the slot's DoctorId, but the caller (identified via
            // ICurrentUserService.UserId) is not actually that doctor.
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = CreateHandler(context);
            var command = new DeleteAvailabilitySlotCommand { SlotId = seeded.Slot.Id, DoctorId = seeded.Doctor.Id };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to delete another doctor's availability slot.");

            context.AvailabilitySlots.Should().ContainSingle();
        }

        [Fact]
        public async Task Handle_NonDoctorRole_BypassesCallerIdentityCheckAndSucceeds()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            await handler.Handle(new DeleteAvailabilitySlotCommand { SlotId = seeded.Slot.Id, DoctorId = seeded.Doctor.Id }, CancellationToken.None);

            context.AvailabilitySlots.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_BookedSlot_ThrowsConflictExceptionAndDoesNotDelete()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDoctorWithSlot(context, isBooked: true);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);
            var command = new DeleteAvailabilitySlotCommand { SlotId = seeded.Slot.Id, DoctorId = seeded.Doctor.Id };

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Booked availability slots cannot be deleted.");

            context.AvailabilitySlots.Should().ContainSingle();
        }
    }
}
