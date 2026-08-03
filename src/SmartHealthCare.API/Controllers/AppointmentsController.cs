using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment;
using SmartHealthcare.Application.Features.Appointments.Commands.CancelAppointment;
using SmartHealthcare.Application.Features.Appointments.Commands.CompleteAppointment;
using SmartHealthcare.Application.Features.Appointments.Commands.RescheduleAppointment;
using SmartHealthcare.Application.Features.Appointments.Queries.GetAppointmentById;
using SmartHealthcare.Application.Features.Appointments.Queries.GetDoctorAppointmentsQuery;
using SmartHealthcare.Application.Features.Appointments.Queries.GetHospitalAppointmentsQuery;
using SmartHealthcare.Application.Features.Appointments.Queries.GetPatientAppointments;

namespace SmartHealthCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IMediator mediator;

        public AppointmentsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(BookAppointmentCommand command) 
        {
            var appointment = await mediator.Send(command);

            return Ok( appointment);
        }

        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetAppointmentsById(Guid appointmentId)
        {
            var appointments = await mediator.Send(new GetAppointmentByIdQuery(appointmentId));
            return Ok( appointments);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientAppointments(Guid patientId) 
        {
            var result = await mediator.Send(new GetPatientAppointmentsQuery(patientId));
            return Ok(result);
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorAppointment(Guid doctorId)
        {
            var result = await mediator.Send(new GetDoctorAppointmentsQuery(doctorId));
            return Ok(result);
        }


         [HttpGet("hospital/{hospitalId}")]
        public async Task<IActionResult> GetHospitalAppointments(Guid hospitalId)
        {
            var result = await mediator.Send(new GetHospitalAppointmentsQuery(hospitalId));

            return Ok(result);
        }

        [HttpPut("{appointmentId}/complete")]
        public async Task<IActionResult> CompleteAppointment(Guid appointmentId)
        {
            await mediator.Send(new CompleteAppointmentCommand
            {
                AppointmentId = appointmentId
            });

            return NoContent();
        }

        [HttpPut("{appointmentId}/cancel")]
        public async Task<IActionResult> CancelAppointment(Guid appointmentId)
        {
            await mediator.Send(new CancelAppointmentCommand
            {
                AppointmentId = appointmentId
            });

            return NoContent();
        }

        [HttpPut("{appointmentId}/reschedule")]
        public async Task<IActionResult> RescheduleAppointment(Guid appointmentId , [FromBody] RescheduleAppointmentCommand command)
        {

            command.AppointmentId = appointmentId;
            await mediator.Send(command);
           
            return NoContent();
            
        }
    }
}
