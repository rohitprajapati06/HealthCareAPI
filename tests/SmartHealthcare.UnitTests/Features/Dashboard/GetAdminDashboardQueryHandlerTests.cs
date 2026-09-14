using FluentAssertions;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetAdminDashboard;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Dashboard
{
    public class GetAdminDashboardQueryHandlerTests
    {
        private GetAdminDashboardQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_MixedData_ReturnsAccurateCountsAcrossAllEntities()
        {
            using var context = TestDbContextFactory.Create();

            var hospitalA = AppointmentTestData.CreateHospital("Hospital A");
            var hospitalB = AppointmentTestData.CreateHospital("Hospital B");
            context.Hospitals.AddRange(hospitalA, hospitalB);

            var approvedDoctor = AppointmentTestData.CreateDoctor(hospitalA.Id, DoctorApprovalStatus.Approved);
            var pendingDoctor = AppointmentTestData.CreateDoctor(hospitalA.Id, DoctorApprovalStatus.Pending, "Neha", "Kapoor");
            var rejectedDoctor = AppointmentTestData.CreateDoctor(hospitalB.Id, DoctorApprovalStatus.Rejected, "Amit", "Rao");
            context.DoctorProfiles.AddRange(approvedDoctor, pendingDoctor, rejectedDoctor);

            var patient1 = AppointmentTestData.CreatePatient("Asha", "Verma");
            var patient2 = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.AddRange(patient1, patient2);

            var slot = AppointmentTestData.CreateSlot(approvedDoctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(9, 0), new TimeOnly(9, 30));
            context.AvailabilitySlots.Add(slot);

            var completedAppointment = AppointmentTestData.CreateAppointment(
                approvedDoctor.Id, patient1.Id, hospitalA.Id, slot.Id, DateTime.Now.AddDays(-1), AppointmentStatus.Completed);
            var pendingAppointment = AppointmentTestData.CreateAppointment(
                approvedDoctor.Id, patient1.Id, hospitalA.Id, slot.Id, DateTime.Now.AddDays(1), AppointmentStatus.Pending);
            var cancelledAppointment = AppointmentTestData.CreateAppointment(
                approvedDoctor.Id, patient2.Id, hospitalA.Id, slot.Id, DateTime.Now.AddDays(2), AppointmentStatus.Cancelled);
            context.Appointments.AddRange(completedAppointment, pendingAppointment, cancelledAppointment);

            context.Prescriptions.Add(new()
            {
                Id = Guid.NewGuid(),
                AppointmentId = completedAppointment.Id,
                DoctorId = approvedDoctor.Id,
                Medication = "Amoxicillin",
                Instructions = "3 times a day"
            });

            context.MedicalRecords.Add(MedicalRecordTestData.CreateRecord(patient1, hospitalA));

            context.SaveChanges();

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetAdminDashboardQuery(), CancellationToken.None);

            response.TotalHospitals.Should().Be(2);
            response.TotalDoctors.Should().Be(3);
            response.ApprovedDoctors.Should().Be(1);
            response.PendingDoctors.Should().Be(1);
            response.RejectedDoctors.Should().Be(1);
            response.TotalPatients.Should().Be(2);
            response.TotalAppointments.Should().Be(3);
            response.CompletedAppointments.Should().Be(1);
            response.PendingAppointments.Should().Be(1);
            response.CancelledAppointments.Should().Be(1);
            response.TotalPrescriptions.Should().Be(1);
            response.TotalMedicalRecords.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyDatabase_ReturnsAllZeroCounts()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetAdminDashboardQuery(), CancellationToken.None);

            response.TotalHospitals.Should().Be(0);
            response.TotalDoctors.Should().Be(0);
            response.TotalPatients.Should().Be(0);
            response.TotalAppointments.Should().Be(0);
            response.TotalPrescriptions.Should().Be(0);
            response.TotalMedicalRecords.Should().Be(0);
        }
    }
}
