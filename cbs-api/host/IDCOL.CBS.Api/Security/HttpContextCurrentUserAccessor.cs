using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;

namespace IDCOL.CBS.Api.Security;

public class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "anonymous";

    public IReadOnlyCollection<string> RoleCodes =>
        _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? new List<string>();

    public bool IsInRole(string roleCode) => RoleCodes.Contains(roleCode, StringComparer.OrdinalIgnoreCase);
}
