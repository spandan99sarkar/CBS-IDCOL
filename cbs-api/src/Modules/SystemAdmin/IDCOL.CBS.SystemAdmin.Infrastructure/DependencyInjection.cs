using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Infrastructure.Audit;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;
using IDCOL.CBS.SystemAdmin.Infrastructure.Repositories;
using IDCOL.CBS.SystemAdmin.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IDCOL.CBS.SystemAdmin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSystemAdminInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Oracle is the real target per the architecture plan. This SQLite branch exists only
        // so the app is runnable end-to-end without provisioning an Oracle instance first - it
        // must be explicitly opted into via config (never inferred from a missing/bad connection
        // string) so a real Oracle misconfiguration in a non-dev environment fails loudly instead
        // of silently falling back to a throwaway local file.
        var useSqliteForLocalDev = configuration.GetValue<bool>("Database:UseSqliteForLocalDevelopment");

        services.AddDbContext<SystemAdminDbContext>(options =>
        {
            if (useSqliteForLocalDev)
            {
                options.UseSqlite(configuration.GetConnectionString("SystemAdminSqlite")
                    ?? "Data Source=cbs-dev.db");
            }
            else
            {
                options.UseOracle(configuration.GetConnectionString("SystemAdmin"));
            }
        });

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IMakerCheckerRoleGate, MakerCheckerRoleGate>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        services.AddScoped<IAuditTrailReader, AuditTrailReader>();

        return services;
    }
}
