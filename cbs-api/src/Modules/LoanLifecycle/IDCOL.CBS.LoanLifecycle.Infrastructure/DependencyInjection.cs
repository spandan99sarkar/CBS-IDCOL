using IDCOL.CBS.CreditSanction.Application.Abstractions;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;
using IDCOL.CBS.PartyKyc.Application.Abstractions;
using IDCOL.CBS.ProductConfig.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLoanLifecycleInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Same opt-in dev fallback as SystemAdmin (see that module's DependencyInjection): Oracle
        // is the real target, SQLite is used only when explicitly enabled for local development.
        var useSqliteForLocalDev = configuration.GetValue<bool>("Database:UseSqliteForLocalDevelopment");

        services.AddDbContext<LoanLifecycleDbContext>(options =>
        {
            if (useSqliteForLocalDev)
            {
                options.UseSqlite(configuration.GetConnectionString("LoanLifecycleSqlite")
                    ?? "Data Source=cbs-lifecycle-dev.db");
            }
            else
            {
                options.UseOracle(configuration.GetConnectionString("LoanLifecycle"));
            }
        });

        services.AddScoped<ILoanProductRepository, LoanProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILoanAgreementRepository, LoanAgreementRepository>();

        return services;
    }
}
