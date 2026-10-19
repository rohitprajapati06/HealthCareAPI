using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.UnitTests.Common
{
    /// <summary>
    /// Helpers for building the Hospital/DoctorProfile/PatientProfile/AvailabilitySlot graph
    /// that Appointment handlers validate against.
    /// </summary>
    public static class AppointmentTestData
    {
        public static Hospital CreateHospital(string name = "City Hospital")
            => new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                RohiniCode = "RH-" + Guid.NewGuid().ToString("N")[..6],
                Address = "1 Health St",
                City = "Mumbai",
                State = "MH",
                Country = "India"
            };

        public static DoctorProfile CreateDoctor(
            Guid hospitalId,
            DoctorApprovalStatus approvalStatus = DoctorApprovalStatus.Approved,
            string firstName = "Rohan",
            string lastName = "Shah")
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                UserName = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com",
                Email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com",
                FirstName = firstName,
                LastName = lastName
            };

            return new DoctorProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                HospitalId = hospitalId,
                Specialization = "Cardiology",
                ExperienceYears = 8,
                ConsultationFee = 1500m,
                Qualification = "MBBS, MD",
                ApprovalStatus = approvalStatus
            };
        }

        public static PatientProfile CreatePatient(string firstName = "Asha", string lastName = "Verma", Guid? userId = null)
        {
            var resolvedUserId = userId ?? Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = resolvedUserId,
                UserName = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com",
                Email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@example.com",
                FirstName = firstName,
                LastName = lastName
            };

            return new PatientProfile
            {
                Id = Guid.NewGuid(),
                UserId = resolvedUserId,
                User = user,
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                BloodGroup = "B+",
                Allergies = "None",
                ExistingConditions = "None"
            };
        }

        public static AvailabilitySlot CreateSlot(Guid doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime, bool isBooked = false)
            => new()
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
                IsBooked = isBooked
            };

        /// <summary>
        /// Seeds a hospital, an approved doctor at that hospital, a patient, and a bookable
        /// future slot for the doctor. Saves everything and returns the pieces so callers can
        /// build a BookAppointmentCommand (or similar) against them.
        /// </summary>
        public static (Hospital Hospital, DoctorProfile Doctor, PatientProfile Patient, AvailabilitySlot Slot) SeedBookableGraph(
            TestApplicationDbContext context)
        {
            var hospital = CreateHospital();
            var doctor = CreateDoctor(hospital.Id);
            var patient = CreatePatient();
            var slot = CreateSlot(
                doctor.Id,
                DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                new TimeOnly(10, 0),
                new TimeOnly(10, 30));

            context.Hospitals.Add(hospital);
            context.DoctorProfiles.Add(doctor);
            context.PatientProfiles.Add(patient);
            context.AvailabilitySlots.Add(slot);
            context.SaveChanges();

            return (hospital, doctor, patient, slot);
        }


        public static Appointment CreateAppointment(
            Guid doctorId,
            Guid patientId,
            Guid hospitalId,
            Guid availabilitySlotId,
            DateTime appointmentDate,
            AppointmentStatus status = AppointmentStatus.Pending)
        {
            return new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                PatientId = patientId,
                HospitalId = hospitalId,
                AvailabilitySlotId = availabilitySlotId,
                AppointmentDate = appointmentDate,
                Status = status,
                Notes = string.Empty
            };
        }

    }
}