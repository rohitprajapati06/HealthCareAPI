using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Doctors.Commands.ApproveDoctors;
using SmartHealthcare.Application.Features.Doctors.Commands.RejectDoctors;
using SmartHealthcare.Application.Features.Doctors.Queries.GetDoctors;
using SmartHealthcare.Application.Features.Doctors.Queries.SearchDoctors;

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


        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetDoctorsById(int id)
        //{
        //    return Ok(await mediator.Send(new GetDoctorsQuery()));
        //}


        //[Authorize(Roles = "HospitalAdmin,SuperAdmin")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            return Ok(await mediator.Send(new ApproveDoctorCommand(id)));
        }

        //[Authorize(Roles = "HospitalAdmin,SuperAdmin")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            return Ok(await mediator.Send(new RejectDoctorCommands(id)));
        }

        [HttpGet("Search Doctors")]
        public async Task<IActionResult> SearchDoctors( [FromQuery] Guid? HospitalId , [FromQuery]string? Specialization, 
            [FromQuery] int? Experience , [FromQuery]int? MaxFee, [FromQuery]int? MinFee)
        {

            return Ok(await mediator.Send(new SearchDoctorsQuery(HospitalId,Specialization,Experience,MaxFee,MinFee)));
        }

    }
}
