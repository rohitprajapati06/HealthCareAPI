using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals;
using SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitalById;
using SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitals;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthCare.API.Controllers
{
    // Browsing hospitals is normal for anyone booking an appointment, so reads
    // only require login. The bulk import is a system-wide, unscoped write
    // (no single hospital to check ownership against), so it's SuperAdmin only.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalsController : ControllerBase
    {
        private readonly IMediator mediator;

        public HospitalsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetHospitals()
        {
            return Ok(await mediator.Send(new GetHospitalsQuery()));
        }

        [Authorize(Roles = UserRoles.SuperAdmin)]
        [HttpPost("import")]
        public async Task<IActionResult> ImportHospitals([FromForm] ImportHospitalsCommand command)
        {
            await mediator.Send(command);

            return Ok(new
            {
                Message = "Hospital import completed"
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHospitalsById(Guid id)
        {
            return Ok(await mediator.Send(new GetHospitalByIdQuery(id)));
        }


    }
}