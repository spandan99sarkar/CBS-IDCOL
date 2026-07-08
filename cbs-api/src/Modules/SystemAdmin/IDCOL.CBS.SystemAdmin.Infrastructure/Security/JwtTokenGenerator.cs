using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options.Value;

    public string GenerateToken(User user, IReadOnlyCollection<string> roleCodes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("displayName", user.DisplayName),
            new("businessUnit", user.BusinessUnitCode),
        };
        claims.AddRange(roleCodes.Select(code => new Claim(ClaimTypes.Role, code)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
