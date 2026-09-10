using FluentAssertions;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Appointments.Queries.GetAppointmentById;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class GetAppointmentByIdQueryHandlerTests
    {
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private GetAppointmentByIdQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context, currentUserServiceMock.Object);

        private static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, AvailabilitySlot Slot, Appointment Appointment) SeedAppointment(
            TestApplicationDbContext context)
        {
            var graph = AppointmentTestData.SeedBookableGraph(context);
            var appointment = AppointmentTestData.CreateAppointment(
                graph.Doctor.Id, graph.Patient.Id, graph.Hospital.Id, graph.Slot.Id,
                graph.Slot.Date.ToDateTime(graph.Slot.StartTime), AppointmentStatus.Confirmed);
            appointment.Notes = "Follow-up";

            context.Appointments.Add(appointment);
            context.SaveChanges();

            return (graph.Hospital, graph.Doctor, graph.Patient, graph.Slot, appointment);
        }

        [Fact]
        public async Task Handle_NonPatientNonDoctorRole_ReturnsMappedResponse()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetAppointmentByIdQuery(seeded.Appointment.Id), CancellationToken.None);

            response.Id.Should().Be(seeded.Appointment.Id);
            response.DoctorId.Should().Be(seeded.Doctor.Id);
            response.DoctorName.Should().Be($"{seeded.Doctor.User.FirstName} {seeded.Doctor.User.LastName}");
            response.PatientId.Should().Be(seeded.Patient.Id);
            response.PatientName.Should().Be($"{seeded.Patient.User.FirstName} {seeded.Patient.User.LastName}");
            response.HospitalId.Should().Be(seeded.Hospital.Id);
            response.HospitalName.Should().Be(seeded.Hospital.Name);
            response.Notes.Should().Be("Follow-up");
            response.Status.Should().Be("Confirmed");
        }

        [Fact]
        public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetAppointmentByIdQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Appointment not found");
        }

        [Fact]
        public async Task Handle_PatientRoleViewingOwnAppointment_ReturnsResponse()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Patient.UserId);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetAppointmentByIdQuery(seeded.Appointment.Id), CancellationToken.None);

            response.Id.Should().Be(seeded.Appointment.Id);
        }

        [Fact]
        public async Task Handle_PatientRoleViewingAnotherPatientsAppointment_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetAppointmentByIdQuery(seeded.Appointment.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view this appointment.");
        }

        [Fact]
        public async Task Handle_DoctorRoleViewingOwnAppointment_ReturnsResponse()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(seeded.Doctor.UserId);

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetAppointmentByIdQuery(seeded.Appointment.Id), CancellationToken.None);

            response.Id.Should().Be(seeded.Appointment.Id);
        }

        [Fact]
        public async Task Handle_DoctorRoleViewingAnotherDoctorsAppointment_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var seeded = SeedAppointment(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(seeded.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);
            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Doctor)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherDoctor.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetAppointmentByIdQuery(seeded.Appointment.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("You are not allowed to view this appointment.");
        }
    }
}
