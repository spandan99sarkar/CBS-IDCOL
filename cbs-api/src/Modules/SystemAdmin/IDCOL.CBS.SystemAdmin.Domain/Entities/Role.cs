using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.SystemAdmin.Domain.Entities;

public class Role : AggregateRoot<Guid>, IAuditable
{
    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public string CreatedBy { get; private set; } = default!;

    public DateTime CreatedAtUtc { get; private set; }

    public string? LastModifiedBy { get; private set; }

    public DateTime? LastModifiedAtUtc { get; private set; }

    private readonly List<Permission> _permissions = new();

    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    private Role()
    {
    }

    public static Role Create(Guid id, string code, string name, string? description, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Role code is required.", nameof(code));

        return new Role
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name,
            Description = description,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void GrantPermission(Permission permission)
    {
        if (!_permissions.Contains(permission))
            _permissions.Add(permission);
    }
}
