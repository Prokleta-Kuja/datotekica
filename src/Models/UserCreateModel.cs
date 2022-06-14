using System.Globalization;

namespace datotekica.Models;

public class UserCreateModel
{
    public string? Username { get; set; }
    public string? TimezoneId { get; set; } = C.Env.TimeZone;
    public string? LocaleId { get; set; } = C.Env.Locale;
    public Dictionary<string, string>? Validate(HashSet<string> usernames)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(Username))
            errors.Add(nameof(Username), "Required");
        else if (usernames.Contains(Username.ToLower()))
            errors.Add(nameof(Username), "Duplicate");

        if (!string.IsNullOrWhiteSpace(TimezoneId))
            try { TimeZoneInfo.FindSystemTimeZoneById(TimezoneId); }
            catch (Exception) { errors.Add(nameof(TimezoneId), "Invalid"); }

        if (!string.IsNullOrWhiteSpace(LocaleId))
            try
            {
                var ci = CultureInfo.GetCultureInfo(LocaleId);
                if (ci.ThreeLetterWindowsLanguageName == "ZZZ")
                    errors.Add(nameof(LocaleId), "Invalid");
            }
            catch (Exception) { errors.Add(nameof(LocaleId), "Invalid"); }

        return errors.Any() ? errors : null;
    }
}