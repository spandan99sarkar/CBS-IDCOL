using IDCOL.CBS.SystemAdmin.Application.Users.Commands.AssignRole;
using IDCOL.CBS.SystemAdmin.Application.Users.Commands.CreateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    [HttpPost("{userId:guid}/role-assignments")]
    public async Task<IActionResult> AssignRole(
        Guid userId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AssignRoleCommand(userId, request.FunctionCode, request.IsMaker, request.IsChecker),
            cancellationToken);
        return NoContent();
    }
}

public sealed record AssignRoleRequest(string FunctionCode, bool IsMaker, bool IsChecker);
