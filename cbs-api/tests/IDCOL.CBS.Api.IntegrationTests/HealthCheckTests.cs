using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IDCOL.CBS.Api.IntegrationTests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task GetHealth_ReturnsOkWithHealthyStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", body);
    }

    [Fact]
    public async Task GetAudit_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Proves the JWT bearer auth middleware is actually wired up (not just that the
        // controller exists) - the request never reaches AuditController, so this doesn't
        // touch the (unavailable in this environment) Oracle database either.
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
