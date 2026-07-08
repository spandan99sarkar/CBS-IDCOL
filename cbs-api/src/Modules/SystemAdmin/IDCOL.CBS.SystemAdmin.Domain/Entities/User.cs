using IDCOL.CBS.SharedKernel.Common;
using IDCOL.CBS.SharedKernel.Domain;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;

namespace IDCOL.CBS.SystemAdmin.Domain.Entities;

public class User : AggregateRoot<Guid>, IAuditable
{
    public string Username { get; private set; } = default!;

    public string DisplayName { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    public string BusinessUnitCode { get; private set; } = default!;

    public string CreatedBy { get; private set; } = default!;

    public DateTime CreatedAtUtc { get; private set; }

    public string? LastModifiedBy { get; private set; }

    public DateTime? LastModifiedAtUtc { get; private set; }

    private readonly List<RoleAssignment> _roleAssignments = new();

    public IReadOnlyCollection<RoleAssignment> RoleAssignments => _roleAssignments.AsReadOnly();

    /// <summary>
    /// General RBAC roles (drives navigation/screen visibility) - distinct from
    /// RoleAssignments, which gate Maker/Checker status for specific business functions.
    /// </summary>
    private readonly List<Role> _roles = new();

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private User()
    {
    }

    public static User Create(
        Guid id,
        string username,
        string displayName,
        string email,
        string passwordHash,
        string businessUnitCode,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            Id = id,
            Username = username.Trim(),
            DisplayName = displayName,
            Email = email,
            PasswordHash = passwordHash,
            BusinessUnitCode = businessUnitCode,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Deactivate(string modifiedBy)
    {
        IsActive = false;
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void ChangePasswordHash(string newHash, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(newHash))
            throw new ArgumentException("Password hash is required.", nameof(newHash));

        PasswordHash = newHash;
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain-level enforcement (layer 1 of 3 - see architecture plan): a user cannot
    /// simultaneously hold Maker and Checker designation for the same function, and cannot
    /// hold more than one assignment for the same function at all.
    /// </summary>
    public Result AssignRole(FunctionCode functionCode, bool isMaker, bool isChecker, string assignedBy)
    {
        if (isMaker && isChecker)
            return Result.Fail(
                "STRUCTURAL_VIOLATION: a single role assignment cannot be both Maker and Checker for the same function.");

        if (_roleAssignments.Any(a => a.FunctionCode == functionCode))
            return Result.Fail($"User already has a role assignment for function '{functionCode}'.");

        _roleAssignments.Add(RoleAssignment.Create(Guid.NewGuid(), Id, functionCode, isMaker, isChecker, assignedBy));
        return Result.Success();
    }

    public void AssignToRole(Role role)
    {
        if (!_roles.Contains(role))
            _roles.Add(role);
    }
}
