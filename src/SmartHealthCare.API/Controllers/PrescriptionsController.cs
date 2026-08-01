using DocumentFormat.OpenXml.Vml.Spreadsheet;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription;
using SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;
using SmartHealthcare.Application.Features.Prescriptions.Queries.GetPrescriptionById;
using SmartHealthcare.Application.Features.Prescriptions.Responses;

namespace SmartHealthCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IMediator mediator;

        public PrescriptionsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("CreatePrescription")]
        public async Task<IActionResult> CreatePrescription(CreatePrescriptionCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        [HttpGet("{prescriptionId}")]
        public async Task<ActionResult<PrescriptionsResponses>> GetPrescriptionById(Guid prescriptionId)
        {
            var result = await mediator.Send(
                new GetPrescriptionByIdQuery(prescriptionId));

            return Ok(result);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<PrescriptionsResponses>> GetPatientPrescription([FromQuery]Guid patientId)
        {
            var result = await mediator.Send(new GetPatientPrescriptionsQuery(patientId));
            return Ok(result);
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<List<PrescriptionsResponses>>> GetDoctorPrescriptions(Guid doctorId)
        {
            var result = await mediator.Send(
                new GetDoctorPrescriptionsQuery(doctorId));

            return Ok(result);
        }

        [HttpPut("UpdatePrescription/{PrescriptionId}")]
        public async Task<IActionResult> UpdatePrescription(UpdatePrescriptionCommand command , Guid PrescriptionId)
        {
            command.PrescriptionId = PrescriptionId;
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}
