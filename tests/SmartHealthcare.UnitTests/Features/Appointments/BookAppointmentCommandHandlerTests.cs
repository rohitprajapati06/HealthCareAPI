using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Appointments
{
    public class BookAppointmentCommandHandlerTests
    {
        private readonly Mock<ILogger<BookAppointmentCommandHandler>> loggerMock = new();
        private readonly Mock<ICurrentUserService> currentUserServiceMock = new();

        private BookAppointmentCommandHandler CreateHandler(TestApplicationDbContext context)
            => new(context, loggerMock.Object, currentUserServiceMock.Object);

        private static BookAppointmentCommand CommandFor(
            (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, AvailabilitySlot Slot) graph)
            => new()
            {
                DoctorId = graph.Doctor.Id,
                PatientId = graph.Patient.Id,
                HospitalId = graph.Hospital.Id,
                AvailabilitySlotId = graph.Slot.Id,
                Notes = "First visit"
            };

        [Fact]
        public async Task Handle_NonPatientRole_ValidRequest_CreatesAppointmentAndMarksSlotBooked()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var appointmentId = await handler.Handle(CommandFor(graph), CancellationToken.None);

            appointmentId.Should().NotBeEmpty();

            var appointment = context.Appointments.Should().ContainSingle().Subject;
            appointment.Id.Should().Be(appointmentId);
            appointment.DoctorId.Should().Be(graph.Doctor.Id);
            appointment.PatientId.Should().Be(graph.Patient.Id);
            appointment.HospitalId.Should().Be(graph.Hospital.Id);
            appointment.AvailabilitySlotId.Should().Be(graph.Slot.Id);
            appointment.AppointmentDate.Should().Be(graph.Slot.Date.ToDateTime(graph.Slot.StartTime));
            appointment.Notes.Should().Be("First visit");

            var slot = context.AvailabilitySlots.Single(s => s.Id == graph.Slot.Id);
            slot.IsBooked.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PatientRoleBookingForSelf_Succeeds()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(graph.Patient.UserId);

            var handler = CreateHandler(context);

            var appointmentId = await handler.Handle(CommandFor(graph), CancellationToken.None);

            appointmentId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_PatientRoleBookingForAnotherPatient_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            var otherPatient = AppointmentTestData.CreatePatient("Priya", "Nair");
            context.PatientProfiles.Add(otherPatient);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(true);
            currentUserServiceMock.Setup(x => x.UserId).Returns(otherPatient.UserId);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You can only book appointments for yourself.");

            context.Appointments.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_DoctorNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var command = CommandFor(graph);
            command.DoctorId = Guid.NewGuid();

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found");
        }

        [Fact]
        public async Task Handle_DoctorNotApproved_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Pending);
            var patient = AppointmentTestData.CreatePatient();
            var slot = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(10, 30));

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.Add(patient);
            context.AvailabilitySlots.Add(slot);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor((hospital, doctor, patient, slot));

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("Doctor is not approved.");
        }

        [Fact]
        public async Task Handle_PatientNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var command = CommandFor(graph);
            command.PatientId = Guid.NewGuid();

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Patient not found");
        }

        [Fact]
        public async Task Handle_HospitalNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var command = CommandFor(graph);
            command.HospitalId = Guid.NewGuid();

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Hospital not found");
        }

        [Fact]
        public async Task Handle_DoctorDoesNotBelongToHospital_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            var otherHospital = AppointmentTestData.CreateHospital("Other Hospital");
            context.Hospitals.Add(otherHospital);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var command = CommandFor(graph);
            command.HospitalId = otherHospital.Id;

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Doctor does not belong to the selected hospital.");
        }

        [Fact]
        public async Task Handle_SlotNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var command = CommandFor(graph);
            command.AvailabilitySlotId = Guid.NewGuid();

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Slot not found");
        }

        [Fact]
        public async Task Handle_SlotInThePast_ThrowsBadRequestException()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var patient = AppointmentTestData.CreatePatient();
            var pastSlot = AppointmentTestData.CreateSlot(doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), new TimeOnly(10, 0), new TimeOnly(10, 30));

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.Add(patient);
            context.AvailabilitySlots.Add(pastSlot);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor((hospital, doctor, patient, pastSlot));

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Cannot book past appointment slots.");
        }

        [Fact]
        public async Task Handle_SlotBelongsToAnotherDoctor_ThrowsForbiddenException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            var otherDoctor = AppointmentTestData.CreateDoctor(graph.Hospital.Id, firstName: "Neha", lastName: "Kapoor");
            context.DoctorProfiles.Add(otherDoctor);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var command = CommandFor(graph);
            command.DoctorId = otherDoctor.Id;

            var handler = CreateHandler(context);

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>().WithMessage("Slot does not belong to the doctor");
        }

        [Fact]
        public async Task Handle_SlotAlreadyBooked_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id);
            var patient = AppointmentTestData.CreatePatient();
            var bookedSlot = AppointmentTestData.CreateSlot(
                doctor.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(10, 30), isBooked: true);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.Add(patient);
            context.AvailabilitySlots.Add(bookedSlot);
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);
            var command = CommandFor((hospital, doctor, patient, bookedSlot));

            var act = () => handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Slot is already booked ");
        }

        [Fact]
        public async Task Handle_PatientAlreadyHasAppointmentAtSameDateTime_ThrowsConflictException()
        {
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            context.Appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(), // a different doctor - only the date/time + patient matters for this check
                PatientId = graph.Patient.Id,
                HospitalId = graph.Hospital.Id,
                AvailabilitySlotId = Guid.NewGuid(),
                AppointmentDate = graph.Slot.Date.ToDateTime(graph.Slot.StartTime)
            });
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var act = () => handler.Handle(CommandFor(graph), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>().WithMessage("Patient already has an appointment at this time.");
        }

        [Fact]
        public async Task Handle_DoctorAlreadyHasAppointmentAtSameDateTime_BookingStillSucceeds()
        {
            // NOTE: the handler computes `doctorBusy` but never checks it - there is no
            // `if (doctorBusy) throw ...` anywhere in the method, so doctor double-booking is
            // NOT actually prevented despite the check being written. This test documents
            // that current (likely unintended) behavior. If you fix this, this test's
            // expectation should flip to ThrowAsync<ConflictException>().
            using var context = TestDbContextFactory.Create();
            var graph = AppointmentTestData.SeedBookableGraph(context);

            context.Appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = graph.Doctor.Id,
                PatientId = Guid.NewGuid(), // a different patient
                HospitalId = graph.Hospital.Id,
                AvailabilitySlotId = Guid.NewGuid(),
                AppointmentDate = graph.Slot.Date.ToDateTime(graph.Slot.StartTime)
            });
            context.SaveChanges();

            currentUserServiceMock.Setup(x => x.IsInRole(UserRoles.Patient)).Returns(false);

            var handler = CreateHandler(context);

            var appointmentId = await handler.Handle(CommandFor(graph), CancellationToken.None);

            appointmentId.Should().NotBeEmpty();
            context.Appointments.Should().HaveCount(2);
        }
    }
}
