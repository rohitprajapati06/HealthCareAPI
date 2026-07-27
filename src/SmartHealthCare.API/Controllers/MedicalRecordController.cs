using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.DeleteMedicalRecord;
using SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord;
using SmartHealthcare.Application.Features.MedicalRecords.Queries.GetMedicalRecordById;
using SmartHealthcare.Application.Features.MedicalRecords.Queries.GetPatientMedicalRecords;

namespace SmartHealthCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMediator mediator;

        public MedicalRecordController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("CreateMedicalRecord")]
        public async Task<IActionResult> CreateMedicalRecord(
            [FromForm] CreateMedicalRecordCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }


        [HttpGet("GetMedicalRecords/{id}")]
        public async Task<IActionResult> GetMedicalRecordsById(Guid id)
        {
            var result = await mediator.Send(
                new GetMedicalRecordById(id));

            return Ok(result);
        }

        [HttpGet("GetPatientMedicalRecords/{patientId}")]
        public async Task<IActionResult> GetPatientMedicalRecords(Guid patientId)
        {
            var result = await mediator.Send(
                new GetPatientMedicalRecordsQuery(patientId));

            return Ok(result);
        }


        [HttpPut("UpdateMedicalRecordsById/{id}")]
        public async Task<IActionResult> UpdateMedicalRecordsById(Guid id,[FromForm] UpdateMedicalRecordCommand command)
        {
            command.Id = id;
            await mediator.Send(command);
            return NoContent();
        }


        [HttpDelete("DeleteMedicalRecordId/{id}")]
        public async Task<IActionResult> DeleteMedicalRecordsById(Guid id)
        {
            await mediator.Send(new DeleteMedicalRecordCommand(id));

            return NoContent();
        }
    }
}