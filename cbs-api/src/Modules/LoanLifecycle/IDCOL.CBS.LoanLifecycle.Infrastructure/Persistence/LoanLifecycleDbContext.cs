using IDCOL.CBS.Collection.Domain;
using IDCOL.CBS.CreditSanction.Domain;
using IDCOL.CBS.Disbursement.Domain;
using IDCOL.CBS.PartyKyc.Domain;
using IDCOL.CBS.ProductConfig.Domain;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;

/// <summary>
/// Shared persistence context for the loan-lifecycle bounded contexts (ProductConfig, PartyKyc,
/// CreditSanction). The module boundaries are enforced at the C# project level (separate Domain
/// and Application projects); the physical database is shared, which is standard for a modular
/// monolith and keeps the dev-DB bootstrap simple.
/// </summary>
public class LoanLifecycleDbContext : DbContext
{
    public LoanLifecycleDbContext(DbContextOptions<LoanLifecycleDbContext> options) : base(options)
    {
    }

    public DbSet<LoanProduct> LoanProducts => Set<LoanProduct>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoanAgreement> LoanAgreements => Set<LoanAgreement>();
    public DbSet<DisbursementRequest> DisbursementRequests => Set<DisbursementRequest>();
    public DbSet<Receipt> Receipts => Set<Receipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoanLifecycleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
