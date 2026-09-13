using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Features.Doctors.Commands.ApproveDoctors;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Doctors
{
    public class ApproveDoctorCommandHandlerTests
    {
        private readonly Mock<ILogger<ApproveDoctorCommandHandler>> loggerMock = new();

        private ApproveDoctorCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object);

        [Fact]
        public async Task Handle_PendingDoctor_ApprovesAndReturnsDoctorId()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Pending);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var result = await handler.Handle(new ApproveDoctorCommand(doctor.Id), CancellationToken.None);

            result.Should().Be(doctor.Id);
            context.DoctorProfiles.Single(d => d.Id == doctor.Id).ApprovalStatus.Should().Be(DoctorApprovalStatus.Approved);
        }

        [Fact]
        public async Task Handle_RejectedDoctor_CanStillBeApproved()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Rejected);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            await handler.Handle(new ApproveDoctorCommand(doctor.Id), CancellationToken.None);

            context.DoctorProfiles.Single(d => d.Id == doctor.Id).ApprovalStatus.Should().Be(DoctorApprovalStatus.Approved);
        }

        [Fact]
        public async Task Handle_DoctorNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new ApproveDoctorCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor Not Found");
        }

        [Fact]
        public async Task Handle_AlreadyApproved_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Approved);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new ApproveDoctorCommand(doctor.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Doctor is already approved.");
        }
    }
}
