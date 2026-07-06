using IDCOL.CBS.SystemAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;

public class SystemAdminDbContext : DbContext
{
    public SystemAdminDbContext(DbContextOptions<SystemAdminDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemAdminDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
