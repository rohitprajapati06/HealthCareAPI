using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription;
using SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetPrescriptionById;
using SmartHealthcare.Application.Features.Prescriptions.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthCare.API.Controllers
{
    // Every action requires a logged-in user. Ownership (a patient/doctor only
    // touching their own prescriptions) is enforced inside the handlers.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IMediator mediator;

        public PrescriptionsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Authorize(Roles = $"{UserRoles.Doctor},{UserRoles.HospitalAdmin},{UserRoles.SuperAdmin}")]
        [HttpPost("CreatePrescription")]
        public async Task<IActionResult> CreatePrescription(CreatePrescriptionCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        [HttpGet("{prescriptionId}")]
        public async Task<ActionResult<PrescriptionsResponse>> GetPrescriptionById(Guid prescriptionId)
        {
            var result = await mediator.Send(
                new GetPrescriptionByIdQuery(prescriptionId));

            return Ok(result);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<PrescriptionsResponse>> GetPatientPrescription(Guid patientId)
        {
            var result = await mediator.Send(new GetPatientPrescriptionsQuery(patientId));
            return Ok(result);
        }

        // Doctor/Admin only: this returns every patient's prescription history
        // with this doctor, so a Patient caller must not be able to reach it.
        [Authorize(Roles = $"{UserRoles.Doctor},{UserRoles.HospitalAdmin},{UserRoles.SuperAdmin}")]
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<List<PrescriptionsResponse>>> GetDoctorPrescriptions(Guid doctorId)
        {
            var result = await mediator.Send(
                new GetDoctorPrescriptionsQuery(doctorId));

            return Ok(result);
        }

        [Authorize(Roles = $"{UserRoles.Doctor},{UserRoles.HospitalAdmin},{UserRoles.SuperAdmin}")]
        [HttpPut("UpdatePrescription/{PrescriptionId}")]
        public async Task<IActionResult> UpdatePrescription(UpdatePrescriptionCommand command, Guid PrescriptionId)
        {
            command.PrescriptionId = PrescriptionId;
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}