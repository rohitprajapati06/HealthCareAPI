using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Doctors.Queries.GetDoctors;

namespace SmartHealthCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IMediator mediator;

        public DoctorsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
             return Ok(await mediator.Send(new GetDoctorsQuery()));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorsById(int id)
        {
            return Ok(await mediator.Send(new GetDoctorsQuery()));
        }
    }
}
