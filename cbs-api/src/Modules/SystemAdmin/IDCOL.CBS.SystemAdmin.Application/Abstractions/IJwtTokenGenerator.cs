using IDCOL.CBS.SystemAdmin.Domain.Entities;

namespace IDCOL.CBS.SystemAdmin.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IReadOnlyCollection<string> roleCodes);
}
