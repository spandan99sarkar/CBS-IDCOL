namespace IDCOL.CBS.SystemAdmin.Infrastructure.Security;

public class JwtOptions
{
    public string Issuer { get; set; } = "IDCOL.CBS";

    public string Audience { get; set; } = "IDCOL.CBS.Clients";

    public string SigningKey { get; set; } = default!;

    public int ExpiryMinutes { get; set; } = 60;
}
