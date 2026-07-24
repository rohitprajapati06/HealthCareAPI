using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Appointments.Queries.GetPatientAppointments;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetAdminDashboard;
using SmartHealthcare.Application.Features.Dashboard.Queries.GetDoctorDashboard;

namespace SmartHealthCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator mediator;

        public DashboardController(IMediator mediator )
        {
            this.mediator = mediator;
        }

        [HttpGet("AdminDashboard")]
        public async Task<IActionResult> AdminDashboard() {

            var result = await mediator.Send(new GetAdminDashboardQuery());
            return Ok(result);
        }

        [HttpGet("DoctorDashboard/{DoctorId}")]
        public async Task<IActionResult> DoctorDashboard(Guid DoctorId)
        {
            var result = await mediator.Send(new GetDoctorDashboardQuery(DoctorId));
            return Ok(result);
        }

        [HttpGet("DoctorDashboard/{PatientId}")]
        public async Task<IActionResult> PatientDashboard(Guid PatientId)
        {
            var result = await mediator.Send(new GetPatientAppointmentsQuery(PatientId));
            return Ok(result);
        }
    }
}
