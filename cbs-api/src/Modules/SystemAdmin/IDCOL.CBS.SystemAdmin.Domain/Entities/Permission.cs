using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.SystemAdmin.Domain.Entities;

public class Permission : Entity<Guid>
{
    public string Code { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    private Permission()
    {
    }

    public static Permission Create(Guid id, string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Permission code is required.", nameof(code));

        return new Permission
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Description = description
        };
    }
}
