using MediatR;

namespace IDCOL.CBS.SystemAdmin.Application.Auth.Commands.Authenticate;

public sealed record AuthenticateCommand(string Username, string PlainTextPassword) : IRequest<AuthenticateResult>;

public sealed record AuthenticateResult(bool Succeeded, string? Token, string? FailureReason);
