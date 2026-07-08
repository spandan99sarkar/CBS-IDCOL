using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Domain.Entities;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SystemAdminDbContext _dbContext;

    public UserRepository(SystemAdminDbContext dbContext) => _dbContext = dbContext;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Users
            .Include(u => u.RoleAssignments)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        _dbContext.Users
            .Include(u => u.RoleAssignments)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.AddAsync(user, cancellationToken);
}
