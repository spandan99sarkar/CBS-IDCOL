using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core Identity's PasswordHasher&lt;TUser&gt; (from Microsoft.Extensions.Identity.Core,
/// which has no ASP.NET Core hosting dependency) rather than pulling in the full Identity
/// framework - we only need battle-tested PBKDF2 hashing, not membership/sign-in management.
/// </summary>
public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string plainTextPassword) => _hasher.HashPassword(null!, plainTextPassword);

    public bool Verify(string plainTextPassword, string hash)
    {
        var result = _hasher.VerifyHashedPassword(null!, hash, plainTextPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
