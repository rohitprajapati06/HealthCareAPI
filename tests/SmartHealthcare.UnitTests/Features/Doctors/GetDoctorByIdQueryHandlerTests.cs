using FluentAssertions;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Features.Doctors.Queries.GetDoctorsById;
using SmartHealthcare.Domain.Enums;
using SmartHealthcare.UnitTests.Common;

namespace SmartHealthcare.UnitTests.Features.Doctors
{
    public class GetDoctorByIdQueryHandlerTests
    {
        private GetDoctorByIdQueryHandler CreateHandler(TestApplicationDbContext context)
            => new(context);

        [Fact]
        public async Task Handle_ExistingApprovedDoctor_ReturnsMappedResponse()
        {
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital("City Hospital");
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Approved, "Rohan", "Shah");

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetDoctorsByIdQuery(doctor.Id), CancellationToken.None);

            response.Id.Should().Be(doctor.Id);
            response.UserId.Should().Be(doctor.UserId);
            response.FirstName.Should().Be("Rohan");
            response.LastName.Should().Be("Shah");
            response.HospitalName.Should().Be("City Hospital");
            response.ConsultationFee.Should().Be(doctor.ConsultationFee);
        }

        [Fact]
        public async Task Handle_PendingDoctor_IsStillReturned()
        {
            // NOTE: unlike GetDoctorsQueryHandler (list), this by-id lookup has no
            // ApprovalStatus filter at all - a Pending or Rejected doctor's profile is
            // still retrievable by anyone who knows (or guesses) the id. Documenting the
            // actual behavior rather than assuming it's wrong, since a details/admin screen
            // might legitimately need this.
            using var context = TestDbContextFactory.Create();
            var hospital = AppointmentTestData.CreateHospital();
            var doctor = AppointmentTestData.CreateDoctor(hospital.Id, DoctorApprovalStatus.Pending);

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.SaveChanges();

            var handler = CreateHandler(context);

            var response = await handler.Handle(new GetDoctorsByIdQuery(doctor.Id), CancellationToken.None);

            response.Id.Should().Be(doctor.Id);
        }

        [Fact]
        public async Task Handle_DoctorNotFound_ThrowsNotFoundException()
        {
            using var context = TestDbContextFactory.Create();
            var handler = CreateHandler(context);

            var act = () => handler.Handle(new GetDoctorsByIdQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Doctor not found");
        }
    }
}
