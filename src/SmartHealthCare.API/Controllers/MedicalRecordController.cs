using MediatR;
using Microsoft.AspNetCore.Http;
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
        
        public async Task<IActionResult> CreateMedicalRecord(CreateMedicalRecordCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("GetMedicalRecords/{Id}")]
        public async Task<IActionResult> GetMedicalRecordsById(Guid Id) 
        {
            var result = await mediator.Send(new GetMedicalRecordById(Id));
            return Ok(result);
        }

        [HttpGet("GetPatientMedicalRecords/{patientId}")]
        public async Task<IActionResult> GetPatientMedicalRecords(Guid paitentId)
        {
            var result = await mediator.Send(new GetPatientMedicalRecordsQuery(paitentId));
            return Ok(result);
        }

        [HttpPut("UpdateMedicalRecordsById/{id}")]
        public async Task<IActionResult> UpdateMedicalRecordsById(Guid id , UpdateMedicalRecordCommand command)
        {
            command.Id = id;
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("DeleteMedicalRecordId/{id}")]
        public async Task<IActionResult> DeleteMedicalRecordsById(Guid id,DeleteMedicalRecordCommand command)
        {
            await mediator.Send(command);
            return NoContent();
        }
    }
}
