namespace IDCOL.CBS.BuildingBlocks.Application.Abstractions;

public interface ICurrentUserAccessor
{
    string UserId { get; }

    IReadOnlyCollection<string> RoleCodes { get; }

    bool IsInRole(string roleCode);
}
