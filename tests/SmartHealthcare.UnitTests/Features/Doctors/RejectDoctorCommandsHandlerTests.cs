using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Features.Doctors.Commands.RejectDoctors;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Doctors
{
    public class RejectDoctorCommandsHandlerTests
    {
        private readonly Mock<ILogger<RejectDoctorCommandsHandler>> loggerMock = new();

        private RejectDoctorCommandsHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object);

        [Fact]
        public async Task Handle_PendingDoctor_RejectsAndReturnsDoctorId()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Pending);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var result = await handler.Handle(new RejectDoctorCommands(doctor.Id), CancellationToken.None);

            result.Should().Be(doctor.Id);
            context.DoctorProfiles.Single(d => d.Id == doctor.Id).ApprovalStatus.Should().Be(DoctorApprovalStatus.Rejected);
        }

        [Fact]
        public async Task Handle_ApprovedDoctor_CanStillBeRejected()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Approved);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            await handler.Handle(new RejectDoctorCommands(doctor.Id), CancellationToken.None);

            context.DoctorProfiles.Single(d => d.Id == doctor.Id).ApprovalStatus.Should().Be(DoctorApprovalStatus.Rejected);
        }

        [Fact]
        public async Task Handle_DoctorNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new RejectDoctorCommands(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found");
        }

        [Fact]
        public async Task Handle_AlreadyRejected_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Rejected);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new RejectDoctorCommands(doctor.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Doctor is already rejected.");
        }
    }
}
