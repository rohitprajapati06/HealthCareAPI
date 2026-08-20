using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals;
using SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitalById;
using SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitals;

namespace SmartHealthCare.API.Controllers
{
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
            return Ok( await mediator.Send(new GetHospitalsQuery()));
        }

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
