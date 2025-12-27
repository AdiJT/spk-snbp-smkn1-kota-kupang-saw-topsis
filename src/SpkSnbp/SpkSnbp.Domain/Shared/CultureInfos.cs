using System.Globalization;

namespace SpkSnbp.Domain.Shared;

public static class CultureInfos
{
    public static CultureInfo CI => new("id-ID");
    public static TimeZoneInfo TimeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("WITA", TimeSpan.FromHours(8), "WITA", "WITA");
    public static DateTime DateTimeNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo);
    public static DateOnly DateOnlyNow => DateOnly.FromDateTime(DateTimeNow);
    public static TimeOnly TimeOnlyNow => TimeOnly.FromDateTime(DateTimeNow);
}
