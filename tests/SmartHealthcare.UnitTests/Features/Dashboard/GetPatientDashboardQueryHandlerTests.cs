using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetPatientDashboard;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Dashboard
{
    public class GetPatientDashboardQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetPatientDashboardQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient) SeedDashboardData(TestApplicationDbContext context)
        {
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var patient = AppointmentTestData.CreatePatient();

            var slot = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(9, 0), new TimeOnly(9, 30));

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.Add(patient);
            context.AvailabilitySlots.Add(slot);

            var upcoming = AppointmentTestData.CreateAppointment(doctor.Id, patient.Id, hospital.Id, slot.Id, DateTime.Now.AddDays(1), AppointmentStatus.Confirmed);
            var completed1 = AppointmentTestData.CreateAppointment(doctor.Id, patient.Id, hospital.Id, slot.Id, DateTime.Now.AddDays(-2), AppointmentStatus.Completed);
            var completed2 = AppointmentTestData.CreateAppointment(doctor.Id, patient.Id, hospital.Id, slot.Id, DateTime.Now.AddDays(-1), AppointmentStatus.Completed);
            var cancelled = AppointmentTestData.CreateAppointment(doctor.Id, patient.Id, hospital.Id, slot.Id, DateTime.Now.AddDays(-3), AppointmentStatus.Cancelled);

            context.Appointments.AddRange(upcoming, completed1, completed2, cancelled);

            context.Prescriptions.Add(new()
            {
                Id = Guid.NewGuid(),
                AppointmentId = completed1.Id,
                Appointment = completed1,
                DoctorId = doctor.Id,
                Medication = "Amoxicillin",
                Instructions = "3 times a day"
            });

            context.MedicalRecords.Add(MedicalRecordTestData.CreateRecord(patient, hospital));
            context.MedicalRecords.Add(MedicalRecordTestData.CreateRecord(patient, hospital, "Prescription scan"));

            context.SaveChanges();

            return (hospital, doctor, patient);
        }

        [Fact]
        public async Task Handle_PatientNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPatientDashboardQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Patient Not Found");
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingOwnDashboard_ReturnsAccurateCounts()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDashboardData(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Patient.UserId);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetPatientDashboardQuery(seeded.Patient.Id), CancellationToken.None);

            response.UpcomingAppointment.Should().Be(1);
            response.CompletedAppointment.Should().Be(2);
            response.CancelledAppointment.Should().Be(1);
            response.MedicalRecords.Should().Be(2);
            response.Prescriptions.Should().Be(1);
        }

        [Fact]
        public async Task Handle_PatientRoleRequestingAnotherPatientsDashboard_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDashboardData(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetPatientDashboardQuery(seeded.Patient.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view this dashboard.");
        }

        [Fact]
        public async Task Handle_NonPatientRole_BypassesOwnershipCheckAndReturnsCounts()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedDashboardData(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetPatientDashboardQuery(seeded.Patient.Id), CancellationToken.None);

            response.CompletedAppointment.Should().Be(2);
        }
    }
}
