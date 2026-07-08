using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Security;

/// <summary>
/// Layer 2 (application-pipeline) implementation of the maker-checker gate, backed by the same
/// SYSAD_ROLE_ASSIGNMENT table that layers 1 (domain) and 3 (database CHECK/UNIQUE constraint)
/// also protect.
/// </summary>
public class MakerCheckerRoleGate : IMakerCheckerRoleGate
{
    private readonly SystemAdminDbContext _dbContext;

    public MakerCheckerRoleGate(SystemAdminDbContext dbContext) => _dbContext = dbContext;

    public async Task<bool> CanActAsCheckerAsync(
        string userId, string functionCode, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return false;

        var code = FunctionCode.Of(functionCode);
        return await _dbContext.RoleAssignments
            .AnyAsync(ra => ra.UserId == userGuid && ra.FunctionCode == code && ra.IsChecker, cancellationToken);
    }
}
