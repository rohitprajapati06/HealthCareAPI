using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot;

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
            var id = await mediator.Send(command);

            return Ok(id);
        }
    }
}
