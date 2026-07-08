using IDCOL.CBS.SystemAdmin.Domain.Entities;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.IntegrationTests.Persistence;

/// <summary>
/// Uses a real relational engine (SQLite, in-memory) rather than the EF Core InMemory provider
/// to verify the UNIQUE(USER_ID, FUNCTION_CODE) invariant configured on SYSAD_ROLE_ASSIGNMENT
/// (see RoleAssignmentConfiguration) - the InMemory provider does not enforce alternate unique
/// indexes at all (only primary keys), so it would silently let a duplicate row through.
/// SQLite genuinely enforces both UNIQUE indexes and CHECK constraints at the SQL level, so
/// EnsureCreated() below applies the same CK_ROLE_ASSIGNMENT_NOT_BOTH / UNIQUE(USER_ID,
/// FUNCTION_CODE) DDL that the Oracle migration carries (translated to SQLite syntax by EF
/// Core's relational model, not the Oracle-specific SQL text) - the Oracle SQL itself was
/// verified separately by inspecting the generated migration and still needs a final check
/// against a live Oracle instance before go-live.
/// </summary>
public class RoleAssignmentUniquenessTests
{
    private static (SqliteConnection Connection, SystemAdminDbContext Context) CreateSqliteContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<SystemAdminDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new SystemAdminDbContext(options);
        context.Database.EnsureCreated();
        return (connection, context);
    }

    [Fact]
    public async Task SaveChanges_WithDuplicateUserAndFunctionCode_ThrowsOnSecondRow()
    {
        var (connection, context) = CreateSqliteContext();
        await using var _ = connection;
        await using var dbContext = context;

        var userId = Guid.NewGuid();
        var functionCode = FunctionCode.Of("DISBURSEMENT_POST");
        var user = User.Create(userId, "jdoe", "Jane Doe", "jane@idcol.example", "hash", "CAD", "system");
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        // Bypasses User.AssignRole's own duplicate check on purpose - simulating a direct
        // persistence-layer write, which is exactly the scenario the DB-level constraint
        // (layer 3) exists to guard against.
        var first = RoleAssignment.Create(
            Guid.NewGuid(), userId, functionCode, isMaker: true, isChecker: false, "admin");
        var second = RoleAssignment.Create(
            Guid.NewGuid(), userId, functionCode, isMaker: false, isChecker: true, "admin");

        await dbContext.RoleAssignments.AddAsync(first);
        await dbContext.SaveChangesAsync();

        await dbContext.RoleAssignments.AddAsync(second);
        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WithSameFunctionForDifferentUsers_Succeeds()
    {
        var (connection, context) = CreateSqliteContext();
        await using var _ = connection;
        await using var dbContext = context;

        var functionCode = FunctionCode.Of("DISBURSEMENT_POST");
        var userA = User.Create(Guid.NewGuid(), "usera", "User A", "a@idcol.example", "hash", "CAD", "system");
        var userB = User.Create(Guid.NewGuid(), "userb", "User B", "b@idcol.example", "hash", "CAD", "system");
        await dbContext.Users.AddRangeAsync(userA, userB);
        await dbContext.SaveChangesAsync();

        var assignmentA = RoleAssignment.Create(
            Guid.NewGuid(), userA.Id, functionCode, isMaker: true, isChecker: false, "admin");
        var assignmentB = RoleAssignment.Create(
            Guid.NewGuid(), userB.Id, functionCode, isMaker: false, isChecker: true, "admin");
        await dbContext.RoleAssignments.AddRangeAsync(assignmentA, assignmentB);

        var affectedRows = await dbContext.SaveChangesAsync();

        Assert.Equal(2, affectedRows);
    }
}
