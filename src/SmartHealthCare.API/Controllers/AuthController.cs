using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Application.Features.Auth.Commands.Login;

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

}
