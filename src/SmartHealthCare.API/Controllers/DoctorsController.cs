using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Doctors.Commands.ApproveDoctors;
using SmartHealthcare.Application.Features.Doctors.Commands.RejectDoctors;
using SmartHealthcare.Application.Features.Doctors.Queries.GetDoctors;
using SmartHealthcare.Application.Features.Doctors.Queries.GetDoctorsById;
using SmartHealthcare.Application.Features.Doctors.Queries.SearchDoctors;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthCare.API.Controllers
{
    // GETs only require login (browsing doctors pre-booking); approve/reject
    // are admin-only. Explicit here even though the global fallback policy
    // in Program.cs would also cover it, to stay consistent with the other
    // controllers and not rely silently on that fallback.
    [Authorize]
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
        public async Task<IActionResult> GetDoctorsById(Guid id)
        {
            return Ok(await mediator.Send(new GetDoctorsByIdQuery(id)));
        }


        [Authorize(Roles = $"{UserRoles.HospitalAdmin},{UserRoles.SuperAdmin}")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            return Ok(await mediator.Send(new ApproveDoctorCommand(id)));
        }

        [Authorize(Roles = $"{UserRoles.HospitalAdmin},{UserRoles.SuperAdmin}")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            return Ok(await mediator.Send(new RejectDoctorCommands(id)));
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchDoctors([FromQuery] Guid? HospitalId, [FromQuery] string? Specialization,
            [FromQuery] int? Experience, [FromQuery] int? MaxFee, [FromQuery] int? MinFee)
        {

            return Ok(await mediator.Send(new SearchDoctorsQuery(HospitalId, Specialization, Experience, MaxFee, MinFee)));
        }

    }
}