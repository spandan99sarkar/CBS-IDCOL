using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace IDCOL.CBS.Api.IntegrationTests;

/// <summary>
/// Boots the API against fresh, unique SQLite files per run. The app uses EnsureCreated (not
/// migrations) for the dev SQLite fallback, which never adds tables to an existing file - so a
/// stale DB from a prior build would be missing newer tables. Pointing each test run at brand-new
/// temp files guarantees the full current schema is created and keeps runs isolated.
/// </summary>
public sealed class IdcolApiFactory : WebApplicationFactory<Program>
{
    private readonly string _systemAdminDb = Path.Combine(Path.GetTempPath(), $"idcol-test-sysadmin-{Guid.NewGuid():N}.db");
    private readonly string _lifecycleDb = Path.Combine(Path.GetTempPath(), $"idcol-test-lifecycle-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:UseSqliteForLocalDevelopment"] = "true",
                ["ConnectionStrings:SystemAdminSqlite"] = $"Data Source={_systemAdminDb}",
                ["ConnectionStrings:LoanLifecycleSqlite"] = $"Data Source={_lifecycleDb}",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        foreach (var f in new[] { _systemAdminDb, _lifecycleDb })
        {
            try { if (File.Exists(f)) File.Delete(f); } catch { /* best-effort temp cleanup */ }
        }
    }
}
