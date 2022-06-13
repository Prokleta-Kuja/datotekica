using System.Globalization;

namespace datotekica.Extensions;

public static class DateTimeExtensions
{
    public static string ToTimeAgo(this DateTime dt)
    {
        var span = DateTime.Now - dt;
        if (span.Days > 365)
        {
            int years = (span.Days / 365);
            if (span.Days % 365 != 0)
                years += 1;
            return years switch
            {
                1 => "last year",
                _ => $"{years} years ago"
            };
        }
        if (span.Days > 30)
        {
            int months = (span.Days / 30);
            if (span.Days % 31 != 0)
                months += 1;
            return months switch
            {
                1 => "last month",
                _ => $"{months} months ago"
            };
        }
        if (span.Days > 0)
            return span.Days switch
            {
                1 => "yesterday",
                _ => $"{span.Days} days ago"
            };
        if (span.Hours > 0)
            return span.Hours switch
            {
                1 => "last hour",
                _ => $"{span.Hours} hours ago"
            };
        if (span.Minutes > 1)
            return $"{span.Minutes} minutes ago";
        if (span.Seconds <= 5)
            return "just now";
        return string.Empty;
    }
}