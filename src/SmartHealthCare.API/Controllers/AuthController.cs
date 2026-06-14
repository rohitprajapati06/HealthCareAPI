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
    private readonly ILogger<AuthController> logger;

    public AuthController(IMediator mediator , ILogger<AuthController> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        logger.LogInformation("Login Endpoint Invoked");

        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("register/patient")]
    public async Task<IActionResult> RegisterPatient(RegisterPatientCommand command)
    {
        logger.LogInformation("Patient Endpoint Invoked");
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("register/doctor")]
    public async Task<IActionResult> RegisterDoctor(RegisterDoctorCommand command) 
    {
        logger.LogInformation("Doctor Endpoint Invoked");
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        logger.LogInformation($"{nameof(GetCurrentUser)}"); 
        var result = await mediator.Send(new GetCurrentUserQuery());
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> GetRefreshToken(RefreshTokenCommand command)
    {
        logger.LogInformation("Refresh-token Endpoint Invoked");
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command)
    {
        logger.LogInformation("Logout Endpoint Invoked");
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command) 
    {
        logger.LogInformation("Change-Password Endpoint Invoked");

        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        logger.LogInformation("Forgot-Password Endpoint Invoked");
        var result = await mediator.Send(command);
        return Ok(result); 
    }
    // Testing only — remove when frontend is ready
    [HttpGet("reset-password")]
    public IActionResult ResetPasswordForm([FromQuery] string email, [FromQuery] string token)
    {
        logger.LogInformation("reset-password Endpoint Invoked");

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
