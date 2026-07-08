using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.SystemAdmin.Application.Auth.Commands.Authenticate;

public sealed class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, AuthenticateResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateCommandHandler(
        IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthenticateResult> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !user.IsActive)
            return new AuthenticateResult(false, null, "Invalid username or password.");

        if (!_passwordHasher.Verify(request.PlainTextPassword, user.PasswordHash))
            return new AuthenticateResult(false, null, "Invalid username or password.");

        var roleCodes = user.Roles.Select(r => r.Code).Distinct().ToList();
        var token = _tokenGenerator.GenerateToken(user, roleCodes);
        return new AuthenticateResult(true, token, null);
    }
}
