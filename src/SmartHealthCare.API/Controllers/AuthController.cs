using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Auth.Commands.ChangePassword;
using SmartHealthcare.Application.Features.Auth.Commands.ForgotPassword;
using SmartHealthcare.Application.Features.Auth.Commands.Login;
using SmartHealthcare.Application.Features.Auth.Commands.Logout;
using SmartHealthcare.Application.Features.Auth.Commands.RefreshUserToken;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;
using SmartHealthcare.Application.Features.Auth.Commands.ResetPassword;
using SmartHealthcare.Application.Features.Auth.Queries.GetCurrentUser;

namespace SmartHealthCare.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator mediator;

    public AuthController(IMediator mediator)
    {
        this.mediator = mediator;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("register/patient")]
    public async Task<IActionResult> RegisterPatient(RegisterPatientCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("register/doctor")]
    public async Task<IActionResult> RegisterDoctor(RegisterDoctorCommand command) 
    { 
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await mediator.Send(new GetCurrentUserQuery());
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> GetRefreshToken(RefreshTokenCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command) 
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result); 
    }
    // Testing only — remove when frontend is ready
    [HttpGet("reset-password")]
    public IActionResult ResetPasswordForm([FromQuery] string email, [FromQuery] string token)
    {
        // Just returns the values so you can copy them cleanly
        return Ok(new { email, token });
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }
}
