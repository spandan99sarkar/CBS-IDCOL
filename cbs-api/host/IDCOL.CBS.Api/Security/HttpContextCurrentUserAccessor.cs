using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;

namespace IDCOL.CBS.Api.Security;

public class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return "anonymous";
            // The JWT bearer handler remaps "sub" to ClaimTypes.NameIdentifier by default, so
            // check both so the audit trail records the real actor rather than "anonymous".
            return user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonymous";
        }
    }

    public IReadOnlyCollection<string> RoleCodes =>
        _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? new List<string>();

    public bool IsInRole(string roleCode) => RoleCodes.Contains(roleCode, StringComparer.OrdinalIgnoreCase);
}
