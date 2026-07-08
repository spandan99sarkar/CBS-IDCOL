namespace IDCOL.CBS.BuildingBlocks.Domain;

/// <summary>
/// Day-granularity date range, used for grace/moratorium periods and facility-version
/// effective windows per the repayment engine's confirmed requirement that grace periods
/// are configured in days, not just whole months.
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateOnly Start { get; }

    public DateOnly? End { get; }

    private DateRange(DateOnly start, DateOnly? end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Of(DateOnly start, DateOnly? end = null)
    {
        if (end.HasValue && end.Value < start)
            throw new ArgumentException("End date cannot be before start date.", nameof(end));

        return new DateRange(start, end);
    }

    public bool Contains(DateOnly date) => date >= Start && (!End.HasValue || date <= End.Value);

    public int? DurationInDays => End.HasValue ? End.Value.DayNumber - Start.DayNumber : null;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
