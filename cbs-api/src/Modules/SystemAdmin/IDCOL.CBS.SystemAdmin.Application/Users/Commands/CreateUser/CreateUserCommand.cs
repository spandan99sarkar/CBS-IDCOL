using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.SystemAdmin.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Username,
    string DisplayName,
    string Email,
    string PlainTextPassword,
    string BusinessUnitCode) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;
