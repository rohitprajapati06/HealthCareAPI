using FluentAssertions;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterDoctor;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;

namespace SmartHealthcare.UnitTests.Features.Auth
{
    public class RegisterDoctorCommandValidatorTests
    {
        private readonly RegisterDoctorCommandValidator validator = new();

        private static RegisterDoctorCommand ValidCommand() => new()
        {
            FirstName = "Rohan",
            LastName = "Shah",
            Email = "rohan.shah@example.com",
            PhoneNumber = "9876543210",
            Password = "Str0ngP@ssw0rd",
            HospitalId = Guid.NewGuid(),
            Specialization = "Cardiology",
            ExperienceYears = 8,
            ConsultationFee = 1500m,
            Qualification = "MBBS, MD"
        };

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var result = validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyHospitalId_HasValidationError()
        {
            var command = ValidCommand();
            command.HospitalId = Guid.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterDoctorCommand.HospitalId));
        }

        [Fact]
        public void Validate_EmptySpecialization_HasValidationError()
        {
            var command = ValidCommand();
            command.Specialization = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterDoctorCommand.Specialization));
        }

        [Fact]
        public void Validate_PhoneNumberExceedsMaxLength_HasValidationError()
        {
            var command = ValidCommand();
            command.PhoneNumber = "12345678901"; // 11 digits, limit is 10

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterDoctorCommand.PhoneNumber));
        }

        [Fact]
        public void Validate_EmptyPhoneNumber_HasValidationError()
        {
            var command = ValidCommand();
            command.PhoneNumber = string.Empty;

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterDoctorCommand.PhoneNumber));
        }

        [Fact]
        public void Validate_MalformedEmail_HasValidationError()
        {
            var command = ValidCommand();
            command.Email = "not-an-email";

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterDoctorCommand.Email));
        }

        [Fact]
        public void Validate_PasswordTooShort_HasValidationError()
        {
            var command = ValidCommand();
            command.Password = "Sh0rt!";

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterDoctorCommand.Password));
        }
    }
}
