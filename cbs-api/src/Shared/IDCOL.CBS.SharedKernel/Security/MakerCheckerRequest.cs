using IDCOL.CBS.SharedKernel.Common;
using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.SharedKernel.Security;

/// <summary>
/// Base shape for any maker-checker-gated change in the system (disbursement posting,
/// parameter changes, classification overrides, etc.). This is the domain-layer half of the
/// three-layer structural enforcement described in the architecture plan: this class can never
/// be approved by the same user id that created it, regardless of what the application layer
/// or UI would otherwise allow.
/// </summary>
public abstract class MakerCheckerRequest<TPayload> : AggregateRoot<Guid>
{
    public string RequestType { get; protected set; } = default!;

    public TPayload ProposedPayload { get; protected set; } = default!;

    public string MakerUserId { get; protected set; } = default!;

    public DateTime MakerTimestampUtc { get; protected set; }

    public MakerCheckerStatus Status { get; protected set; } = MakerCheckerStatus.Pending;

    public string? CheckerUserId { get; protected set; }

    public DateTime? CheckerTimestampUtc { get; protected set; }

    public string? CheckerComment { get; protected set; }

    protected MakerCheckerRequest()
    {
    }

    protected MakerCheckerRequest(Guid id, string requestType, TPayload proposedPayload, string makerUserId)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(makerUserId))
            throw new ArgumentException("Maker user id is required.", nameof(makerUserId));
        if (string.IsNullOrWhiteSpace(requestType))
            throw new ArgumentException("Request type is required.", nameof(requestType));

        RequestType = requestType;
        ProposedPayload = proposedPayload;
        MakerUserId = makerUserId;
        MakerTimestampUtc = DateTime.UtcNow;
        Status = MakerCheckerStatus.Pending;
    }

    public Result Approve(string checkerUserId, string? comment = null)
    {
        if (Status != MakerCheckerStatus.Pending)
            return Result.Fail($"Request is not pending (current status: {Status}).");

        if (string.Equals(checkerUserId, MakerUserId, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("STRUCTURAL_VIOLATION: a checker cannot approve their own request (maker == checker).");

        Status = MakerCheckerStatus.Approved;
        CheckerUserId = checkerUserId;
        CheckerTimestampUtc = DateTime.UtcNow;
        CheckerComment = comment;
        return Result.Success();
    }

    public Result Reject(string checkerUserId, string comment)
    {
        if (Status != MakerCheckerStatus.Pending)
            return Result.Fail($"Request is not pending (current status: {Status}).");

        if (string.Equals(checkerUserId, MakerUserId, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("STRUCTURAL_VIOLATION: a checker cannot reject their own request (maker == checker).");

        if (string.IsNullOrWhiteSpace(comment))
            return Result.Fail("A rejection requires a comment explaining why.");

        Status = MakerCheckerStatus.Rejected;
        CheckerUserId = checkerUserId;
        CheckerTimestampUtc = DateTime.UtcNow;
        CheckerComment = comment;
        return Result.Success();
    }
}
