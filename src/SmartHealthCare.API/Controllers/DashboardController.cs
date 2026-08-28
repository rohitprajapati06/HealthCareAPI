using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetAdminDashboard;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetDoctorDashboard;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetPatientDashboard;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthCare.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator mediator;

        public DashboardController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Authorize(Roles = $"{UserRoles.HospitalAdmin},{UserRoles.SuperAdmin}")]
        [HttpGet("AdminDashboard")]
        public async Task<IActionResult> AdminDashboard()
        {

            var result = await mediator.Send(new GetAdminDashboardQuery());
            return Ok(result);
        }

        [Authorize(Roles = $"{UserRoles.Doctor},{UserRoles.HospitalAdmin},{UserRoles.SuperAdmin}")]
        [HttpGet("DoctorDashboard/{DoctorId}")]
        public async Task<IActionResult> DoctorDashboard(Guid DoctorId)
        {
            var result = await mediator.Send(new GetDoctorDashboardQuery(DoctorId));
            return Ok(result);
        }

        [HttpGet("PatientDashboard/{patientId}")]
        public async Task<IActionResult> PatientDashboard(Guid patientId)
        {
            var result = await mediator.Send(new GetPatientDashboardQuery(patientId));
            return Ok(result);
        }
    }
}