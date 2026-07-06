namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// Excel-style serial-date arithmetic (epoch 1899-12-30), matching the validated reference
/// engine (backend/src/engine/engine.service.ts) exactly. IDCOL's real repayment workbooks are
/// Excel-based, so the whole engine operates in integer serial-day space and only converts to
/// calendar dates for month-arithmetic and weekday lookups.
/// </summary>
public static class SerialDate
{
    private static readonly DateTime Epoch = new(1899, 12, 30, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime ToDate(double serial) => Epoch.AddDays(serial);

    public static int? FromIso(string? iso)
    {
        if (string.IsNullOrEmpty(iso)) return null;
        var parts = iso.Split('-');
        var d = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), 0, 0, 0, DateTimeKind.Utc);
        return (int)Math.Round((d - Epoch).TotalDays);
    }

    public static string ToIso(double serial) => ToDate(serial).ToString("yyyy-MM-dd");

    /// <summary>
    /// Adds <paramref name="months"/> calendar months to a serial date, optionally pinning the
    /// day-of-month (clamped to the target month's last day). Mirrors addMonthsSerial().
    /// </summary>
    public static int AddMonths(double serial, int months, int? day = null)
    {
        var d = ToDate(serial);
        var year = d.Year;
        var mo = d.Month - 1 + months; // 0-based month like the JS getUTCMonth()
        year += (int)Math.Floor(mo / 12.0);
        mo = ((mo % 12) + 12) % 12;
        var useDay = day ?? d.Day;
        var lastDay = DateTime.DaysInMonth(year, mo + 1);
        var result = new DateTime(year, mo + 1, Math.Min(useDay, lastDay), 0, 0, 0, DateTimeKind.Utc);
        return (int)Math.Round((result - Epoch).TotalDays);
    }

    /// <summary>Day of week as JS getUTCDay(): Sunday = 0 ... Saturday = 6.</summary>
    public static int DayOfWeek(double serial) => (int)ToDate(serial).DayOfWeek;
}
