using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.DeleteAvailabilitySlot;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.UpdateAvailabilitySlot;
using SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetAvailableSlots;
using SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetDoctorSlots;

namespace SmartHealthCare.API.Controllers
{
    [Route("api/doctors/{doctorId}/slots")]
    [ApiController]
    public class AvailabilitySlotsController : ControllerBase
    {
        private readonly IMediator mediator;

        public AvailabilitySlotsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid doctorId ,CreateAvailabilitySlot command )
        {
            command.DoctorId = doctorId;
            var id = await mediator.Send(command);

            return Ok(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorsSlot([FromRoute]Guid doctorId)
        {
            var result = await mediator.Send(new GetDoctorSlotsQuery(doctorId));
            return Ok(result);

        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSlots([FromRoute]Guid doctorId)
        {
            var result = await mediator.Send(new GetAvailableSlotQuery(doctorId));
            return Ok(result);
        }

        [HttpPut("{slotId}")]
        public async Task<IActionResult> UpdateSlot ([FromRoute] Guid doctorId ,[FromRoute] Guid slotId , UpdateAvailabilitySlotCommand command)
        {
            command.DoctorId = doctorId;
            command.SlotId = slotId;

            await mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{slotId}")]
        public async Task<IActionResult> DeleteSlots([FromRoute] Guid slotId , [FromRoute] Guid doctorId)
        {
            await mediator.Send(new DeleteAvailabilitySlotCommand
            {
                SlotId = slotId,
                DoctorId = doctorId
            });
            return NoContent();
        }
    }
}
