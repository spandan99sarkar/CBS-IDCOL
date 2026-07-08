using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;
using MediatR;

namespace IDCOL.CBS.SystemAdmin.Application.Users.Commands.AssignRole;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserAccessor _currentUser;

    public AssignRoleCommandHandler(IUserRepository userRepository, ICurrentUserAccessor currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User '{request.UserId}' was not found.");

        var result = user.AssignRole(
            FunctionCode.Of(request.FunctionCode), request.IsMaker, request.IsChecker, _currentUser.UserId);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
    }
}
