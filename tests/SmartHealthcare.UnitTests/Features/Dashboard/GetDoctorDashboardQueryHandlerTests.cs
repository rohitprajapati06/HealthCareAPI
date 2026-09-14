using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetDoctorDashboard;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Dashboard
{
    public class GetDoctorDashboardQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetDoctorDashboardQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor) SeedDashboardData(TestApplicationDbContext context)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var patientA = AppointmentTestData.CreatePatient("Asha", "Verma");
            var patientB = AppointmentTestData.CreatePatient("Priya", "Nair");

            var today = DateTime.Now;
            var todaySlot = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(today), new TimeOnly(9, 0), new TimeOnly(9, 30), isBooked: true);
            var freeSlot1 = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(today.AddDays(3)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            var freeSlot2 = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(today.AddDays(4)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            var bookedSlot = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(today.AddDays(-1)), new TimeOnly(9, 0), new TimeOnly(9, 30), isBooked: true);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.AddRange(patientA, patientB);
            context.AvailabilitySlots.AddRange(todaySlot, freeSlot1, freeSlot2, bookedSlot);

            // Today, pending -> counts toward TodayAppointments and PendingAppointments.
            var todayPending = AppointmentTestData.CreateAppointment(doctor.Id, patientA.Id, hospital.Id, todaySlot.Id, today, AppointmentStatus.Pending);
            // Today, completed -> counts toward TodayAppointments, CompletedAppointments, and PatientsTreated.
            var todayCompleted = AppointmentTestData.CreateAppointment(doctor.Id, patientB.Id, hospital.Id, todaySlot.Id, today, AppointmentStatus.Completed);
            // Tomorrow, pending -> counts toward PendingAppointments only.
            var futurePending = AppointmentTestData.CreateAppointment(doctor.Id, patientA.Id, hospital.Id, freeSlot1.Id, today.AddDays(1), AppointmentStatus.Pending);
            // Yesterday, completed, SAME patient as todayCompleted -> tests PatientsTreated uses Distinct().
            var pastCompletedSamePatient = AppointmentTestData.CreateAppointment(doctor.Id, patientB.Id, hospital.Id, bookedSlot.Id, today.AddDays(-1), AppointmentStatus.Completed);

            context.Appointments.AddRange(todayPending, todayCompleted, futurePending, pastCompletedSamePatient);

            context.Prescriptions.Add(new()
            {
                Id = Guid.NewGuid(),
                AppointmentId = todayCompleted.Id,
                DoctorId = doctor.Id,
                Medication = "Amoxicillin",
                Instructions = "3 times a day"
            });
            context.Prescriptions.Add(new()
            {
                Id = Guid.NewGuid(),
                AppointmentId = pastCompletedSamePatient.Id,
                DoctorId = doctor.Id,
                Medication = "Ibuprofen",
                Instructions = "Once a day"
            });

            context.SaveChanges();

            return (hospital, doctor);
        }

        [Fact]
        public async Task Handle_DoctorRoleRequestingOwnDashboard_ReturnsAccurateCounts()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDashboardData(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetDoctorDashboardQuery(seeded.Doctor.Id), CancellationToken.None);

            response.TodayAppointments.Should().Be(2);
            response.PendingAppointments.Should().Be(2);
            response.CompletedAppointments.Should().Be(2);
            response.AvailableSlots.Should().Be(2);
            response.PrescriptionsCreated.Should().Be(2);
            response.PatientsTreated.Should().Be(1); // distinct - same patient in both completed appointments
        }

        [Fact]
        public async Task Handle_DoctorRoleRequestingAnotherDoctorsDashboard_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDashboardData(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetDoctorDashboardQuery(seeded.Doctor.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view this dashboard.");
        }

        [Fact]
        public async Task Handle_NonDoctorRole_BypassesOwnershipCheckAndReturnsCounts()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDashboardData(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetDoctorDashboardQuery(seeded.Doctor.Id), CancellationToken.None);

            response.TodayAppointments.Should().Be(2);
        }

        [Fact]
        public async Task Handle_NonExistentDoctorId_NonDoctorRole_ReturnsAllZeroCountsInsteadOfNotFound()
        {
            // NOTE: unlike GetPatientDashboardQueryHandler (which throws NotFoundException for
            // an unknown PatientId), this handler never checks whether the DoctorId actually
            // exists - a non-Doctor caller passing a random/nonexistent Guid just gets a
            // dashboard full of zeros instead of a 404. Documents the actual, inconsistent
            // behavior between the two handlers.
            using var context = TestDbContextFactory.Create();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetDoctorDashboardQuery(Guid.NewGuid()), CancellationToken.None);

            response.TodayAppointments.Should().Be(0);
            response.PendingAppointments.Should().Be(0);
            response.CompletedAppointments.Should().Be(0);
            response.AvailableSlots.Should().Be(0);
            response.PrescriptionsCreated.Should().Be(0);
            response.PatientsTreated.Should().Be(0);
        }
    }
}
