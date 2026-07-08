using IDCOL.CBS.SystemAdmin.Application.Auth.Commands.Authenticate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] AuthenticateCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Succeeded ? Ok(result) : Unauthorized(new { result.FailureReason });
    }
}
