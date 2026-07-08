using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Domain.Entities;
using MediatR;

namespace IDCOL.CBS.SystemAdmin.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateUserCommandHandler(
        IUserRepository userRepository, IPasswordHasher passwordHasher, ICurrentUserAccessor currentUser)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        var userId = Guid.NewGuid();
        var user = User.Create(
            userId,
            request.Username,
            request.DisplayName,
            request.Email,
            _passwordHasher.Hash(request.PlainTextPassword),
            request.BusinessUnitCode,
            _currentUser.UserId);

        await _userRepository.AddAsync(user, cancellationToken);
        return userId;
    }
}
