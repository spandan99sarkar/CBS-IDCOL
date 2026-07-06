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
        services.AddDbContext<SystemAdminDbContext>(options =>
            options.UseOracle(configuration.GetConnectionString("SystemAdmin")));

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
